using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgChecksumsOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo SyncMethod = MaintenanceAvailability.Since("--sync-method", PostgreSqlMajorVersion.V17);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new MaintenanceOptionAvailabilityInfo[]
    {
        SyncMethod,
    };
}
