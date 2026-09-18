using System.Globalization;

namespace PgCliSharp.Internal.PgDump;

internal static class PgDumpArgumentBuilder
{
    internal static IReadOnlyList<string> Build(
        PgDumpOptions options,
        PgDumpOutput output,
        PostgreSqlMajorVersion version)
    {
        var arguments = new List<string>();

        AddValue(arguments, "--dbname", options.Database);
        AddValue(arguments, "--host", options.Host);
        AddValue(arguments, "--port", options.Port);
        AddValue(arguments, "--username", options.Username);

        switch (options.PasswordPrompt)
        {
            case PgPasswordPromptMode.NeverPrompt:
                arguments.Add("--no-password");
                break;
            case PgPasswordPromptMode.ForcePrompt:
                arguments.Add("--password");
                break;
        }

        AddValue(arguments, "--role", options.Role);

        if (options.DataOnly) arguments.Add("--data-only");

        switch (options.LargeObjects)
        {
            case PgDumpLargeObjectMode.Include:
                arguments.Add(version >= PostgreSqlMajorVersion.V16 ? "--large-objects" : "--blobs");
                break;
            case PgDumpLargeObjectMode.Exclude:
                arguments.Add(version >= PostgreSqlMajorVersion.V16 ? "--no-large-objects" : "--no-blobs");
                break;
        }

        if (options.Clean) arguments.Add("--clean");
        if (options.Create) arguments.Add("--create");
        AddValue(arguments, "--encoding", options.Encoding);

        arguments.Add("--format");
        arguments.Add(Format(options.Format));

        if (output.Kind != PgDumpOutputKind.StandardOutput)
        {
            AddValue(arguments, "--file", output.Path);
        }

        AddValue(arguments, "--jobs", options.Jobs);
        AddRepeatable(arguments, "--schema", options.Schemas);
        AddRepeatable(arguments, "--exclude-schema", options.ExcludedSchemas);

        if (options.IncludeOids) arguments.Add("--oids");
        if (options.NoOwner) arguments.Add("--no-owner");
        if (options.NoReconnect) arguments.Add("--no-reconnect");
        if (options.SchemaOnly) arguments.Add("--schema-only");
        AddValue(arguments, "--superuser", options.Superuser);
        AddRepeatable(arguments, "--table", options.Tables);
        AddRepeatable(arguments, "--exclude-table", options.ExcludedTables);

        for (int index = 0; index < options.Verbosity; index++)
        {
            arguments.Add("--verbose");
        }

        if (options.NoPrivileges) arguments.Add("--no-privileges");

        if (options.Compression is not null)
        {
            AddValue(
                arguments,
                "--compress",
                FormatCompression(options.Compression));
        }

        if (options.BinaryUpgrade) arguments.Add("--binary-upgrade");
        if (options.ColumnInserts) arguments.Add("--column-inserts");
        if (options.DisableDollarQuoting) arguments.Add("--disable-dollar-quoting");
        if (options.DisableTriggers) arguments.Add("--disable-triggers");
        if (options.EnableRowSecurity) arguments.Add("--enable-row-security");

        AddRepeatable(arguments, "--exclude-table-data", options.ExcludedTableData);

        if (options.IfExists) arguments.Add("--if-exists");
        if (options.Inserts) arguments.Add("--inserts");

        if (options.LockWaitTimeout.HasValue)
        {
            long milliseconds = checked(
                (long)Math.Ceiling(options.LockWaitTimeout.Value.TotalMilliseconds));
            AddValue(arguments, "--lock-wait-timeout", milliseconds);
        }

        if (options.NoPublications) arguments.Add("--no-publications");
        if (options.NoSecurityLabels) arguments.Add("--no-security-labels");
        if (options.NoSubscriptions) arguments.Add("--no-subscriptions");
        if (options.NoSync) arguments.Add("--no-sync");
        if (options.NoSynchronizedSnapshots) arguments.Add("--no-synchronized-snapshots");
        if (options.NoTablespaces) arguments.Add("--no-tablespaces");
        if (options.NoUnloggedTableData) arguments.Add("--no-unlogged-table-data");
        if (options.QuoteAllIdentifiers) arguments.Add("--quote-all-identifiers");

        foreach (PgDumpSection section in options.Sections)
        {
            AddValue(arguments, "--section", FormatSection(section));
        }

        if (options.SerializableDeferrable) arguments.Add("--serializable-deferrable");
        AddValue(arguments, "--snapshot", options.Snapshot);
        if (options.StrictNames) arguments.Add("--strict-names");
        if (options.UseSetSessionAuthorization) arguments.Add("--use-set-session-authorization");

        if (options.LoadViaPartitionRoot) arguments.Add("--load-via-partition-root");
        if (options.NoComments) arguments.Add("--no-comments");
        AddValue(arguments, "--extra-float-digits", options.ExtraFloatDigits);
        if (options.OnConflictDoNothing) arguments.Add("--on-conflict-do-nothing");
        AddValue(arguments, "--rows-per-insert", options.RowsPerInsert);

        AddRepeatable(arguments, "--include-foreign-data", options.IncludedForeignData);

        if (options.RestrictKey is not null)
        {
            AddValue(arguments, "--restrict-key", options.RestrictKey.Value);
        }

        AddRepeatable(arguments, "--extension", options.Extensions);
        if (options.NoToastCompression) arguments.Add("--no-toast-compression");
        if (options.NoTableAccessMethod) arguments.Add("--no-table-access-method");

        AddRepeatable(arguments, "--table-and-children", options.TablesAndChildren);
        AddRepeatable(arguments, "--exclude-table-and-children", options.ExcludedTablesAndChildren);
        AddRepeatable(arguments, "--exclude-table-data-and-children", options.ExcludedTableDataAndChildren);

        AddRepeatable(arguments, "--exclude-extension", options.ExcludedExtensions);

        foreach (PgDumpFilterSource filter in options.Filters)
        {
            AddValue(
                arguments,
                "--filter",
                filter.Kind == PgDumpFilterSourceKind.StandardInput
                    ? "-"
                    : filter.Path);
        }

        if (options.SyncMethod.HasValue)
        {
            AddValue(arguments, "--sync-method", FormatSyncMethod(options.SyncMethod.Value));
        }

        if (options.NoData) arguments.Add("--no-data");
        if (options.NoPolicies) arguments.Add("--no-policies");
        if (options.NoSchema) arguments.Add("--no-schema");
        if (options.NoStatistics) arguments.Add("--no-statistics");
        if (options.SequenceData) arguments.Add("--sequence-data");
        if (options.Statistics) arguments.Add("--statistics");
        if (options.StatisticsOnly) arguments.Add("--statistics-only");

        return arguments;
    }

    internal static string FormatCompression(PgDumpCompression compression)
    {
        if (compression.IsLevelOnly)
        {
            return compression.Level!.Value.ToString(CultureInfo.InvariantCulture);
        }

        string method = compression.Method switch
        {
            PgDumpCompressionMethod.Gzip => "gzip",
            PgDumpCompressionMethod.Lz4 => "lz4",
            PgDumpCompressionMethod.Zstd => "zstd",
            PgDumpCompressionMethod.None => "none",
            _ => throw new ArgumentOutOfRangeException(
                nameof(compression),
                compression.Method,
                null),
        };

        if (!compression.Level.HasValue && !compression.LongMode)
        {
            return method;
        }

        var details = new List<string>();
        if (compression.Level.HasValue)
        {
            details.Add(
                "level=" +
                compression.Level.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (compression.LongMode)
        {
            details.Add("long");
        }

        return method + ":" + string.Join(",", details);
    }

    private static string Format(PgDumpFormat format)
    {
        return format switch
        {
            PgDumpFormat.Plain => "plain",
            PgDumpFormat.Custom => "custom",
            PgDumpFormat.Directory => "directory",
            PgDumpFormat.Tar => "tar",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
    }

    private static string FormatSection(PgDumpSection section)
    {
        return section switch
        {
            PgDumpSection.PreData => "pre-data",
            PgDumpSection.Data => "data",
            PgDumpSection.PostData => "post-data",
            _ => throw new ArgumentOutOfRangeException(nameof(section), section, null),
        };
    }

    private static string FormatSyncMethod(PgDumpSyncMethod method)
    {
        return method switch
        {
            PgDumpSyncMethod.Fsync => "fsync",
            PgDumpSyncMethod.Syncfs => "syncfs",
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
        };
    }

    private static void AddRepeatable(
        ICollection<string> arguments,
        string option,
        IEnumerable<string> values)
    {
        foreach (string value in values)
        {
            AddValue(arguments, option, value);
        }
    }

    private static void AddValue(
        ICollection<string> arguments,
        string option,
        object? value)
    {
        if (value is null)
        {
            return;
        }

        string formatted = value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value.ToString()!;

        arguments.Add(option);
        arguments.Add(formatted);
    }
}
