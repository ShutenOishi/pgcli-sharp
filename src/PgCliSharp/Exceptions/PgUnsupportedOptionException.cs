using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a typed option that is unavailable for the selected PostgreSQL executable version.</para>
/// <para>JA: 選択した PostgreSQL 実行ファイルのバージョンでは利用できない型付きオプションを表します。</para>
/// </summary>
public sealed class PgUnsupportedOptionException : PgOptionValidationException
{
    internal PgUnsupportedOptionException(
        PostgreSqlMajorVersion selectedVersion,
        string optionName,
        PostgreSqlMajorVersion? supportedSince = null,
        PostgreSqlMajorVersion? supportedUntil = null,
        Version? minimumExecutableVersion = null,
        Version? actualExecutableVersion = null)
        : base(
            minimumExecutableVersion is null
                ? MessageProvider.Format(MessageKeys.OptionNotSupported, optionName, (int)selectedVersion)
                : MessageProvider.Format(
                    MessageKeys.OptionRequiresExecutableVersion,
                    optionName,
                    minimumExecutableVersion,
                    actualExecutableVersion ?? new Version((int)selectedVersion, 0)),
            selectedVersion,
            optionName)
    {
        SupportedSince = supportedSince;
        SupportedUntil = supportedUntil;
        MinimumExecutableVersion = minimumExecutableVersion;
        ActualExecutableVersion = actualExecutableVersion;
    }

    /// <summary><para>EN: Gets the first supported major version when known.</para><para>JA: 判明している場合、最初に対応するメジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion? SupportedSince { get; }

    /// <summary><para>EN: Gets the last supported major version when known.</para><para>JA: 判明している場合、最後に対応するメジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion? SupportedUntil { get; }

    /// <summary>
    /// <para>EN: Gets the minimum exact executable version when availability was introduced by a patch release.</para>
    /// <para>JA: パッチリリースで利用可能になった場合の、必要な最小実行ファイルバージョンを取得します。</para>
    /// </summary>
    public Version? MinimumExecutableVersion { get; }

    /// <summary>
    /// <para>EN: Gets the actual executable version when an exact-version availability check failed.</para>
    /// <para>JA: 正確なバージョンの利用可否検証に失敗した場合の、実際の実行ファイルバージョンを取得します。</para>
    /// </summary>
    public Version? ActualExecutableVersion { get; }
}
