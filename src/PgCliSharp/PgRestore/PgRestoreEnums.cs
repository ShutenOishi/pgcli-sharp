namespace PgCliSharp;

/// <summary>
/// <para>EN: Specifies a pg_restore archive format when automatic detection is not used.</para>
/// <para>JA: 自動判定を使用しない場合の pg_restore アーカイブ形式を指定します。</para>
/// </summary>
public enum PgRestoreArchiveFormat
{
    /// <summary><para>EN: Custom archive format.</para><para>JA: カスタムアーカイブ形式です。</para></summary>
    Custom,

    /// <summary><para>EN: Directory archive format.</para><para>JA: ディレクトリアーカイブ形式です。</para></summary>
    Directory,

    /// <summary><para>EN: Tar archive format.</para><para>JA: tar アーカイブ形式です。</para></summary>
    Tar,
}

/// <summary>
/// <para>EN: Identifies a pg_restore archive section.</para>
/// <para>JA: pg_restore のアーカイブセクションを表します。</para>
/// </summary>
public enum PgRestoreSection
{
    /// <summary><para>EN: Definitions that precede data.</para><para>JA: データより前の定義です。</para></summary>
    PreData,

    /// <summary><para>EN: Data section.</para><para>JA: データセクションです。</para></summary>
    Data,

    /// <summary><para>EN: Definitions that follow data.</para><para>JA: データより後の定義です。</para></summary>
    PostData,
}

/// <summary>
/// <para>EN: Selects the mutually exclusive primary pg_restore content mode.</para>
/// <para>JA: 排他的な pg_restore の主要復元内容モードを選択します。</para>
/// </summary>
public enum PgRestoreContentMode
{
    /// <summary><para>EN: Restore the normal archive content.</para><para>JA: 通常のアーカイブ内容を復元します。</para></summary>
    All,

    /// <summary><para>EN: Restore data only.</para><para>JA: データのみを復元します。</para></summary>
    DataOnly,

    /// <summary><para>EN: Restore schema only.</para><para>JA: スキーマのみを復元します。</para></summary>
    SchemaOnly,

    /// <summary><para>EN: Restore optimizer statistics only. Available in PostgreSQL 18 and later.</para><para>JA: オプティマイザ統計情報のみを復元します。PostgreSQL 18 以降で使用できます。</para></summary>
    StatisticsOnly,
}

/// <summary>
/// <para>EN: Selects whether pg_restore restores archive contents or lists the archive table of contents.</para>
/// <para>JA: pg_restore がアーカイブを復元するか、アーカイブの TOC を一覧表示するかを選択します。</para>
/// </summary>
public enum PgRestoreMode
{
    /// <summary><para>EN: Restore directly or generate a SQL script.</para><para>JA: 直接復元するか SQL スクリプトを生成します。</para></summary>
    Restore,

    /// <summary><para>EN: List the archive table of contents.</para><para>JA: アーカイブの TOC を一覧表示します。</para></summary>
    List,
}

/// <summary>
/// <para>EN: Identifies a pg_restore transaction strategy.</para>
/// <para>JA: pg_restore のトランザクション方式を表します。</para>
/// </summary>
public enum PgRestoreTransactionModeKind
{
    /// <summary><para>EN: Restore everything in one transaction.</para><para>JA: すべてを1つのトランザクションで復元します。</para></summary>
    SingleTransaction,

    /// <summary><para>EN: Commit after a configured number of archive objects. Available in PostgreSQL 17 and later.</para><para>JA: 指定したアーカイブオブジェクト数ごとにコミットします。PostgreSQL 17 以降で使用できます。</para></summary>
    Batch,
}

/// <summary>
/// <para>EN: Identifies the pg_restore archive input source.</para>
/// <para>JA: pg_restore アーカイブの入力元を表します。</para>
/// </summary>
public enum PgRestoreInputKind
{
    /// <summary><para>EN: Archive file.</para><para>JA: アーカイブファイルです。</para></summary>
    File,

    /// <summary><para>EN: Directory-format archive.</para><para>JA: ディレクトリ形式アーカイブです。</para></summary>
    Directory,

    /// <summary><para>EN: Archive bytes supplied through standard input.</para><para>JA: 標準入力から供給するアーカイブバイト列です。</para></summary>
    StandardInput,
}

/// <summary>
/// <para>EN: Identifies the pg_restore execution/output destination.</para>
/// <para>JA: pg_restore の実行・出力先を表します。</para>
/// </summary>
public enum PgRestoreOutputKind
{
    /// <summary><para>EN: Restore directly to a database.</para><para>JA: データベースへ直接復元します。</para></summary>
    Database,

    /// <summary><para>EN: Stream generated SQL or list output to a caller-owned stream.</para><para>JA: 生成 SQL または一覧出力を呼び出し側所有のストリームへ転送します。</para></summary>
    StandardOutput,

    /// <summary><para>EN: Let pg_restore write generated SQL or list output to a file.</para><para>JA: pg_restore 自身に生成 SQL または一覧をファイルへ書き込ませます。</para></summary>
    File,
}
