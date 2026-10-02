using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.ServerApplications;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Typed pg_upgrade options for PostgreSQL 10-18. </para><para>JA: PostgreSQL 10〜18 の型付き pg_upgrade オプションです。</para></summary>
public sealed class PgUpgradeOptions
{
    /// <summary><para>EN: Old cluster configuration directory; null uses PGDATAOLD. </para><para>JA: 旧クラスタ設定ディレクトリ。null は PGDATAOLD を使用します。</para></summary>
    public string? OldDataDirectory { get; set; }
    /// <summary><para>EN: New cluster configuration directory; null uses PGDATANEW. </para><para>JA: 新クラスタ設定ディレクトリ。null は PGDATANEW を使用します。</para></summary>
    public string? NewDataDirectory { get; set; }
    /// <summary><para>EN: Old executable directory; null uses PGBINOLD. </para><para>JA: 旧実行ファイルディレクトリ。null は PGBINOLD を使用します。</para></summary>
    public string? OldBinaryDirectory { get; set; }
    /// <summary><para>EN: New executable directory; null uses PGBINNEW or upstream executable directory. </para><para>JA: 新実行ファイルディレクトリ。null は PGBINNEW または upstream 実行ファイルのディレクトリです。</para></summary>
    public string? NewBinaryDirectory { get; set; }
    /// <summary><para>EN: Ordered upstream old-postgres option fragments; trusted input only. </para><para>JA: 旧 postgres に渡す順序付き upstream オプション断片です。信頼できる入力のみ指定してください。</para></summary>
    public IList<string> OldServerOptions { get; } = new List<string>();
    /// <summary><para>EN: Ordered upstream new-postgres option fragments; trusted input only. </para><para>JA: 新 postgres に渡す順序付き upstream オプション断片です。信頼できる入力のみ指定してください。</para></summary>
    public IList<string> NewServerOptions { get; } = new List<string>();
    /// <summary><para>EN: Old cluster port; null uses PGPORTOLD/default 50432. </para><para>JA: 旧クラスタのポート。null は PGPORTOLD／既定50432です。</para></summary>
    public int? OldPort { get; set; }
    /// <summary><para>EN: New cluster port; null uses PGPORTNEW/default 50432. </para><para>JA: 新クラスタのポート。null は PGPORTNEW／既定50432です。</para></summary>
    public int? NewPort { get; set; }
    /// <summary><para>EN: Cluster install user; null preserves PGUSER/upstream default. </para><para>JA: クラスタのインストールユーザー。null は PGUSER／upstream 既定値です。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Run compatibility checks without performing the upgrade. </para><para>JA: アップグレードを実施せず互換性検査を行います。</para></summary>
    public bool CheckOnly { get; set; }
    /// <summary><para>EN: File transfer policy. Null uses copying; Copy works before --copy was added in 16. Link/Swap can make the old cluster unusable. </para><para>JA: ファイル転送方針。null はコピーです。Copy は --copy 追加前の16より前でも利用できます。Link/Swap では旧クラスタが使用不能になることがあります。</para></summary>
    public PgUpgradeTransferMode? TransferMode { get; set; }
    /// <summary><para>EN: Retain SQL and log files after success. </para><para>JA: 成功後も SQL とログファイルを保持します。</para></summary>
    public bool Retain { get; set; }
    /// <summary><para>EN: Positive simultaneous job count. </para><para>JA: 正の並列ジョブ数です。</para></summary>
    public int? Jobs { get; set; }
    /// <summary><para>EN: Unix socket directory (PostgreSQL 12+). </para><para>JA: Unix ソケットディレクトリです（PostgreSQL 12 以降）。</para></summary>
    public string? SocketDirectory { get; set; }
    /// <summary><para>EN: Enable verbose upgrade diagnostics. </para><para>JA: アップグレードの詳細診断を有効にします。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: Skip durable synchronization (PostgreSQL 15+). </para><para>JA: 永続化の同期を省略します（PostgreSQL 15 以降）。</para></summary>
    public bool NoSync { get; set; }
    /// <summary><para>EN: Filesystem sync method (PostgreSQL 17+); syncfs requires Linux. </para><para>JA: ファイルシステム同期方式です（PostgreSQL 17 以降）。syncfs は Linux が必要です。</para></summary>
    public PgFileSyncMethod? SyncMethod { get; set; }
    /// <summary><para>EN: Do not transfer planner statistics (PostgreSQL 18+). </para><para>JA: プランナー統計を転送しません（PostgreSQL 18 以降）。</para></summary>
    public bool NoStatistics { get; set; }
    /// <summary><para>EN: Override cluster char signedness (PostgreSQL 18+). </para><para>JA: クラスタの char 符号を上書きします（PostgreSQL 18 以降）。</para></summary>
    public PgCharSignedness? CharSignedness { get; set; }
    /// <summary><para>EN: Process-only environment overrides; omitted keys inherit the calling process. </para><para>JA: プロセス専用の環境変数上書きです。省略キーは呼び出し側プロセスを継承します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes pg_upgrade directly. Cluster state and filesystem suitability remain upstream checks. </para><para>JA: pg_upgrade を直接実行します。クラスタ状態とファイルシステム適合性は upstream が検証します。</para></summary>
public sealed partial class PgUpgrade
{
    private static readonly string[] HelpArguments = { "--help" };
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a wrapper with an explicit executable and expected CLI major version. </para><para>JA: 明示的な実行ファイルと期待する CLI メジャーバージョンでラッパーを作成します。</para></summary>
    public PgUpgrade(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal PgUpgrade(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner, "pg_upgrade", PostgreSqlMajorVersion.V10);
    /// <summary><para>EN: Gets the explicit tool executable path. </para><para>JA: 明示的なツール実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets the expected CLI major version. </para><para>JA: 期待する CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates options before the version probe, then executes. Cancellation/timeout is best effort and does not roll back cluster mutations or guarantee a stopped server. </para><para>JA: バージョン確認前にオプションを検証して実行します。キャンセル／タイムアウトは最善努力で、クラスタ変更の取り消しやサーバー停止を保証しません。</para></summary>
    public async Task<PgServerResult> ExecuteAsync(PgUpgradeOptions options, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        options = PgCliSharp.Internal.Configuration.OptionsSnapshot.Copy(options);
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgUpgradeImplementation.Validate(options, Version);
        IReadOnlyList<string> arguments = PgUpgradeImplementation.Build(options, Version);
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
