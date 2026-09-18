using System.Collections.ObjectModel;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.PgRestore;

internal sealed class PgRestoreOptionAvailabilityInfo
{
    internal PgRestoreOptionAvailabilityInfo(
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

internal static class PgRestoreOptionAvailabilityCatalog
{
    internal static readonly PgRestoreOptionAvailabilityInfo NoComments =
        Since("--no-comments", PostgreSqlMajorVersion.V11);

    internal static readonly PgRestoreOptionAvailabilityInfo RestrictKey =
        new PgRestoreOptionAvailabilityInfo(
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

    internal static readonly PgRestoreOptionAvailabilityInfo NoTableAccessMethod =
        Since("--no-table-access-method", PostgreSqlMajorVersion.V15);

    internal static readonly PgRestoreOptionAvailabilityInfo TransactionSize =
        Since("--transaction-size", PostgreSqlMajorVersion.V17);

    internal static readonly PgRestoreOptionAvailabilityInfo Filter =
        Since("--filter", PostgreSqlMajorVersion.V17);

    internal static readonly PgRestoreOptionAvailabilityInfo NoData =
        Since("--no-data", PostgreSqlMajorVersion.V18);

    internal static readonly PgRestoreOptionAvailabilityInfo NoPolicies =
        Since("--no-policies", PostgreSqlMajorVersion.V18);

    internal static readonly PgRestoreOptionAvailabilityInfo NoSchema =
        Since("--no-schema", PostgreSqlMajorVersion.V18);

    internal static readonly PgRestoreOptionAvailabilityInfo NoStatistics =
        Since("--no-statistics", PostgreSqlMajorVersion.V18);

    internal static readonly PgRestoreOptionAvailabilityInfo Statistics =
        Since("--statistics", PostgreSqlMajorVersion.V18);

    internal static readonly PgRestoreOptionAvailabilityInfo StatisticsOnly =
        Since("--statistics-only", PostgreSqlMajorVersion.V18);

    internal static readonly IReadOnlyList<PgRestoreOptionAvailabilityInfo> All =
        new ReadOnlyCollection<PgRestoreOptionAvailabilityInfo>(
            new[]
            {
                NoComments,
                RestrictKey,
                NoTableAccessMethod,
                TransactionSize,
                Filter,
                NoData,
                NoPolicies,
                NoSchema,
                NoStatistics,
                Statistics,
                StatisticsOnly,
            });

    private static PgRestoreOptionAvailabilityInfo Since(
        string optionName,
        PostgreSqlMajorVersion since)
    {
        return new PgRestoreOptionAvailabilityInfo(
            optionName,
            since,
            PostgreSqlMajorVersion.V18);
    }
}

internal static class PgRestoreOptionAvailability
{
    internal static void Ensure(
        PgRestoreOptionAvailabilityInfo availability,
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
