namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents the typed union of pg_dumpall options supported by PgCliSharp for PostgreSQL 10 through 18.</para>
/// <para>JA: PgCliSharp が PostgreSQL 10〜18 向けにサポートする pg_dumpall オプションの型付き union を表します。</para>
/// </summary>
public sealed class PgDumpAllOptions
{
    /// <summary><para>EN: Gets or sets the mutually exclusive cluster scope.</para><para>JA: 排他的なクラスタ出力範囲を取得または設定します。</para></summary>
    public PgDumpAllScope Scope { get; set; } = PgDumpAllScope.All;

    /// <summary><para>EN: Gets or sets the mutually exclusive database-content mode.</para><para>JA: 排他的なデータベース内容モードを取得または設定します。</para></summary>
    public PgDumpAllContentMode ContentMode { get; set; } = PgDumpAllContentMode.All;

    /// <summary><para>EN: Gets or sets -d/--dbname as a libpq connection string. pg_dumpall ignores its database-name component because it connects to multiple databases.</para><para>JA: -d/--dbname を libpq 接続文字列として取得または設定します。pg_dumpall は複数データベースへ接続するため、接続文字列内のデータベース名部分は無視します。</para></summary>
    public string? ConnectionString { get; set; }

    /// <summary><para>EN: Gets or sets -l/--database, the initial database used for global objects and database discovery.</para><para>JA: グローバルオブジェクトとデータベース探索に使用する初期データベース -l/--database を取得または設定します。</para></summary>
    public string? InitialDatabase { get; set; }

    /// <summary><para>EN: Gets or sets the server host or Unix-domain socket directory.</para><para>JA: サーバーホストまたは Unix ドメインソケットディレクトリを取得または設定します。</para></summary>
    public string? Host { get; set; }

    /// <summary><para>EN: Gets or sets the server port.</para><para>JA: サーバーポートを取得または設定します。</para></summary>
    public int? Port { get; set; }

    /// <summary><para>EN: Gets or sets the database user name.</para><para>JA: データベースユーザー名を取得または設定します。</para></summary>
    public string? Username { get; set; }

    /// <summary><para>EN: Gets or sets password-prompt behavior.</para><para>JA: パスワードプロンプト動作を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;

    /// <summary><para>EN: Gets or sets the role applied with SET ROLE.</para><para>JA: SET ROLE で適用するロールを取得または設定します。</para></summary>
    public string? Role { get; set; }

    /// <summary><para>EN: Gets or sets whether DROP commands are emitted before creation commands.</para><para>JA: 作成コマンドの前に DROP コマンドを出力するかどうかを取得または設定します。</para></summary>
    public bool Clean { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 11+ dump encoding.</para><para>JA: PostgreSQL 11 以降のダンプエンコーディングを取得または設定します。</para></summary>
    public string? Encoding { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 10-11 --oids forwarding.</para><para>JA: PostgreSQL 10〜11 の --oids 転送を取得または設定します。</para></summary>
    public bool IncludeOids { get; set; }

    /// <summary><para>EN: Gets or sets whether ownership commands are omitted.</para><para>JA: 所有者設定コマンドを省略するかどうかを取得または設定します。</para></summary>
    public bool NoOwner { get; set; }

    /// <summary><para>EN: Gets or sets the superuser name used when disabling triggers.</para><para>JA: トリガー無効化時に使用するスーパーユーザー名を取得または設定します。</para></summary>
    public string? Superuser { get; set; }

    /// <summary><para>EN: Gets or sets how many times -v/--verbose is emitted.</para><para>JA: -v/--verbose を出力する回数を取得または設定します。</para></summary>
    public int Verbosity { get; set; }

    /// <summary><para>EN: Gets or sets whether privileges are omitted.</para><para>JA: 権限を省略するかどうかを取得または設定します。</para></summary>
    public bool NoPrivileges { get; set; }

    /// <summary><para>EN: Gets or sets upstream internal-use --binary-upgrade.</para><para>JA: 上流で内部用途とされる --binary-upgrade を取得または設定します。</para></summary>
    public bool BinaryUpgrade { get; set; }

    /// <summary><para>EN: Gets or sets explicit-column INSERT output.</para><para>JA: 列名を明示する INSERT 出力を取得または設定します。</para></summary>
    public bool ColumnInserts { get; set; }

    /// <summary><para>EN: Gets or sets whether dollar quoting is disabled.</para><para>JA: ドル引用を無効化するかどうかを取得または設定します。</para></summary>
    public bool DisableDollarQuoting { get; set; }

    /// <summary><para>EN: Gets or sets whether trigger-disabling commands are emitted for data restore.</para><para>JA: データ復元用のトリガー無効化コマンドを出力するかどうかを取得または設定します。</para></summary>
    public bool DisableTriggers { get; set; }

    /// <summary><para>EN: Gets or sets whether DROP commands use IF EXISTS. Requires Clean.</para><para>JA: DROP コマンドで IF EXISTS を使用するかどうかを取得または設定します。Clean が必要です。</para></summary>
    public bool IfExists { get; set; }

    /// <summary><para>EN: Gets or sets INSERT output instead of COPY.</para><para>JA: COPY の代わりに INSERT 出力を使用するかどうかを取得または設定します。</para></summary>
    public bool Inserts { get; set; }

    /// <summary><para>EN: Gets or sets the table-lock wait timeout. PgCliSharp serializes it as invariant integer milliseconds.</para><para>JA: テーブルロック待機タイムアウトを取得または設定します。PgCliSharp は不変カルチャの整数ミリ秒として出力します。</para></summary>
    public TimeSpan? LockWaitTimeout { get; set; }

    /// <summary><para>EN: Gets or sets whether tablespace commands are omitted.</para><para>JA: tablespace コマンドを省略するかどうかを取得または設定します。</para></summary>
    public bool NoTablespaces { get; set; }

    /// <summary><para>EN: Gets or sets whether every identifier is quoted.</para><para>JA: すべての識別子を引用するかどうかを取得または設定します。</para></summary>
    public bool QuoteAllIdentifiers { get; set; }

    /// <summary><para>EN: Gets or sets whether ownership is represented with SET SESSION AUTHORIZATION.</para><para>JA: 所有権を SET SESSION AUTHORIZATION で表現するかどうかを取得または設定します。</para></summary>
    public bool UseSetSessionAuthorization { get; set; }

    /// <summary><para>EN: Gets or sets whether publications are omitted.</para><para>JA: publication を省略するかどうかを取得または設定します。</para></summary>
    public bool NoPublications { get; set; }

    /// <summary><para>EN: Gets or sets whether role passwords are omitted.</para><para>JA: ロールパスワードを省略するかどうかを取得または設定します。</para></summary>
    public bool NoRolePasswords { get; set; }

    /// <summary><para>EN: Gets or sets whether security labels are omitted.</para><para>JA: セキュリティラベルを省略するかどうかを取得または設定します。</para></summary>
    public bool NoSecurityLabels { get; set; }

    /// <summary><para>EN: Gets or sets whether subscriptions are omitted.</para><para>JA: subscription を省略するかどうかを取得または設定します。</para></summary>
    public bool NoSubscriptions { get; set; }

    /// <summary><para>EN: Gets or sets whether safe output-file synchronization is skipped.</para><para>JA: 安全な出力ファイル同期を省略するかどうかを取得または設定します。</para></summary>
    public bool NoSync { get; set; }

    /// <summary><para>EN: Gets or sets whether unlogged relation data is omitted.</para><para>JA: unlogged relation データを省略するかどうかを取得または設定します。</para></summary>
    public bool NoUnloggedTableData { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 11+ partition-root loading.</para><para>JA: PostgreSQL 11 以降の partition root 経由ロードを取得または設定します。</para></summary>
    public bool LoadViaPartitionRoot { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 11+ comment suppression.</para><para>JA: PostgreSQL 11 以降のコメント抑止を取得または設定します。</para></summary>
    public bool NoComments { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 12+ extra_float_digits.</para><para>JA: PostgreSQL 12 以降の extra_float_digits を取得または設定します。</para></summary>
    public int? ExtraFloatDigits { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 12+ ON CONFLICT DO NOTHING for INSERT output.</para><para>JA: PostgreSQL 12 以降で INSERT 出力へ ON CONFLICT DO NOTHING を追加するかどうかを取得または設定します。</para></summary>
    public bool OnConflictDoNothing { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 12+ maximum rows per INSERT.</para><para>JA: PostgreSQL 12 以降の1 INSERT 当たり最大行数を取得または設定します。</para></summary>
    public int? RowsPerInsert { get; set; }

    /// <summary><para>EN: Gets or sets the patch-version-aware restrict key.</para><para>JA: パッチバージョン差を考慮した restrict key を取得または設定します。</para></summary>
    public PgDumpAllRestrictKey? RestrictKey { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 14+ TOAST compression suppression.</para><para>JA: PostgreSQL 14 以降の TOAST 圧縮指定抑止を取得または設定します。</para></summary>
    public bool NoToastCompression { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 15+ table-access-method suppression.</para><para>JA: PostgreSQL 15 以降の table access method 抑止を取得または設定します。</para></summary>
    public bool NoTableAccessMethod { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ data exclusion.</para><para>JA: PostgreSQL 18 以降のデータ除外を取得または設定します。</para></summary>
    public bool NoData { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ policy exclusion.</para><para>JA: PostgreSQL 18 以降のポリシー除外を取得または設定します。</para></summary>
    public bool NoPolicies { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ schema exclusion.</para><para>JA: PostgreSQL 18 以降のスキーマ除外を取得または設定します。</para></summary>
    public bool NoSchema { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ statistics exclusion.</para><para>JA: PostgreSQL 18 以降の統計情報除外を取得または設定します。</para></summary>
    public bool NoStatistics { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ optimizer-statistics inclusion.</para><para>JA: PostgreSQL 18 以降でオプティマイザ統計情報を含めるかどうかを取得または設定します。</para></summary>
    public bool Statistics { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ explicit sequence-data inclusion.</para><para>JA: PostgreSQL 18 以降で sequence data を明示的に含めるかどうかを取得または設定します。</para></summary>
    public bool SequenceData { get; set; }

    /// <summary><para>EN: Gets PostgreSQL 12+ repeatable database exclusion patterns.</para><para>JA: PostgreSQL 12 以降の複数指定可能なデータベース除外パターンを取得します。</para></summary>
    public IList<string> ExcludedDatabases { get; } = new List<string>();

    /// <summary><para>EN: Gets PostgreSQL 17+ repeatable database filter sources.</para><para>JA: PostgreSQL 17 以降の複数指定可能なデータベースフィルター入力元を取得します。</para></summary>
    public IList<PgDumpAllFilterSource> Filters { get; } =
        new List<PgDumpAllFilterSource>();

    /// <summary><para>EN: Gets caller-supplied environment variables passed to pg_dumpall. Values are not rendered into command-line diagnostics.</para><para>JA: pg_dumpall に渡す呼び出し側指定の環境変数を取得します。値はコマンドライン診断へ展開されません。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
