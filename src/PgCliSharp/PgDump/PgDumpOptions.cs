namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents the typed union of pg_dump options supported by PgCliSharp for PostgreSQL 10 through 18.</para>
/// <para>JA: PgCliSharp が PostgreSQL 10〜18 向けにサポートする pg_dump オプションの型付き union を表します。</para>
/// </summary>
/// <remarks>
/// <para>EN: Version-specific availability and incompatible combinations are validated before pg_dump starts. The -f/--file destination is modeled separately by <see cref="PgDumpOutput"/> so stdout can remain binary-safe.</para>
/// <para>JA: バージョン別の利用可否と不正な組み合わせは pg_dump 起動前に検証されます。-f/--file の出力先は <see cref="PgDumpOutput"/> で分離して表現し、標準出力をバイナリセーフに扱います。</para>
/// </remarks>
public sealed class PgDumpOptions
{
    /// <summary><para>EN: Gets or sets the database name or libpq connection string.</para><para>JA: データベース名または libpq 接続文字列を取得または設定します。</para></summary>
    public string? Database { get; set; }

    /// <summary><para>EN: Gets or sets the database server host or Unix-domain socket directory.</para><para>JA: データベースサーバーのホストまたは Unix ドメインソケットディレクトリを取得または設定します。</para></summary>
    public string? Host { get; set; }

    /// <summary><para>EN: Gets or sets the database server port.</para><para>JA: データベースサーバーのポートを取得または設定します。</para></summary>
    public int? Port { get; set; }

    /// <summary><para>EN: Gets or sets the database user name.</para><para>JA: データベースユーザー名を取得または設定します。</para></summary>
    public string? Username { get; set; }

    /// <summary><para>EN: Gets or sets the password-prompt policy.</para><para>JA: パスワードプロンプトの方針を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;

    /// <summary><para>EN: Gets or sets the role applied with SET ROLE after connecting.</para><para>JA: 接続後に SET ROLE で適用するロールを取得または設定します。</para></summary>
    public string? Role { get; set; }

    /// <summary><para>EN: Gets or sets whether only data is dumped (-a/--data-only).</para><para>JA: データのみをダンプするかどうか（-a/--data-only）を取得または設定します。</para></summary>
    public bool DataOnly { get; set; }

    /// <summary><para>EN: Gets or sets explicit large-object inclusion or exclusion.</para><para>JA: ラージオブジェクトを明示的に含めるか除外するかを取得または設定します。</para></summary>
    public PgDumpLargeObjectMode LargeObjects { get; set; } = PgDumpLargeObjectMode.Default;

    /// <summary><para>EN: Gets or sets whether DROP commands are emitted before creation commands.</para><para>JA: 作成コマンドの前に DROP コマンドを出力するかどうかを取得または設定します。</para></summary>
    public bool Clean { get; set; }

    /// <summary><para>EN: Gets or sets whether the dump begins by creating the database.</para><para>JA: ダンプの先頭でデータベースを作成するかどうかを取得または設定します。</para></summary>
    public bool Create { get; set; }

    /// <summary><para>EN: Gets or sets the dump character encoding.</para><para>JA: ダンプの文字エンコーディングを取得または設定します。</para></summary>
    public string? Encoding { get; set; }

    /// <summary><para>EN: Gets or sets the dump format. Plain is the default.</para><para>JA: ダンプ形式を取得または設定します。既定値は Plain です。</para></summary>
    public PgDumpFormat Format { get; set; } = PgDumpFormat.Plain;

    /// <summary><para>EN: Gets or sets the number of parallel dump jobs. This is valid only for directory format.</para><para>JA: 並列ダンプのジョブ数を取得または設定します。ディレクトリ形式でのみ有効です。</para></summary>
    public int? Jobs { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 10-11 --oids output. This option was removed in PostgreSQL 12.</para><para>JA: PostgreSQL 10〜11 の --oids 出力を取得または設定します。このオプションは PostgreSQL 12 で廃止されました。</para></summary>
    public bool IncludeOids { get; set; }

    /// <summary><para>EN: Gets or sets whether ownership-setting commands are omitted.</para><para>JA: 所有者設定コマンドを省略するかどうかを取得または設定します。</para></summary>
    public bool NoOwner { get; set; }

    /// <summary><para>EN: Gets or sets the obsolete but accepted --no-reconnect compatibility flag.</para><para>JA: 廃止済みですが互換性のため受理される --no-reconnect フラグを取得または設定します。</para></summary>
    public bool NoReconnect { get; set; }

    /// <summary><para>EN: Gets or sets whether only schema definitions are dumped.</para><para>JA: スキーマ定義のみをダンプするかどうかを取得または設定します。</para></summary>
    public bool SchemaOnly { get; set; }

    /// <summary><para>EN: Gets or sets the superuser name used by --disable-triggers output.</para><para>JA: --disable-triggers の出力で使用するスーパーユーザー名を取得または設定します。</para></summary>
    public string? Superuser { get; set; }

    /// <summary><para>EN: Gets or sets how many times -v/--verbose is emitted. Zero disables verbose output.</para><para>JA: -v/--verbose を何回出力するかを取得または設定します。0 では verbose 出力を無効にします。</para></summary>
    public int Verbosity { get; set; }

    /// <summary><para>EN: Gets or sets whether GRANT/REVOKE commands are omitted.</para><para>JA: GRANT/REVOKE コマンドを省略するかどうかを取得または設定します。</para></summary>
    public bool NoPrivileges { get; set; }

    /// <summary><para>EN: Gets or sets the version-aware compression specification.</para><para>JA: バージョン差を考慮した圧縮指定を取得または設定します。</para></summary>
    public PgDumpCompression? Compression { get; set; }

    /// <summary><para>EN: Gets or sets the upstream internal-use --binary-upgrade flag.</para><para>JA: 上流で内部用途とされる --binary-upgrade フラグを取得または設定します。</para></summary>
    public bool BinaryUpgrade { get; set; }

    /// <summary><para>EN: Gets or sets whether INSERT statements include explicit column names.</para><para>JA: INSERT 文に明示的な列名を含めるかどうかを取得または設定します。</para></summary>
    public bool ColumnInserts { get; set; }

    /// <summary><para>EN: Gets or sets whether dollar quoting is disabled.</para><para>JA: ドル引用を無効化するかどうかを取得または設定します。</para></summary>
    public bool DisableDollarQuoting { get; set; }

    /// <summary><para>EN: Gets or sets whether restore-time trigger disabling commands are emitted for data-only style output.</para><para>JA: データのみの出力等で、復元時にトリガーを無効化するコマンドを出力するかどうかを取得または設定します。</para></summary>
    public bool DisableTriggers { get; set; }

    /// <summary><para>EN: Gets or sets whether row security is enabled while dumping table contents.</para><para>JA: テーブル内容のダンプ時に行セキュリティを有効にするかどうかを取得または設定します。</para></summary>
    public bool EnableRowSecurity { get; set; }

    /// <summary><para>EN: Gets or sets whether DROP commands use IF EXISTS. Requires Clean.</para><para>JA: DROP コマンドで IF EXISTS を使用するかどうかを取得または設定します。Clean が必要です。</para></summary>
    public bool IfExists { get; set; }

    /// <summary><para>EN: Gets or sets whether data is emitted as INSERT statements instead of COPY.</para><para>JA: データを COPY ではなく INSERT 文として出力するかどうかを取得または設定します。</para></summary>
    public bool Inserts { get; set; }

    /// <summary><para>EN: Gets or sets the table-lock wait timeout. PgCliSharp serializes it as integer milliseconds.</para><para>JA: テーブルロック待機タイムアウトを取得または設定します。PgCliSharp は整数ミリ秒としてシリアライズします。</para></summary>
    public TimeSpan? LockWaitTimeout { get; set; }

    /// <summary><para>EN: Gets or sets whether publications are omitted.</para><para>JA: publication を省略するかどうかを取得または設定します。</para></summary>
    public bool NoPublications { get; set; }

    /// <summary><para>EN: Gets or sets whether security labels are omitted.</para><para>JA: セキュリティラベルを省略するかどうかを取得または設定します。</para></summary>
    public bool NoSecurityLabels { get; set; }

    /// <summary><para>EN: Gets or sets whether subscriptions are omitted.</para><para>JA: subscription を省略するかどうかを取得または設定します。</para></summary>
    public bool NoSubscriptions { get; set; }

    /// <summary><para>EN: Gets or sets whether pg_dump skips waiting for safe filesystem synchronization.</para><para>JA: pg_dump が安全なファイルシステム同期を待たないかどうかを取得または設定します。</para></summary>
    public bool NoSync { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 10-14 --no-synchronized-snapshots. The option was removed in PostgreSQL 15.</para><para>JA: PostgreSQL 10〜14 の --no-synchronized-snapshots を取得または設定します。このオプションは PostgreSQL 15 で廃止されました。</para></summary>
    public bool NoSynchronizedSnapshots { get; set; }

    /// <summary><para>EN: Gets or sets whether tablespace selection commands are omitted.</para><para>JA: tablespace 選択コマンドを省略するかどうかを取得または設定します。</para></summary>
    public bool NoTablespaces { get; set; }

    /// <summary><para>EN: Gets or sets whether data for unlogged relations is omitted.</para><para>JA: unlogged relation のデータを省略するかどうかを取得または設定します。</para></summary>
    public bool NoUnloggedTableData { get; set; }

    /// <summary><para>EN: Gets or sets whether every identifier is quoted.</para><para>JA: すべての識別子を引用するかどうかを取得または設定します。</para></summary>
    public bool QuoteAllIdentifiers { get; set; }

    /// <summary><para>EN: Gets or sets whether the dump uses a serializable deferrable transaction.</para><para>JA: serializable deferrable トランザクションを使用するかどうかを取得または設定します。</para></summary>
    public bool SerializableDeferrable { get; set; }

    /// <summary><para>EN: Gets or sets the synchronized snapshot name.</para><para>JA: 同期スナップショット名を取得または設定します。</para></summary>
    public string? Snapshot { get; set; }

    /// <summary><para>EN: Gets or sets whether include patterns must match at least one object.</para><para>JA: include パターンが少なくとも1つのオブジェクトに一致する必要があるかどうかを取得または設定します。</para></summary>
    public bool StrictNames { get; set; }

    /// <summary><para>EN: Gets or sets whether ownership is represented with SET SESSION AUTHORIZATION instead of ALTER OWNER.</para><para>JA: 所有権を ALTER OWNER ではなく SET SESSION AUTHORIZATION で表すかどうかを取得または設定します。</para></summary>
    public bool UseSetSessionAuthorization { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 11+ --load-via-partition-root.</para><para>JA: PostgreSQL 11 以降の --load-via-partition-root を取得または設定します。</para></summary>
    public bool LoadViaPartitionRoot { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 11+ --no-comments.</para><para>JA: PostgreSQL 11 以降の --no-comments を取得または設定します。</para></summary>
    public bool NoComments { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 12+ --extra-float-digits.</para><para>JA: PostgreSQL 12 以降の --extra-float-digits を取得または設定します。</para></summary>
    public int? ExtraFloatDigits { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 12+ --on-conflict-do-nothing. An INSERT-producing option is required.</para><para>JA: PostgreSQL 12 以降の --on-conflict-do-nothing を取得または設定します。INSERT を生成するオプションが必要です。</para></summary>
    public bool OnConflictDoNothing { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 12+ maximum rows per INSERT. The value must be greater than zero.</para><para>JA: PostgreSQL 12 以降で1つの INSERT に含める最大行数を取得または設定します。0 より大きい必要があります。</para></summary>
    public int? RowsPerInsert { get; set; }

    /// <summary><para>EN: Gets or sets the patch-version-aware --restrict-key value for plain dumps.</para><para>JA: プレーンダンプ用の、パッチバージョン差を考慮した --restrict-key 値を取得または設定します。</para></summary>
    public PgDumpRestrictKey? RestrictKey { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 14+ --no-toast-compression.</para><para>JA: PostgreSQL 14 以降の --no-toast-compression を取得または設定します。</para></summary>
    public bool NoToastCompression { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 15+ --no-table-access-method.</para><para>JA: PostgreSQL 15 以降の --no-table-access-method を取得または設定します。</para></summary>
    public bool NoTableAccessMethod { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 17+ directory synchronization method.</para><para>JA: PostgreSQL 17 以降のディレクトリ同期方式を取得または設定します。</para></summary>
    public PgDumpSyncMethod? SyncMethod { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ --no-data.</para><para>JA: PostgreSQL 18 以降の --no-data を取得または設定します。</para></summary>
    public bool NoData { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ --no-policies.</para><para>JA: PostgreSQL 18 以降の --no-policies を取得または設定します。</para></summary>
    public bool NoPolicies { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ --no-schema.</para><para>JA: PostgreSQL 18 以降の --no-schema を取得または設定します。</para></summary>
    public bool NoSchema { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ --no-statistics.</para><para>JA: PostgreSQL 18 以降の --no-statistics を取得または設定します。</para></summary>
    public bool NoStatistics { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ --sequence-data.</para><para>JA: PostgreSQL 18 以降の --sequence-data を取得または設定します。</para></summary>
    public bool SequenceData { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ --statistics.</para><para>JA: PostgreSQL 18 以降の --statistics を取得または設定します。</para></summary>
    public bool Statistics { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ --statistics-only.</para><para>JA: PostgreSQL 18 以降の --statistics-only を取得または設定します。</para></summary>
    public bool StatisticsOnly { get; set; }

    /// <summary><para>EN: Gets schema include patterns (-n/--schema).</para><para>JA: スキーマ include パターン（-n/--schema）のコレクションを取得します。</para></summary>
    public IList<string> Schemas { get; } = new List<string>();

    /// <summary><para>EN: Gets schema exclude patterns (-N/--exclude-schema).</para><para>JA: スキーマ exclude パターン（-N/--exclude-schema）のコレクションを取得します。</para></summary>
    public IList<string> ExcludedSchemas { get; } = new List<string>();

    /// <summary><para>EN: Gets table include patterns (-t/--table).</para><para>JA: テーブル include パターン（-t/--table）のコレクションを取得します。</para></summary>
    public IList<string> Tables { get; } = new List<string>();

    /// <summary><para>EN: Gets table exclude patterns (-T/--exclude-table).</para><para>JA: テーブル exclude パターン（-T/--exclude-table）のコレクションを取得します。</para></summary>
    public IList<string> ExcludedTables { get; } = new List<string>();

    /// <summary><para>EN: Gets repeatable --exclude-table-data patterns.</para><para>JA: 複数指定可能な --exclude-table-data パターンを取得します。</para></summary>
    public IList<string> ExcludedTableData { get; } = new List<string>();

    /// <summary><para>EN: Gets repeatable --section values.</para><para>JA: 複数指定可能な --section 値を取得します。</para></summary>
    public IList<PgDumpSection> Sections { get; } = new List<PgDumpSection>();

    /// <summary><para>EN: Gets PostgreSQL 13+ --include-foreign-data patterns.</para><para>JA: PostgreSQL 13 以降の --include-foreign-data パターンを取得します。</para></summary>
    public IList<string> IncludedForeignData { get; } = new List<string>();

    /// <summary><para>EN: Gets PostgreSQL 14+ extension include patterns (-e/--extension).</para><para>JA: PostgreSQL 14 以降の extension include パターン（-e/--extension）を取得します。</para></summary>
    public IList<string> Extensions { get; } = new List<string>();

    /// <summary><para>EN: Gets PostgreSQL 16+ --table-and-children patterns.</para><para>JA: PostgreSQL 16 以降の --table-and-children パターンを取得します。</para></summary>
    public IList<string> TablesAndChildren { get; } = new List<string>();

    /// <summary><para>EN: Gets PostgreSQL 16+ --exclude-table-and-children patterns.</para><para>JA: PostgreSQL 16 以降の --exclude-table-and-children パターンを取得します。</para></summary>
    public IList<string> ExcludedTablesAndChildren { get; } = new List<string>();

    /// <summary><para>EN: Gets PostgreSQL 16+ --exclude-table-data-and-children patterns.</para><para>JA: PostgreSQL 16 以降の --exclude-table-data-and-children パターンを取得します。</para></summary>
    public IList<string> ExcludedTableDataAndChildren { get; } = new List<string>();

    /// <summary><para>EN: Gets PostgreSQL 17+ --exclude-extension patterns.</para><para>JA: PostgreSQL 17 以降の --exclude-extension パターンを取得します。</para></summary>
    public IList<string> ExcludedExtensions { get; } = new List<string>();

    /// <summary><para>EN: Gets PostgreSQL 17+ repeatable filter sources.</para><para>JA: PostgreSQL 17 以降の複数指定可能なフィルター入力元を取得します。</para></summary>
    public IList<PgDumpFilterSource> Filters { get; } = new List<PgDumpFilterSource>();

    /// <summary><para>EN: Gets caller-supplied environment variables passed to pg_dump. Values are not rendered into command-line arguments.</para><para>JA: pg_dump に渡す呼び出し側指定の環境変数を取得します。値はコマンドライン引数には展開されません。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

}
