using System.Globalization;
using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.PgBench;

internal static class PgBenchOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo DbName = MaintenanceAvailability.Since("--dbname", PostgreSqlMajorVersion.V17);
    internal static readonly MaintenanceOptionAvailabilityInfo InitSteps = MaintenanceAvailability.Since("--init-steps", PostgreSqlMajorVersion.V11);
    internal static readonly MaintenanceOptionAvailabilityInfo ReportLatencies = MaintenanceAvailability.Range("--report-latencies", PostgreSqlMajorVersion.V10, PostgreSqlMajorVersion.V14);
    internal static readonly MaintenanceOptionAvailabilityInfo ReportPerCommand = MaintenanceAvailability.Since("--report-per-command", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo RandomSeed = MaintenanceAvailability.Since("--random-seed", PostgreSqlMajorVersion.V11);
    internal static readonly MaintenanceOptionAvailabilityInfo ShowScript = MaintenanceAvailability.Since("--show-script", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo Partitions = MaintenanceAvailability.Since("--partitions", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo PartitionMethod = MaintenanceAvailability.Since("--partition-method", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo FailuresDetailed = MaintenanceAvailability.Since("--failures-detailed", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo MaxTries = MaintenanceAvailability.Since("--max-tries", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo VerboseErrors = MaintenanceAvailability.Since("--verbose-errors", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo ExitOnAbort = MaintenanceAvailability.Since("--exit-on-abort", PostgreSqlMajorVersion.V17);

    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All =
        new[] { DbName, InitSteps, ReportLatencies, ReportPerCommand, RandomSeed, ShowScript, Partitions, PartitionMethod, FailuresDetailed, MaxTries, VerboseErrors, ExitOnAbort };
}

internal static class PgBenchArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgBenchOptions options, PostgreSqlMajorVersion version)
    {
        var args = new List<string>();

        MaintenanceArgument.AddFlag(args, "--initialize", options.Initialize);
        if (options.InitializationSteps.Count > 0)
            MaintenanceArgument.AddValue(args, "--init-steps", string.Concat(options.InitializationSteps.Select(FormatInitializationStep)));
        MaintenanceArgument.AddValue(args, "--fillfactor", options.FillFactor);
        MaintenanceArgument.AddFlag(args, "--no-vacuum", options.NoVacuum);
        MaintenanceArgument.AddFlag(args, "--quiet", options.Quiet);
        MaintenanceArgument.AddValue(args, "--scale", options.Scale);
        MaintenanceArgument.AddFlag(args, "--foreign-keys", options.ForeignKeys);
        MaintenanceArgument.AddValue(args, "--tablespace", options.Tablespace);
        MaintenanceArgument.AddValue(args, "--index-tablespace", options.IndexTablespace);
        MaintenanceArgument.AddFlag(args, "--unlogged-tables", options.UnloggedTables);
        MaintenanceArgument.AddValue(args, "--partitions", options.Partitions);
        if (options.PartitionMethod.HasValue)
            MaintenanceArgument.AddValue(args, "--partition-method", options.PartitionMethod.Value == PgBenchPartitionMethod.Range ? "range" : "hash");

        foreach (PgBenchScript script in options.Scripts)
        {
            string value = script.Weight == 1
                ? script.Value
                : script.Value + "@" + script.Weight.ToString(CultureInfo.InvariantCulture);
            MaintenanceArgument.AddValue(args, script.Kind == PgBenchScriptKind.Builtin ? "--builtin" : "--file", value);
        }

        MaintenanceArgument.AddValue(args, "--client", options.Clients);
        MaintenanceArgument.AddFlag(args, "--connect", options.ConnectPerTransaction);
        foreach (PgBenchVariableAssignment variable in options.Variables)
            MaintenanceArgument.AddValue(args, "--define", variable.Name + "=" + variable.Value);
        MaintenanceArgument.AddValue(args, "--jobs", options.Jobs);
        MaintenanceArgument.AddFlag(args, "--log", options.LogTransactions);
        MaintenanceArgument.AddValue(args, "--latency-limit", options.LatencyLimitMilliseconds);
        MaintenanceArgument.AddValue(args, "--progress", options.ProgressSeconds);

        if (options.Protocol.HasValue)
            MaintenanceArgument.AddValue(args, "--protocol", options.Protocol.Value switch
            {
                PgBenchProtocol.Simple => "simple",
                PgBenchProtocol.Extended => "extended",
                PgBenchProtocol.Prepared => "prepared",
                _ => throw new ArgumentOutOfRangeException(nameof(options)),
            });

        if (options.ReportPerCommand)
            args.Add((int)version <= 14 ? "--report-latencies" : "--report-per-command");

        MaintenanceArgument.AddValue(args, "--rate", options.Rate);
        MaintenanceArgument.AddFlag(args, "--select-only", options.SelectOnly);
        MaintenanceArgument.AddFlag(args, "--skip-some-updates", options.SkipSomeUpdates);
        MaintenanceArgument.AddValue(args, "--time", options.DurationSeconds);
        MaintenanceArgument.AddValue(args, "--transactions", options.TransactionsPerClient);
        MaintenanceArgument.AddFlag(args, "--vacuum-all", options.VacuumAll);
        MaintenanceArgument.AddValue(args, "--sampling-rate", options.SamplingRate);
        MaintenanceArgument.AddValue(args, "--aggregate-interval", options.AggregateIntervalSeconds);
        MaintenanceArgument.AddFlag(args, "--progress-timestamp", options.ProgressTimestamp);
        MaintenanceArgument.AddValue(args, "--log-prefix", options.LogPrefix);

        if (options.RandomSeed is not null)
            MaintenanceArgument.AddValue(args, "--random-seed", FormatRandomSeed(options.RandomSeed));
        MaintenanceArgument.AddValue(args, "--show-script", options.ShowScript);
        MaintenanceArgument.AddFlag(args, "--failures-detailed", options.FailuresDetailed);
        MaintenanceArgument.AddValue(args, "--max-tries", options.MaxTries);
        MaintenanceArgument.AddFlag(args, "--verbose-errors", options.VerboseErrors);
        MaintenanceArgument.AddFlag(args, "--exit-on-abort", options.ExitOnAbort);
        MaintenanceArgument.AddFlag(args, "--debug", options.Debug);

        MaintenanceArgument.AddValue(args, "--host", options.Host);
        MaintenanceArgument.AddValue(args, "--port", options.Port);
        MaintenanceArgument.AddValue(args, "--username", options.Username);

        if (options.Database is not null)
            args.Add(options.Database);

        return args;
    }

    private static string FormatInitializationStep(PgBenchInitializationStep step) => step switch
    {
        PgBenchInitializationStep.Drop => "d",
        PgBenchInitializationStep.CreateTables => "t",
        PgBenchInitializationStep.GenerateClientSide => "g",
        PgBenchInitializationStep.GenerateServerSide => "G",
        PgBenchInitializationStep.Vacuum => "v",
        PgBenchInitializationStep.CreatePrimaryKeys => "p",
        PgBenchInitializationStep.CreateForeignKeys => "f",
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, null),
    };

    private static string FormatRandomSeed(PgBenchRandomSeed seed) => seed.Kind switch
    {
        PgBenchRandomSeedKind.Time => "time",
        PgBenchRandomSeedKind.StrongRandom => "rand",
        PgBenchRandomSeedKind.Numeric => seed.NumericValue.ToString(CultureInfo.InvariantCulture),
        _ => throw new ArgumentOutOfRangeException(nameof(seed), seed.Kind, null),
    };
}

internal static class PgBenchValidator
{
    internal static void Validate(PgBenchOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);

        if (options.InitializationSteps.Count > 0) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.InitSteps, version);
        if (options.RandomSeed is not null) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.RandomSeed, version);
        if (options.ShowScript is not null) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.ShowScript, version);
        if (options.Partitions.HasValue) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.Partitions, version);
        if (options.PartitionMethod.HasValue) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.PartitionMethod, version);
        if (options.FailuresDetailed) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.FailuresDetailed, version);
        if (options.MaxTries.HasValue) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.MaxTries, version);
        if (options.VerboseErrors) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.VerboseErrors, version);
        if (options.ExitOnAbort) MaintenanceAvailability.Ensure(PgBenchOptionAvailabilityCatalog.ExitOnAbort, version);

        ValidatePositive(options.Clients, "--client", version);
        ValidatePositive(options.Jobs, "--jobs", version);
        ValidatePositive(options.ProgressSeconds, "--progress", version);
        ValidatePositive(options.Scale, "--scale", version);
        ValidatePositive(options.DurationSeconds, "--time", version);
        ValidatePositive(options.TransactionsPerClient, "--transactions", version);
        ValidatePositive(options.Partitions, "--partitions", version);
        ValidatePositive(options.AggregateIntervalSeconds, "--aggregate-interval", version);

        if (options.FillFactor.HasValue && (options.FillFactor.Value < 10 || options.FillFactor.Value > 100))
            throw new PgInvalidOptionValueException(version, "--fillfactor", options.FillFactor.Value);
        if (options.Rate.HasValue && options.Rate.Value <= 0)
            throw new PgInvalidOptionValueException(version, "--rate", options.Rate.Value);
        if (options.LatencyLimitMilliseconds.HasValue && options.LatencyLimitMilliseconds.Value <= 0)
            throw new PgInvalidOptionValueException(version, "--latency-limit", options.LatencyLimitMilliseconds.Value);
        if (options.SamplingRate.HasValue && (options.SamplingRate.Value <= 0 || options.SamplingRate.Value > 1))
            throw new PgInvalidOptionValueException(version, "--sampling-rate", options.SamplingRate.Value);
        if (options.MaxTries.HasValue && options.MaxTries.Value < 0)
            throw new PgInvalidOptionValueException(version, "--max-tries", options.MaxTries.Value);

        if (options.DurationSeconds.HasValue && options.TransactionsPerClient.HasValue)
            throw new PgInvalidOptionCombinationException(version, "--time", "--transactions");
        if (options.AggregateIntervalSeconds.HasValue && !options.LogTransactions)
            throw new PgInvalidOptionCombinationException(version, "--aggregate-interval", "--log");
        if (options.MaxTries == 0 && !options.LatencyLimitMilliseconds.HasValue && !options.DurationSeconds.HasValue)
            throw new PgInvalidOptionCombinationException(version, "--max-tries=0", "--latency-limit/--time");
        if (options.SelectOnly && options.SkipSomeUpdates)
            throw new PgInvalidOptionCombinationException(version, "--select-only", "--skip-some-updates");

        foreach (PgBenchScript script in options.Scripts)
        {
            if (script is null || string.IsNullOrWhiteSpace(script.Value) || script.Weight <= 0)
                throw new PgInvalidOptionValueException(version, "--builtin/--file", script?.Value);
        }

        foreach (PgBenchVariableAssignment variable in options.Variables)
        {
            if (variable is null || string.IsNullOrWhiteSpace(variable.Name))
                throw new PgInvalidOptionValueException(version, "--define", variable?.Name);
        }

        if (options.ShowScript is not null && string.IsNullOrWhiteSpace(options.ShowScript))
            throw new PgInvalidOptionValueException(version, "--show-script", options.ShowScript);
    }

    private static void ValidatePositive(int? value, string option, PostgreSqlMajorVersion version)
    {
        if (value.HasValue && value.Value <= 0)
            throw new PgInvalidOptionValueException(version, option, value.Value);
    }
}
