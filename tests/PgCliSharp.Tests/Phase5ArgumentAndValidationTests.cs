using PgCliSharp.Internal.Clusterdb;
using PgCliSharp.Internal.Createdb;
using PgCliSharp.Internal.Createuser;
using PgCliSharp.Internal.Dropdb;
using PgCliSharp.Internal.Dropuser;
using PgCliSharp.Internal.PgAmcheck;
using PgCliSharp.Internal.PgIsReady;
using PgCliSharp.Internal.Reindexdb;
using PgCliSharp.Internal.Vacuumdb;

namespace PgCliSharp.Tests;

public sealed class Phase5ArgumentAndValidationTests
{
    [Fact]
    public void CreateDb_Pg18Arguments_AreTypedAndDeterministic()
    {
        var options = new CreateDbOptions
        {
            Host = "db.example",
            Port = 5433,
            Username = "admin",
            PasswordPrompt = PgPasswordPromptMode.NeverPrompt,
            Owner = "owner1",
            Strategy = PgCreateDbStrategy.FileCopy,
            LocaleProvider = PgCreateDbLocaleProvider.Builtin,
            BuiltinLocale = "C.UTF-8",
            DatabaseName = "appdb",
            Description = "application database",
        };

        CreatedbValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = CreatedbArgumentBuilder.Build(options, PostgreSqlMajorVersion.V18);

        Assert.Equal("db.example", ValueAfter(args, "--host"));
        Assert.Equal("5433", ValueAfter(args, "--port"));
        Assert.Contains("--no-password", args);
        Assert.Equal("file_copy", ValueAfter(args, "--strategy"));
        Assert.Equal("builtin", ValueAfter(args, "--locale-provider"));
        Assert.Equal("C.UTF-8", ValueAfter(args, "--builtin-locale"));
        Assert.Equal("appdb", args[args.Count - 2]);
        Assert.Equal("application database", args[args.Count - 1]);
    }

    [Fact]
    public void CreateDb_VersionBoundaries_AreEnforced()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            CreatedbValidator.Validate(
                new CreateDbOptions { Strategy = PgCreateDbStrategy.WalLog },
                PostgreSqlMajorVersion.V14));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            CreatedbValidator.Validate(
                new CreateDbOptions { IcuRules = "&a<b" },
                PostgreSqlMajorVersion.V15));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            CreatedbValidator.Validate(
                new CreateDbOptions { LocaleProvider = PgCreateDbLocaleProvider.Builtin },
                PostgreSqlMajorVersion.V16));
    }

    [Fact]
    public void DropDb_ForceBoundaryAndRequiredName_AreEnforced()
    {
        Assert.Throws<PgInvalidOptionValueException>(() =>
            DropdbValidator.Validate(new DropDbOptions(), PostgreSqlMajorVersion.V18));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            DropdbValidator.Validate(
                new DropDbOptions { DatabaseName = "appdb", Force = true },
                PostgreSqlMajorVersion.V12));

        var options = new DropDbOptions { DatabaseName = "appdb", Force = true, IfExists = true };
        DropdbValidator.Validate(options, PostgreSqlMajorVersion.V13);
        IReadOnlyList<string> args = DropdbArgumentBuilder.Build(options);
        Assert.Contains("--force", args);
        Assert.Contains("--if-exists", args);
        Assert.Equal("appdb", args[args.Count - 1]);
    }

    [Fact]
    public void CreateUser_MembershipUsesVersionCanonicalSpelling()
    {
        var options = new CreateUserOptions { RoleName = "app_role" };
        options.MemberOfRoles.Add("readers");

        CreateuserValidator.Validate(options, PostgreSqlMajorVersion.V15);
        IReadOnlyList<string> pg15 = CreateuserArgumentBuilder.Build(options, PostgreSqlMajorVersion.V15);
        Assert.Equal("readers", ValueAfter(pg15, "--role"));
        Assert.DoesNotContain("--member-of", pg15);

        CreateuserValidator.Validate(options, PostgreSqlMajorVersion.V16);
        IReadOnlyList<string> pg16 = CreateuserArgumentBuilder.Build(options, PostgreSqlMajorVersion.V16);
        Assert.Equal("readers", ValueAfter(pg16, "--member-of"));
        Assert.DoesNotContain("--role", pg16);
    }

    [Fact]
    public void CreateUser_Pg16OnlyRoleOptions_AreRejectedEarlier()
    {
        var options = new CreateUserOptions { RoleName = "app_role", BypassRls = true };
        options.AdminOfRoles.Add("admins");

        Assert.Throws<PgUnsupportedOptionException>(() =>
            CreateuserValidator.Validate(options, PostgreSqlMajorVersion.V15));
    }

    [Fact]
    public void DropUser_MissingRoleRequiresInteractiveMode()
    {
        Assert.Throws<PgInvalidOptionValueException>(() =>
            DropuserValidator.Validate(new DropUserOptions(), PostgreSqlMajorVersion.V18));

        var interactive = new DropUserOptions { Interactive = true };
        DropuserValidator.Validate(interactive, PostgreSqlMajorVersion.V18);
        Assert.Contains("--interactive", DropuserArgumentBuilder.Build(interactive));
    }

    [Fact]
    public void VacuumDb_VersionBoundariesAndAnalyzeOnlyConflicts_AreEnforced()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            VacuumdbValidator.Validate(
                new VacuumDbOptions { SkipLocked = true },
                PostgreSqlMajorVersion.V11));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            VacuumdbValidator.Validate(
                new VacuumDbOptions { AnalyzeOnly = true, Full = true },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            VacuumdbValidator.Validate(
                new VacuumDbOptions { MissingStatsOnly = true, AnalyzeOnly = true },
                PostgreSqlMajorVersion.V17));
    }

    [Fact]
    public void VacuumDb_RepeatableSelectorsPreserveOrder()
    {
        var options = new VacuumDbOptions { Analyze = true, Jobs = 2 };
        options.Tables.Add("public.a");
        options.Tables.Add("public.b");

        VacuumdbValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = VacuumdbArgumentBuilder.Build(options);

        List<string> tableValues = ValuesAfter(args, "--table");
        Assert.Equal(2, tableValues.Count);
        Assert.Equal("public.a", tableValues[0]);
        Assert.Equal("public.b", tableValues[1]);
        Assert.Equal("2", ValueAfter(args, "--jobs"));
    }

    [Fact]
    public void ReindexDb_BoundariesAndSystemJobsConflict_AreEnforced()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            ReindexdbValidator.Validate(
                new ReindexDbOptions { Concurrently = true },
                PostgreSqlMajorVersion.V11));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            ReindexdbValidator.Validate(
                new ReindexDbOptions { SystemCatalogs = true, Jobs = 2 },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            ReindexdbValidator.Validate(
                new ReindexDbOptions { Tablespace = "fastspace" },
                PostgreSqlMajorVersion.V13));
    }

    [Fact]
    public void ClusterDb_AllDatabasesConflictsWithDatabaseName()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            ClusterdbValidator.Validate(
                new ClusterDbOptions { AllDatabases = true, DatabaseName = "appdb" },
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgIsReady_ZeroTimeoutIsValidAndSerialized()
    {
        var options = new PgIsReadyOptions
        {
            Database = "appdb",
            Host = "localhost",
            Port = 5432,
            ConnectTimeoutSeconds = 0,
            Username = "app",
            Quiet = true,
        };

        PgIsReadyValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgIsReadyArgumentBuilder.Build(options);

        Assert.Equal("0", ValueAfter(args, "--timeout"));
        Assert.Contains("--quiet", args);
        Assert.Equal("appdb", ValueAfter(args, "--dbname"));
    }

    [Fact]
    public void PgAmcheck_OptionalArgumentAndVersionBoundary_AreModeled()
    {
        var options = new PgAmcheckOptions
        {
            DatabaseName = "appdb",
            InstallMissing = PgAmcheckInstallMissing.InSchema("audit"),
            Skip = PgAmcheckSkipMode.AllVisible,
            CheckUnique = true,
        };

        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgAmcheckValidator.Validate(options, PostgreSqlMajorVersion.V16));

        PgAmcheckValidator.Validate(options, PostgreSqlMajorVersion.V17);
        IReadOnlyList<string> args = PgAmcheckArgumentBuilder.Build(options);

        Assert.Contains("--install-missing=audit", args);
        Assert.Equal("all-visible", ValueAfter(args, "--skip"));
        Assert.Contains("--checkunique", args);
        Assert.Equal("appdb", args[args.Count - 1]);
    }

    [Fact]
    public void PgAmcheck_BlockRangeAndDatabaseSelectors_AreValidated()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgAmcheckValidator.Validate(
                new PgAmcheckOptions { StartBlock = 10, EndBlock = 9 },
                PostgreSqlMajorVersion.V18));

        var options = new PgAmcheckOptions { DatabaseName = "appdb" };
        options.DatabasePatterns.Add("other*");
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgAmcheckValidator.Validate(options, PostgreSqlMajorVersion.V18));
    }

    private static string ValueAfter(IReadOnlyList<string> args, string option)
    {
        int index = args.ToList().IndexOf(option);
        Assert.True(index >= 0 && index + 1 < args.Count, $"Missing value for {option}.");
        return args[index + 1];
    }

    private static List<string> ValuesAfter(IReadOnlyList<string> args, string option)
    {
        var values = new List<string>();
        for (int index = 0; index < args.Count - 1; index++)
            if (args[index] == option)
                values.Add(args[index + 1]);
        return values;
    }
}
