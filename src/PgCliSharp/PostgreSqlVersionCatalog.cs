using System.Collections.ObjectModel;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Provides metadata for the PostgreSQL major versions supported by PgCliSharp.</para>
/// <para>JA: PgCliSharp が対応する PostgreSQL メジャーバージョンのメタデータを提供します。</para>
/// </summary>
public static class PostgreSqlVersionCatalog
{
    private static readonly ReadOnlyCollection<PostgreSqlVersionInfo> VersionEntries =
        Array.AsReadOnly(
            new[]
            {
                Create(PostgreSqlMajorVersion.V10, 2017, 10, 5, 2022, 11, 10),
                Create(PostgreSqlMajorVersion.V11, 2018, 10, 18, 2023, 11, 9),
                Create(PostgreSqlMajorVersion.V12, 2019, 10, 3, 2024, 11, 21),
                Create(PostgreSqlMajorVersion.V13, 2020, 9, 24, 2025, 11, 13),
                Create(PostgreSqlMajorVersion.V14, 2021, 9, 30, 2026, 11, 12),
                Create(PostgreSqlMajorVersion.V15, 2022, 10, 13, 2027, 11, 11),
                Create(PostgreSqlMajorVersion.V16, 2023, 9, 14, 2028, 11, 9),
                Create(PostgreSqlMajorVersion.V17, 2024, 9, 26, 2029, 11, 8),
                Create(PostgreSqlMajorVersion.V18, 2025, 9, 25, 2030, 11, 14),
            });

    /// <summary>
    /// <para>EN: Gets metadata for PostgreSQL 10 through 18 in ascending major-version order.</para>
    /// <para>JA: PostgreSQL 10 から 18 までのメタデータをメジャーバージョン昇順で取得します。</para>
    /// </summary>
    public static IReadOnlyList<PostgreSqlVersionInfo> All => VersionEntries;

    /// <summary>
    /// <para>EN: Gets metadata for the specified supported PostgreSQL major version.</para>
    /// <para>JA: 指定した対応 PostgreSQL メジャーバージョンのメタデータを取得します。</para>
    /// </summary>
    /// <param name="version">
    /// <para>EN: The PostgreSQL major version.</para>
    /// <para>JA: PostgreSQL のメジャーバージョンです。</para>
    /// </param>
    /// <returns>
    /// <para>EN: Metadata for the requested version.</para>
    /// <para>JA: 指定バージョンのメタデータです。</para>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para>EN: Thrown when the enum value is outside PostgreSQL 10 through 18.</para>
    /// <para>JA: 列挙値が PostgreSQL 10 から 18 の範囲外の場合にスローされます。</para>
    /// </exception>
    public static PostgreSqlVersionInfo Get(PostgreSqlMajorVersion version)
    {
        int index = (int)version - (int)PostgreSqlMajorVersion.V10;
        if (index < 0 || index >= VersionEntries.Count || VersionEntries[index].Version != version)
        {
            throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported PostgreSQL major-version enum value.");
        }

        return VersionEntries[index];
    }

    private static PostgreSqlVersionInfo Create(
        PostgreSqlMajorVersion version,
        int firstYear,
        int firstMonth,
        int firstDay,
        int finalYear,
        int finalMonth,
        int finalDay)
    {
        return new PostgreSqlVersionInfo(
            version,
            new DateTimeOffset(firstYear, firstMonth, firstDay, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(finalYear, finalMonth, finalDay, 0, 0, 0, TimeSpan.Zero));
    }
}
