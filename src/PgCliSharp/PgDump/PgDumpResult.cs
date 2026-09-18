namespace PgCliSharp;

/// <summary>
/// <para>EN: Contains pg_dump execution metadata without buffering the dump payload.</para>
/// <para>JA: ダンプ本体をバッファリングせず、pg_dump の実行メタデータを保持します。</para>
/// </summary>
public sealed class PgDumpResult
{
    internal PgDumpResult(
        int exitCode,
        TimeSpan duration,
        Version executableVersion,
        string rawExecutableVersion,
        string standardError)
    {
        ExitCode = exitCode;
        Duration = duration;
        ExecutableVersion = executableVersion;
        RawExecutableVersion = rawExecutableVersion;
        StandardError = standardError;
    }

    /// <summary><para>EN: Gets the pg_dump process exit code.</para><para>JA: pg_dump プロセスの終了コードを取得します。</para></summary>
    public int ExitCode { get; }

    /// <summary><para>EN: Gets the pg_dump process duration, excluding the cached version-probe duration.</para><para>JA: キャッシュされたバージョン確認時間を除く、pg_dump プロセスの実行時間を取得します。</para></summary>
    public TimeSpan Duration { get; }

    /// <summary><para>EN: Gets the parsed numeric pg_dump executable version.</para><para>JA: 解析された pg_dump 実行ファイルの数値バージョンを取得します。</para></summary>
    public Version ExecutableVersion { get; }

    /// <summary><para>EN: Gets the numeric version text parsed from pg_dump --version.</para><para>JA: pg_dump --version から解析した数値バージョン文字列を取得します。</para></summary>
    public string RawExecutableVersion { get; }

    /// <summary>
    /// <para>EN: Gets the original PostgreSQL standard-error text without machine translation.</para>
    /// <para>JA: 機械翻訳していない PostgreSQL の元の標準エラーテキストを取得します。</para>
    /// </summary>
    public string StandardError { get; }
}
