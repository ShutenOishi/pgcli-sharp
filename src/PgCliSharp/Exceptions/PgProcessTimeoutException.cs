using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a PostgreSQL command-line process that exceeded the configured timeout.</para>
/// <para>JA: PostgreSQL コマンドラインプロセスが設定されたタイムアウトを超過したことを表します。</para>
/// </summary>
public sealed class PgProcessTimeoutException : PgCliSharpException
{
    internal PgProcessTimeoutException(string executablePath, TimeSpan timeout)
        : base(MessageProvider.Format(MessageKeys.ProcessTimedOut, executablePath, timeout))
    {
        ExecutablePath = executablePath;
        Timeout = timeout;
    }

    /// <summary><para>EN: Gets the executable path.</para><para>JA: 実行ファイルのパスを取得します。</para></summary>
    public string ExecutablePath { get; }

    /// <summary><para>EN: Gets the configured timeout.</para><para>JA: 設定されたタイムアウトを取得します。</para></summary>
    public TimeSpan Timeout { get; }
}
