using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.ServerApplications;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Typed pg_rewind options for PostgreSQL 10-18. </para><para>JA: PostgreSQL 10〜18 の型付き pg_rewind オプションです。</para></summary>
public sealed class PgRewindOptions
{
    /// <summary><para>EN: Required target data directory; may be modified even during recovery preparation. </para><para>JA: 必須のターゲットデータディレクトリ。リカバリー準備でも変更される場合があります。</para></summary>
    public string? TargetDataDirectory { get; set; }
    /// <summary><para>EN: Offline source directory; exactly one source must be supplied. </para><para>JA: 停止中のソースディレクトリ。ソースは必ず1つだけ指定します。</para></summary>
    public string? SourceDataDirectory { get; set; }
    /// <summary><para>EN: Live source libpq connection string; exactly one source must be supplied. </para><para>JA: 稼働中ソースの libpq 接続文字列。ソースは必ず1つだけ指定します。</para></summary>
    public string? SourceConnectionString { get; set; }
    /// <summary><para>EN: Write standby recovery configuration; requires SourceConnectionString (PostgreSQL 13+). </para><para>JA: スタンバイ復旧設定を書き込みます。SourceConnectionString が必要です（PostgreSQL 13 以降）。</para></summary>
    public bool WriteRecoveryConfiguration { get; set; }
    /// <summary><para>EN: Skip automatic recovery to a clean shutdown (PostgreSQL 13+). </para><para>JA: クリーンな停止状態にする自動リカバリーを省略します（PostgreSQL 13 以降）。</para></summary>
    public bool NoEnsureShutdown { get; set; }
    /// <summary><para>EN: Target configuration file for recovery (PostgreSQL 15+). </para><para>JA: リカバリーに使用するターゲット設定ファイルです（PostgreSQL 15 以降）。</para></summary>
    public string? ConfigurationFile { get; set; }
    /// <summary><para>EN: Fetch missing target WAL through restore_command (PostgreSQL 13+). </para><para>JA: restore_command で不足したターゲット WAL を取得します（PostgreSQL 13 以降）。</para></summary>
    public bool RestoreTargetWal { get; set; }
    /// <summary><para>EN: Report rewind actions; automatic shutdown preparation may still write. </para><para>JA: 巻き戻し操作を表示します。自動停止準備は書き込みを行う場合があります。</para></summary>
    public bool DryRun { get; set; }
    /// <summary><para>EN: Skip durable synchronization (PostgreSQL 12+). </para><para>JA: 永続化の同期を省略します（PostgreSQL 12 以降）。</para></summary>
    public bool NoSync { get; set; }
    /// <summary><para>EN: Report progress. </para><para>JA: 進捗を表示します。</para></summary>
    public bool Progress { get; set; }
    /// <summary><para>EN: Enable detailed debug diagnostics. </para><para>JA: 詳細デバッグ診断を有効にします。</para></summary>
    public bool Debug { get; set; }
    /// <summary><para>EN: Filesystem sync method (PostgreSQL 17+); syncfs requires Linux. </para><para>JA: ファイルシステム同期方式です（PostgreSQL 17 以降）。syncfs は Linux が必要です。</para></summary>
    public PgFileSyncMethod? SyncMethod { get; set; }
    /// <summary><para>EN: Process-only environment overrides; omitted keys inherit the calling process. </para><para>JA: プロセス専用の環境変数上書きです。省略キーは呼び出し側プロセスを継承します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes pg_rewind directly. Cluster state and filesystem suitability remain upstream checks. </para><para>JA: pg_rewind を直接実行します。クラスタ状態とファイルシステム適合性は upstream が検証します。</para></summary>
public sealed partial class PgRewind
{
    private static readonly string[] HelpArguments = { "--help" };
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a wrapper with an explicit executable and expected CLI major version. </para><para>JA: 明示的な実行ファイルと期待する CLI メジャーバージョンでラッパーを作成します。</para></summary>
    public PgRewind(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal PgRewind(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner, "pg_rewind", PostgreSqlMajorVersion.V10);
    /// <summary><para>EN: Gets the explicit tool executable path. </para><para>JA: 明示的なツール実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets the expected CLI major version. </para><para>JA: 期待する CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates options before the version probe, then executes. Cancellation/timeout is best effort and does not roll back cluster mutations or guarantee a stopped server. </para><para>JA: バージョン確認前にオプションを検証して実行します。キャンセル／タイムアウトは最善努力で、クラスタ変更の取り消しやサーバー停止を保証しません。</para></summary>
    public async Task<PgServerResult> ExecuteAsync(PgRewindOptions options, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        options = PgCliSharp.Internal.Configuration.OptionsSnapshot.Copy(options);
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgRewindImplementation.Validate(options, Version);
        IReadOnlyList<string> arguments = PgRewindImplementation.Build(options, Version);
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
