namespace PgCliSharp;

/// <summary>
/// <para>EN: Base exception for failures reported by PgCliSharp.</para>
/// <para>JA: PgCliSharp が報告する失敗の基底例外です。</para>
/// </summary>
public abstract class PgCliSharpException : Exception
{
    /// <summary>
    /// <para>EN: Initializes the exception with a localized human-readable message and optional inner exception.</para>
    /// <para>JA: ローカライズされた人向けメッセージと任意の内部例外を指定して例外を初期化します。</para>
    /// </summary>
    /// <param name="message"><para>EN: The localized message.</para><para>JA: ローカライズされたメッセージです。</para></param>
    /// <param name="innerException"><para>EN: The underlying exception, if any.</para><para>JA: 存在する場合は原因となった内部例外です。</para></param>
    protected PgCliSharpException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
