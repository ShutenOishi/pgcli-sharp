using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.PgRestore;

internal static class PgRestoreValidator
{
    internal static void Validate(
        PgRestoreOptions options,
        PgRestoreInput input,
        PgRestoreOutput output,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        ValidateEnumValues(options, input, output, selectedVersion);
        ValidateScalarValues(options, selectedVersion);
        ValidateAvailability(options, selectedVersion, executableVersion);
        ValidateCombinations(options, input, output, selectedVersion);
        ValidateFilters(options, input, selectedVersion);
    }

    internal static Stream? GetStandardInput(
        PgRestoreOptions options,
        PgRestoreInput input)
    {
        if (input.Kind == PgRestoreInputKind.StandardInput)
        {
            return input.StandardInput;
        }

        PgRestoreFilterSource? source = options.Filters.FirstOrDefault(
            filter => filter.Kind == PgRestoreFilterSourceKind.StandardInput);
        return source?.Input;
    }

    private static void ValidateEnumValues(
        PgRestoreOptions options,
        PgRestoreInput input,
        PgRestoreOutput output,
        PostgreSqlMajorVersion selectedVersion)
    {
        EnsureEnum(options.Mode, "--list", selectedVersion);
        EnsureEnum(
            options.ContentMode,
            "--data-only/--schema-only/--statistics-only",
            selectedVersion);
        EnsureEnum(
            options.PasswordPrompt,
            "--password/--no-password",
            selectedVersion);
        EnsureEnum(input.Kind, "archive input", selectedVersion);
        EnsureEnum(output.Kind, "restore output", selectedVersion);

        if (options.ArchiveFormat.HasValue)
        {
            EnsureEnum(options.ArchiveFormat.Value, "--format", selectedVersion);
        }

        foreach (PgRestoreSection section in options.Sections)
        {
            EnsureEnum(section, "--section", selectedVersion);
        }

        if (options.TransactionMode is not null)
        {
            EnsureEnum(
                options.TransactionMode.Kind,
                "--single-transaction/--transaction-size",
                selectedVersion);
        }
    }

    private static void ValidateScalarValues(
        PgRestoreOptions options,
        PostgreSqlMajorVersion selectedVersion)
    {
        if (options.Port.HasValue &&
            (options.Port.Value < 1 || options.Port.Value > 65535))
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--port",
                options.Port.Value);
        }

        if (options.Jobs.HasValue && options.Jobs.Value <= 0)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--jobs",
                options.Jobs.Value);
        }

        if (options.Verbosity < 0)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--verbose",
                options.Verbosity);
        }

        if (options.TransactionMode?.Kind == PgRestoreTransactionModeKind.Batch &&
            options.TransactionMode.ObjectCount.GetValueOrDefault() <= 0)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--transaction-size",
                options.TransactionMode.ObjectCount);
        }

        if (options.UseListFile is not null &&
            string.IsNullOrWhiteSpace(options.UseListFile))
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--use-list",
                options.UseListFile);
        }
    }

    private static void ValidateAvailability(
        PgRestoreOptions options,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        EnsureIf(
            options.NoComments,
            PgRestoreOptionAvailabilityCatalog.NoComments,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.RestrictKey is not null,
            PgRestoreOptionAvailabilityCatalog.RestrictKey,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoTableAccessMethod,
            PgRestoreOptionAvailabilityCatalog.NoTableAccessMethod,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.TransactionMode?.Kind == PgRestoreTransactionModeKind.Batch,
            PgRestoreOptionAvailabilityCatalog.TransactionSize,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.Filters.Count > 0,
            PgRestoreOptionAvailabilityCatalog.Filter,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoData,
            PgRestoreOptionAvailabilityCatalog.NoData,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoPolicies,
            PgRestoreOptionAvailabilityCatalog.NoPolicies,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoSchema,
            PgRestoreOptionAvailabilityCatalog.NoSchema,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoStatistics,
            PgRestoreOptionAvailabilityCatalog.NoStatistics,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.Statistics,
            PgRestoreOptionAvailabilityCatalog.Statistics,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.ContentMode == PgRestoreContentMode.StatisticsOnly,
            PgRestoreOptionAvailabilityCatalog.StatisticsOnly,
            selectedVersion,
            executableVersion);
    }

    private static void ValidateCombinations(
        PgRestoreOptions options,
        PgRestoreInput input,
        PgRestoreOutput output,
        PostgreSqlMajorVersion selectedVersion)
    {
        Reject(
            options.ContentMode == PgRestoreContentMode.DataOnly && options.Clean,
            selectedVersion,
            "--data-only",
            "--clean");

        if (options.IfExists && !options.Clean)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--if-exists",
                "--clean");
        }

        if (options.TransactionMode?.Kind ==
            PgRestoreTransactionModeKind.SingleTransaction)
        {
            Reject(
                options.Jobs.GetValueOrDefault() > 1,
                selectedVersion,
                "--single-transaction",
                "--jobs");

            if (selectedVersion >= PostgreSqlMajorVersion.V12)
            {
                Reject(
                    options.Create,
                    selectedVersion,
                    "--create",
                    "--single-transaction");
            }
        }

        if (options.RestrictKey is not null &&
            output.Kind == PgRestoreOutputKind.Database)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--restrict-key",
                "--dbname");
        }

        if (options.ArchiveFormat == PgRestoreArchiveFormat.Directory &&
            input.Kind != PgRestoreInputKind.Directory)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--format=directory",
                "archive input");
        }

        if (input.Kind == PgRestoreInputKind.Directory &&
            options.ArchiveFormat.HasValue &&
            options.ArchiveFormat != PgRestoreArchiveFormat.Directory)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--format",
                "directory archive input");
        }

        if (options.Jobs.GetValueOrDefault() > 1 &&
            output.Kind == PgRestoreOutputKind.Database)
        {
            if (input.Kind == PgRestoreInputKind.StandardInput)
            {
                throw new PgInvalidOptionCombinationException(
                    selectedVersion,
                    "--jobs",
                    "standard-input archive");
            }

            if (options.ArchiveFormat == PgRestoreArchiveFormat.Tar)
            {
                throw new PgInvalidOptionCombinationException(
                    selectedVersion,
                    "--jobs",
                    "--format=tar");
            }
        }

        if (selectedVersion == PostgreSqlMajorVersion.V18)
        {
            Reject(
                options.ContentMode == PgRestoreContentMode.DataOnly &&
                options.NoData,
                selectedVersion,
                "--data-only",
                "--no-data");
            Reject(
                options.ContentMode == PgRestoreContentMode.SchemaOnly &&
                options.NoSchema,
                selectedVersion,
                "--schema-only",
                "--no-schema");
            Reject(
                options.ContentMode == PgRestoreContentMode.StatisticsOnly &&
                options.NoStatistics,
                selectedVersion,
                "--statistics-only",
                "--no-statistics");
            Reject(
                options.Statistics && options.NoStatistics,
                selectedVersion,
                "--statistics",
                "--no-statistics");
            Reject(
                options.ContentMode == PgRestoreContentMode.DataOnly &&
                options.Statistics,
                selectedVersion,
                "--data-only",
                "--statistics");
            Reject(
                options.ContentMode == PgRestoreContentMode.SchemaOnly &&
                options.Statistics,
                selectedVersion,
                "--schema-only",
                "--statistics");
        }
    }

    private static void ValidateFilters(
        PgRestoreOptions options,
        PgRestoreInput input,
        PostgreSqlMajorVersion selectedVersion)
    {
        int standardInputCount = 0;

        foreach (PgRestoreFilterSource filter in options.Filters)
        {
            if (filter is null)
            {
                throw new PgInvalidOptionValueException(
                    selectedVersion,
                    "--filter",
                    null);
            }

            EnsureEnum(filter.Kind, "--filter", selectedVersion);

            if (filter.Kind == PgRestoreFilterSourceKind.StandardInput)
            {
                standardInputCount++;

                if (filter.Input is null || !filter.Input.CanRead)
                {
                    throw new PgInvalidOptionValueException(
                        selectedVersion,
                        "--filter=-",
                        "unreadable stream");
                }
            }
            else if (string.IsNullOrWhiteSpace(filter.Path))
            {
                throw new PgInvalidOptionValueException(
                    selectedVersion,
                    "--filter",
                    filter.Path);
            }
        }

        if (standardInputCount > 1)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--filter=-",
                "--filter=-");
        }

        if (standardInputCount == 1 &&
            input.Kind == PgRestoreInputKind.StandardInput)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--filter=-",
                "archive standard input");
        }
    }

    private static void EnsureIf(
        bool requested,
        PgRestoreOptionAvailabilityInfo availability,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        if (requested)
        {
            PgRestoreOptionAvailability.Ensure(
                availability,
                selectedVersion,
                executableVersion);
        }
    }

    private static void Reject(
        bool condition,
        PostgreSqlMajorVersion selectedVersion,
        params string[] optionNames)
    {
        if (condition)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                optionNames);
        }
    }

    private static void EnsureEnum<T>(
        T value,
        string optionName,
        PostgreSqlMajorVersion selectedVersion)
        where T : struct, Enum
    {
#if NETSTANDARD2_0
        bool defined = Enum.IsDefined(typeof(T), value);
#else
        bool defined = Enum.IsDefined(value);
#endif
        if (!defined)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                optionName,
                value);
        }
    }
}
