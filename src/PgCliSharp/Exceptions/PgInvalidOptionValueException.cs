using System.Globalization;
using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents an invalid typed value for a PostgreSQL command-line option.</para>
/// <para>JA: PostgreSQL コマンドラインオプションに指定された無効な型付き値を表します。</para>
/// </summary>
public sealed class PgInvalidOptionValueException : PgOptionValidationException
{
    internal PgInvalidOptionValueException(
        PostgreSqlMajorVersion selectedVersion,
        string optionName,
        object? value)
        : base(
            MessageProvider.Format(MessageKeys.InvalidOptionValue, optionName, value ?? "<null>"),
            selectedVersion,
            optionName)
    {
        Value = value is null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    /// <summary><para>EN: Gets a culture-independent representation of the rejected value when available.</para><para>JA: 利用可能な場合、拒否された値のカルチャに依存しない表現を取得します。</para></summary>
    public string? Value { get; }
}
