using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a failure to start a configured PostgreSQL executable.</para>
/// <para>JA: 設定された PostgreSQL 実行ファイルを開始できなかったことを表します。</para>
/// </summary>
public sealed class PgExecutableStartException : PgCliSharpException
{
    internal PgExecutableStartException(string executablePath, Exception innerException)
        : base(MessageProvider.Format(MessageKeys.ExecutableStartFailed, executablePath), innerException)
    {
        ExecutablePath = executablePath;
    }

    /// <summary>
    /// <para>EN: Gets the executable path that failed to start.</para>
    /// <para>JA: 開始に失敗した実行ファイルのパスを取得します。</para>
    /// </summary>
    public string ExecutablePath { get; }
}
