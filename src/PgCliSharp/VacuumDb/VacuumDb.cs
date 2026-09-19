using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Vacuumdb;

namespace PgCliSharp;

/// <summary><para>EN: Typed vacuumdb options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き vacuumdb オプションです。</para></summary>
public sealed class VacuumDbOptions
{
    /// <summary><para>EN: Gets or sets database name/connection string.</para><para>JA: database 名/connection string を取得または設定します。</para></summary>
    public string? DatabaseName { get; set; }
    /// <summary><para>EN: Gets or sets host/socket directory.</para><para>JA: host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets port.</para><para>JA: port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets or sets password prompting policy.</para><para>JA: password prompt 方針を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Gets or sets SQL echo.</para><para>JA: SQL echo を取得または設定します。</para></summary>
    public bool Echo { get; set; }
    /// <summary><para>EN: Gets or sets quiet mode.</para><para>JA: quiet mode を取得または設定します。</para></summary>
    public bool Quiet { get; set; }
    /// <summary><para>EN: Gets or sets VACUUM followed by ANALYZE.</para><para>JA: VACUUM 後の ANALYZE を取得または設定します。</para></summary>
    public bool Analyze { get; set; }
    /// <summary><para>EN: Gets or sets ANALYZE-only mode.</para><para>JA: ANALYZE-only mode を取得または設定します。</para></summary>
    public bool AnalyzeOnly { get; set; }
    /// <summary><para>EN: Gets or sets FREEZE.</para><para>JA: FREEZE を取得または設定します。</para></summary>
    public bool Freeze { get; set; }
    /// <summary><para>EN: Gets or sets all-databases mode.</para><para>JA: all-databases mode を取得または設定します。</para></summary>
    public bool AllDatabases { get; set; }
    /// <summary><para>EN: Gets repeatable table selectors.</para><para>JA: 複数指定可能な table selector を取得します。</para></summary>
    public IList<string> Tables { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets FULL vacuum.</para><para>JA: FULL vacuum を取得または設定します。</para></summary>
    public bool Full { get; set; }
    /// <summary><para>EN: Gets or sets verbose vacuum output.</para><para>JA: verbose vacuum output を取得または設定します。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: Gets or sets number of database jobs; must be positive.</para><para>JA: database job 数を取得または設定します。正数である必要があります。</para></summary>
    public int? Jobs { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 13+ parallel vacuum workers.</para><para>JA: PostgreSQL 13 以降の parallel vacuum worker 数を取得または設定します。</para></summary>
    public int? ParallelWorkers { get; set; }
    /// <summary><para>EN: Gets PostgreSQL 16+ schema selectors.</para><para>JA: PostgreSQL 16 以降の schema selector を取得します。</para></summary>
    public IList<string> Schemas { get; } = new List<string>();
    /// <summary><para>EN: Gets PostgreSQL 16+ excluded schema selectors.</para><para>JA: PostgreSQL 16 以降の除外 schema selector を取得します。</para></summary>
    public IList<string> ExcludedSchemas { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets maintenance database for all-databases mode.</para><para>JA: all-databases mode 用 maintenance database を取得または設定します。</para></summary>
    public string? MaintenanceDatabase { get; set; }
    /// <summary><para>EN: Gets or sets staged analyze mode.</para><para>JA: staged analyze mode を取得または設定します。</para></summary>
    public bool AnalyzeInStages { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 12+ page-skipping disable.</para><para>JA: PostgreSQL 12 以降の page-skipping 無効化を取得または設定します。</para></summary>
    public bool DisablePageSkipping { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 12+ SKIP_LOCKED.</para><para>JA: PostgreSQL 12 以降の SKIP_LOCKED を取得または設定します。</para></summary>
    public bool SkipLocked { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 12+ minimum XID age.</para><para>JA: PostgreSQL 12 以降の minimum XID age を取得または設定します。</para></summary>
    public uint? MinXidAge { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 12+ minimum multixact age.</para><para>JA: PostgreSQL 12 以降の minimum multixact age を取得または設定します。</para></summary>
    public uint? MinMultiXactIdAge { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 14+ explicit index-cleanup policy.</para><para>JA: PostgreSQL 14 以降の明示的 index-cleanup 方針を取得または設定します。</para></summary>
    public PgVacuumIndexCleanup? IndexCleanup { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 14+ NO TRUNCATE.</para><para>JA: PostgreSQL 14 以降の NO TRUNCATE を取得または設定します。</para></summary>
    public bool NoTruncate { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 14+ NO PROCESS TOAST.</para><para>JA: PostgreSQL 14 以降の NO PROCESS TOAST を取得または設定します。</para></summary>
    public bool NoProcessToast { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 16+ NO PROCESS MAIN.</para><para>JA: PostgreSQL 16 以降の NO PROCESS MAIN を取得または設定します。</para></summary>
    public bool NoProcessMain { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 16+ buffer usage limit text accepted by vacuumdb.</para><para>JA: vacuumdb が受理する PostgreSQL 16 以降の buffer usage limit 文字列を取得または設定します。</para></summary>
    public string? BufferUsageLimit { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 18+ missing-stats-only mode.</para><para>JA: PostgreSQL 18 以降の missing-stats-only mode を取得または設定します。</para></summary>
    public bool MissingStatsOnly { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes vacuumdb.</para><para>JA: vacuumdb を実行します。</para></summary>
public sealed class VacuumDb
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a vacuumdb wrapper.</para><para>JA: vacuumdb wrapper を作成します。</para></summary>
    public VacuumDb(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal VacuumDb(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) => _executor = new MaintenanceExecutor(executablePath, version, runner);
    /// <summary><para>EN: Gets executable path.</para><para>JA: executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes vacuumdb.</para><para>JA: vacuumdb を検証して実行します。</para></summary>
    public async Task<PgMaintenanceResult> ExecuteAsync(VacuumDbOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        VacuumdbValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = VacuumdbArgumentBuilder.Build(options);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return MaintenanceExecutor.ToResult(info);
    }
}
