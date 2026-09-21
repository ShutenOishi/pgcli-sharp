using PgCliSharp.Internal.PgBench;
using PgCliSharp.Internal.Psql;

namespace PgCliSharp.Tests;

public sealed class Phase6ArgumentAndValidationTests
{
    [Fact]
    public void Psql_OrderedActionsAndVariableStates_ArePreserved()
    {
        var options = new PsqlOptions
        {
            Database = "appdb",
            Username = "app",
            PasswordPrompt = PgPasswordPromptMode.NeverPrompt,
            OutputFormat = PsqlOutputFormat.Csv,
            SingleTransaction = true,
        };
        options.Variables.Add(PsqlVariableAssignment.Unset("old"));
        options.Variables.Add(PsqlVariableAssignment.Set("empty", string.Empty));
        options.Actions.Add(PsqlAction.Command("SELECT 1"));
        options.Actions.Add(PsqlAction.File("script.sql"));
        options.Actions.Add(PsqlAction.Command("\\dt"));

        PsqlValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PsqlArgumentBuilder.Build(options);

        Assert.Contains("--csv", args);
        Assert.Contains("--single-transaction", args);
        Assert.Contains("old", ValuesAfter(args, "--set"));
        Assert.Contains("empty=", ValuesAfter(args, "--set"));

        List<string> ordered = ActionValues(args);
        Assert.Equal(3, ordered.Count);
        Assert.Equal("C:SELECT 1", ordered[0]);
        Assert.Equal("F:script.sql", ordered[1]);
        Assert.Equal("C:\\dt", ordered[2]);
    }

    [Fact]
    public void Psql_CsvAndSingleTransactionBoundaries_AreEnforced()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            PsqlValidator.Validate(
                new PsqlOptions { OutputFormat = PsqlOutputFormat.Csv },
                PostgreSqlMajorVersion.V11));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PsqlValidator.Validate(
                new PsqlOptions { SingleTransaction = true },
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Psql_ZeroSeparatorsUseDedicatedSwitches()
    {
        var options = new PsqlOptions
        {
            FieldSeparator = PsqlSeparator.ZeroByte(),
            RecordSeparator = PsqlSeparator.Text("|"),
        };

        PsqlValidator.Validate(options, PostgreSqlMajorVersion.V10);
        IReadOnlyList<string> args = PsqlArgumentBuilder.Build(options);

        Assert.Contains("--field-separator-zero", args);
        Assert.Equal("|", ValueAfter(args, "--record-separator"));
    }

    [Fact]
    public void PgBench_VersionCanonicalReportAndDebugSpellings_AreUsed()
    {
        var options = new PgBenchOptions { ReportPerCommand = true, Debug = true, Database = "appdb" };

        PgBenchValidator.Validate(options, PostgreSqlMajorVersion.V14);
        IReadOnlyList<string> pg14 = PgBenchArgumentBuilder.Build(options, PostgreSqlMajorVersion.V14);
        Assert.Contains("--report-latencies", pg14);
        Assert.DoesNotContain("--report-per-command", pg14);
        Assert.Contains("--debug", pg14);

        PgBenchValidator.Validate(options, PostgreSqlMajorVersion.V17);
        IReadOnlyList<string> pg17 = PgBenchArgumentBuilder.Build(options, PostgreSqlMajorVersion.V17);
        Assert.Contains("--report-per-command", pg17);
        Assert.DoesNotContain("--report-latencies", pg17);
        Assert.Contains("--debug", pg17);
        Assert.Equal("appdb", pg17[pg17.Count - 1]);
        Assert.DoesNotContain("--dbname", pg17);
    }

    [Fact]
    public void PgBench_VersionBoundaries_AreEnforced()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { RandomSeed = PgBenchRandomSeed.Time },
                PostgreSqlMajorVersion.V10));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { Partitions = 2 },
                PostgreSqlMajorVersion.V12));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { MaxTries = 2 },
                PostgreSqlMajorVersion.V14));

        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { ExitOnAbort = true },
                PostgreSqlMajorVersion.V16));
    }

    [Fact]
    public void PgBench_OrderedScriptsInitStepsAndRandomSeed_AreSerialized()
    {
        var options = new PgBenchOptions
        {
            Initialize = true,
            RandomSeed = PgBenchRandomSeed.Numeric(42),
            Database = "bench",
        };
        options.InitializationSteps.Add(PgBenchInitializationStep.Drop);
        options.InitializationSteps.Add(PgBenchInitializationStep.CreateTables);
        options.InitializationSteps.Add(PgBenchInitializationStep.GenerateServerSide);
        options.Scripts.Add(PgBenchScript.Builtin("select-only", 2));
        options.Scripts.Add(PgBenchScript.File("custom.sql"));

        PgBenchValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgBenchArgumentBuilder.Build(options, PostgreSqlMajorVersion.V18);

        Assert.Equal("dtG", ValueAfter(args, "--init-steps"));
        Assert.Equal("42", ValueAfter(args, "--random-seed"));
        List<string> scripts = ScriptValues(args);
        Assert.Equal(2, scripts.Count);
        Assert.Equal("B:select-only@2", scripts[0]);
        Assert.Equal("F:custom.sql", scripts[1]);
    }

    [Fact]
    public void PgBench_KnownHardCombinations_AreRejected()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { DurationSeconds = 10, TransactionsPerClient = 100 },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { AggregateIntervalSeconds = 5 },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { MaxTries = 0 },
                PostgreSqlMajorVersion.V18));
    }

 
    [Fact]
    public void PgBench_InitStepServerGeneration_BeginsInPostgreSql13()
    {
        var options = new PgBenchOptions { Initialize = true };
        options.InitializationSteps.Add(PgBenchInitializationStep.GenerateServerSide);

        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgBenchValidator.Validate(options, PostgreSqlMajorVersion.V12));

        PgBenchValidator.Validate(options, PostgreSqlMajorVersion.V13);
        Assert.Equal(
            "G",
            ValueAfter(
                PgBenchArgumentBuilder.Build(options, PostgreSqlMajorVersion.V13),
                "--init-steps"));
    }

    [Fact]
    public void PgBench_SelectOnlyAndSkipSomeUpdates_CanBeCombinedAsScripts()
    {
        var options = new PgBenchOptions
        {
            SelectOnly = true,
            SkipSomeUpdates = true,
        };

        PgBenchValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgBenchArgumentBuilder.Build(
            options,
            PostgreSqlMajorVersion.V18);

        Assert.Contains("--select-only", args);
        Assert.Contains("--skip-some-updates", args);
    }

    [Fact]
    public void PgBench_LoggingAndProgressDependencies_AreValidated()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { SamplingRate = 0.5 },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions
                {
                    SamplingRate = 0.5,
                    AggregateIntervalSeconds = 5,
                    LogTransactions = true,
                },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { LogPrefix = "benchlog" },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { ProgressTimestamp = true },
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgBench_AggregationAndPartitionConstraints_AreValidated()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions
                {
                    LogTransactions = true,
                    DurationSeconds = 10,
                    AggregateIntervalSeconds = 6,
                },
                PostgreSqlMajorVersion.V18));

        var zeroPartitions = new PgBenchOptions
        {
            Initialize = true,
            Partitions = 0,
        };
        PgBenchValidator.Validate(zeroPartitions, PostgreSqlMajorVersion.V18);

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions
                {
                    Initialize = true,
                    Partitions = 0,
                    PartitionMethod = PgBenchPartitionMethod.Hash,
                },
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgBench_InitializationAndBenchmarkModesRejectOppositeModeOptions()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions
                {
                    Initialize = true,
                    Clients = 4,
                },
                PostgreSqlMajorVersion.V18));

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBenchValidator.Validate(
                new PgBenchOptions { FillFactor = 90 },
                PostgreSqlMajorVersion.V18));

        PgBenchValidator.Validate(
            new PgBenchOptions
            {
                Initialize = true,
                NoVacuum = true,
                Scale = 10,
            },
            PostgreSqlMajorVersion.V18);
    }

    [Fact]
    public void PgBench_DefineRequiresANonEmptyValue()
    {
        var options = new PgBenchOptions();
        options.Variables.Add(new PgBenchVariableAssignment("name", string.Empty));

        Assert.Throws<PgInvalidOptionValueException>(() =>
            PgBenchValidator.Validate(options, PostgreSqlMajorVersion.V18));
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
            if (args[index] == option) values.Add(args[index + 1]);
        return values;
    }

    private static List<string> ActionValues(IReadOnlyList<string> args)
    {
        var values = new List<string>();
        for (int index = 0; index < args.Count - 1; index++)
        {
            if (args[index] == "--command") values.Add("C:" + args[index + 1]);
            if (args[index] == "--file") values.Add("F:" + args[index + 1]);
        }
        return values;
    }

    private static List<string> ScriptValues(IReadOnlyList<string> args)
    {
        var values = new List<string>();
        for (int index = 0; index < args.Count - 1; index++)
        {
            if (args[index] == "--builtin") values.Add("B:" + args[index + 1]);
            if (args[index] == "--file") values.Add("F:" + args[index + 1]);
        }
        return values;
    }
}
