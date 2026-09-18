namespace PgCliSharp;

/// <summary>
/// <para>EN: Base exception for typed PostgreSQL option validation failures detected before process startup.</para>
/// <para>JA: プロセス開始前に検出された型付き PostgreSQL オプション検証エラーの基底例外です。</para>
/// </summary>
public abstract class PgOptionValidationException : PgCliSharpException
{
    internal PgOptionValidationException(
        string message,
        PostgreSqlMajorVersion selectedVersion,
        string optionName)
        : base(message)
    {
        SelectedVersion = selectedVersion;
        OptionName = optionName;
    }

    /// <summary><para>EN: Gets the selected PostgreSQL CLI major version.</para><para>JA: 選択された PostgreSQL CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion SelectedVersion { get; }

    /// <summary><para>EN: Gets the option name involved in the validation failure.</para><para>JA: 検証エラーに関係するオプション名を取得します。</para></summary>
    public string OptionName { get; }
}
