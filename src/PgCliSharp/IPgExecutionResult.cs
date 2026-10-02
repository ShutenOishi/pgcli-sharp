namespace PgCliSharp;

/// <summary><para>EN: Read-only process metadata shared by tool results; domain statuses remain tool-specific.</para><para>JA: 各ツールの結果で共有する読み取り専用プロセスメタデータです。意味を持つ状態はツール固有のまま保持します。</para></summary>
public interface IPgExecutionResult
{
    /// <summary><para>EN: Gets the tool process exit code.</para><para>JA: ツールプロセスの終了コードを取得します。</para></summary>
    int ExitCode { get; }
    /// <summary><para>EN: Gets process duration, excluding executable-version probing.</para><para>JA: 実行ファイルのバージョン確認を除くプロセス実行時間を取得します。</para></summary>
    TimeSpan Duration { get; }
    /// <summary><para>EN: Gets the actual parsed executable version.</para><para>JA: 実際の実行ファイルから解析したバージョンを取得します。</para></summary>
    Version ExecutableVersion { get; }
    /// <summary><para>EN: Gets the original numeric executable-version text.</para><para>JA: 実行ファイルの元の数値バージョン文字列を取得します。</para></summary>
    string RawExecutableVersion { get; }
}
