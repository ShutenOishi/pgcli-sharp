using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.ServerApplications;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Typed pg_checksums options for PostgreSQL 12-18. </para><para>JA: PostgreSQL 12〜18 の型付き pg_checksums オプションです。</para></summary>
public sealed class PgChecksumsOptions
{
    /// <summary><para>EN: Cleanly shut down cluster directory; null uses PGDATA. </para><para>JA: クリーンに停止したクラスタのディレクトリ。null は PGDATA を使用します。</para></summary>
    public string? DataDirectory { get; set; }
    /// <summary><para>EN: Check (default), enable or disable checksums; enable/disable modify files. </para><para>JA: チェックサムの検査（既定）・有効化・無効化です。有効化／無効化はファイルを変更します。</para></summary>
    public PgChecksumsMode Mode { get; set; }
    /// <summary><para>EN: Check only this nonnegative filenode (up to Int32.MaxValue); zero requires PostgreSQL 15+; check mode only. </para><para>JA: 0〜Int32.MaxValue のこの filenode のみ検査します。0は PostgreSQL 15以降で、検査モード専用です。</para></summary>
    public int? FileNode { get; set; }
    /// <summary><para>EN: Skip durable synchronization when enabling/disabling. </para><para>JA: 有効化／無効化時の永続化同期を省略します。</para></summary>
    public bool NoSync { get; set; }
    /// <summary><para>EN: Report progress. </para><para>JA: 進捗を表示します。</para></summary>
    public bool Progress { get; set; }
    /// <summary><para>EN: Report each scanned file. </para><para>JA: 検査した各ファイルを表示します。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: Filesystem sync method (PostgreSQL 17+); syncfs requires Linux. </para><para>JA: ファイルシステム同期方式です（PostgreSQL 17 以降）。syncfs は Linux が必要です。</para></summary>
    public PgFileSyncMethod? SyncMethod { get; set; }
    /// <summary><para>EN: Process-only environment overrides; omitted keys inherit the calling process. </para><para>JA: プロセス専用の環境変数上書きです。省略キーは呼び出し側プロセスを継承します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes pg_checksums directly. Cluster state and filesystem suitability remain upstream checks. </para><para>JA: pg_checksums を直接実行します。クラスタ状態とファイルシステム適合性は upstream が検証します。</para></summary>
public sealed class PgChecksums
{
    private static readonly string[] HelpArguments = { "--help" };
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a wrapper with an explicit executable and expected CLI major version. </para><para>JA: 明示的な実行ファイルと期待する CLI メジャーバージョンでラッパーを作成します。</para></summary>
    public PgChecksums(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal PgChecksums(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner, "pg_checksums", PostgreSqlMajorVersion.V12);
    /// <summary><para>EN: Gets the explicit tool executable path. </para><para>JA: 明示的なツール実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets the expected CLI major version. </para><para>JA: 期待する CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates options before the version probe, then executes. Cancellation/timeout is best effort and does not roll back cluster mutations or guarantee a stopped server. </para><para>JA: バージョン確認前にオプションを検証して実行します。キャンセル／タイムアウトは最善努力で、クラスタ変更の取り消しやサーバー停止を保証しません。</para></summary>
    public async Task<PgServerResult> ExecuteAsync(PgChecksumsOptions options, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgChecksumsImplementation.Validate(options, Version);
        IReadOnlyList<string> arguments = PgChecksumsImplementation.Build(options, Version);
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
