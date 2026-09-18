using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a mismatch between the caller-selected PostgreSQL major version and the configured executable.</para>
/// <para>JA: 呼び出し側が選択した PostgreSQL メジャーバージョンと設定された実行ファイルのバージョン不一致を表します。</para>
/// </summary>
public sealed class PgExecutableVersionMismatchException : PgCliSharpException
{
    internal PgExecutableVersionMismatchException(
        string executablePath,
        PostgreSqlMajorVersion expectedVersion,
        int actualMajorVersion)
        : base(
            MessageProvider.Format(
                MessageKeys.ExecutableVersionMismatch,
                (int)expectedVersion,
                executablePath,
                actualMajorVersion))
    {
        ExecutablePath = executablePath;
        ExpectedVersion = expectedVersion;
        ActualMajorVersion = actualMajorVersion;
    }

    /// <summary><para>EN: Gets the configured executable path.</para><para>JA: 設定された実行ファイルのパスを取得します。</para></summary>
    public string ExecutablePath { get; }

    /// <summary><para>EN: Gets the PostgreSQL major version selected by the caller.</para><para>JA: 呼び出し側が選択した PostgreSQL メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion ExpectedVersion { get; }

    /// <summary><para>EN: Gets the major version reported by the executable.</para><para>JA: 実行ファイルが報告したメジャーバージョンを取得します。</para></summary>
    public int ActualMajorVersion { get; }
}
