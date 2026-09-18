using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a typed option that is unavailable for the selected PostgreSQL CLI major version.</para>
/// <para>JA: 選択した PostgreSQL CLI メジャーバージョンでは利用できない型付きオプションを表します。</para>
/// </summary>
public sealed class PgUnsupportedOptionException : PgOptionValidationException
{
    internal PgUnsupportedOptionException(
        PostgreSqlMajorVersion selectedVersion,
        string optionName,
        PostgreSqlMajorVersion? supportedSince = null,
        PostgreSqlMajorVersion? supportedUntil = null)
        : base(
            MessageProvider.Format(MessageKeys.OptionNotSupported, optionName, (int)selectedVersion),
            selectedVersion,
            optionName)
    {
        SupportedSince = supportedSince;
        SupportedUntil = supportedUntil;
    }

    /// <summary><para>EN: Gets the first supported major version when known.</para><para>JA: 判明している場合、最初に対応するメジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion? SupportedSince { get; }

    /// <summary><para>EN: Gets the last supported major version when known.</para><para>JA: 判明している場合、最後に対応するメジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion? SupportedUntil { get; }
}
