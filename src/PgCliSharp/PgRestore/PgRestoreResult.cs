namespace PgCliSharp;

/// <summary>
/// <para>EN: Contains pg_restore execution metadata without buffering generated SQL/list output.</para>
/// <para>JA: 生成 SQL・一覧出力を全量バッファリングせず、pg_restore の実行メタデータを保持します。</para>
/// </summary>
public sealed class PgRestoreResult
{
    internal PgRestoreResult(
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

    /// <summary><para>EN: Gets the process exit code.</para><para>JA: プロセス終了コードを取得します。</para></summary>
    public int ExitCode { get; }

    /// <summary><para>EN: Gets execution duration excluding a cached version probe.</para><para>JA: キャッシュされたバージョン確認を除く実行時間を取得します。</para></summary>
    public TimeSpan Duration { get; }

    /// <summary><para>EN: Gets the parsed numeric executable version.</para><para>JA: 解析された実行ファイルの数値バージョンを取得します。</para></summary>
    public Version ExecutableVersion { get; }

    /// <summary><para>EN: Gets the numeric version text parsed from --version.</para><para>JA: --version から解析した数値バージョン文字列を取得します。</para></summary>
    public string RawExecutableVersion { get; }

    /// <summary><para>EN: Gets original PostgreSQL standard-error text without machine translation.</para><para>JA: 機械翻訳していない PostgreSQL の元の標準エラーテキストを取得します。</para></summary>
    public string StandardError { get; }
}
