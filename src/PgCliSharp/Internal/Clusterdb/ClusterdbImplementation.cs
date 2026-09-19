using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Clusterdb;

internal static class ClusterdbOptionAvailabilityCatalog
{
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = Array.Empty<MaintenanceOptionAvailabilityInfo>();
}

internal static class ClusterdbArgumentBuilder
{
    internal static IReadOnlyList<string> Build(ClusterDbOptions options)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddFlag(args, "--quiet", options.Quiet);
        MaintenanceArgument.AddValue(args, "--dbname", options.DatabaseName);
        MaintenanceArgument.AddFlag(args, "--all", options.AllDatabases);
        MaintenanceArgument.AddRepeatable(args, "--table", options.Tables);
        MaintenanceArgument.AddFlag(args, "--verbose", options.Verbose);
        MaintenanceArgument.AddValue(args, "--maintenance-db", options.MaintenanceDatabase);
        return args;
    }
}

internal static class ClusterdbValidator
{
    internal static void Validate(ClusterDbOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        if (options.AllDatabases && options.DatabaseName is not null)
            throw new PgInvalidOptionCombinationException(version, "--all", "--dbname");
        MaintenanceArgument.ValidateNonEmpty(options.Tables, "--table", version);
    }
}
