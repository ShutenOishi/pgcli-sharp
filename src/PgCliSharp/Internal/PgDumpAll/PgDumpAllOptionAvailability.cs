using System.Collections.ObjectModel;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.PgDumpAll;

internal sealed class PgDumpAllOptionAvailabilityInfo
{
    internal PgDumpAllOptionAvailabilityInfo(
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

internal static class PgDumpAllOptionAvailabilityCatalog
{
    internal static readonly PgDumpAllOptionAvailabilityInfo Oids =
        new PgDumpAllOptionAvailabilityInfo(
            "--oids",
            PostgreSqlMajorVersion.V10,
            PostgreSqlMajorVersion.V11);

    internal static readonly PgDumpAllOptionAvailabilityInfo Encoding =
        Since("--encoding", PostgreSqlMajorVersion.V11);

    internal static readonly PgDumpAllOptionAvailabilityInfo LoadViaPartitionRoot =
        Since("--load-via-partition-root", PostgreSqlMajorVersion.V11);

    internal static readonly PgDumpAllOptionAvailabilityInfo NoComments =
        Since("--no-comments", PostgreSqlMajorVersion.V11);

    internal static readonly PgDumpAllOptionAvailabilityInfo ExcludeDatabase =
        Since("--exclude-database", PostgreSqlMajorVersion.V12);

    internal static readonly PgDumpAllOptionAvailabilityInfo ExtraFloatDigits =
        Since("--extra-float-digits", PostgreSqlMajorVersion.V12);

    internal static readonly PgDumpAllOptionAvailabilityInfo OnConflictDoNothing =
        Since("--on-conflict-do-nothing", PostgreSqlMajorVersion.V12);

    internal static readonly PgDumpAllOptionAvailabilityInfo RowsPerInsert =
        Since("--rows-per-insert", PostgreSqlMajorVersion.V12);

    internal static readonly PgDumpAllOptionAvailabilityInfo RestrictKey =
        new PgDumpAllOptionAvailabilityInfo(
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

    internal static readonly PgDumpAllOptionAvailabilityInfo NoToastCompression =
        Since("--no-toast-compression", PostgreSqlMajorVersion.V14);

    internal static readonly PgDumpAllOptionAvailabilityInfo NoTableAccessMethod =
        Since("--no-table-access-method", PostgreSqlMajorVersion.V15);

    internal static readonly PgDumpAllOptionAvailabilityInfo Filter =
        Since("--filter", PostgreSqlMajorVersion.V17);

    internal static readonly PgDumpAllOptionAvailabilityInfo NoData =
        Since("--no-data", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpAllOptionAvailabilityInfo NoPolicies =
        Since("--no-policies", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpAllOptionAvailabilityInfo NoSchema =
        Since("--no-schema", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpAllOptionAvailabilityInfo NoStatistics =
        Since("--no-statistics", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpAllOptionAvailabilityInfo Statistics =
        Since("--statistics", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpAllOptionAvailabilityInfo StatisticsOnly =
        Since("--statistics-only", PostgreSqlMajorVersion.V18);

    internal static readonly PgDumpAllOptionAvailabilityInfo SequenceData =
        Since("--sequence-data", PostgreSqlMajorVersion.V18);

    internal static readonly IReadOnlyList<PgDumpAllOptionAvailabilityInfo> All =
        new ReadOnlyCollection<PgDumpAllOptionAvailabilityInfo>(
            new[]
            {
                Oids,
                Encoding,
                LoadViaPartitionRoot,
                NoComments,
                ExcludeDatabase,
                ExtraFloatDigits,
                OnConflictDoNothing,
                RowsPerInsert,
                RestrictKey,
                NoToastCompression,
                NoTableAccessMethod,
                Filter,
                NoData,
                NoPolicies,
                NoSchema,
                NoStatistics,
                Statistics,
                StatisticsOnly,
                SequenceData,
            });

    private static PgDumpAllOptionAvailabilityInfo Since(
        string optionName,
        PostgreSqlMajorVersion since)
    {
        return new PgDumpAllOptionAvailabilityInfo(
            optionName,
            since,
            PostgreSqlMajorVersion.V18);
    }
}

internal static class PgDumpAllOptionAvailability
{
    internal static void Ensure(
        PgDumpAllOptionAvailabilityInfo availability,
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        if ((int)selectedVersion < (int)availability.Since ||
            (int)selectedVersion > (int)availability.Until)
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
