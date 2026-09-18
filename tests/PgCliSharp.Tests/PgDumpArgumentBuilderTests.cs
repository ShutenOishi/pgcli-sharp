using PgCliSharp.Internal.PgDump;

namespace PgCliSharp.Tests;

public sealed class PgDumpArgumentBuilderTests
{
    [Fact]
    public void Build_LegacyPostgreSql10_UsesLegacyNamesAndPreservesRepeatableOrder()
    {
        var options = new PgDumpOptions
        {
            Database = "db name",
            Host = "db-host",
            Port = 5433,
            Username = "user",
            PasswordPrompt = PgPasswordPromptMode.NeverPrompt,
            DataOnly = true,
            LargeObjects = PgDumpLargeObjectMode.Include,
            Encoding = "UTF8",
            IncludeOids = true,
            NoSynchronizedSnapshots = true,
            Compression = PgDumpCompression.FromLevel(6),
            Verbosity = 2,
        };
        options.Schemas.Add("first");
        options.Schemas.Add("second");
        options.Tables.Add("public.a");
        options.Tables.Add("public.b");
        options.Sections.Add(PgDumpSection.PreData);
        options.Sections.Add(PgDumpSection.Data);

        IReadOnlyList<string> arguments = PgDumpArgumentBuilder.Build(
            options,
            PgDumpOutput.ToFile("backup.dump"),
            PostgreSqlMajorVersion.V10);

        AssertContainsPair(arguments, "--dbname", "db name");
        AssertContainsPair(arguments, "--format", "plain");
        AssertContainsPair(arguments, "--file", "backup.dump");
        Assert.Contains("--blobs", arguments);
        Assert.DoesNotContain("--large-objects", arguments);
        Assert.Contains("--oids", arguments);
        Assert.Contains("--no-synchronized-snapshots", arguments);
        AssertContainsPair(arguments, "--compress", "6");
        Assert.Equal(2, arguments.Count(a => a == "--verbose"));
        AssertRepeatableValues(arguments, "--schema", "first", "second");
        AssertRepeatableValues(arguments, "--table", "public.a", "public.b");
        AssertRepeatableValues(arguments, "--section", "pre-data", "data");
    }

    [Fact]
    public void Build_PostgreSql18_EmitsModernTypedOptions()
    {
        using var filterInput = new MemoryStream(new byte[] { 10, 20 });
        var options = new PgDumpOptions
        {
            LargeObjects = PgDumpLargeObjectMode.Exclude,
            NoOwner = true,
            NoReconnect = true,
            NoPrivileges = true,
            Compression = PgDumpCompression.ForMethod(
                PgDumpCompressionMethod.Zstd,
                level: 8,
                longMode: true),
            BinaryUpgrade = true,
            ColumnInserts = true,
            DisableDollarQuoting = true,
            DisableTriggers = true,
            EnableRowSecurity = true,
            IfExists = true,
            Clean = true,
            Inserts = true,
            LockWaitTimeout = TimeSpan.FromMilliseconds(1250),
            NoPublications = true,
            NoSecurityLabels = true,
            NoSubscriptions = true,
            NoSync = true,
            NoTablespaces = true,
            NoUnloggedTableData = true,
            QuoteAllIdentifiers = true,
            SerializableDeferrable = true,
            Snapshot = "snapshot-1",
            StrictNames = true,
            UseSetSessionAuthorization = true,
            LoadViaPartitionRoot = true,
            NoComments = true,
            ExtraFloatDigits = 3,
            OnConflictDoNothing = true,
            RowsPerInsert = 20,
            RestrictKey = new PgDumpRestrictKey("Key123"),
            NoToastCompression = true,
            NoTableAccessMethod = true,
            SyncMethod = PgDumpSyncMethod.Fsync,
            NoData = true,
            NoPolicies = true,
            NoSchema = true,
            SequenceData = true,
        };
        options.ExcludedSchemas.Add("private");
        options.ExcludedTables.Add("public.old");
        options.ExcludedTableData.Add("public.audit");
        options.IncludedForeignData.Add("foreign_*");
        options.Extensions.Add("postgis");
        options.TablesAndChildren.Add("public.parent");
        options.ExcludedTablesAndChildren.Add("public.skip");
        options.ExcludedTableDataAndChildren.Add("public.logs");
        options.ExcludedExtensions.Add("legacy_ext");
        options.Filters.Add(PgDumpFilterSource.FromFile("one.filter"));
        options.Filters.Add(PgDumpFilterSource.FromStandardInput(filterInput));

        IReadOnlyList<string> arguments = PgDumpArgumentBuilder.Build(
            options,
            PgDumpOutput.ToFile("dump.sql"),
            PostgreSqlMajorVersion.V18);

        Assert.Contains("--no-large-objects", arguments);
        Assert.DoesNotContain("--no-blobs", arguments);
        AssertContainsPair(arguments, "--compress", "zstd:level=8,long");
        AssertContainsPair(arguments, "--lock-wait-timeout", "1250");
        AssertContainsPair(arguments, "--restrict-key", "Key123");
        AssertContainsPair(arguments, "--sync-method", "fsync");
        AssertRepeatableValues(arguments, "--filter", "one.filter", "-");
        Assert.Contains("--no-data", arguments);
        Assert.Contains("--no-policies", arguments);
        Assert.Contains("--no-schema", arguments);
        Assert.Contains("--sequence-data", arguments);
    }

    [Theory]
    [InlineData(PgDumpCompressionMethod.Gzip, 6, false, "gzip:level=6")]
    [InlineData(PgDumpCompressionMethod.Lz4, 0, false, "lz4:level=0")]
    [InlineData(PgDumpCompressionMethod.Zstd, -5, true, "zstd:level=-5,long")]
    [InlineData(PgDumpCompressionMethod.None, null, false, "none")]
    public void FormatCompression_MethodSyntax_IsDeterministic(
        PgDumpCompressionMethod method,
        int? level,
        bool longMode,
        string expected)
    {
        PgDumpCompression compression = PgDumpCompression.ForMethod(
            method,
            level,
            longMode);

        Assert.Equal(expected, PgDumpArgumentBuilder.FormatCompression(compression));
    }

    [Fact]
    public void Build_DirectoryOutput_UsesFileOptionAndDirectoryFormat()
    {
        var options = new PgDumpOptions
        {
            Format = PgDumpFormat.Directory,
            Jobs = 4,
        };

        IReadOnlyList<string> arguments = PgDumpArgumentBuilder.Build(
            options,
            PgDumpOutput.ToDirectory("dump-dir"),
            PostgreSqlMajorVersion.V18);

        AssertContainsPair(arguments, "--format", "directory");
        AssertContainsPair(arguments, "--file", "dump-dir");
        AssertContainsPair(arguments, "--jobs", "4");
    }

    private static void AssertContainsPair(
        IReadOnlyList<string> arguments,
        string option,
        string value)
    {
        int index = IndexOf(arguments, option);
        Assert.True(index >= 0, $"Option {option} was not emitted.");
        Assert.True(index + 1 < arguments.Count);
        Assert.Equal(value, arguments[index + 1]);
    }

    private static void AssertRepeatableValues(
        IReadOnlyList<string> arguments,
        string option,
        params string[] expected)
    {
        var values = new List<string>();

        for (int index = 0; index < arguments.Count - 1; index++)
        {
            if (arguments[index] == option)
            {
                values.Add(arguments[index + 1]);
            }
        }

        Assert.Equal(expected, values);
    }

    private static int IndexOf(
        IReadOnlyList<string> arguments,
        string option)
    {
        for (int index = 0; index < arguments.Count; index++)
        {
            if (arguments[index] == option)
            {
                return index;
            }
        }

        return -1;
    }
}
