using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents an invalid typed value for a pg_dump option.</para>
/// <para>JA: pg_dump オプションに指定された無効な型付き値を表します。</para>
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
        Value = value?.ToString();
    }

    /// <summary><para>EN: Gets a culture-independent representation of the rejected value when available.</para><para>JA: 利用可能な場合、拒否された値のカルチャに依存しない表現を取得します。</para></summary>
    public string? Value { get; }
}
