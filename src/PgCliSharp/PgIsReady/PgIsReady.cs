using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.PgIsReady;

namespace PgCliSharp;

/// <summary><para>EN: Typed pg_isready options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き pg_isready オプションです。</para></summary>
public sealed class PgIsReadyOptions
{
    /// <summary><para>EN: Gets or sets database name/connection string.</para><para>JA: database 名/connection string を取得または設定します。</para></summary>
    public string? Database { get; set; }
    /// <summary><para>EN: Gets or sets host/socket directory.</para><para>JA: host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets port.</para><para>JA: port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets quiet mode.</para><para>JA: quiet mode を取得または設定します。</para></summary>
    public bool Quiet { get; set; }
    /// <summary><para>EN: Gets or sets connection timeout seconds. Zero explicitly disables the pg_isready timeout.</para><para>JA: 接続 timeout 秒を取得または設定します。0 は pg_isready の timeout を明示的に無効化します。</para></summary>
    public int? ConnectTimeoutSeconds { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string,string> EnvironmentVariables { get; } = new Dictionary<string,string>(StringComparer.Ordinal);
}

/// <summary><para>EN: pg_isready result including semantic readiness status.</para><para>JA: 意味を持つ readiness status を含む pg_isready 結果です。</para></summary>
public sealed class PgIsReadyResult : PgMaintenanceResult
{
    internal PgIsReadyResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError, PgIsReadyStatus status)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) => Status = status;
    /// <summary><para>EN: Gets readiness status derived from exit code 0-3.</para><para>JA: 終了コード 0〜3 から得た readiness status を取得します。</para></summary>
    public PgIsReadyStatus Status { get; }
}

/// <summary><para>EN: Executes pg_isready while preserving its semantic nonzero exit statuses.</para><para>JA: 意味を持つ非0終了statusを保持して pg_isready を実行します。</para></summary>
public sealed class PgIsReady
{
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a pg_isready wrapper.</para><para>JA: pg_isready wrapper を作成します。</para></summary>
    public PgIsReady(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal PgIsReady(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) => _executor = new MaintenanceExecutor(executablePath, version, runner);
    /// <summary><para>EN: Gets executable path.</para><para>JA: executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates and executes pg_isready. Exit codes 0-3 are returned as status rather than exceptions.</para><para>JA: pg_isready を検証して実行します。終了コード 0〜3 は例外ではなく status として返します。</para></summary>
    public async Task<PgIsReadyResult> ExecuteAsync(PgIsReadyOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgIsReadyValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = PgIsReadyArgumentBuilder.Build(options);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io, options.EnvironmentVariables, timeout, cancellationToken, throwOnNonZeroExitCode: false).ConfigureAwait(false);
        if (info.Process.ExitCode < 0 || info.Process.ExitCode > 3)
            throw new PgProcessExecutionException(ExecutablePath, info.Process.ExitCode, info.Process.StandardError);

        return new PgIsReadyResult(
            info.Process.ExitCode,
            info.Process.Duration,
            info.ExecutableVersion.NumericVersion,
            info.ExecutableVersion.RawVersion,
            info.Process.StandardError,
            (PgIsReadyStatus)info.Process.ExitCode);
    }
}
