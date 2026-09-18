using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a validated pg_dump psql restrict key.</para>
/// <para>JA: 検証済みの pg_dump psql restrict key を表します。</para>
/// </summary>
/// <remarks>
/// <para>EN: PostgreSQL requires a non-empty key containing ASCII alphanumeric characters only. Availability is patch-dependent for PostgreSQL 13 through 17.</para>
/// <para>JA: PostgreSQL では空ではない ASCII 英数字のみのキーが必要です。PostgreSQL 13〜17 では利用可否がマイナーバージョンにも依存します。</para>
/// </remarks>
public sealed class PgDumpRestrictKey
{
    /// <summary>
    /// <para>EN: Initializes a restrict key after validating PostgreSQL's character requirements.</para>
    /// <para>JA: PostgreSQL の文字要件を検証して restrict key を初期化します。</para>
    /// </summary>
    /// <param name="value"><para>EN: Non-empty ASCII alphanumeric key.</para><para>JA: 空ではない ASCII 英数字のキーです。</para></param>
    public PgDumpRestrictKey(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.InvalidRestrictKey),
                nameof(value));
        }

        Value = value;
    }

    /// <summary><para>EN: Gets the validated key.</para><para>JA: 検証済みのキーを取得します。</para></summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }

    private static bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            bool asciiAlphaNumeric =
                (character >= '0' && character <= '9') ||
                (character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z');

            if (!asciiAlphaNumeric)
            {
                return false;
            }
        }

        return true;
    }
}
