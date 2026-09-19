using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Dropuser;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp;

/// <summary><para>EN: Typed dropuser options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き dropuser オプションです。</para></summary>
public sealed class DropUserOptions
{
    /// <summary><para>EN: Gets or sets role name. It may be omitted only when Interactive is true.</para><para>JA: role 名を取得または設定します。Interactive が true の場合のみ省略できます。</para></summary>
    public string? RoleName { get; set; }
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
    /// <summary><para>EN: Gets or sets interactive role-name/confirmation prompting.</para><para>JA: interactive な role 名/確認 prompt を取得または設定します。</para></summary>
    public bool Interactive { get; set; }
    /// <summary><para>EN: Gets or sets IF EXISTS behavior.</para><para>JA: IF EXISTS 動作を取得または設定します。</para></summary>
    public bool IfExists { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes dropuser.</para><para>JA: dropuser を実行します。</para></summary>
public sealed class DropUser
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a dropuser wrapper.</para><para>JA: dropuser wrapper を作成します。</para></summary>
    public DropUser(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal DropUser(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) => _executor = new MaintenanceExecutor(executablePath, version, runner);
    /// <summary><para>EN: Gets executable path.</para><para>JA: executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes dropuser.</para><para>JA: dropuser を検証して実行します。</para></summary>
    public async Task<PgMaintenanceResult> ExecuteAsync(DropUserOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        DropuserValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = DropuserArgumentBuilder.Build(options);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return MaintenanceExecutor.ToResult(info);
    }
}
