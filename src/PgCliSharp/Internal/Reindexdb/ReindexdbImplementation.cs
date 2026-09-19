using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Reindexdb;

internal static class ReindexdbOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo Concurrently = MaintenanceAvailability.Since("--concurrently", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo Jobs = MaintenanceAvailability.Since("--jobs", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo Tablespace = MaintenanceAvailability.Since("--tablespace", PostgreSqlMajorVersion.V14);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new[] { Concurrently, Jobs, Tablespace };
}

internal static class ReindexdbArgumentBuilder
{
    internal static IReadOnlyList<string> Build(ReindexDbOptions options)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddFlag(args, "--quiet", options.Quiet);
        MaintenanceArgument.AddRepeatable(args, "--schema", options.Schemas);
        MaintenanceArgument.AddValue(args, "--dbname", options.DatabaseName);
        MaintenanceArgument.AddFlag(args, "--all", options.AllDatabases);
        MaintenanceArgument.AddFlag(args, "--system", options.SystemCatalogs);
        MaintenanceArgument.AddRepeatable(args, "--table", options.Tables);
        MaintenanceArgument.AddRepeatable(args, "--index", options.Indexes);
        MaintenanceArgument.AddValue(args, "--jobs", options.Jobs);
        MaintenanceArgument.AddFlag(args, "--verbose", options.Verbose);
        MaintenanceArgument.AddFlag(args, "--concurrently", options.Concurrently);
        MaintenanceArgument.AddValue(args, "--maintenance-db", options.MaintenanceDatabase);
        MaintenanceArgument.AddValue(args, "--tablespace", options.Tablespace);
        return args;
    }
}

internal static class ReindexdbValidator
{
    internal static void Validate(ReindexDbOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        MaintenanceArgument.ValidatePositive(options.Jobs, "--jobs", version);

        if (options.Concurrently) MaintenanceAvailability.Ensure(ReindexdbOptionAvailabilityCatalog.Concurrently, version);
        if (options.Jobs.HasValue) MaintenanceAvailability.Ensure(ReindexdbOptionAvailabilityCatalog.Jobs, version);
        if (options.Tablespace is not null) MaintenanceAvailability.Ensure(ReindexdbOptionAvailabilityCatalog.Tablespace, version);

        if (options.AllDatabases && options.DatabaseName is not null)
            throw new PgInvalidOptionCombinationException(version, "--all", "--dbname");
        if (options.SystemCatalogs && options.Jobs.GetValueOrDefault(1) > 1)
            throw new PgInvalidOptionCombinationException(version, "--system", "--jobs");

        MaintenanceArgument.ValidateNonEmpty(options.Schemas, "--schema", version);
        MaintenanceArgument.ValidateNonEmpty(options.Tables, "--table", version);
        MaintenanceArgument.ValidateNonEmpty(options.Indexes, "--index", version);
        if (options.Tablespace is not null && string.IsNullOrWhiteSpace(options.Tablespace))
            throw new PgInvalidOptionValueException(version, "--tablespace", options.Tablespace);
    }
}
