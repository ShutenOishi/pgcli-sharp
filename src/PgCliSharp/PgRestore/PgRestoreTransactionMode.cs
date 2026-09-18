using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents pg_restore's mutually exclusive single-transaction and PostgreSQL 17+ transaction-batch modes.</para>
/// <para>JA: pg_restore の排他的な単一トランザクションモードと PostgreSQL 17 以降のトランザクション分割モードを表します。</para>
/// </summary>
public sealed class PgRestoreTransactionMode
{
    private PgRestoreTransactionMode(PgRestoreTransactionModeKind kind, int? objectCount)
    {
        Kind = kind;
        ObjectCount = objectCount;
    }

    /// <summary><para>EN: Gets the transaction strategy.</para><para>JA: トランザクション方式を取得します。</para></summary>
    public PgRestoreTransactionModeKind Kind { get; }

    /// <summary><para>EN: Gets the maximum archive-object count per transaction for batch mode.</para><para>JA: 分割モードで1トランザクション当たりの最大アーカイブオブジェクト数を取得します。</para></summary>
    public int? ObjectCount { get; }

    /// <summary><para>EN: Gets -1/--single-transaction mode.</para><para>JA: -1/--single-transaction モードを取得します。</para></summary>
    public static PgRestoreTransactionMode SingleTransaction { get; } =
        new PgRestoreTransactionMode(PgRestoreTransactionModeKind.SingleTransaction, null);

    /// <summary>
    /// <para>EN: Creates PostgreSQL 17+ --transaction-size mode.</para>
    /// <para>JA: PostgreSQL 17 以降の --transaction-size モードを作成します。</para>
    /// </summary>
    /// <param name="objectCount"><para>EN: Positive maximum number of archive objects per transaction.</para><para>JA: 1トランザクション当たりの正の最大アーカイブオブジェクト数です。</para></param>
    /// <returns><para>EN: The batch transaction mode.</para><para>JA: 分割トランザクションモードです。</para></returns>
    public static PgRestoreTransactionMode Batch(int objectCount)
    {
        if (objectCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(objectCount),
                objectCount,
                MessageProvider.GetString(MessageKeys.PositiveValueRequired));
        }

        return new PgRestoreTransactionMode(PgRestoreTransactionModeKind.Batch, objectCount);
    }
}
