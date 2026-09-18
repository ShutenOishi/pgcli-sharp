using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.PgDumpAll;

internal static class PgDumpAllValidator
{
    internal static void Validate(
        PgDumpAllOptions options,
        PgDumpAllOutput output,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        ValidateEnumValues(options, output, selectedVersion);
        ValidateScalarValues(options, selectedVersion);
        ValidateAvailability(options, selectedVersion, executableVersion);
        ValidateCombinations(options, selectedVersion);
        ValidateFilters(options, selectedVersion);
    }

    internal static Stream? GetStandardInput(PgDumpAllOptions options)
    {
        PgDumpAllFilterSource? source = options.Filters.FirstOrDefault(
            filter => filter.Kind == PgDumpAllFilterSourceKind.StandardInput);
        return source?.Input;
    }

    private static void ValidateEnumValues(
        PgDumpAllOptions options,
        PgDumpAllOutput output,
        PostgreSqlMajorVersion selectedVersion)
    {
        EnsureEnum(options.Scope, "scope", selectedVersion);
        EnsureEnum(
            options.ContentMode,
            "--data-only/--schema-only/--statistics-only",
            selectedVersion);
        EnsureEnum(
            options.PasswordPrompt,
            "--password/--no-password",
            selectedVersion);
        EnsureEnum(output.Kind, "output", selectedVersion);
    }

    private static void ValidateScalarValues(
        PgDumpAllOptions options,
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

        if (options.Verbosity < 0)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--verbose",
                options.Verbosity);
        }

        if (options.LockWaitTimeout.HasValue &&
            options.LockWaitTimeout.Value < TimeSpan.Zero)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--lock-wait-timeout",
                options.LockWaitTimeout.Value);
        }

        if (options.ExtraFloatDigits.HasValue &&
            (options.ExtraFloatDigits.Value < -15 ||
             options.ExtraFloatDigits.Value > 3))
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--extra-float-digits",
                options.ExtraFloatDigits.Value);
        }

        if (options.RowsPerInsert.HasValue &&
            options.RowsPerInsert.Value <= 0)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--rows-per-insert",
                options.RowsPerInsert.Value);
        }
    }

    private static void ValidateAvailability(
        PgDumpAllOptions options,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        EnsureIf(
            options.IncludeOids,
            PgDumpAllOptionAvailabilityCatalog.Oids,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.Encoding is not null,
            PgDumpAllOptionAvailabilityCatalog.Encoding,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.LoadViaPartitionRoot,
            PgDumpAllOptionAvailabilityCatalog.LoadViaPartitionRoot,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoComments,
            PgDumpAllOptionAvailabilityCatalog.NoComments,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.ExcludedDatabases.Count > 0,
            PgDumpAllOptionAvailabilityCatalog.ExcludeDatabase,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.ExtraFloatDigits.HasValue,
            PgDumpAllOptionAvailabilityCatalog.ExtraFloatDigits,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.OnConflictDoNothing,
            PgDumpAllOptionAvailabilityCatalog.OnConflictDoNothing,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.RowsPerInsert.HasValue,
            PgDumpAllOptionAvailabilityCatalog.RowsPerInsert,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.RestrictKey is not null,
            PgDumpAllOptionAvailabilityCatalog.RestrictKey,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoToastCompression,
            PgDumpAllOptionAvailabilityCatalog.NoToastCompression,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoTableAccessMethod,
            PgDumpAllOptionAvailabilityCatalog.NoTableAccessMethod,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.Filters.Count > 0,
            PgDumpAllOptionAvailabilityCatalog.Filter,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoData,
            PgDumpAllOptionAvailabilityCatalog.NoData,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoPolicies,
            PgDumpAllOptionAvailabilityCatalog.NoPolicies,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoSchema,
            PgDumpAllOptionAvailabilityCatalog.NoSchema,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.NoStatistics,
            PgDumpAllOptionAvailabilityCatalog.NoStatistics,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.Statistics,
            PgDumpAllOptionAvailabilityCatalog.Statistics,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.ContentMode == PgDumpAllContentMode.StatisticsOnly,
            PgDumpAllOptionAvailabilityCatalog.StatisticsOnly,
            selectedVersion,
            executableVersion);
        EnsureIf(
            options.SequenceData,
            PgDumpAllOptionAvailabilityCatalog.SequenceData,
            selectedVersion,
            executableVersion);
    }

    private static void ValidateCombinations(
        PgDumpAllOptions options,
        PostgreSqlMajorVersion selectedVersion)
    {
        if (options.Scope != PgDumpAllScope.All &&
            (options.ExcludedDatabases.Count > 0 ||
             options.Filters.Count > 0))
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--exclude-database/--filter",
                "--globals-only/--roles-only/--tablespaces-only");
        }

        if (options.IfExists && !options.Clean)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--if-exists",
                "--clean");
        }

        if (options.Scope == PgDumpAllScope.All &&
            options.OnConflictDoNothing &&
            !options.Inserts &&
            !options.ColumnInserts &&
            !options.RowsPerInsert.HasValue)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--on-conflict-do-nothing",
                "--inserts/--column-inserts/--rows-per-insert");
        }

        if (selectedVersion == PostgreSqlMajorVersion.V18 &&
            options.Scope == PgDumpAllScope.All)
        {
            Reject(
                options.ContentMode == PgDumpAllContentMode.DataOnly &&
                options.NoData,
                selectedVersion,
                "--data-only",
                "--no-data");
            Reject(
                options.ContentMode == PgDumpAllContentMode.SchemaOnly &&
                options.NoSchema,
                selectedVersion,
                "--schema-only",
                "--no-schema");
            Reject(
                options.ContentMode == PgDumpAllContentMode.StatisticsOnly &&
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
                options.ContentMode == PgDumpAllContentMode.DataOnly &&
                options.Statistics,
                selectedVersion,
                "--data-only",
                "--statistics");
            Reject(
                options.ContentMode == PgDumpAllContentMode.SchemaOnly &&
                options.Statistics,
                selectedVersion,
                "--schema-only",
                "--statistics");
        }
    }

    private static void ValidateFilters(
        PgDumpAllOptions options,
        PostgreSqlMajorVersion selectedVersion)
    {
        int standardInputCount = 0;

        foreach (PgDumpAllFilterSource filter in options.Filters)
        {
            if (filter is null)
            {
                throw new PgInvalidOptionValueException(
                    selectedVersion,
                    "--filter",
                    null);
            }

            EnsureEnum(filter.Kind, "--filter", selectedVersion);

            if (filter.Kind == PgDumpAllFilterSourceKind.StandardInput)
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
    }

    private static void EnsureIf(
        bool requested,
        PgDumpAllOptionAvailabilityInfo availability,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        if (requested)
        {
            PgDumpAllOptionAvailability.Ensure(
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
