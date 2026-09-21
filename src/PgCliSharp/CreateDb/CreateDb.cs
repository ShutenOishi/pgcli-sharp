using PgCliSharp.Internal.Createdb;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp;

/// <summary><para>EN: Typed createdb options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き createdb オプションです。</para></summary>
public sealed class CreateDbOptions
{
    /// <summary><para>EN: Gets or sets the database name. Null preserves createdb's environment/user fallback.</para><para>JA: database 名を取得または設定します。null は createdb の環境変数/user fallback を維持します。</para></summary>
    public string? DatabaseName { get; set; }
    /// <summary><para>EN: Gets or sets the optional database description; requires DatabaseName.</para><para>JA: 任意の database description を取得または設定します。DatabaseName が必要です。</para></summary>
    public string? Description { get; set; }
    /// <summary><para>EN: Gets or sets server host/socket directory.</para><para>JA: server host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets server port.</para><para>JA: server port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets or sets password prompting policy.</para><para>JA: password prompt 方針を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Gets or sets whether SQL commands are echoed.</para><para>JA: SQL command を echo するか取得または設定します。</para></summary>
    public bool Echo { get; set; }
    /// <summary><para>EN: Gets or sets database owner.</para><para>JA: database owner を取得または設定します。</para></summary>
    public string? Owner { get; set; }
    /// <summary><para>EN: Gets or sets default tablespace.</para><para>JA: 既定 tablespace を取得または設定します。</para></summary>
    public string? Tablespace { get; set; }
    /// <summary><para>EN: Gets or sets template database.</para><para>JA: template database を取得または設定します。</para></summary>
    public string? Template { get; set; }
    /// <summary><para>EN: Gets or sets database encoding.</para><para>JA: database encoding を取得または設定します。</para></summary>
    public string? Encoding { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 15+ creation strategy.</para><para>JA: PostgreSQL 15 以降の作成 strategy を取得または設定します。</para></summary>
    public PgCreateDbStrategy? Strategy { get; set; }
    /// <summary><para>EN: Gets or sets LC_COLLATE.</para><para>JA: LC_COLLATE を取得または設定します。</para></summary>
    public string? LcCollate { get; set; }
    /// <summary><para>EN: Gets or sets LC_CTYPE.</para><para>JA: LC_CTYPE を取得または設定します。</para></summary>
    public string? LcCtype { get; set; }
    /// <summary><para>EN: Gets or sets locale shorthand.</para><para>JA: locale shorthand を取得または設定します。</para></summary>
    public string? Locale { get; set; }
    /// <summary><para>EN: Gets or sets maintenance database.</para><para>JA: maintenance database を取得または設定します。</para></summary>
    public string? MaintenanceDatabase { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 15+ locale provider.</para><para>JA: PostgreSQL 15 以降の locale provider を取得または設定します。</para></summary>
    public PgCreateDbLocaleProvider? LocaleProvider { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 17+ builtin locale.</para><para>JA: PostgreSQL 17 以降の builtin locale を取得または設定します。</para></summary>
    public string? BuiltinLocale { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 15+ ICU locale.</para><para>JA: PostgreSQL 15 以降の ICU locale を取得または設定します。</para></summary>
    public string? IcuLocale { get; set; }
    /// <summary><para>EN: Gets or sets PostgreSQL 16+ ICU collation rules.</para><para>JA: PostgreSQL 16 以降の ICU collation rule を取得または設定します。</para></summary>
    public string? IcuRules { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes createdb with typed PostgreSQL 10-18 options.</para><para>JA: PostgreSQL 10〜18 の型付きオプションで createdb を実行します。</para></summary>
public sealed class CreateDb
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a createdb wrapper for an explicit executable path/version.</para><para>JA: 明示的な executable path/version の createdb wrapper を作成します。</para></summary>
    public CreateDb(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal CreateDb(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) => _executor = new MaintenanceExecutor(executablePath, version, runner);
    /// <summary><para>EN: Gets selected executable path.</para><para>JA: 選択した executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected PostgreSQL CLI major version.</para><para>JA: 期待する PostgreSQL CLI major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes createdb.</para><para>JA: createdb を検証して実行します。</para></summary>
    public async Task<PgMaintenanceResult> ExecuteAsync(CreateDbOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        CreatedbValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = CreatedbArgumentBuilder.Build(options, Version);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return MaintenanceExecutor.ToResult(info);
    }
}
