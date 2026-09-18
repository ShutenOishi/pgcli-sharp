using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a failure to parse PostgreSQL version information from an executable's version output.</para>
/// <para>JA: 実行ファイルのバージョン出力から PostgreSQL バージョンを解析できなかったことを表します。</para>
/// </summary>
public sealed class PgExecutableVersionParseException : PgCliSharpException
{
    internal PgExecutableVersionParseException(string executablePath, string versionOutput)
        : base(MessageProvider.Format(MessageKeys.ExecutableVersionOutputInvalid, executablePath))
    {
        ExecutablePath = executablePath;
        VersionOutput = versionOutput;
    }

    /// <summary><para>EN: Gets the executable path.</para><para>JA: 実行ファイルのパスを取得します。</para></summary>
    public string ExecutablePath { get; }

    /// <summary><para>EN: Gets the original version output that could not be parsed.</para><para>JA: 解析できなかった元のバージョン出力を取得します。</para></summary>
    public string VersionOutput { get; }
}
