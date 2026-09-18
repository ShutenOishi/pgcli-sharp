using PgCliSharp.Internal.PgDumpAll;

namespace PgCliSharp.Tests;

public sealed class PgDumpAllArgumentBuilderTests
{
    [Fact]
    public void Build_AllModeledOptions_UseCanonicalDeterministicTokens()
    {
        var options = new PgDumpAllOptions
        {
            Scope = PgDumpAllScope.RolesOnly,
            ContentMode = PgDumpAllContentMode.StatisticsOnly,
            ConnectionString = "host=conn-host sslmode=require",
            InitialDatabase = "postgres",
            Host = "cli-host",
            Port = 5433,
            Username = "app user",
            PasswordPrompt = PgPasswordPromptMode.ForcePrompt,
            Role = "dump_role",
            Clean = true,
            Encoding = "UTF8",
            IncludeOids = true,
            NoOwner = true,
            Superuser = "postgres",
            Verbosity = 2,
            NoPrivileges = true,
            BinaryUpgrade = true,
            ColumnInserts = true,
            DisableDollarQuoting = true,
            DisableTriggers = true,
            IfExists = true,
            Inserts = true,
            LockWaitTimeout = TimeSpan.FromMilliseconds(1250),
            NoTablespaces = true,
            QuoteAllIdentifiers = true,
            UseSetSessionAuthorization = true,
            NoPublications = true,
            NoRolePasswords = true,
            NoSecurityLabels = true,
            NoSubscriptions = true,
            NoSync = true,
            NoUnloggedTableData = true,
            LoadViaPartitionRoot = true,
            NoComments = true,
            ExtraFloatDigits = 2,
            OnConflictDoNothing = true,
            RowsPerInsert = 50,
            RestrictKey = new PgDumpAllRestrictKey("SafeKey123"),
            NoToastCompression = true,
            NoTableAccessMethod = true,
            NoData = true,
            NoPolicies = true,
            NoSchema = true,
            NoStatistics = true,
            Statistics = true,
            SequenceData = true,
        };
        options.ExcludedDatabases.Add("db_one");
        options.ExcludedDatabases.Add("db_two");
        options.Filters.Add(PgDumpAllFilterSource.FromFile("dumpall.filter"));

        IReadOnlyList<string> arguments = PgDumpAllArgumentBuilder.Build(
            options,
            PgDumpAllOutput.ToFile("cluster.sql"));

        string[] expectedOptions =
        {
            "--dbname", "--host", "--database", "--port", "--username",
            "--password", "--role", "--statistics-only", "--clean",
            "--encoding", "--file", "--roles-only", "--oids", "--no-owner",
            "--superuser", "--verbose", "--no-privileges", "--binary-upgrade",
            "--column-inserts", "--disable-dollar-quoting", "--disable-triggers",
            "--exclude-database", "--extra-float-digits", "--if-exists",
            "--inserts", "--lock-wait-timeout", "--no-table-access-method",
            "--no-tablespaces", "--quote-all-identifiers",
            "--load-via-partition-root", "--use-set-session-authorization",
            "--no-comments", "--no-data", "--no-policies",
            "--no-publications", "--no-role-passwords", "--no-schema",
            "--no-security-labels", "--no-statistics", "--no-subscriptions",
            "--no-sync", "--no-toast-compression",
            "--no-unlogged-table-data", "--on-conflict-do-nothing",
            "--rows-per-insert", "--statistics", "--sequence-data",
            "--filter", "--restrict-key",
        };

        foreach (string option in expectedOptions)
        {
            Assert.Contains(option, arguments);
        }

        Assert.Equal("1250", ValueAfter(arguments, "--lock-wait-timeout"));
        Assert.Equal("cluster.sql", ValueAfter(arguments, "--file"));
        Assert.Equal("SafeKey123", ValueAfter(arguments, "--restrict-key"));
        AssertOrderedValues(
            arguments,
            "--exclude-database",
            "db_one",
            "db_two");
        Assert.Equal(2, arguments.Count(value => value == "--verbose"));
        Assert.DoesNotContain("--no-acl", arguments);
        Assert.DoesNotContain("--attribute-inserts", arguments);
    }

    [Theory]
    [InlineData(PgDumpAllScope.GlobalsOnly, "--globals-only")]
    [InlineData(PgDumpAllScope.RolesOnly, "--roles-only")]
    [InlineData(PgDumpAllScope.TablespacesOnly, "--tablespaces-only")]
    public void Build_Scope_EmitsOneCanonicalSwitch(
        PgDumpAllScope scope,
        string expected)
    {
        var options = new PgDumpAllOptions { Scope = scope };

        IReadOnlyList<string> arguments = PgDumpAllArgumentBuilder.Build(
            options,
            PgDumpAllOutput.ToStream(Stream.Null));

        Assert.Contains(expected, arguments);
    }

    [Theory]
    [InlineData(PgDumpAllContentMode.DataOnly, "--data-only")]
    [InlineData(PgDumpAllContentMode.SchemaOnly, "--schema-only")]
    [InlineData(PgDumpAllContentMode.StatisticsOnly, "--statistics-only")]
    public void Build_ContentMode_EmitsOneCanonicalSwitch(
        PgDumpAllContentMode mode,
        string expected)
    {
        var options = new PgDumpAllOptions { ContentMode = mode };

        IReadOnlyList<string> arguments = PgDumpAllArgumentBuilder.Build(
            options,
            PgDumpAllOutput.ToStream(Stream.Null));

        Assert.Contains(expected, arguments);
    }

    [Fact]
    public void Build_StandardOutputAndNeverPrompt_DoNotEmitFile()
    {
        var options = new PgDumpAllOptions
        {
            PasswordPrompt = PgPasswordPromptMode.NeverPrompt,
        };

        IReadOnlyList<string> arguments = PgDumpAllArgumentBuilder.Build(
            options,
            PgDumpAllOutput.ToStream(Stream.Null));

        Assert.Contains("--no-password", arguments);
        Assert.DoesNotContain("--file", arguments);
    }

    private static string ValueAfter(
        IReadOnlyList<string> arguments,
        string option)
    {
        int index = arguments.ToList().IndexOf(option);
        Assert.True(index >= 0 && index + 1 < arguments.Count);
        return arguments[index + 1];
    }

    private static void AssertOrderedValues(
        IReadOnlyList<string> arguments,
        string option,
        params string[] expected)
    {
        var actual = new List<string>();
        for (int index = 0; index < arguments.Count - 1; index++)
        {
            if (arguments[index] == option)
            {
                actual.Add(arguments[index + 1]);
            }
        }

        Assert.Equal(expected, actual);
    }
}
