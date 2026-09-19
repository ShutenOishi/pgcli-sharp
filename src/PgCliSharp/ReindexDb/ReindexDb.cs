using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Reindexdb;

namespace PgCliSharp;

/// <summary><para>EN: Typed reindexdb options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き reindexdb オプションです。</para></summary>
public sealed class ReindexDbOptions
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
    /// <summary><para>EN: Gets repeatable schema selectors.</para><para>JA: 複数指定可能な schema selector を取得します。</para></summary>
    public IList<string> Schemas { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets all-databases mode.</para><para>JA: all-databases mode を取得または設定します。</para></summary>
    public bool AllDatabases { get; set; }
    /// <summary><para>EN: Gets or sets system-catalog reindexing.</para><para>JA: system catalog の reindex を取得または設定します。</para></summary>
    public bool SystemCatalogs { get; set; }
    /// <summary><para>EN: Gets repeatable table selectors.</para><para>JA: 複数指定可能な table selector を取得します。</para></summary>
    public IList<string> Tables { get; } = new List<string>();
    /// <summary><para>EN: Gets repeatable index selectors.</para><para>JA: 複数指定可能な index selector を取得します。</para></summary>
    public IList<string> Indexes { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets PostgreSQL 13+ job count.</para><para>JA: PostgreSQL 13 以降の job 数を取得または設定します。</para></summary>
    public int? Jobs { get; set; }
    /// <summary><para>EN: Gets or sets verbose mode.</para><para>JA: verbose mode を取得または設定します。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 12+ concurrent reindexing.</para><para>JA: PostgreSQL 12 以降の concurrent reindex を取得または設定します。</para></summary>
    public bool Concurrently { get; set; }
    /// <summary><para>EN: Gets or sets maintenance database.</para><para>JA: maintenance database を取得または設定します。</para></summary>
    public string? MaintenanceDatabase { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 14+ target tablespace.</para><para>JA: PostgreSQL 14 以降の target tablespace を取得または設定します。</para></summary>
    public string? Tablespace { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes reindexdb.</para><para>JA: reindexdb を実行します。</para></summary>
public sealed class ReindexDb
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a reindexdb wrapper.</para><para>JA: reindexdb wrapper を作成します。</para></summary>
    public ReindexDb(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal ReindexDb(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) => _executor = new MaintenanceExecutor(executablePath, version, runner);
    /// <summary><para>EN: Gets executable path.</para><para>JA: executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes reindexdb.</para><para>JA: reindexdb を検証して実行します。</para></summary>
    public async Task<PgMaintenanceResult> ExecuteAsync(ReindexDbOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        ReindexdbValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = ReindexdbArgumentBuilder.Build(options);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return MaintenanceExecutor.ToResult(info);
    }
}
