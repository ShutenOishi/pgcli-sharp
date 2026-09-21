using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.PgAmcheck;

namespace PgCliSharp;

/// <summary><para>EN: Typed pg_amcheck options for PostgreSQL 14-18.</para><para>JA: PostgreSQL 14〜18 の型付き pg_amcheck オプションです。</para></summary>
public sealed class PgAmcheckOptions
{
    /// <summary><para>EN: Gets or sets a single positional database name/connection string.</para><para>JA: 単一の positional database 名/connection string を取得または設定します。</para></summary>
    public string? DatabaseName { get; set; }
    /// <summary><para>EN: Gets or sets host/socket directory.</para><para>JA: host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets port.</para><para>JA: port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets or sets password prompting policy.</para><para>JA: password prompt 方針を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Gets or sets maintenance database.</para><para>JA: maintenance database を取得または設定します。</para></summary>
    public string? MaintenanceDatabase { get; set; }
    /// <summary><para>EN: Gets or sets all-databases mode.</para><para>JA: all-databases mode を取得または設定します。</para></summary>
    public bool AllDatabases { get; set; }
    /// <summary><para>EN: Gets database include patterns.</para><para>JA: database include pattern を取得します。</para></summary>
    public IList<string> DatabasePatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets database exclude patterns.</para><para>JA: database exclude pattern を取得します。</para></summary>
    public IList<string> ExcludedDatabasePatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets SQL echo.</para><para>JA: SQL echo を取得または設定します。</para></summary>
    public bool Echo { get; set; }
    /// <summary><para>EN: Gets btree-index include patterns.</para><para>JA: btree index include pattern を取得します。</para></summary>
    public IList<string> IndexPatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets btree-index exclude patterns.</para><para>JA: btree index exclude pattern を取得します。</para></summary>
    public IList<string> ExcludedIndexPatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets parallel job count; must be positive.</para><para>JA: parallel job 数を取得または設定します。正数である必要があります。</para></summary>
    public int? Jobs { get; set; }
    /// <summary><para>EN: Gets or sets progress reporting.</para><para>JA: progress reporting を取得または設定します。</para></summary>
    public bool Progress { get; set; }
    /// <summary><para>EN: Gets relation include patterns.</para><para>JA: relation include pattern を取得します。</para></summary>
    public IList<string> RelationPatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets relation exclude patterns.</para><para>JA: relation exclude pattern を取得します。</para></summary>
    public IList<string> ExcludedRelationPatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets schema include patterns.</para><para>JA: schema include pattern を取得します。</para></summary>
    public IList<string> SchemaPatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets schema exclude patterns.</para><para>JA: schema exclude pattern を取得します。</para></summary>
    public IList<string> ExcludedSchemaPatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets heap-table include patterns.</para><para>JA: heap table include pattern を取得します。</para></summary>
    public IList<string> TablePatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets heap-table exclude patterns.</para><para>JA: heap table exclude pattern を取得します。</para></summary>
    public IList<string> ExcludedTablePatterns { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets verbose logging.</para><para>JA: verbose logging を取得または設定します。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: Gets or sets --no-dependent-indexes.</para><para>JA: --no-dependent-indexes を取得または設定します。</para></summary>
    public bool NoDependentIndexes { get; set; }
    /// <summary><para>EN: Gets or sets --no-dependent-toast.</para><para>JA: --no-dependent-toast を取得または設定します。</para></summary>
    public bool NoDependentToast { get; set; }
    /// <summary><para>EN: Gets or sets --exclude-toast-pointers.</para><para>JA: --exclude-toast-pointers を取得または設定します。</para></summary>
    public bool ExcludeToastPointers { get; set; }
    /// <summary><para>EN: Gets or sets stop-on-first-corruption behavior.</para><para>JA: 最初の corruption で停止する動作を取得または設定します。</para></summary>
    public bool OnErrorStop { get; set; }
    /// <summary><para>EN: Gets or sets heap page skip policy.</para><para>JA: heap page skip 方針を取得または設定します。</para></summary>
    public PgAmcheckSkipMode? Skip { get; set; }
    /// <summary><para>EN: Gets or sets first heap block to check.</para><para>JA: 最初に検査する heap block を取得または設定します。</para></summary>
    public uint? StartBlock { get; set; }
    /// <summary><para>EN: Gets or sets last heap block to check.</para><para>JA: 最後に検査する heap block を取得または設定します。</para></summary>
    public uint? EndBlock { get; set; }
    /// <summary><para>EN: Gets or sets root-descend btree checking.</para><para>JA: root-descend btree check を取得または設定します。</para></summary>
    public bool RootDescend { get; set; }
    /// <summary><para>EN: Gets or sets strict include-pattern matching. False emits --no-strict-names.</para><para>JA: include pattern の厳密一致を取得または設定します。false は --no-strict-names を出力します。</para></summary>
    public bool StrictNames { get; set; } = true;
    /// <summary><para>EN: Gets or sets heapallindexed btree checking.</para><para>JA: heapallindexed btree check を取得または設定します。</para></summary>
    public bool HeapAllIndexed { get; set; }
    /// <summary><para>EN: Gets or sets parent-check btree checking.</para><para>JA: parent-check btree check を取得または設定します。</para></summary>
    public bool ParentCheck { get; set; }
    /// <summary><para>EN: Gets or sets optional installation of missing amcheck extension/functions.</para><para>JA: 不足している amcheck extension/function の任意 install を取得または設定します。</para></summary>
    public PgAmcheckInstallMissing? InstallMissing { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 17+ unique-index checking.</para><para>JA: PostgreSQL 17 以降の unique-index check を取得または設定します。</para></summary>
    public bool CheckUnique { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes PostgreSQL 14+ pg_amcheck.</para><para>JA: PostgreSQL 14 以降の pg_amcheck を実行します。</para></summary>
public sealed class PgAmcheck
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a pg_amcheck wrapper and rejects PostgreSQL 10-13 before process startup.</para><para>JA: pg_amcheck wrapper を作成し、PostgreSQL 10〜13 は process 起動前に拒否します。</para></summary>
    public PgAmcheck(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal PgAmcheck(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner, "pg_amcheck", PostgreSqlMajorVersion.V14);
    /// <summary><para>EN: Gets executable path.</para><para>JA: executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes pg_amcheck.</para><para>JA: pg_amcheck を検証して実行します。</para></summary>
    public async Task<PgMaintenanceResult> ExecuteAsync(PgAmcheckOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgAmcheckValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = PgAmcheckArgumentBuilder.Build(options);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return MaintenanceExecutor.ToResult(info);
    }
}
