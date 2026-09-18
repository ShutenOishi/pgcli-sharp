namespace PgCliSharp;

/// <summary>
/// <para>EN: Describes the PostgreSQL upstream lifecycle state for a major version at a point in time.</para>
/// <para>JA: 指定時点における PostgreSQL 上流プロジェクトのメジャーバージョンのライフサイクル状態を表します。</para>
/// </summary>
public enum PostgreSqlUpstreamLifecycleStatus
{
    /// <summary>
    /// <para>EN: The major version is still supported by the PostgreSQL upstream project.</para>
    /// <para>JA: PostgreSQL 上流プロジェクトによるサポート期間内です。</para>
    /// </summary>
    Supported,

    /// <summary>
    /// <para>EN: The major version has reached upstream end of life. This does not disable PgCliSharp compatibility.</para>
    /// <para>JA: 上流プロジェクトでは EOL です。PgCliSharp の互換性を無効にする意味ではありません。</para>
    /// </summary>
    EndOfLife,
}
