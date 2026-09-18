using System.Collections.ObjectModel;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.PgDump;

internal sealed class PgDumpOptionAvailabilityInfo
{
    internal PgDumpOptionAvailabilityInfo(
        string optionName,
        PostgreSqlMajorVersion since,
        PostgreSqlMajorVersion until,
        IReadOnlyDictionary<PostgreSqlMajorVersion, Version>? minimumVersions = null)
    {
        OptionName = optionName;
        Since = since;
        Until = until;
        MinimumVersions = minimumVersions ??
            new ReadOnlyDictionary<PostgreSqlMajorVersion, Version>(
                new Dictionary<PostgreSqlMajorVersion, Version>());
    }

    internal string OptionName { get; }

    internal PostgreSqlMajorVersion Since { get; }

    internal PostgreSqlMajorVersion Until { get; }

    internal IReadOnlyDictionary<PostgreSqlMajorVersion, Version> MinimumVersions { get; }
}

internal static class PgDumpOptionAvailabilityCatalog
{
    internal static readonly PgDumpOptionAvailabilityInfo Oids =
        Major("--oids", PostgreSqlMajorVersion.V10, PostgreSqlMajorVersion.V11);

    internal static readonly PgDumpOptionAvailabilityInfo NoSynchronizedSnapshots =
        Major("--no-synchronized-snapshots", PostgreSqlMajorVersion.V10, PostgreSqlMajorVersion.V14);

    internal static readonly PgDumpOptionAvailabilityInfo LoadViaPartitionRoot =
        Since("--load-via-partition-root", PostgreSqlMajorVersion.V11);

    internal static readonly PgDumpOptionAvailabilityInfo NoComments =
        Since("--no-comments", PostgreSqlMajorVersion.V11);

    internal static readonly PgDumpOptionAvailabilityInfo ExtraFloatDigits =
        Since("--extra-float-digits", PostgreSqlMajorVersion.V12);

    internal static readonly PgDumpOptionAvailabilityInfo OnConflictDoNothing =
        Since("--on-conflict-do-nothing", PostgreSqlMajorVersion.V12);

    internal static readonly PgDumpOptionAvailabilityInfo RowsPerInsert =
        Since("--rows-per-insert", PostgreSqlMajorVersion.V12);

    internal static readonly PgDumpOptionAvailabilityInfo IncludeForeignData =
        Since("--include-foreign-data", PostgreSqlMajorVersion.V13);

    internal static readonly PgDumpOptionAvailabilityInfo RestrictKey =
        new PgDumpOptionAvailabilityInfo(
            "--restrict-key",
            PostgreSqlMajorVersion.V13,
            PostgreSqlMajorVersion.V18,
            new ReadOnlyDictionary<PostgreSqlMajorVersion, Version>(
                new Dictionary<PostgreSqlMajorVersion, Version>
                {
                    [PostgreSqlMajorVersion.V13] = new Version(13, 22),
                    [PostgreSqlMajorVersion.V14] = new Version(14, 19),
                    [PostgreSqlMajorVersion.V15] = new Version(15, 14),
                    [PostgreSqlMajorVersion.V16] = new Version(16, 10),
                    [PostgreSqlMajorVersion.V17] = new Version(17, 6),
                    [PostgreSqlMajorVersion.V18] = new Version(18, 0),
                }));

    internal static readonly PgDumpOptionAvailabilityInfo Extension =
        Since("--extension", PostgreSqlMajorVersion.V14);

    internal static readonly PgDumpOptionAvailabilityInfo NoToastCompression =
        Since("--no-toast-compression", PostgreSqlMajorVersion.V14);

    internal static readonly PgDumpOptionAvailabilityInfo NoTableAccessMethod =
        Since("--no-table-access-method", PostgreSqlMajorVersion.V15);

    internal static readonly PgDumpOptionAvailabilityInfo MethodCompression =
        Since("--compress=method[:detail]", PostgreSqlMajorVersion.V16);

    internal static readonly PgDumpOptionAvailabilityInfo TableAndChildren =
        Since("--table-and-children", PostgreSqlMajorVersion.V16);

    internal static readonly PgDumpOptionAvailabilityInfo ExcludeTableAndChildren =
        Since("--exclude-table-and-children", PostgreSqlMajorVersion.V16);

    internal static readonly PgDumpOptionAvailabilityInfo ExcludeTableDataAndChildren =
        Since("--exclude-table-data-and-children", PostgreSqlMajorVersion.V16);

    internal static readonly PgDumpOptionAvailabilityInfo ExcludeExtension =
        Since("--exclude-extension", PostgreSqlMajorVersion.V17);

    internal static readonly PgDumpOptionAvailabilityInfo Filter =
        Since("--filter", PostgreSqlMajorVersion.V17);

    internal static readonly PgDumpOptionAvailabilityInfo SyncMethod =
        Since("--sync-method", PostgreSqlMajorVersion.V17);

    internal static readonly PgDumpOptionAvailabilityInfo NoData =
        Since("--no-data", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpOptionAvailabilityInfo NoPolicies =
        Since("--no-policies", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpOptionAvailabilityInfo NoSchema =
        Since("--no-schema", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpOptionAvailabilityInfo NoStatistics =
        Since("--no-statistics", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpOptionAvailabilityInfo SequenceData =
        Since("--sequence-data", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpOptionAvailabilityInfo Statistics =
        Since("--statistics", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpOptionAvailabilityInfo StatisticsOnly =
        Since("--statistics-only", PostgreSqlMajorVersion.V18);

    internal static readonly IReadOnlyList<PgDumpOptionAvailabilityInfo> All =
        new ReadOnlyCollection<PgDumpOptionAvailabilityInfo>(
            new[]
            {
                Oids,
                NoSynchronizedSnapshots,
                LoadViaPartitionRoot,
                NoComments,
                ExtraFloatDigits,
                OnConflictDoNothing,
                RowsPerInsert,
                IncludeForeignData,
                RestrictKey,
                Extension,
                NoToastCompression,
                NoTableAccessMethod,
                MethodCompression,
                TableAndChildren,
                ExcludeTableAndChildren,
                ExcludeTableDataAndChildren,
                ExcludeExtension,
                Filter,
                SyncMethod,
                NoData,
                NoPolicies,
                NoSchema,
                NoStatistics,
                SequenceData,
                Statistics,
                StatisticsOnly,
            });

    private static PgDumpOptionAvailabilityInfo Since(
        string optionName,
        PostgreSqlMajorVersion since)
    {
        return Major(optionName, since, PostgreSqlMajorVersion.V18);
    }

    private static PgDumpOptionAvailabilityInfo Major(
        string optionName,
        PostgreSqlMajorVersion since,
        PostgreSqlMajorVersion until)
    {
        return new PgDumpOptionAvailabilityInfo(optionName, since, until);
    }
}

internal static class PgDumpOptionAvailability
{
    internal static void Ensure(
        PgDumpOptionAvailabilityInfo availability,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        int major = (int)selectedVersion;
        if (major < (int)availability.Since ||
            major > (int)availability.Until)
        {
            throw new PgUnsupportedOptionException(
                selectedVersion,
                availability.OptionName,
                availability.Since,
                availability.Until);
        }

        if (availability.MinimumVersions.TryGetValue(
                selectedVersion,
                out Version? minimumVersion) &&
            executableVersion.NumericVersion < minimumVersion)
        {
            throw new PgUnsupportedOptionException(
                selectedVersion,
                availability.OptionName,
                availability.Since,
                availability.Until,
                minimumVersion,
                executableVersion.NumericVersion);
        }
    }
}
