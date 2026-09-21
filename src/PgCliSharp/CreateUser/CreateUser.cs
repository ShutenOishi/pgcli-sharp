using PgCliSharp.Internal.Createuser;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp;

/// <summary><para>EN: Typed createuser options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き createuser オプションです。</para></summary>
public sealed class CreateUserOptions
{
    /// <summary><para>EN: Gets or sets role name. Null preserves createuser's upstream fallback/prompt behavior.</para><para>JA: role 名を取得または設定します。null は createuser の upstream fallback/prompt 動作を維持します。</para></summary>
    public string? RoleName { get; set; }
    /// <summary><para>EN: Gets or sets host/socket directory.</para><para>JA: host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets port.</para><para>JA: port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets or sets password prompting policy for the connection.</para><para>JA: 接続用 password prompt 方針を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Gets or sets SQL echo.</para><para>JA: SQL echo を取得または設定します。</para></summary>
    public bool Echo { get; set; }
    /// <summary><para>EN: Gets or sets CREATEDB/NO CREATEDB state; null leaves createuser's default logic.</para><para>JA: CREATEDB/NO CREATEDB 状態を取得または設定します。null は createuser の既定ロジックを維持します。</para></summary>
    public bool? CanCreateDatabase { get; set; }
    /// <summary><para>EN: Gets or sets SUPERUSER/NO SUPERUSER state.</para><para>JA: SUPERUSER/NO SUPERUSER 状態を取得または設定します。</para></summary>
    public bool? IsSuperuser { get; set; }
    /// <summary><para>EN: Gets or sets CREATEROLE/NO CREATEROLE state.</para><para>JA: CREATEROLE/NO CREATEROLE 状態を取得または設定します。</para></summary>
    public bool? CanCreateRole { get; set; }
    /// <summary><para>EN: Gets or sets INHERIT/NO INHERIT state.</para><para>JA: INHERIT/NO INHERIT 状態を取得または設定します。</para></summary>
    public bool? Inherit { get; set; }
    /// <summary><para>EN: Gets or sets LOGIN/NO LOGIN state.</para><para>JA: LOGIN/NO LOGIN 状態を取得または設定します。</para></summary>
    public bool? Login { get; set; }
    /// <summary><para>EN: Gets or sets REPLICATION/NO REPLICATION state.</para><para>JA: REPLICATION/NO REPLICATION 状態を取得または設定します。</para></summary>
    public bool? Replication { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 16+ BYPASSRLS/NO BYPASSRLS state.</para><para>JA: PostgreSQL 16 以降の BYPASSRLS/NO BYPASSRLS 状態を取得または設定します。</para></summary>
    public bool? BypassRls { get; set; }
    /// <summary><para>EN: Gets or sets upstream interactive mode.</para><para>JA: upstream interactive mode を取得または設定します。</para></summary>
    public bool Interactive { get; set; }
    /// <summary><para>EN: Gets or sets role connection limit (-1 means unlimited).</para><para>JA: role connection limit を取得または設定します（-1 は無制限）。</para></summary>
    public int? ConnectionLimit { get; set; }
    /// <summary><para>EN: Gets or sets whether createuser prompts for the new role password.</para><para>JA: createuser が新しい role の password を prompt するか取得または設定します。</para></summary>
    public bool PromptForRolePassword { get; set; }
    /// <summary><para>EN: Gets or sets the upstream --encrypted switch.</para><para>JA: upstream --encrypted switch を取得または設定します。</para></summary>
    public bool Encrypted { get; set; }
    /// <summary><para>EN: Gets roles the new role should belong to; emitted with the canonical spelling for the selected version.</para><para>JA: 新しい role が所属する role を取得します。選択した version の canonical spelling で出力します。</para></summary>
    public IList<string> MemberOfRoles { get; } = new List<string>();
    /// <summary><para>EN: Gets PostgreSQL 16+ roles granted WITH ADMIN OPTION to the new role.</para><para>JA: 新しい role に WITH ADMIN OPTION 付きで付与する PostgreSQL 16+ role を取得します。</para></summary>
    public IList<string> AdminOfRoles { get; } = new List<string>();
    /// <summary><para>EN: Gets PostgreSQL 16+ existing roles made members of the new role.</para><para>JA: 新しい role の member にする PostgreSQL 16+ 既存 role を取得します。</para></summary>
    public IList<string> Members { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets PostgreSQL 16+ password validity timestamp text.</para><para>JA: PostgreSQL 16 以降の password 有効期限文字列を取得または設定します。</para></summary>
    public string? ValidUntil { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes createuser.</para><para>JA: createuser を実行します。</para></summary>
public sealed class CreateUser
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a createuser wrapper.</para><para>JA: createuser wrapper を作成します。</para></summary>
    public CreateUser(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal CreateUser(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) => _executor = new MaintenanceExecutor(executablePath, version, runner);
    /// <summary><para>EN: Gets executable path.</para><para>JA: executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes createuser.</para><para>JA: createuser を検証して実行します。</para></summary>
    public async Task<PgMaintenanceResult> ExecuteAsync(CreateUserOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        CreateuserValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = CreateuserArgumentBuilder.Build(options, Version);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return MaintenanceExecutor.ToResult(info);
    }
}
