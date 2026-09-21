using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.PgBench;

namespace PgCliSharp;

/// <summary><para>EN: pgbench query protocol.</para><para>JA: pgbench query protocol です。</para></summary>
public enum PgBenchProtocol
{
    /// <summary><para>EN: Simple query protocol.</para><para>JA: simple query protocol です。</para></summary>
    Simple,
    /// <summary><para>EN: Extended query protocol.</para><para>JA: extended query protocol です。</para></summary>
    Extended,
    /// <summary><para>EN: Extended protocol with prepared statements.</para><para>JA: prepared statement を用いる extended protocol です。</para></summary>
    Prepared,
}

/// <summary><para>EN: pgbench partitioning method.</para><para>JA: pgbench partitioning方式です。</para></summary>
public enum PgBenchPartitionMethod
{
    /// <summary><para>EN: Range partitioning.</para><para>JA: range partitioning です。</para></summary>
    Range,
    /// <summary><para>EN: Hash partitioning.</para><para>JA: hash partitioning です。</para></summary>
    Hash,
}

/// <summary><para>EN: One pgbench initialization step.</para><para>JA: pgbench initialization step を表します。</para></summary>
public enum PgBenchInitializationStep
{
    /// <summary><para>EN: Drop existing pgbench tables (d).</para><para>JA: 既存 pgbench table を drop します (d)。</para></summary>
    Drop,
    /// <summary><para>EN: Create tables (t).</para><para>JA: table を作成します (t)。</para></summary>
    CreateTables,
    /// <summary><para>EN: Generate data client-side (g).</para><para>JA: client-side で data を生成します (g)。</para></summary>
    GenerateClientSide,
    /// <summary><para>EN: Generate data server-side (G).</para><para>JA: server-side で data を生成します (G)。</para></summary>
    GenerateServerSide,
    /// <summary><para>EN: Vacuum tables (v).</para><para>JA: table を vacuum します (v)。</para></summary>
    Vacuum,
    /// <summary><para>EN: Create primary keys (p).</para><para>JA: primary key を作成します (p)。</para></summary>
    CreatePrimaryKeys,
    /// <summary><para>EN: Create foreign keys (f).</para><para>JA: foreign key を作成します (f)。</para></summary>
    CreateForeignKeys,
}

/// <summary><para>EN: Identifies a pgbench script source.</para><para>JA: pgbench script source の種類です。</para></summary>
public enum PgBenchScriptKind
{
    /// <summary><para>EN: Builtin script.</para><para>JA: builtin script です。</para></summary>
    Builtin,
    /// <summary><para>EN: Script file.</para><para>JA: script file です。</para></summary>
    File,
}

/// <summary><para>EN: Represents a weighted pgbench builtin or file script.</para><para>JA: weight 付き pgbench builtin/file script を表します。</para></summary>
public sealed class PgBenchScript
{
    private PgBenchScript(PgBenchScriptKind kind, string value, int weight)
    {
        Kind = kind;
        Value = value;
        Weight = weight;
    }

    /// <summary><para>EN: Gets script kind.</para><para>JA: script 種類を取得します。</para></summary>
    public PgBenchScriptKind Kind { get; }
    /// <summary><para>EN: Gets builtin name or file path.</para><para>JA: builtin 名または file path を取得します。</para></summary>
    public string Value { get; }
    /// <summary><para>EN: Gets selection weight. One is the upstream default.</para><para>JA: 選択 weight を取得します。1 は upstream 既定値です。</para></summary>
    public int Weight { get; }

    /// <summary><para>EN: Creates a builtin script selection.</para><para>JA: builtin script 選択を作成します。</para></summary>
    public static PgBenchScript Builtin(string name, int weight = 1)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        return new PgBenchScript(PgBenchScriptKind.Builtin, name, weight);
    }

    /// <summary><para>EN: Creates a file script selection.</para><para>JA: file script 選択を作成します。</para></summary>
    public static PgBenchScript File(string path, int weight = 1)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        return new PgBenchScript(PgBenchScriptKind.File, path, weight);
    }
}

/// <summary><para>EN: Represents a pgbench -D variable assignment.</para><para>JA: pgbench -D variable assignment を表します。</para></summary>
public sealed class PgBenchVariableAssignment
{
    /// <summary><para>EN: Creates a variable assignment.</para><para>JA: variable assignment を作成します。</para></summary>
    public PgBenchVariableAssignment(string name, string value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><para>EN: Gets variable name.</para><para>JA: variable 名を取得します。</para></summary>
    public string Name { get; }
    /// <summary><para>EN: Gets variable value.</para><para>JA: variable 値を取得します。</para></summary>
    public string Value { get; }
}

/// <summary><para>EN: pgbench random seed kind.</para><para>JA: pgbench random seed の種類です。</para></summary>
public enum PgBenchRandomSeedKind
{
    /// <summary><para>EN: Seed from current time.</para><para>JA: 現在時刻を seed にします。</para></summary>
    Time,
    /// <summary><para>EN: Seed from a strong random source.</para><para>JA: strong random source を seed にします。</para></summary>
    StrongRandom,
    /// <summary><para>EN: Use an explicit unsigned integer.</para><para>JA: 明示的な unsigned integer を使用します。</para></summary>
    Numeric,
}

/// <summary><para>EN: Typed pgbench random seed.</para><para>JA: 型付き pgbench random seed です。</para></summary>
public sealed class PgBenchRandomSeed
{
    private PgBenchRandomSeed(PgBenchRandomSeedKind kind, ulong numericValue)
    {
        Kind = kind;
        NumericValue = numericValue;
    }

    /// <summary><para>EN: Gets seed kind.</para><para>JA: seed 種類を取得します。</para></summary>
    public PgBenchRandomSeedKind Kind { get; }
    /// <summary><para>EN: Gets numeric seed when Kind is Numeric.</para><para>JA: Kind が Numeric の場合の数値 seed を取得します。</para></summary>
    public ulong NumericValue { get; }

    /// <summary><para>EN: Gets a time-based seed value.</para><para>JA: time-based seed 値を取得します。</para></summary>
    public static PgBenchRandomSeed Time { get; } = new PgBenchRandomSeed(PgBenchRandomSeedKind.Time, 0);
    /// <summary><para>EN: Gets a strong-random seed value.</para><para>JA: strong-random seed 値を取得します。</para></summary>
    public static PgBenchRandomSeed StrongRandom { get; } = new PgBenchRandomSeed(PgBenchRandomSeedKind.StrongRandom, 0);
    /// <summary><para>EN: Creates an explicit unsigned numeric seed.</para><para>JA: 明示的な unsigned 数値 seed を作成します。</para></summary>
    public static PgBenchRandomSeed Numeric(ulong value) => new PgBenchRandomSeed(PgBenchRandomSeedKind.Numeric, value);
}

/// <summary><para>EN: pgbench's documented process exit status.</para><para>JA: pgbench が定義する process 終了 status です。</para></summary>
public enum PgBenchExitStatus
{
    /// <summary><para>EN: Successful run.</para><para>JA: 正常終了です。</para></summary>
    Success = 0,
    /// <summary><para>EN: Static, startup, or internal error.</para><para>JA: static/startup/internal error です。</para></summary>
    StartupOrStaticError = 1,
    /// <summary><para>EN: Error during benchmark/script execution.</para><para>JA: benchmark/script 実行中の error です。</para></summary>
    RuntimeError = 2,
}

/// <summary><para>EN: Typed pgbench options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き pgbench オプションです。</para></summary>
public sealed class PgBenchOptions
{
    /// <summary><para>EN: Gets or sets database name. Null preserves environment/user fallback.</para><para>JA: database 名を取得または設定します。null は environment/user fallback を維持します。</para></summary>
    public string? Database { get; set; }
    /// <summary><para>EN: Gets or sets host/socket directory.</para><para>JA: host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets port.</para><para>JA: port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets ordered builtin/file scripts.</para><para>JA: 順序付き builtin/file script を取得します。</para></summary>
    public IList<PgBenchScript> Scripts { get; } = new List<PgBenchScript>();
    /// <summary><para>EN: Gets repeatable -D variable assignments.</para><para>JA: 繰り返し指定可能な -D variable assignment を取得します。</para></summary>
    public IList<PgBenchVariableAssignment> Variables { get; } = new List<PgBenchVariableAssignment>();
    /// <summary><para>EN: Gets ordered PostgreSQL 11+ initialization steps.</para><para>JA: PostgreSQL 11 以降の順序付き initialization step を取得します。</para></summary>
    public IList<PgBenchInitializationStep> InitializationSteps { get; } = new List<PgBenchInitializationStep>();
    /// <summary><para>EN: Gets or sets initialization mode.</para><para>JA: initialization mode を取得または設定します。</para></summary>
    public bool Initialize { get; set; }
    /// <summary><para>EN: Gets or sets client count.</para><para>JA: client 数を取得または設定します。</para></summary>
    public int? Clients { get; set; }
    /// <summary><para>EN: Gets or sets connection-per-transaction mode.</para><para>JA: transaction ごとに接続する mode を取得または設定します。</para></summary>
    public bool ConnectPerTransaction { get; set; }
    /// <summary><para>EN: Gets or sets debug output.</para><para>JA: debug 出力を取得または設定します。</para></summary>
    public bool Debug { get; set; }
    /// <summary><para>EN: Gets or sets initialization fillfactor.</para><para>JA: initialization fillfactor を取得または設定します。</para></summary>
    public int? FillFactor { get; set; }
    /// <summary><para>EN: Gets or sets worker thread count.</para><para>JA: worker thread 数を取得または設定します。</para></summary>
    public int? Jobs { get; set; }
    /// <summary><para>EN: Gets or sets transaction logging.</para><para>JA: transaction logging を取得または設定します。</para></summary>
    public bool LogTransactions { get; set; }
    /// <summary><para>EN: Gets or sets latency limit in milliseconds.</para><para>JA: latency limit を millisecond 単位で取得または設定します。</para></summary>
    public decimal? LatencyLimitMilliseconds { get; set; }
    /// <summary><para>EN: Gets or sets no-vacuum mode.</para><para>JA: no-vacuum mode を取得または設定します。</para></summary>
    public bool NoVacuum { get; set; }
    /// <summary><para>EN: Gets or sets progress interval in seconds.</para><para>JA: progress interval を秒単位で取得または設定します。</para></summary>
    public int? ProgressSeconds { get; set; }
    /// <summary><para>EN: Gets or sets query protocol.</para><para>JA: query protocol を取得または設定します。</para></summary>
    public PgBenchProtocol? Protocol { get; set; }
    /// <summary><para>EN: Gets or sets quiet mode.</para><para>JA: quiet mode を取得または設定します。</para></summary>
    public bool Quiet { get; set; }
    /// <summary><para>EN: Gets or sets per-command reporting. PgCliSharp chooses the version-canonical long spelling.</para><para>JA: command ごとの report を取得または設定します。PgCliSharp が version に応じた canonical long spelling を選択します。</para></summary>
    public bool ReportPerCommand { get; set; }
    /// <summary><para>EN: Gets or sets target transactions per second.</para><para>JA: 目標 transaction/second を取得または設定します。</para></summary>
    public decimal? Rate { get; set; }
    /// <summary><para>EN: Gets or sets scale factor.</para><para>JA: scale factor を取得または設定します。</para></summary>
    public int? Scale { get; set; }
    /// <summary><para>EN: Gets or sets the select-only builtin shorthand.</para><para>JA: select-only builtin shorthand を取得または設定します。</para></summary>
    public bool SelectOnly { get; set; }
    /// <summary><para>EN: Gets or sets the simple-update builtin shorthand.</para><para>JA: simple-update builtin shorthand を取得または設定します。</para></summary>
    public bool SkipSomeUpdates { get; set; }
    /// <summary><para>EN: Gets or sets benchmark duration in seconds.</para><para>JA: benchmark duration を秒単位で取得または設定します。</para></summary>
    public int? DurationSeconds { get; set; }
    /// <summary><para>EN: Gets or sets transactions per client.</para><para>JA: client ごとの transaction 数を取得または設定します。</para></summary>
    public int? TransactionsPerClient { get; set; }
    /// <summary><para>EN: Gets or sets vacuum-all before benchmark.</para><para>JA: benchmark 前の vacuum-all を取得または設定します。</para></summary>
    public bool VacuumAll { get; set; }
    /// <summary><para>EN: Gets or sets unlogged-table initialization.</para><para>JA: unlogged table initialization を取得または設定します。</para></summary>
    public bool UnloggedTables { get; set; }
    /// <summary><para>EN: Gets or sets table tablespace.</para><para>JA: table tablespace を取得または設定します。</para></summary>
    public string? Tablespace { get; set; }
    /// <summary><para>EN: Gets or sets index tablespace.</para><para>JA: index tablespace を取得または設定します。</para></summary>
    public string? IndexTablespace { get; set; }
    /// <summary><para>EN: Gets or sets log sampling rate in (0,1].</para><para>JA: log sampling rate を (0,1] で取得または設定します。</para></summary>
    public double? SamplingRate { get; set; }
    /// <summary><para>EN: Gets or sets aggregate-log interval in seconds.</para><para>JA: aggregate log interval を秒単位で取得または設定します。</para></summary>
    public int? AggregateIntervalSeconds { get; set; }
    /// <summary><para>EN: Gets or sets timestamped progress output.</para><para>JA: timestamp 付き progress 出力を取得または設定します。</para></summary>
    public bool ProgressTimestamp { get; set; }
    /// <summary><para>EN: Gets or sets log-file prefix.</para><para>JA: log file prefix を取得または設定します。</para></summary>
    public string? LogPrefix { get; set; }
    /// <summary><para>EN: Gets or sets foreign-key creation.</para><para>JA: foreign key 作成を取得または設定します。</para></summary>
    public bool ForeignKeys { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 11+ random seed.</para><para>JA: PostgreSQL 11 以降の random seed を取得または設定します。</para></summary>
    public PgBenchRandomSeed? RandomSeed { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 13+ builtin script to show and exit.</para><para>JA: PostgreSQL 13 以降で表示して終了する builtin script を取得または設定します。</para></summary>
    public string? ShowScript { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 13+ partition count.</para><para>JA: PostgreSQL 13 以降の partition 数を取得または設定します。</para></summary>
    public int? Partitions { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 13+ partition method.</para><para>JA: PostgreSQL 13 以降の partition method を取得または設定します。</para></summary>
    public PgBenchPartitionMethod? PartitionMethod { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 15+ detailed failure reporting.</para><para>JA: PostgreSQL 15 以降の詳細 failure report を取得または設定します。</para></summary>
    public bool FailuresDetailed { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 15+ maximum retry attempts; zero means unlimited subject to upstream limits.</para><para>JA: PostgreSQL 15 以降の最大 retry 回数を取得または設定します。0 は upstream 制約下で無制限を表します。</para></summary>
    public int? MaxTries { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 15+ verbose error reporting.</para><para>JA: PostgreSQL 15 以降の verbose error report を取得または設定します。</para></summary>
    public bool VerboseErrors { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 17+ immediate exit when a client aborts.</para><para>JA: PostgreSQL 17 以降で client abort 時に即終了するか取得または設定します。</para></summary>
    public bool ExitOnAbort { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Result of one pgbench execution.</para><para>JA: 1 回の pgbench 実行結果です。</para></summary>
public sealed class PgBenchResult : PgMaintenanceResult
{
    internal PgBenchResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError, PgBenchExitStatus status)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) => Status = status;

    /// <summary><para>EN: Gets pgbench's documented exit status.</para><para>JA: pgbench が定義する終了 status を取得します。</para></summary>
    public PgBenchExitStatus Status { get; }
}

/// <summary><para>EN: Executes pgbench with typed PostgreSQL 10-18 options.</para><para>JA: PostgreSQL 10〜18 の型付きオプションで pgbench を実行します。</para></summary>
public sealed class PgBench
{
    private readonly MaintenanceExecutor _executor;

    /// <summary><para>EN: Creates a pgbench wrapper for an explicit executable path/version.</para><para>JA: 明示的な executable path/version の pgbench wrapper を作成します。</para></summary>
    public PgBench(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }

    internal PgBench(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner);

    /// <summary><para>EN: Gets selected executable path.</para><para>JA: 選択した executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected PostgreSQL CLI major version.</para><para>JA: 期待する PostgreSQL CLI major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;

    /// <summary><para>EN: Validates and executes pgbench. Known pgbench exit codes 0-2 are returned as typed status.</para><para>JA: pgbench を検証して実行します。既知の pgbench 終了コード 0〜2 は型付き status として返します。</para></summary>
    public async Task<PgBenchResult> ExecuteAsync(PgBenchOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgBenchValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = PgBenchArgumentBuilder.Build(options, Version);
        MaintenanceExecutionInfo info = await _executor.RunAsync(
            arguments,
            io,
            options.EnvironmentVariables,
            timeout,
            cancellationToken,
            throwOnNonZeroExitCode: false).ConfigureAwait(false);

        if (info.Process.ExitCode < 0 || info.Process.ExitCode > 2)
            throw new PgProcessExecutionException(ExecutablePath, info.Process.ExitCode, info.Process.StandardError);

        return new PgBenchResult(
            info.Process.ExitCode,
            info.Process.Duration,
            info.ExecutableVersion.NumericVersion,
            info.ExecutableVersion.RawVersion,
            info.Process.StandardError,
            (PgBenchExitStatus)info.Process.ExitCode);
    }
}
