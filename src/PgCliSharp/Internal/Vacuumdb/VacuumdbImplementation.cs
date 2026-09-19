using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Vacuumdb;

internal static class VacuumdbOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo DisablePageSkipping = MaintenanceAvailability.Since("--disable-page-skipping", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo SkipLocked = MaintenanceAvailability.Since("--skip-locked", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo MinXidAge = MaintenanceAvailability.Since("--min-xid-age", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo MinMxidAge = MaintenanceAvailability.Since("--min-mxid-age", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo Parallel = MaintenanceAvailability.Since("--parallel", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo NoIndexCleanup = MaintenanceAvailability.Since("--no-index-cleanup", PostgreSqlMajorVersion.V14);
    internal static readonly MaintenanceOptionAvailabilityInfo ForceIndexCleanup = MaintenanceAvailability.Since("--force-index-cleanup", PostgreSqlMajorVersion.V14);
    internal static readonly MaintenanceOptionAvailabilityInfo NoTruncate = MaintenanceAvailability.Since("--no-truncate", PostgreSqlMajorVersion.V14);
    internal static readonly MaintenanceOptionAvailabilityInfo NoProcessToast = MaintenanceAvailability.Since("--no-process-toast", PostgreSqlMajorVersion.V14);
    internal static readonly MaintenanceOptionAvailabilityInfo Schema = MaintenanceAvailability.Since("--schema", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo ExcludeSchema = MaintenanceAvailability.Since("--exclude-schema", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo NoProcessMain = MaintenanceAvailability.Since("--no-process-main", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo BufferUsageLimit = MaintenanceAvailability.Since("--buffer-usage-limit", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo MissingStatsOnly = MaintenanceAvailability.Since("--missing-stats-only", PostgreSqlMajorVersion.V18);

    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new[]
    {
        DisablePageSkipping, SkipLocked, MinXidAge, MinMxidAge, Parallel,
        NoIndexCleanup, ForceIndexCleanup, NoTruncate, NoProcessToast,
        Schema, ExcludeSchema, NoProcessMain, BufferUsageLimit, MissingStatsOnly,
    };
}

internal static class VacuumdbArgumentBuilder
{
    internal static IReadOnlyList<string> Build(VacuumDbOptions options)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddFlag(args, "--quiet", options.Quiet);
        MaintenanceArgument.AddValue(args, "--dbname", options.DatabaseName);
        MaintenanceArgument.AddFlag(args, "--analyze", options.Analyze);
        MaintenanceArgument.AddFlag(args, "--analyze-only", options.AnalyzeOnly);
        MaintenanceArgument.AddFlag(args, "--freeze", options.Freeze);
        MaintenanceArgument.AddFlag(args, "--all", options.AllDatabases);
        MaintenanceArgument.AddRepeatable(args, "--table", options.Tables);
        MaintenanceArgument.AddFlag(args, "--full", options.Full);
        MaintenanceArgument.AddFlag(args, "--verbose", options.Verbose);
        MaintenanceArgument.AddValue(args, "--jobs", options.Jobs);
        MaintenanceArgument.AddValue(args, "--parallel", options.ParallelWorkers);
        MaintenanceArgument.AddRepeatable(args, "--schema", options.Schemas);
        MaintenanceArgument.AddRepeatable(args, "--exclude-schema", options.ExcludedSchemas);
        MaintenanceArgument.AddValue(args, "--maintenance-db", options.MaintenanceDatabase);
        MaintenanceArgument.AddFlag(args, "--analyze-in-stages", options.AnalyzeInStages);
        MaintenanceArgument.AddFlag(args, "--disable-page-skipping", options.DisablePageSkipping);
        MaintenanceArgument.AddFlag(args, "--skip-locked", options.SkipLocked);
        MaintenanceArgument.AddValue(args, "--min-xid-age", options.MinXidAge);
        MaintenanceArgument.AddValue(args, "--min-mxid-age", options.MinMultiXactIdAge);

        if (options.IndexCleanup.HasValue)
            args.Add(options.IndexCleanup.Value == PgVacuumIndexCleanup.Disabled ? "--no-index-cleanup" : "--force-index-cleanup");

        MaintenanceArgument.AddFlag(args, "--no-truncate", options.NoTruncate);
        MaintenanceArgument.AddFlag(args, "--no-process-toast", options.NoProcessToast);
        MaintenanceArgument.AddFlag(args, "--no-process-main", options.NoProcessMain);
        MaintenanceArgument.AddValue(args, "--buffer-usage-limit", options.BufferUsageLimit);
        MaintenanceArgument.AddFlag(args, "--missing-stats-only", options.MissingStatsOnly);
        return args;
    }
}

internal static class VacuumdbValidator
{
    internal static void Validate(VacuumDbOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        MaintenanceArgument.ValidatePositive(options.Jobs, "--jobs", version);
        MaintenanceArgument.ValidatePositive(options.ParallelWorkers, "--parallel", version, allowZero: true);

        Ensure(options.DisablePageSkipping, VacuumdbOptionAvailabilityCatalog.DisablePageSkipping, version);
        Ensure(options.SkipLocked, VacuumdbOptionAvailabilityCatalog.SkipLocked, version);
        Ensure(options.MinXidAge.HasValue, VacuumdbOptionAvailabilityCatalog.MinXidAge, version);
        Ensure(options.MinMultiXactIdAge.HasValue, VacuumdbOptionAvailabilityCatalog.MinMxidAge, version);
        Ensure(options.ParallelWorkers.HasValue, VacuumdbOptionAvailabilityCatalog.Parallel, version);
        Ensure(options.Schemas.Count > 0, VacuumdbOptionAvailabilityCatalog.Schema, version);
        Ensure(options.ExcludedSchemas.Count > 0, VacuumdbOptionAvailabilityCatalog.ExcludeSchema, version);
        Ensure(options.IndexCleanup == PgVacuumIndexCleanup.Disabled, VacuumdbOptionAvailabilityCatalog.NoIndexCleanup, version);
        Ensure(options.IndexCleanup == PgVacuumIndexCleanup.Forced, VacuumdbOptionAvailabilityCatalog.ForceIndexCleanup, version);
        Ensure(options.NoTruncate, VacuumdbOptionAvailabilityCatalog.NoTruncate, version);
        Ensure(options.NoProcessToast, VacuumdbOptionAvailabilityCatalog.NoProcessToast, version);
        Ensure(options.NoProcessMain, VacuumdbOptionAvailabilityCatalog.NoProcessMain, version);
        Ensure(options.BufferUsageLimit is not null, VacuumdbOptionAvailabilityCatalog.BufferUsageLimit, version);
        Ensure(options.MissingStatsOnly, VacuumdbOptionAvailabilityCatalog.MissingStatsOnly, version);

        if (options.MinXidAge == 0) throw new PgInvalidOptionValueException(version, "--min-xid-age", 0);
        if (options.MinMultiXactIdAge == 0) throw new PgInvalidOptionValueException(version, "--min-mxid-age", 0);

        bool analyzeOnly = options.AnalyzeOnly || options.AnalyzeInStages;
        if (analyzeOnly)
        {
            if (options.Full) Reject(version, "--analyze-only", "--full");
            if (options.Freeze) Reject(version, "--analyze-only", "--freeze");
            if (options.DisablePageSkipping) Reject(version, "--analyze-only", "--disable-page-skipping");
            if (options.IndexCleanup.HasValue) Reject(version, "--analyze-only", "index-cleanup");
            if (options.NoTruncate) Reject(version, "--analyze-only", "--no-truncate");
            if (options.NoProcessMain) Reject(version, "--analyze-only", "--no-process-main");
            if (options.NoProcessToast) Reject(version, "--analyze-only", "--no-process-toast");
            if (options.ParallelWorkers.HasValue) Reject(version, "--analyze-only", "--parallel");
        }

        if (options.Full && options.ParallelWorkers.HasValue) Reject(version, "--full", "--parallel");
        if (options.BufferUsageLimit is not null && options.Full && !options.Analyze) Reject(version, "--buffer-usage-limit", "--full");
        if (options.MissingStatsOnly && !analyzeOnly) Reject(version, "--missing-stats-only", "--analyze-only/--analyze-in-stages");

        if (options.AllDatabases && options.DatabaseName is not null) Reject(version, "--all", "--dbname");
        if (options.Tables.Count > 0 && options.Schemas.Count > 0) Reject(version, "--table", "--schema");
        if (options.Tables.Count > 0 && options.ExcludedSchemas.Count > 0) Reject(version, "--table", "--exclude-schema");
        if (options.Schemas.Count > 0 && options.ExcludedSchemas.Count > 0) Reject(version, "--schema", "--exclude-schema");

        MaintenanceArgument.ValidateNonEmpty(options.Tables, "--table", version);
        MaintenanceArgument.ValidateNonEmpty(options.Schemas, "--schema", version);
        MaintenanceArgument.ValidateNonEmpty(options.ExcludedSchemas, "--exclude-schema", version);
        if (options.BufferUsageLimit is not null && string.IsNullOrWhiteSpace(options.BufferUsageLimit))
            throw new PgInvalidOptionValueException(version, "--buffer-usage-limit", options.BufferUsageLimit);
    }

    private static void Ensure(bool requested, MaintenanceOptionAvailabilityInfo info, PostgreSqlMajorVersion version)
    {
        if (requested) MaintenanceAvailability.Ensure(info, version);
    }

    private static void Reject(PostgreSqlMajorVersion version, params string[] options) =>
        throw new PgInvalidOptionCombinationException(version, options);
}
