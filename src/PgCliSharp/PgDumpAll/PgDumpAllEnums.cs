namespace PgCliSharp;

/// <summary>
/// <para>EN: Selects pg_dumpall's mutually exclusive cluster scope.</para>
/// <para>JA: pg_dumpall の排他的なクラスタ出力範囲を選択します。</para>
/// </summary>
public enum PgDumpAllScope
{
    /// <summary><para>EN: Dump global objects and all non-excluded databases.</para><para>JA: グローバルオブジェクトと除外されていない全データベースをダンプします。</para></summary>
    All,

    /// <summary><para>EN: Dump global objects only: roles and tablespaces, without databases.</para><para>JA: データベースを含めず、ロールと tablespace のグローバルオブジェクトのみをダンプします。</para></summary>
    GlobalsOnly,

    /// <summary><para>EN: Dump roles only.</para><para>JA: ロールのみをダンプします。</para></summary>
    RolesOnly,

    /// <summary><para>EN: Dump tablespaces only.</para><para>JA: tablespace のみをダンプします。</para></summary>
    TablespacesOnly,
}

/// <summary>
/// <para>EN: Selects pg_dumpall's mutually exclusive database-content mode.</para>
/// <para>JA: pg_dumpall の排他的なデータベース内容モードを選択します。</para>
/// </summary>
public enum PgDumpAllContentMode
{
    /// <summary><para>EN: Dump normal schema and data content.</para><para>JA: 通常のスキーマとデータをダンプします。</para></summary>
    All,

    /// <summary><para>EN: Dump data only.</para><para>JA: データのみをダンプします。</para></summary>
    DataOnly,

    /// <summary><para>EN: Dump schema only.</para><para>JA: スキーマのみをダンプします。</para></summary>
    SchemaOnly,

    /// <summary><para>EN: Dump optimizer statistics only. Available in PostgreSQL 18 and later.</para><para>JA: オプティマイザ統計情報のみをダンプします。PostgreSQL 18 以降で使用できます。</para></summary>
    StatisticsOnly,
}

/// <summary>
/// <para>EN: Identifies the pg_dumpall SQL-script destination.</para>
/// <para>JA: pg_dumpall の SQL スクリプト出力先を表します。</para>
/// </summary>
public enum PgDumpAllOutputKind
{
    /// <summary><para>EN: Stream stdout to a caller-owned stream.</para><para>JA: 標準出力を呼び出し側所有のストリームへ転送します。</para></summary>
    StandardOutput,

    /// <summary><para>EN: Let pg_dumpall write an output file.</para><para>JA: pg_dumpall 自身に出力ファイルを書き込ませます。</para></summary>
    File,
}
