using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.PgIsReady;

internal static class PgIsReadyOptionAvailabilityCatalog
{
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = Array.Empty<MaintenanceOptionAvailabilityInfo>();
}

internal static class PgIsReadyArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgIsReadyOptions options)
    {
        var args = new List<string>();
        MaintenanceArgument.AddValue(args, "--dbname", options.Database);
        MaintenanceArgument.AddValue(args, "--host", options.Host);
        MaintenanceArgument.AddValue(args, "--port", options.Port);
        MaintenanceArgument.AddFlag(args, "--quiet", options.Quiet);
        MaintenanceArgument.AddValue(args, "--timeout", options.ConnectTimeoutSeconds);
        MaintenanceArgument.AddValue(args, "--username", options.Username);
        return args;
    }
}

internal static class PgIsReadyValidator
{
    internal static void Validate(PgIsReadyOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        MaintenanceArgument.ValidatePositive(options.ConnectTimeoutSeconds, "--timeout", version, allowZero: true);
    }
}
