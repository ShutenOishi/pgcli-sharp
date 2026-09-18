using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a validated pg_dumpall psql restrict key.</para>
/// <para>JA: 検証済みの pg_dumpall psql restrict key を表します。</para>
/// </summary>
public sealed class PgDumpAllRestrictKey
{
    /// <summary><para>EN: Initializes a non-empty ASCII alphanumeric restrict key.</para><para>JA: 空ではない ASCII 英数字の restrict key を初期化します。</para></summary>
    /// <param name="value"><para>EN: Restrict key.</para><para>JA: restrict key です。</para></param>
    public PgDumpAllRestrictKey(string value)
    {
        if (string.IsNullOrEmpty(value) || !IsAsciiAlphanumeric(value))
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
    public override string ToString() => Value;

    private static bool IsAsciiAlphanumeric(string value)
    {
        foreach (char character in value)
        {
            bool valid =
                (character >= '0' && character <= '9') ||
                (character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z');

            if (!valid)
            {
                return false;
            }
        }

        return true;
    }
}
