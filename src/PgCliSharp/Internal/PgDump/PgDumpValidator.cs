using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.PgDump;

internal static class PgDumpValidator
{
    internal static void Validate(
        PgDumpOptions options,
        PgDumpOutput output,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        ValidateEnumValues(options, selectedVersion);
        ValidateScalarValues(options, selectedVersion);
        ValidateAvailability(options, selectedVersion, executableVersion);
        ValidateOutput(options, output, selectedVersion);
        ValidateCombinations(options, selectedVersion);
        ValidateFilters(options, selectedVersion);
    }

    internal static Stream? GetStandardInput(PgDumpOptions options)
    {
        PgDumpFilterSource? source = options.Filters.FirstOrDefault(
            filter => filter.Kind == PgDumpFilterSourceKind.StandardInput);
        return source?.Input;
    }

    private static void ValidateEnumValues(
        PgDumpOptions options,
        PostgreSqlMajorVersion selectedVersion)
    {
        EnsureEnum(options.Format, "--format", selectedVersion);
        EnsureEnum(options.LargeObjects, "--large-objects/--no-large-objects", selectedVersion);
        EnsureEnum(options.PasswordPrompt, "--password/--no-password", selectedVersion);

        if (options.SyncMethod.HasValue)
        {
            EnsureEnum(options.SyncMethod.Value, "--sync-method", selectedVersion);
        }

        foreach (PgDumpSection section in options.Sections)
        {
            EnsureEnum(section, "--section", selectedVersion);
        }
    }

    private static void ValidateScalarValues(
        PgDumpOptions options,
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

        if (options.RowsPerInsert.HasValue && options.RowsPerInsert.Value <= 0)
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                "--rows-per-insert",
                options.RowsPerInsert.Value);
        }

        ValidateNonEmptyValues(options.IncludedForeignData, "--include-foreign-data", selectedVersion);
    }

    private static void ValidateAvailability(
        PgDumpOptions options,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        if (options.IncludeOids)
        {
            PgDumpOptionAvailability.EnsureMajor(
                selectedVersion,
                "--oids",
                PostgreSqlMajorVersion.V10,
                PostgreSqlMajorVersion.V11);
        }

        if (options.NoSynchronizedSnapshots)
        {
            PgDumpOptionAvailability.EnsureMajor(
                selectedVersion,
                "--no-synchronized-snapshots",
                PostgreSqlMajorVersion.V10,
                PostgreSqlMajorVersion.V14);
        }

        if (options.LoadViaPartitionRoot)
        {
            EnsureSince(selectedVersion, "--load-via-partition-root", PostgreSqlMajorVersion.V11);
        }

        if (options.NoComments)
        {
            EnsureSince(selectedVersion, "--no-comments", PostgreSqlMajorVersion.V11);
        }

        if (options.ExtraFloatDigits.HasValue)
        {
            EnsureSince(selectedVersion, "--extra-float-digits", PostgreSqlMajorVersion.V12);
        }

        if (options.OnConflictDoNothing)
        {
            EnsureSince(selectedVersion, "--on-conflict-do-nothing", PostgreSqlMajorVersion.V12);
        }

        if (options.RowsPerInsert.HasValue)
        {
            EnsureSince(selectedVersion, "--rows-per-insert", PostgreSqlMajorVersion.V12);
        }

        if (options.IncludedForeignData.Count > 0)
        {
            EnsureSince(selectedVersion, "--include-foreign-data", PostgreSqlMajorVersion.V13);
        }

        if (options.RestrictKey is not null)
        {
            PgDumpOptionAvailability.EnsureRestrictKey(
                selectedVersion,
                executableVersion);
        }

        if (options.Extensions.Count > 0)
        {
            EnsureSince(selectedVersion, "--extension", PostgreSqlMajorVersion.V14);
        }

        if (options.NoToastCompression)
        {
            EnsureSince(selectedVersion, "--no-toast-compression", PostgreSqlMajorVersion.V14);
        }

        if (options.NoTableAccessMethod)
        {
            EnsureSince(selectedVersion, "--no-table-access-method", PostgreSqlMajorVersion.V15);
        }

        if (options.Compression is not null &&
            !options.Compression.IsLevelOnly)
        {
            EnsureSince(selectedVersion, "--compress=method[:detail]", PostgreSqlMajorVersion.V16);
        }

        if (options.TablesAndChildren.Count > 0)
        {
            EnsureSince(selectedVersion, "--table-and-children", PostgreSqlMajorVersion.V16);
        }

        if (options.ExcludedTablesAndChildren.Count > 0)
        {
            EnsureSince(selectedVersion, "--exclude-table-and-children", PostgreSqlMajorVersion.V16);
        }

        if (options.ExcludedTableDataAndChildren.Count > 0)
        {
            EnsureSince(selectedVersion, "--exclude-table-data-and-children", PostgreSqlMajorVersion.V16);
        }

        if (options.ExcludedExtensions.Count > 0)
        {
            EnsureSince(selectedVersion, "--exclude-extension", PostgreSqlMajorVersion.V17);
        }

        if (options.Filters.Count > 0)
        {
            EnsureSince(selectedVersion, "--filter", PostgreSqlMajorVersion.V17);
        }

        if (options.SyncMethod.HasValue)
        {
            EnsureSince(selectedVersion, "--sync-method", PostgreSqlMajorVersion.V17);
        }

        if (options.NoData)
        {
            EnsureSince(selectedVersion, "--no-data", PostgreSqlMajorVersion.V18);
        }

        if (options.NoPolicies)
        {
            EnsureSince(selectedVersion, "--no-policies", PostgreSqlMajorVersion.V18);
        }

        if (options.NoSchema)
        {
            EnsureSince(selectedVersion, "--no-schema", PostgreSqlMajorVersion.V18);
        }

        if (options.NoStatistics)
        {
            EnsureSince(selectedVersion, "--no-statistics", PostgreSqlMajorVersion.V18);
        }

        if (options.SequenceData)
        {
            EnsureSince(selectedVersion, "--sequence-data", PostgreSqlMajorVersion.V18);
        }

        if (options.Statistics)
        {
            EnsureSince(selectedVersion, "--statistics", PostgreSqlMajorVersion.V18);
        }

        if (options.StatisticsOnly)
        {
            EnsureSince(selectedVersion, "--statistics-only", PostgreSqlMajorVersion.V18);
        }
    }

    private static void ValidateOutput(
        PgDumpOptions options,
        PgDumpOutput output,
        PostgreSqlMajorVersion selectedVersion)
    {
        if (options.Format == PgDumpFormat.Directory &&
            output.Kind != PgDumpOutputKind.Directory)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--format=directory",
                "output target");
        }

        if (options.Format != PgDumpFormat.Directory &&
            output.Kind == PgDumpOutputKind.Directory)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--format",
                "directory output target");
        }

        if (options.Jobs.HasValue &&
            options.Format != PgDumpFormat.Directory)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--jobs",
                "--format=directory");
        }

        if (options.Compression is not null &&
            options.Format == PgDumpFormat.Tar &&
            !MeansNoCompression(options.Compression))
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--compress",
                "--format=tar");
        }

        if (options.RestrictKey is not null &&
            options.Format != PgDumpFormat.Plain)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--restrict-key",
                "--format=plain");
        }
    }

    private static void ValidateCombinations(
        PgDumpOptions options,
        PostgreSqlMajorVersion selectedVersion)
    {
        Reject(options.DataOnly && options.SchemaOnly, selectedVersion, "--data-only", "--schema-only");
        Reject(options.DataOnly && options.Clean, selectedVersion, "--data-only", "--clean");

        if (options.IfExists && !options.Clean)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--if-exists",
                "--clean");
        }

        if (options.OnConflictDoNothing &&
            !options.Inserts &&
            !options.ColumnInserts &&
            !options.RowsPerInsert.HasValue)
        {
            throw new PgInvalidOptionCombinationException(
                selectedVersion,
                "--on-conflict-do-nothing",
                "--inserts/--column-inserts/--rows-per-insert");
        }

        if (options.IncludedForeignData.Count > 0)
        {
            Reject(
                options.SchemaOnly,
                selectedVersion,
                "--schema-only",
                "--include-foreign-data");

            Reject(
                options.Jobs.GetValueOrDefault() > 1,
                selectedVersion,
                "--jobs",
                "--include-foreign-data");
        }

        if (selectedVersion == PostgreSqlMajorVersion.V18)
        {
            Reject(options.DataOnly && options.StatisticsOnly, selectedVersion, "--data-only", "--statistics-only");
            Reject(options.SchemaOnly && options.StatisticsOnly, selectedVersion, "--schema-only", "--statistics-only");
            Reject(options.DataOnly && options.NoData, selectedVersion, "--data-only", "--no-data");
            Reject(options.SchemaOnly && options.NoSchema, selectedVersion, "--schema-only", "--no-schema");
            Reject(options.StatisticsOnly && options.NoStatistics, selectedVersion, "--statistics-only", "--no-statistics");
            Reject(options.Statistics && options.NoStatistics, selectedVersion, "--statistics", "--no-statistics");
            Reject(options.DataOnly && options.Statistics, selectedVersion, "--data-only", "--statistics");
            Reject(options.SchemaOnly && options.Statistics, selectedVersion, "--schema-only", "--statistics");
        }
    }

    private static void ValidateFilters(
        PgDumpOptions options,
        PostgreSqlMajorVersion selectedVersion)
    {
        int standardInputCount = 0;

        foreach (PgDumpFilterSource filter in options.Filters)
        {
            if (filter is null)
            {
                throw new PgInvalidOptionValueException(
                    selectedVersion,
                    "--filter",
                    null);
            }

            if (!Enum.IsDefined(typeof(PgDumpFilterSourceKind), filter.Kind))
            {
                throw new PgInvalidOptionValueException(
                    selectedVersion,
                    "--filter",
                    filter.Kind);
            }

            if (filter.Kind == PgDumpFilterSourceKind.StandardInput)
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

    private static void EnsureSince(
        PostgreSqlMajorVersion selectedVersion,
        string optionName,
        PostgreSqlMajorVersion since)
    {
        PgDumpOptionAvailability.EnsureMajor(
            selectedVersion,
            optionName,
            since,
            PostgreSqlMajorVersion.V18);
    }

    private static void EnsureEnum<T>(
        T value,
        string optionName,
        PostgreSqlMajorVersion selectedVersion)
        where T : struct
    {
        if (!Enum.IsDefined(typeof(T), value))
        {
            throw new PgInvalidOptionValueException(
                selectedVersion,
                optionName,
                value);
        }
    }

    private static void ValidateNonEmptyValues(
        IEnumerable<string> values,
        string optionName,
        PostgreSqlMajorVersion selectedVersion)
    {
        foreach (string value in values)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new PgInvalidOptionValueException(
                    selectedVersion,
                    optionName,
                    value);
            }
        }
    }

    private static bool MeansNoCompression(PgDumpCompression compression)
    {
        return (compression.IsLevelOnly && compression.Level == 0) ||
               (!compression.IsLevelOnly &&
                compression.Method == PgDumpCompressionMethod.None);
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
}
