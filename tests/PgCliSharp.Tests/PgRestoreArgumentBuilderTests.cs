using PgCliSharp.Internal.PgRestore;

namespace PgCliSharp.Tests;

public sealed class PgRestoreArgumentBuilderTests
{
    [Fact]
    public void Build_AllModeledOptions_UseCanonicalDeterministicTokens()
    {
        var options = new PgRestoreOptions
        {
            Mode = PgRestoreMode.List,
            ContentMode = PgRestoreContentMode.StatisticsOnly,
            Clean = true,
            Create = true,
            ExitOnError = true,
            ArchiveFormat = PgRestoreArchiveFormat.Custom,
            Jobs = 3,
            NoPrivileges = true,
            NoOwner = true,
            NoReconnect = true,
            Superuser = "postgres",
            Verbosity = 2,
            TransactionMode = PgRestoreTransactionMode.Batch(25),
            DisableTriggers = true,
            EnableRowSecurity = true,
            IfExists = true,
            NoDataForFailedTables = true,
            NoTablespaces = true,
            UseSetSessionAuthorization = true,
            StrictNames = true,
            NoComments = true,
            NoPublications = true,
            NoSecurityLabels = true,
            NoSubscriptions = true,
            RestrictKey = new PgRestoreRestrictKey("SafeKey123"),
            NoTableAccessMethod = true,
            NoData = true,
            NoPolicies = true,
            NoSchema = true,
            NoStatistics = true,
            Statistics = true,
            Host = "db host",
            Port = 5433,
            Username = "app user",
            PasswordPrompt = PgPasswordPromptMode.ForcePrompt,
            Role = "restore_role",
            UseListFile = "toc.list",
        };
        options.Functions.Add("public.fn");
        options.Indexes.Add("public.idx");
        options.Schemas.Add("one");
        options.Schemas.Add("two");
        options.ExcludedSchemas.Add("secret");
        options.Tables.Add("public.table");
        options.Triggers.Add("trigger_name");
        options.Sections.Add(PgRestoreSection.PreData);
        options.Sections.Add(PgRestoreSection.PostData);
        options.Filters.Add(PgRestoreFilterSource.FromFile("restore.filter"));

        IReadOnlyList<string> arguments = PgRestoreArgumentBuilder.Build(
            options,
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToStream(Stream.Null));

        string[] expectedOptions =
        {
            "--file", "--host", "--port", "--username", "--password", "--role",
            "--clean", "--create", "--statistics-only", "--exit-on-error",
            "--format", "--function", "--index", "--jobs", "--list",
            "--no-privileges", "--no-owner", "--no-reconnect", "--schema",
            "--exclude-schema", "--superuser", "--table", "--trigger",
            "--use-list", "--verbose", "--transaction-size",
            "--disable-triggers", "--enable-row-security", "--if-exists",
            "--no-data-for-failed-tables", "--no-table-access-method",
            "--no-tablespaces", "--section", "--strict-names",
            "--use-set-session-authorization", "--no-comments", "--no-data",
            "--no-policies", "--no-publications", "--no-schema",
            "--no-security-labels", "--no-statistics", "--no-subscriptions",
            "--statistics", "--filter", "--restrict-key",
        };

        foreach (string option in expectedOptions)
        {
            Assert.Contains(option, arguments);
        }

        Assert.Equal("-", ValueAfter(arguments, "--file"));
        Assert.Equal("custom", ValueAfter(arguments, "--format"));
        Assert.Equal("25", ValueAfter(arguments, "--transaction-size"));
        Assert.Equal("SafeKey123", ValueAfter(arguments, "--restrict-key"));
        Assert.Equal("backup.dump", arguments[arguments.Count - 1]);

        AssertOrderedValues(arguments, "--schema", "one", "two");
        AssertOrderedValues(arguments, "--section", "pre-data", "post-data");
        Assert.Equal(2, arguments.Count(value => value == "--verbose"));
        Assert.DoesNotContain("--no-acl", arguments);
    }

    [Theory]
    [InlineData(PgRestoreContentMode.DataOnly, "--data-only")]
    [InlineData(PgRestoreContentMode.SchemaOnly, "--schema-only")]
    [InlineData(PgRestoreContentMode.StatisticsOnly, "--statistics-only")]
    public void Build_ContentModes_EmitOneCanonicalSwitch(
        PgRestoreContentMode mode,
        string expected)
    {
        var options = new PgRestoreOptions { ContentMode = mode };

        IReadOnlyList<string> arguments = PgRestoreArgumentBuilder.Build(
            options,
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToFile("restore.sql"));

        Assert.Contains(expected, arguments);
    }

    [Fact]
    public void Build_SingleTransactionAndNeverPrompt_EmitCanonicalSwitches()
    {
        var options = new PgRestoreOptions
        {
            TransactionMode = PgRestoreTransactionMode.SingleTransaction,
            PasswordPrompt = PgPasswordPromptMode.NeverPrompt,
        };

        IReadOnlyList<string> arguments = PgRestoreArgumentBuilder.Build(
            options,
            PgRestoreInput.FromDirectory("backupdir"),
            PgRestoreOutput.ToDatabase("appdb"));

        Assert.Contains("--single-transaction", arguments);
        Assert.Contains("--no-password", arguments);
        Assert.Equal("appdb", ValueAfter(arguments, "--dbname"));
        Assert.Equal("backupdir", arguments[^1]);
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
