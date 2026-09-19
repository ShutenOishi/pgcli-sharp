using PgCliSharp.Internal.BackupWal;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgRecvLogical;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary><para>EN: Specifies pg_recvlogical actions. CreateSlot and Start may be combined; DropSlot is exclusive.</para><para>JA: pg_recvlogical の action を指定します。CreateSlot と Start は併用でき、DropSlot は排他的です。</para></summary>
[Flags]
public enum PgRecvLogicalAction
{
    /// <summary><para>EN: No action.</para><para>JA: action を指定しません。</para></summary>
    None = 0,
    /// <summary><para>EN: Create the logical replication slot.</para><para>JA: logical replication slot を作成します。</para></summary>
    CreateSlot = 1,
    /// <summary><para>EN: Start logical decoding streaming.</para><para>JA: logical decoding streaming を開始します。</para></summary>
    Start = 2,
    /// <summary><para>EN: Drop the replication slot.</para><para>JA: replication slot を削除します。</para></summary>
    DropSlot = 4,
}

/// <summary><para>EN: Identifies pg_recvlogical streaming output.</para><para>JA: pg_recvlogical の streaming 出力先を表します。</para></summary>
public enum PgRecvLogicalOutputKind
{
    /// <summary><para>EN: Let pg_recvlogical write a file.</para><para>JA: pg_recvlogical 自身にファイルを書かせます。</para></summary>
    File,
    /// <summary><para>EN: Stream pg_recvlogical stdout to a caller-owned stream.</para><para>JA: pg_recvlogical stdout を呼び出し側所有の stream へ転送します。</para></summary>
    StandardOutput,
}

/// <summary><para>EN: Represents output for pg_recvlogical --start.</para><para>JA: pg_recvlogical --start の出力先を表します。</para></summary>
public sealed class PgRecvLogicalOutput
{
    private PgRecvLogicalOutput(PgRecvLogicalOutputKind kind, string value, Stream? stream)
    {
        Kind = kind;
        Value = value;
        StandardOutput = stream;
    }

    /// <summary><para>EN: Gets output kind.</para><para>JA: 出力種別を取得します。</para></summary>
    public PgRecvLogicalOutputKind Kind { get; }
    /// <summary><para>EN: Gets --file value.</para><para>JA: --file 値を取得します。</para></summary>
    public string Value { get; }
    /// <summary><para>EN: Gets stdout destination when used.</para><para>JA: 使用時の stdout 出力先を取得します。</para></summary>
    public Stream? StandardOutput { get; }

    /// <summary><para>EN: Creates file output.</para><para>JA: file 出力を作成します。</para></summary>
    public static PgRecvLogicalOutput ToFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.OutputPathRequired), nameof(path));
        return new PgRecvLogicalOutput(PgRecvLogicalOutputKind.File, path, null);
    }

    /// <summary><para>EN: Creates stdout streaming output using --file=-.</para><para>JA: --file=- による stdout streaming 出力を作成します。</para></summary>
    public static PgRecvLogicalOutput ToStream(Stream output)
    {
#if NETSTANDARD2_0
        if (output is null) throw new ArgumentNullException(nameof(output));
#else
        ArgumentNullException.ThrowIfNull(output);
#endif
        if (!output.CanWrite)
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.OutputStreamMustBeWritable), nameof(output));
        return new PgRecvLogicalOutput(PgRecvLogicalOutputKind.StandardOutput, "-", output);
    }
}

/// <summary><para>EN: Typed pg_recvlogical options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き pg_recvlogical オプションです。</para></summary>
public sealed class PgRecvLogicalOptions
{
    /// <summary><para>EN: Fsync interval; zero disables periodic fsync.</para><para>JA: fsync 間隔です。0 は定期 fsync を無効にします。</para></summary>
    public TimeSpan? FsyncInterval { get; set; }
    /// <summary><para>EN: Exit after connection loss rather than reconnecting.</para><para>JA: 接続断後に再接続せず終了します。</para></summary>
    public bool NoLoop { get; set; }
    /// <summary><para>EN: Verbose logging.</para><para>JA: verbose logging です。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: Enable two-phase decoding when creating a slot (PostgreSQL 15+).</para><para>JA: slot 作成時に two-phase decoding を有効化します（PostgreSQL 15+）。</para></summary>
    public bool EnableTwoPhase { get; set; }
    /// <summary><para>EN: Enable failover slot synchronization when creating a slot (PostgreSQL 18+).</para><para>JA: slot 作成時に failover slot synchronization を有効化します（PostgreSQL 18+）。</para></summary>
    public bool EnableFailover { get; set; }
    /// <summary><para>EN: Database name or connection string.</para><para>JA: database 名または connection string です。</para></summary>
    public string? Database { get; set; }
    /// <summary><para>EN: Server host/socket directory.</para><para>JA: server host/socket directory です。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Server port.</para><para>JA: server port です。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: User name.</para><para>JA: user 名です。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Password prompting mode.</para><para>JA: password prompt mode です。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Optional start LSN.</para><para>JA: 任意の start LSN です。</para></summary>
    public PgLogSequenceNumber? StartPosition { get; set; }
    /// <summary><para>EN: Optional end LSN.</para><para>JA: 任意の end LSN です。</para></summary>
    public PgLogSequenceNumber? EndPosition { get; set; }
    /// <summary><para>EN: Logical decoding plugin options, preserved in caller order.</para><para>JA: 呼び出し順を保持する logical decoding plugin option です。</para></summary>
    public IList<string> PluginOptions { get; } = new List<string>();
    /// <summary><para>EN: Logical decoding plugin name.</para><para>JA: logical decoding plugin 名です。</para></summary>
    public string? Plugin { get; set; }
    /// <summary><para>EN: Standby status interval; zero disables periodic status.</para><para>JA: standby status 間隔です。0 は定期 status を無効にします。</para></summary>
    public TimeSpan? StatusInterval { get; set; }
    /// <summary><para>EN: Logical replication slot name.</para><para>JA: logical replication slot 名です。</para></summary>
    public string? Slot { get; set; }
    /// <summary><para>EN: Requested action flags.</para><para>JA: 要求する action flag です。</para></summary>
    public PgRecvLogicalAction Action { get; set; }
    /// <summary><para>EN: Allow an existing slot during create.</para><para>JA: create 時に既存 slot を許可します。</para></summary>
    public bool IfNotExists { get; set; }
    /// <summary><para>EN: Process-only environment variables.</para><para>JA: process 専用環境変数です。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: pg_recvlogical execution metadata.</para><para>JA: pg_recvlogical 実行メタデータです。</para></summary>
public sealed class PgRecvLogicalResult : PgBackupWalResult
{
    internal PgRecvLogicalResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) { }
}

/// <summary><para>EN: Executes pg_recvlogical using typed PostgreSQL 10-18 options.</para><para>JA: PostgreSQL 10〜18 の型付きオプションで pg_recvlogical を実行します。</para></summary>
public sealed class PgRecvLogical
{
    private readonly IProcessRunner _runner;
    private readonly PostgreSqlExecutableVersionProvider _versions;

    /// <summary><para>EN: Creates a pg_recvlogical wrapper.</para><para>JA: pg_recvlogical wrapper を作成します。</para></summary>
    public PgRecvLogical(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }

    internal PgRecvLogical(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.ExecutablePathRequired), nameof(executablePath));
        _ = PostgreSqlVersionCatalog.Get(version);
        ExecutablePath = executablePath;
        Version = version;
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _versions = new PostgreSqlExecutableVersionProvider(_runner);
    }

    /// <summary><para>EN: Gets executable path.</para><para>JA: 実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath { get; }
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary><para>EN: Validates and executes pg_recvlogical.</para><para>JA: pg_recvlogical を検証して実行します。</para></summary>
    public async Task<PgRecvLogicalResult> ExecuteAsync(PgRecvLogicalOptions options, PgRecvLogicalOutput? output = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        PostgreSqlExecutableVersion executableVersion = await _versions.ValidateVersionAsync(ExecutablePath, Version, cancellationToken).ConfigureAwait(false);
        PgRecvLogicalValidator.Validate(options, output, Version);
        var request = new ProcessRunRequest(
            ExecutablePath,
            PgRecvLogicalArgumentBuilder.Build(options, output, Version),
            output?.Kind == PgRecvLogicalOutputKind.StandardOutput ? output.StandardOutput : null,
            timeout,
            throwOnNonZeroExitCode: true,
            environmentVariables: BackupWalArgument.Environment(options.EnvironmentVariables));
        ProcessRunResult result = await _runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
        return new PgRecvLogicalResult(result.ExitCode, result.Duration, executableVersion.NumericVersion, executableVersion.RawVersion, result.StandardError);
    }
}
