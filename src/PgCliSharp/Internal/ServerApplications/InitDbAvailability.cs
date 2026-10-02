using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.ServerApplications;

internal static class InitDbOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo WalSegsize = MaintenanceAvailability.Since("--wal-segsize", PostgreSqlMajorVersion.V11);
    internal static readonly MaintenanceOptionAvailabilityInfo AllowGroupAccess = MaintenanceAvailability.Since("--allow-group-access", PostgreSqlMajorVersion.V11);
    internal static readonly MaintenanceOptionAvailabilityInfo NoInstructions = MaintenanceAvailability.Since("--no-instructions", PostgreSqlMajorVersion.V14);
    internal static readonly MaintenanceOptionAvailabilityInfo DiscardCaches = MaintenanceAvailability.Since("--discard-caches", PostgreSqlMajorVersion.V14);
    internal static readonly MaintenanceOptionAvailabilityInfo LocaleProvider = MaintenanceAvailability.Since("--locale-provider", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo IcuLocale = MaintenanceAvailability.Since("--icu-locale", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo Set = MaintenanceAvailability.Since("--set", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo IcuRules = MaintenanceAvailability.Since("--icu-rules", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo BuiltinLocale = MaintenanceAvailability.Since("--builtin-locale", PostgreSqlMajorVersion.V17);
    internal static readonly MaintenanceOptionAvailabilityInfo SyncMethod = MaintenanceAvailability.Since("--sync-method", PostgreSqlMajorVersion.V17);
    internal static readonly MaintenanceOptionAvailabilityInfo NoDataChecksums = MaintenanceAvailability.Since("--no-data-checksums", PostgreSqlMajorVersion.V18);
    internal static readonly MaintenanceOptionAvailabilityInfo NoSyncDataFiles = MaintenanceAvailability.Since("--no-sync-data-files", PostgreSqlMajorVersion.V18);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new MaintenanceOptionAvailabilityInfo[]
    {
        WalSegsize,
        AllowGroupAccess,
        NoInstructions,
        DiscardCaches,
        LocaleProvider,
        IcuLocale,
        Set,
        IcuRules,
        BuiltinLocale,
        SyncMethod,
        NoDataChecksums,
        NoSyncDataFiles,
    };
}
