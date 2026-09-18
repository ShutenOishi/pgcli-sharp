namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents the typed union of pg_restore options supported by PgCliSharp for PostgreSQL 10 through 18.</para>
/// <para>JA: PgCliSharp が PostgreSQL 10〜18 向けにサポートする pg_restore オプションの型付き union を表します。</para>
/// </summary>
public sealed class PgRestoreOptions
{
    /// <summary><para>EN: Gets or sets the operation mode.</para><para>JA: 操作モードを取得または設定します。</para></summary>
    public PgRestoreMode Mode { get; set; } = PgRestoreMode.Restore;

    /// <summary><para>EN: Gets or sets the mutually exclusive primary content mode.</para><para>JA: 排他的な主要復元内容モードを取得または設定します。</para></summary>
    public PgRestoreContentMode ContentMode { get; set; } = PgRestoreContentMode.All;

    /// <summary><para>EN: Gets or sets whether objects are dropped before restore.</para><para>JA: 復元前にオブジェクトを削除するかどうかを取得または設定します。</para></summary>
    public bool Clean { get; set; }

    /// <summary><para>EN: Gets or sets whether the archived database is created before restoring.</para><para>JA: 復元前にアーカイブ元のデータベースを作成するかどうかを取得または設定します。</para></summary>
    public bool Create { get; set; }

    /// <summary><para>EN: Gets or sets whether restore stops on the first SQL error.</para><para>JA: 最初の SQL エラーで復元を停止するかどうかを取得または設定します。</para></summary>
    public bool ExitOnError { get; set; }

    /// <summary><para>EN: Gets or sets an explicit archive format; null enables automatic detection.</para><para>JA: 明示的なアーカイブ形式を取得または設定します。null では自動判定します。</para></summary>
    public PgRestoreArchiveFormat? ArchiveFormat { get; set; }

    /// <summary><para>EN: Gets or sets the number of parallel restore jobs.</para><para>JA: 並列復元ジョブ数を取得または設定します。</para></summary>
    public int? Jobs { get; set; }

    /// <summary><para>EN: Gets or sets whether access privileges are omitted.</para><para>JA: アクセス権限の復元を省略するかどうかを取得または設定します。</para></summary>
    public bool NoPrivileges { get; set; }

    /// <summary><para>EN: Gets or sets whether ownership commands are omitted.</para><para>JA: 所有者設定コマンドを省略するかどうかを取得または設定します。</para></summary>
    public bool NoOwner { get; set; }

    /// <summary><para>EN: Gets or sets the obsolete but accepted --no-reconnect compatibility no-op.</para><para>JA: 廃止済みですが互換性のため受理される --no-reconnect の no-op を取得または設定します。</para></summary>
    public bool NoReconnect { get; set; }

    /// <summary><para>EN: Gets or sets the superuser name used when disabling triggers.</para><para>JA: トリガー無効化時に使用するスーパーユーザー名を取得または設定します。</para></summary>
    public string? Superuser { get; set; }

    /// <summary><para>EN: Gets or sets how many times -v/--verbose is emitted.</para><para>JA: -v/--verbose を出力する回数を取得または設定します。</para></summary>
    public int Verbosity { get; set; }

    /// <summary><para>EN: Gets or sets the transaction strategy. Null uses pg_restore defaults.</para><para>JA: トランザクション方式を取得または設定します。null では pg_restore の既定動作を使用します。</para></summary>
    public PgRestoreTransactionMode? TransactionMode { get; set; }

    /// <summary><para>EN: Gets or sets whether triggers are disabled while restoring data.</para><para>JA: データ復元中にトリガーを無効化するかどうかを取得または設定します。</para></summary>
    public bool DisableTriggers { get; set; }

    /// <summary><para>EN: Gets or sets whether row security is enabled while restoring data.</para><para>JA: データ復元中に行セキュリティを有効化するかどうかを取得または設定します。</para></summary>
    public bool EnableRowSecurity { get; set; }

    /// <summary><para>EN: Gets or sets whether DROP commands use IF EXISTS. Requires Clean.</para><para>JA: DROP コマンドで IF EXISTS を使用するかどうかを取得または設定します。Clean が必要です。</para></summary>
    public bool IfExists { get; set; }

    /// <summary><para>EN: Gets or sets whether table data is skipped after a table-creation failure.</para><para>JA: テーブル作成に失敗した場合、そのテーブルデータを復元しないかどうかを取得または設定します。</para></summary>
    public bool NoDataForFailedTables { get; set; }

    /// <summary><para>EN: Gets or sets whether tablespace-selection commands are omitted.</para><para>JA: tablespace 選択コマンドを省略するかどうかを取得または設定します。</para></summary>
    public bool NoTablespaces { get; set; }

    /// <summary><para>EN: Gets or sets whether SET SESSION AUTHORIZATION is used instead of ALTER OWNER.</para><para>JA: ALTER OWNER の代わりに SET SESSION AUTHORIZATION を使用するかどうかを取得または設定します。</para></summary>
    public bool UseSetSessionAuthorization { get; set; }

    /// <summary><para>EN: Gets or sets whether schema/table selectors must match at least one object.</para><para>JA: schema/table 選択条件が少なくとも1つのオブジェクトに一致する必要があるかどうかを取得または設定します。</para></summary>
    public bool StrictNames { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 11+ comment suppression.</para><para>JA: PostgreSQL 11 以降でコメント復元を抑止するかどうかを取得または設定します。</para></summary>
    public bool NoComments { get; set; }

    /// <summary><para>EN: Gets or sets whether publications are omitted.</para><para>JA: publication を省略するかどうかを取得または設定します。</para></summary>
    public bool NoPublications { get; set; }

    /// <summary><para>EN: Gets or sets whether security labels are omitted.</para><para>JA: セキュリティラベルを省略するかどうかを取得または設定します。</para></summary>
    public bool NoSecurityLabels { get; set; }

    /// <summary><para>EN: Gets or sets whether subscriptions are omitted.</para><para>JA: subscription を省略するかどうかを取得または設定します。</para></summary>
    public bool NoSubscriptions { get; set; }

    /// <summary><para>EN: Gets or sets the patch-version-aware restrict key for generated SQL scripts.</para><para>JA: 生成 SQL スクリプト用の、パッチバージョン差を考慮した restrict key を取得または設定します。</para></summary>
    public PgRestoreRestrictKey? RestrictKey { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 15+ table-access-method suppression.</para><para>JA: PostgreSQL 15 以降で table access method の復元を抑止するかどうかを取得または設定します。</para></summary>
    public bool NoTableAccessMethod { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ data exclusion.</para><para>JA: PostgreSQL 18 以降でデータを除外するかどうかを取得または設定します。</para></summary>
    public bool NoData { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ policy exclusion.</para><para>JA: PostgreSQL 18 以降でポリシーを除外するかどうかを取得または設定します。</para></summary>
    public bool NoPolicies { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ schema exclusion.</para><para>JA: PostgreSQL 18 以降でスキーマを除外するかどうかを取得または設定します。</para></summary>
    public bool NoSchema { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ statistics exclusion.</para><para>JA: PostgreSQL 18 以降で統計情報を除外するかどうかを取得または設定します。</para></summary>
    public bool NoStatistics { get; set; }

    /// <summary><para>EN: Gets or sets PostgreSQL 18+ explicit statistics restoration in addition to normal content.</para><para>JA: PostgreSQL 18 以降で通常の内容に加えて統計情報を明示的に復元するかどうかを取得または設定します。</para></summary>
    public bool Statistics { get; set; }

    /// <summary><para>EN: Gets schema include selectors.</para><para>JA: schema include 選択条件を取得します。</para></summary>
    public IList<string> Schemas { get; } = new List<string>();

    /// <summary><para>EN: Gets schema exclude selectors.</para><para>JA: schema exclude 選択条件を取得します。</para></summary>
    public IList<string> ExcludedSchemas { get; } = new List<string>();

    /// <summary><para>EN: Gets exact function selectors.</para><para>JA: function の完全一致選択条件を取得します。</para></summary>
    public IList<string> Functions { get; } = new List<string>();

    /// <summary><para>EN: Gets exact index selectors.</para><para>JA: index の完全一致選択条件を取得します。</para></summary>
    public IList<string> Indexes { get; } = new List<string>();

    /// <summary><para>EN: Gets exact table selectors.</para><para>JA: table の完全一致選択条件を取得します。</para></summary>
    public IList<string> Tables { get; } = new List<string>();

    /// <summary><para>EN: Gets exact trigger selectors.</para><para>JA: trigger の完全一致選択条件を取得します。</para></summary>
    public IList<string> Triggers { get; } = new List<string>();

    /// <summary><para>EN: Gets repeatable archive sections.</para><para>JA: 複数指定可能なアーカイブセクションを取得します。</para></summary>
    public IList<PgRestoreSection> Sections { get; } = new List<PgRestoreSection>();

    /// <summary><para>EN: Gets or sets a TOC list file used by -L/--use-list.</para><para>JA: -L/--use-list で使用する TOC リストファイルを取得または設定します。</para></summary>
    public string? UseListFile { get; set; }

    /// <summary><para>EN: Gets PostgreSQL 17+ repeatable filter sources.</para><para>JA: PostgreSQL 17 以降の複数指定可能なフィルター入力元を取得します。</para></summary>
    public IList<PgRestoreFilterSource> Filters { get; } = new List<PgRestoreFilterSource>();

    /// <summary><para>EN: Gets or sets the database server host or Unix-domain socket directory.</para><para>JA: データベースサーバーのホストまたは Unix ドメインソケットディレクトリを取得または設定します。</para></summary>
    public string? Host { get; set; }

    /// <summary><para>EN: Gets or sets the database server port.</para><para>JA: データベースサーバーのポートを取得または設定します。</para></summary>
    public int? Port { get; set; }

    /// <summary><para>EN: Gets or sets the database user name.</para><para>JA: データベースユーザー名を取得または設定します。</para></summary>
    public string? Username { get; set; }

    /// <summary><para>EN: Gets or sets password-prompt behavior.</para><para>JA: パスワードプロンプト動作を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;

    /// <summary><para>EN: Gets or sets the role applied with SET ROLE after connecting.</para><para>JA: 接続後に SET ROLE で適用するロールを取得または設定します。</para></summary>
    public string? Role { get; set; }

    /// <summary><para>EN: Gets caller-supplied environment variables passed to pg_restore. Values are not rendered into command-line diagnostics.</para><para>JA: pg_restore に渡す呼び出し側指定の環境変数を取得します。値はコマンドライン診断へ展開されません。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
