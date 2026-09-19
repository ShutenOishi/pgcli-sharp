using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Indicates that a PostgreSQL CLI tool does not exist for the selected major version.</para>
/// <para>JA: 選択したメジャーバージョンに PostgreSQL CLI ツールが存在しないことを表します。</para>
/// </summary>
public sealed class PgUnsupportedToolException : PgCliSharpException
{
    internal PgUnsupportedToolException(string toolName, PostgreSqlMajorVersion selectedVersion, PostgreSqlMajorVersion supportedSince)
        : base(MessageProvider.Format(MessageKeys.ToolNotSupported, toolName, (int)selectedVersion, (int)supportedSince))
    {
        ToolName = toolName;
        SelectedVersion = selectedVersion;
        SupportedSince = supportedSince;
    }

    /// <summary><para>EN: Gets the tool name.</para><para>JA: ツール名を取得します。</para></summary>
    public string ToolName { get; }
    /// <summary><para>EN: Gets the rejected PostgreSQL major version.</para><para>JA: 拒否された PostgreSQL メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion SelectedVersion { get; }
    /// <summary><para>EN: Gets the first supported major version.</para><para>JA: 最初に対応するメジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion SupportedSince { get; }
}
