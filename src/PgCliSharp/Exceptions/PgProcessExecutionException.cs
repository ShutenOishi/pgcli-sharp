using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a PostgreSQL command-line process that completed with a non-zero exit code.</para>
/// <para>JA: PostgreSQL コマンドラインプロセスが 0 以外の終了コードで完了したことを表します。</para>
/// </summary>
public sealed class PgProcessExecutionException : PgCliSharpException
{
    internal PgProcessExecutionException(string executablePath, int exitCode, string standardError)
        : base(MessageProvider.Format(MessageKeys.ProcessExitedWithError, executablePath, exitCode))
    {
        ExecutablePath = executablePath;
        ExitCode = exitCode;
        StandardError = standardError;
    }

    /// <summary><para>EN: Gets the executable path.</para><para>JA: 実行ファイルのパスを取得します。</para></summary>
    public string ExecutablePath { get; }

    /// <summary><para>EN: Gets the process exit code.</para><para>JA: プロセスの終了コードを取得します。</para></summary>
    public int ExitCode { get; }

    /// <summary>
    /// <para>EN: Gets the original PostgreSQL standard-error text without machine translation.</para>
    /// <para>JA: 機械翻訳していない PostgreSQL の元の標準エラーテキストを取得します。</para>
    /// </summary>
    public string StandardError { get; }
}
