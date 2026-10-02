using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgUpgradeOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo Socketdir = MaintenanceAvailability.Since("--socketdir", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo Clone = MaintenanceAvailability.Since("--clone", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo NoSync = MaintenanceAvailability.Since("--no-sync", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo Copy = MaintenanceAvailability.Since("--copy", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo CopyFileRange = MaintenanceAvailability.Since("--copy-file-range", PostgreSqlMajorVersion.V17);
    internal static readonly MaintenanceOptionAvailabilityInfo SyncMethod = MaintenanceAvailability.Since("--sync-method", PostgreSqlMajorVersion.V17);
    internal static readonly MaintenanceOptionAvailabilityInfo NoStatistics = MaintenanceAvailability.Since("--no-statistics", PostgreSqlMajorVersion.V18);
    internal static readonly MaintenanceOptionAvailabilityInfo SetCharSignedness = MaintenanceAvailability.Since("--set-char-signedness", PostgreSqlMajorVersion.V18);
    internal static readonly MaintenanceOptionAvailabilityInfo Swap = MaintenanceAvailability.Since("--swap", PostgreSqlMajorVersion.V18);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new MaintenanceOptionAvailabilityInfo[]
    {
        Socketdir,
        Clone,
        NoSync,
        Copy,
        CopyFileRange,
        SyncMethod,
        NoStatistics,
        SetCharSignedness,
        Swap,
    };
}
