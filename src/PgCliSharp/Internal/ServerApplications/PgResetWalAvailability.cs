using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgResetWalOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo WalSegsize = MaintenanceAvailability.Since("--wal-segsize", PostgreSqlMajorVersion.V11);
    internal static readonly MaintenanceOptionAvailabilityInfo CharSignedness = MaintenanceAvailability.Since("--char-signedness", PostgreSqlMajorVersion.V18);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new MaintenanceOptionAvailabilityInfo[]
    {
        WalSegsize,
        CharSignedness,
    };
}
