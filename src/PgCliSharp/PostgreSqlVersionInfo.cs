namespace PgCliSharp;

/// <summary>
/// <para>EN: Provides release and upstream lifecycle metadata for a PostgreSQL major version.</para>
/// <para>JA: PostgreSQL メジャーバージョンのリリース日と上流ライフサイクルのメタデータを提供します。</para>
/// </summary>
public sealed class PostgreSqlVersionInfo
{
    internal PostgreSqlVersionInfo(
        PostgreSqlMajorVersion version,
        DateTimeOffset firstReleaseDate,
        DateTimeOffset finalReleaseDate)
    {
        Version = version;
        FirstReleaseDate = firstReleaseDate;
        FinalReleaseDate = finalReleaseDate;
    }

    /// <summary>
    /// <para>EN: Gets the PostgreSQL major version.</para>
    /// <para>JA: PostgreSQL のメジャーバージョンを取得します。</para>
    /// </summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary>
    /// <para>EN: Gets the upstream first-release calendar date in UTC.</para>
    /// <para>JA: 上流プロジェクトでの初回リリース日を UTC の日付として取得します。</para>
    /// </summary>
    public DateTimeOffset FirstReleaseDate { get; }

    /// <summary>
    /// <para>EN: Gets the upstream final-release/end-of-life calendar date in UTC.</para>
    /// <para>JA: 上流プロジェクトでの最終リリース/EOL 日を UTC の日付として取得します。</para>
    /// </summary>
    public DateTimeOffset FinalReleaseDate { get; }

    /// <summary>
    /// <para>EN: Gets the upstream lifecycle state on the specified date. Upstream EOL does not block PgCliSharp execution.</para>
    /// <para>JA: 指定日における上流ライフサイクル状態を取得します。上流 EOL であっても PgCliSharp の実行はブロックされません。</para>
    /// </summary>
    /// <param name="asOf">
    /// <para>EN: The point in time used to evaluate lifecycle state.</para>
    /// <para>JA: ライフサイクル状態を判定する基準時点です。</para>
    /// </param>
    /// <returns>
    /// <para>EN: The upstream lifecycle state.</para>
    /// <para>JA: 上流ライフサイクル状態です。</para>
    /// </returns>
    public PostgreSqlUpstreamLifecycleStatus GetLifecycleStatus(DateTimeOffset asOf)
    {
        DateTime asOfDate = asOf.UtcDateTime.Date;
        DateTime finalDate = FinalReleaseDate.UtcDateTime.Date;
        return asOfDate <= finalDate
            ? PostgreSqlUpstreamLifecycleStatus.Supported
            : PostgreSqlUpstreamLifecycleStatus.EndOfLife;
    }
}
