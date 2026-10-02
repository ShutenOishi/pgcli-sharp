using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.ServerApplications;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Typed initdb options for PostgreSQL 10-18. </para><para>JA: PostgreSQL 10〜18 の型付き initdb オプションです。</para></summary>
public sealed class InitDbOptions
{
    /// <summary><para>EN: Cluster data directory; null uses PGDATA. </para><para>JA: クラスタデータディレクトリ。null は PGDATA を使用します。</para></summary>
    public string? DataDirectory { get; set; }
    /// <summary><para>EN: Template database encoding; null preserves the locale-derived default. </para><para>JA: テンプレートデータベースの文字コード。null はロケール由来の既定値を維持します。</para></summary>
    public string? Encoding { get; set; }
    /// <summary><para>EN: Default locale; null inherits the environment. </para><para>JA: 既定ロケール。null は環境を継承します。</para></summary>
    public string? Locale { get; set; }
    /// <summary><para>EN: Override the collate locale category. </para><para>JA: collate ロケールカテゴリを上書きします。</para></summary>
    public string? LcCollate { get; set; }
    /// <summary><para>EN: Override the ctype locale category. </para><para>JA: ctype ロケールカテゴリを上書きします。</para></summary>
    public string? LcCtype { get; set; }
    /// <summary><para>EN: Override the messages locale category. </para><para>JA: messages ロケールカテゴリを上書きします。</para></summary>
    public string? LcMessages { get; set; }
    /// <summary><para>EN: Override the monetary locale category. </para><para>JA: monetary ロケールカテゴリを上書きします。</para></summary>
    public string? LcMonetary { get; set; }
    /// <summary><para>EN: Override the numeric locale category. </para><para>JA: numeric ロケールカテゴリを上書きします。</para></summary>
    public string? LcNumeric { get; set; }
    /// <summary><para>EN: Override the time locale category. </para><para>JA: time ロケールカテゴリを上書きします。</para></summary>
    public string? LcTime { get; set; }
    /// <summary><para>EN: Use the C locale. Cannot be combined with Locale. </para><para>JA: C ロケールを使用します。Locale と併用できません。</para></summary>
    public bool NoLocale { get; set; }
    /// <summary><para>EN: Default text search configuration. </para><para>JA: 既定の全文検索設定です。</para></summary>
    public string? TextSearchConfiguration { get; set; }
    /// <summary><para>EN: Authentication policy for both local and host connections. </para><para>JA: ローカル接続とホスト接続共通の認証方針です。</para></summary>
    public PgInitDbAuthentication? Authentication { get; set; }
    /// <summary><para>EN: Override local authentication; peer is local-only. </para><para>JA: ローカル認証を上書きします。peer はローカル専用です。</para></summary>
    public PgInitDbAuthentication? LocalAuthentication { get; set; }
    /// <summary><para>EN: Override host authentication; ident/GSS/SSPI/cert are host-only. </para><para>JA: ホスト認証を上書きします。ident/GSS/SSPI/cert はホスト専用です。</para></summary>
    public PgInitDbAuthentication? HostAuthentication { get; set; }
    /// <summary><para>EN: Prompt for the bootstrap password; mutually exclusive with PasswordFile. </para><para>JA: 初期管理者パスワードの入力を要求します。PasswordFile と排他的です。</para></summary>
    public bool PasswordPrompt { get; set; }
    /// <summary><para>EN: Read the bootstrap password from the first line of this file. </para><para>JA: このファイルの最初の行から初期管理者パスワードを読みます。</para></summary>
    public string? PasswordFile { get; set; }
    /// <summary><para>EN: Bootstrap superuser; null uses the operating-system user. </para><para>JA: 初期管理者名。null は OS ユーザー名を使用します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Enable bootstrap debugging. </para><para>JA: 初期化のデバッグ出力を有効にします。</para></summary>
    public bool Debug { get; set; }
    /// <summary><para>EN: Show internal settings and exit without creating a cluster. </para><para>JA: 内部設定を表示し、クラスタを作成せず終了します。</para></summary>
    public bool ShowSettings { get; set; }
    /// <summary><para>EN: Retain incomplete files on failure. </para><para>JA: 失敗時に未完成ファイルを保持します。</para></summary>
    public bool NoClean { get; set; }
    /// <summary><para>EN: Skip durable synchronization; intended for disposable testing. </para><para>JA: 永続化の同期を省略します。破棄可能なテスト用途向けです。</para></summary>
    public bool NoSync { get; set; }
    /// <summary><para>EN: Suppress startup instructions (PostgreSQL 14+). </para><para>JA: 起動方法の案内を省略します（PostgreSQL 14 以降）。</para></summary>
    public bool NoInstructions { get; set; }
    /// <summary><para>EN: Only synchronize an existing cluster. </para><para>JA: 既存クラスタの同期のみ実行します。</para></summary>
    public bool SyncOnly { get; set; }
    /// <summary><para>EN: Absolute WAL directory. </para><para>JA: WAL の絶対ディレクトリパスです。</para></summary>
    public string? WalDirectory { get; set; }
    /// <summary><para>EN: WAL segment size: power of two, 1-1024 MiB (PostgreSQL 11+). </para><para>JA: WAL セグメントサイズ。1〜1024 MiB の2の累乗です（PostgreSQL 11 以降）。</para></summary>
    public int? WalSegmentSizeMegabytes { get; set; }
    /// <summary><para>EN: Explicit checksum policy. Null preserves upstream default: off through 17, on from 18. False omits the flag before 18. </para><para>JA: 明示的なチェックサム方針。null は17まで無効、18以降有効という既定値を維持します。18より前の false はフラグを省略します。</para></summary>
    public bool? DataChecksums { get; set; }
    /// <summary><para>EN: Allow group read access (PostgreSQL 11+, ignored on Windows). </para><para>JA: グループの読み取りアクセスを許可します（PostgreSQL 11 以降。Windows では無視）。</para></summary>
    public bool AllowGroupAccess { get; set; }
    /// <summary><para>EN: Enable deep bootstrap cache debugging (PostgreSQL 14+). </para><para>JA: 初期化キャッシュの詳細デバッグを有効にします（PostgreSQL 14 以降）。</para></summary>
    public bool DiscardCaches { get; set; }
    /// <summary><para>EN: Locale provider: libc/ICU from 15, builtin from 17. </para><para>JA: ロケールプロバイダー。libc/ICU は15以降、builtin は17以降です。</para></summary>
    public PgInitDbLocaleProvider? LocaleProvider { get; set; }
    /// <summary><para>EN: ICU locale identifier (PostgreSQL 15+). </para><para>JA: ICU ロケール識別子です（PostgreSQL 15 以降）。</para></summary>
    public string? IcuLocale { get; set; }
    /// <summary><para>EN: ICU collation rules (PostgreSQL 16+). </para><para>JA: ICU 照合規則です（PostgreSQL 16 以降）。</para></summary>
    public string? IcuRules { get; set; }
    /// <summary><para>EN: Builtin locale (PostgreSQL 17+); UnicodeFast requires 18. </para><para>JA: 組み込みロケールです（PostgreSQL 17 以降）。UnicodeFast は18以降です。</para></summary>
    public PgBuiltinLocale? BuiltinLocale { get; set; }
    /// <summary><para>EN: Ordered server settings, repeated as --set=name=value (PostgreSQL 16+). </para><para>JA: --set=name=value として繰り返す順序付きサーバー設定です（PostgreSQL 16 以降）。</para></summary>
    public IList<PgServerSetting> Settings { get; } = new List<PgServerSetting>();
    /// <summary><para>EN: Filesystem sync method (PostgreSQL 17+); syncfs requires Linux. </para><para>JA: ファイルシステム同期方式です（PostgreSQL 17 以降）。syncfs は Linux が必要です。</para></summary>
    public PgFileSyncMethod? SyncMethod { get; set; }
    /// <summary><para>EN: Skip syncing database data files (PostgreSQL 18+). </para><para>JA: データベースのデータファイル同期を省略します（PostgreSQL 18 以降）。</para></summary>
    public bool NoSyncDataFiles { get; set; }
    /// <summary><para>EN: Override bootstrap input-file location (-L). </para><para>JA: 初期化入力ファイルの場所を上書きします（-L）。</para></summary>
    public string? InputDirectory { get; set; }
    /// <summary><para>EN: Process-only environment overrides; omitted keys inherit the calling process. </para><para>JA: プロセス専用の環境変数上書きです。省略キーは呼び出し側プロセスを継承します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes initdb directly. Cluster state and filesystem suitability remain upstream checks. </para><para>JA: initdb を直接実行します。クラスタ状態とファイルシステム適合性は upstream が検証します。</para></summary>
public sealed class InitDb
{
    private static readonly string[] HelpArguments = { "--help" };
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a wrapper with an explicit executable and expected CLI major version. </para><para>JA: 明示的な実行ファイルと期待する CLI メジャーバージョンでラッパーを作成します。</para></summary>
    public InitDb(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal InitDb(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner, "initdb", PostgreSqlMajorVersion.V10);
    /// <summary><para>EN: Gets the explicit tool executable path. </para><para>JA: 明示的なツール実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets the expected CLI major version. </para><para>JA: 期待する CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates options before the version probe, then executes. Cancellation/timeout is best effort and does not roll back cluster mutations or guarantee a stopped server. </para><para>JA: バージョン確認前にオプションを検証して実行します。キャンセル／タイムアウトは最善努力で、クラスタ変更の取り消しやサーバー停止を保証しません。</para></summary>
    public async Task<PgServerResult> ExecuteAsync(InitDbOptions options, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        InitDbImplementation.Validate(options, Version);
        IReadOnlyList<string> arguments = InitDbImplementation.Build(options, Version);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io?.ToMaintenanceIo(), options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return new PgServerResult(info);
    }
    /// <summary><para>EN: Streams upstream help to caller-owned output after validating the executable version. </para><para>JA: 実行ファイルのバージョンを確認し、upstream のヘルプを呼び出し側所有の出力へ転送します。</para></summary>
    public async Task<PgServerResult> GetHelpAsync(PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        MaintenanceExecutionInfo info = await _executor.RunAsync(HelpArguments, io?.ToMaintenanceIo(), new Dictionary<string, string>(), timeout, cancellationToken).ConfigureAwait(false);
        return new PgServerResult(info);
    }
}
