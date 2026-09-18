namespace PgCliSharp;

/// <summary>
/// <para>EN: Specifies the pg_dump output format.</para>
/// <para>JA: pg_dump の出力形式を指定します。</para>
/// </summary>
public enum PgDumpFormat
{
    /// <summary><para>EN: Plain-text SQL script format.</para><para>JA: プレーンテキストの SQL スクリプト形式です。</para></summary>
    Plain,

    /// <summary><para>EN: Custom archive format for pg_restore.</para><para>JA: pg_restore 用のカスタムアーカイブ形式です。</para></summary>
    Custom,

    /// <summary><para>EN: Directory archive format. This is the only format that supports parallel dumps.</para><para>JA: ディレクトリアーカイブ形式です。並列ダンプをサポートする唯一の形式です。</para></summary>
    Directory,

    /// <summary><para>EN: Tar archive format. Compression is not supported by pg_dump for this format.</para><para>JA: tar アーカイブ形式です。pg_dump ではこの形式の圧縮はサポートされません。</para></summary>
    Tar,
}

/// <summary>
/// <para>EN: Identifies a pg_dump archive section.</para>
/// <para>JA: pg_dump のアーカイブセクションを表します。</para>
/// </summary>
public enum PgDumpSection
{
    /// <summary><para>EN: Definitions that must precede data.</para><para>JA: データより前に必要な定義です。</para></summary>
    PreData,

    /// <summary><para>EN: Data section.</para><para>JA: データセクションです。</para></summary>
    Data,

    /// <summary><para>EN: Definitions that follow data.</para><para>JA: データより後に配置される定義です。</para></summary>
    PostData,
}

/// <summary>
/// <para>EN: Specifies a PostgreSQL 16+ pg_dump compression method.</para>
/// <para>JA: PostgreSQL 16 以降の pg_dump 圧縮方式を指定します。</para>
/// </summary>
public enum PgDumpCompressionMethod
{
    /// <summary><para>EN: gzip compression.</para><para>JA: gzip 圧縮です。</para></summary>
    Gzip,

    /// <summary><para>EN: LZ4 compression.</para><para>JA: LZ4 圧縮です。</para></summary>
    Lz4,

    /// <summary><para>EN: Zstandard compression.</para><para>JA: Zstandard 圧縮です。</para></summary>
    Zstd,

    /// <summary><para>EN: No compression.</para><para>JA: 圧縮しません。</para></summary>
    None,
}

/// <summary>
/// <para>EN: Specifies the synchronization method supported by PostgreSQL 17+ pg_dump.</para>
/// <para>JA: PostgreSQL 17 以降の pg_dump で使用する同期方式を指定します。</para>
/// </summary>
public enum PgDumpSyncMethod
{
    /// <summary><para>EN: Synchronize files individually with fsync. This is the PostgreSQL default.</para><para>JA: 各ファイルを fsync で同期します。PostgreSQL の既定値です。</para></summary>
    Fsync,

    /// <summary><para>EN: Synchronize the archive directory with syncfs on supported Linux systems.</para><para>JA: 対応する Linux で syncfs を使用してアーカイブディレクトリを同期します。</para></summary>
    Syncfs,
}

/// <summary>
/// <para>EN: Controls whether large objects are explicitly included or excluded.</para>
/// <para>JA: ラージオブジェクトを明示的に含めるか除外するかを制御します。</para>
/// </summary>
public enum PgDumpLargeObjectMode
{
    /// <summary><para>EN: Use pg_dump's context-dependent default behavior.</para><para>JA: pg_dump の状況依存の既定動作を使用します。</para></summary>
    Default,

    /// <summary><para>EN: Explicitly include large objects.</para><para>JA: ラージオブジェクトを明示的に含めます。</para></summary>
    Include,

    /// <summary><para>EN: Explicitly exclude large objects.</para><para>JA: ラージオブジェクトを明示的に除外します。</para></summary>
    Exclude,
}

/// <summary>
/// <para>EN: Controls pg_dump password prompting.</para>
/// <para>JA: pg_dump のパスワードプロンプト動作を制御します。</para>
/// </summary>
public enum PgPasswordPromptMode
{
    /// <summary><para>EN: Use pg_dump/libpq default prompting behavior.</para><para>JA: pg_dump/libpq の既定のプロンプト動作を使用します。</para></summary>
    Default,

    /// <summary><para>EN: Never prompt for a password (-w/--no-password).</para><para>JA: パスワードを要求しません（-w/--no-password）。</para></summary>
    NeverPrompt,

    /// <summary><para>EN: Prompt before connecting (-W/--password).</para><para>JA: 接続前にパスワード入力を要求します（-W/--password）。</para></summary>
    ForcePrompt,
}

/// <summary>
/// <para>EN: Identifies where pg_dump writes its output.</para>
/// <para>JA: pg_dump の出力先の種類を表します。</para>
/// </summary>
public enum PgDumpOutputKind
{
    /// <summary><para>EN: Stream pg_dump stdout to a caller-provided stream.</para><para>JA: pg_dump の標準出力を呼び出し側のストリームへ転送します。</para></summary>
    StandardOutput,

    /// <summary><para>EN: Let pg_dump write a file through -f/--file.</para><para>JA: -f/--file を使用して pg_dump 自身にファイルを書き込ませます。</para></summary>
    File,

    /// <summary><para>EN: Let pg_dump create/write a directory-format archive.</para><para>JA: pg_dump 自身にディレクトリ形式アーカイブを作成・書き込みさせます。</para></summary>
    Directory,
}
