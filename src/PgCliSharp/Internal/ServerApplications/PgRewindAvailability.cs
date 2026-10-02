using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgRewindOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo NoSync = MaintenanceAvailability.Since("--no-sync", PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo WriteRecoveryConf = MaintenanceAvailability.Since("--write-recovery-conf", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo NoEnsureShutdown = MaintenanceAvailability.Since("--no-ensure-shutdown", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo RestoreTargetWal = MaintenanceAvailability.Since("--restore-target-wal", PostgreSqlMajorVersion.V13);
    internal static readonly MaintenanceOptionAvailabilityInfo ConfigFile = MaintenanceAvailability.Since("--config-file", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo SyncMethod = MaintenanceAvailability.Since("--sync-method", PostgreSqlMajorVersion.V17);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new MaintenanceOptionAvailabilityInfo[]
    {
        NoSync,
        WriteRecoveryConf,
        NoEnsureShutdown,
        RestoreTargetWal,
        ConfigFile,
        SyncMethod,
    };
}
