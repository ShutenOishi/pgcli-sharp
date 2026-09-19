using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Dropdb;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp;

/// <summary><para>EN: Typed dropdb options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き dropdb オプションです。</para></summary>
public sealed class DropDbOptions
{
    /// <summary><para>EN: Gets or sets required database name.</para><para>JA: 必須の database 名を取得または設定します。</para></summary>
    public string? DatabaseName { get; set; }
    /// <summary><para>EN: Gets or sets server host/socket directory.</para><para>JA: server host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets server port.</para><para>JA: server port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets or sets password prompting policy.</para><para>JA: password prompt 方針を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Gets or sets SQL echo.</para><para>JA: SQL echo を取得または設定します。</para></summary>
    public bool Echo { get; set; }
    /// <summary><para>EN: Gets or sets interactive confirmation.</para><para>JA: interactive confirmation を取得または設定します。</para></summary>
    public bool Interactive { get; set; }
    /// <summary><para>EN: Gets or sets IF EXISTS behavior.</para><para>JA: IF EXISTS 動作を取得または設定します。</para></summary>
    public bool IfExists { get; set; }
    /// <summary><para>EN: Gets or sets maintenance database.</para><para>JA: maintenance database を取得または設定します。</para></summary>
    public string? MaintenanceDatabase { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 13+ FORCE behavior.</para><para>JA: PostgreSQL 13 以降の FORCE 動作を取得または設定します。</para></summary>
    public bool Force { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes dropdb.</para><para>JA: dropdb を実行します。</para></summary>
public sealed class DropDb
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a dropdb wrapper.</para><para>JA: dropdb wrapper を作成します。</para></summary>
    public DropDb(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal DropDb(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) => _executor = new MaintenanceExecutor(executablePath, version, runner);
    /// <summary><para>EN: Gets executable path.</para><para>JA: executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes dropdb.</para><para>JA: dropdb を検証して実行します。</para></summary>
    public async Task<PgMaintenanceResult> ExecuteAsync(DropDbOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        DropdbValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = DropdbArgumentBuilder.Build(options);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return MaintenanceExecutor.ToResult(info);
    }
}
