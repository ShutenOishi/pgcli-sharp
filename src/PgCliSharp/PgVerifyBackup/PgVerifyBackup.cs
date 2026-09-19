using PgCliSharp.Internal.BackupWal;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgVerifyBackup;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary><para>EN: Specifies pg_verifybackup input format.</para><para>JA: pg_verifybackup の入力形式を指定します。</para></summary>
public enum PgVerifyBackupFormat
{
    /// <summary><para>EN: Plain directory backup.</para><para>JA: plain directory backup です。</para></summary>
    Plain,
    /// <summary><para>EN: PostgreSQL 18+ tar-format backup.</para><para>JA: PostgreSQL 18+ の tar-format backup です。</para></summary>
    Tar,
}

/// <summary><para>EN: Represents the backup path verified by pg_verifybackup.</para><para>JA: pg_verifybackup が検証する backup path を表します。</para></summary>
public sealed class PgVerifyBackupInput
{
    /// <summary><para>EN: Creates a backup input path.</para><para>JA: backup input path を作成します。</para></summary>
    public PgVerifyBackupInput(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.InputPathRequired), nameof(path));
        Path = path;
    }

    /// <summary><para>EN: Gets backup directory/archive path.</para><para>JA: backup directory/archive path を取得します。</para></summary>
    public string Path { get; }
}

/// <summary><para>EN: Typed pg_verifybackup options for PostgreSQL 13-18.</para><para>JA: PostgreSQL 13〜18 の型付き pg_verifybackup オプションです。</para></summary>
public sealed class PgVerifyBackupOptions
{
    /// <summary><para>EN: Exit after the first error.</para><para>JA: 最初の error で終了します。</para></summary>
    public bool ExitOnError { get; set; }
    /// <summary><para>EN: Explicit backup format (PostgreSQL 18+).</para><para>JA: 明示的な backup format です（PostgreSQL 18+）。</para></summary>
    public PgVerifyBackupFormat? Format { get; set; }
    /// <summary><para>EN: Relative backup paths to ignore.</para><para>JA: 無視する backup 内相対 path です。</para></summary>
    public IList<string> IgnoredPaths { get; } = new List<string>();
    /// <summary><para>EN: Alternate manifest path.</para><para>JA: 別 manifest path です。</para></summary>
    public string? ManifestPath { get; set; }
    /// <summary><para>EN: Skip WAL parsing/verification.</para><para>JA: WAL parsing/verification を省略します。</para></summary>
    public bool NoParseWal { get; set; }
    /// <summary><para>EN: Show verification progress (PostgreSQL 16+).</para><para>JA: verification progress を表示します（PostgreSQL 16+）。</para></summary>
    public bool Progress { get; set; }
    /// <summary><para>EN: Suppress successful-verification output.</para><para>JA: 成功時の verification 出力を抑制します。</para></summary>
    public bool Quiet { get; set; }
    /// <summary><para>EN: Skip file checksum verification.</para><para>JA: file checksum verification を省略します。</para></summary>
    public bool SkipChecksums { get; set; }
    /// <summary><para>EN: Alternate WAL directory.</para><para>JA: 別 WAL directory です。</para></summary>
    public string? WalDirectory { get; set; }
    /// <summary><para>EN: Process-only environment variables.</para><para>JA: process 専用環境変数です。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: pg_verifybackup execution metadata.</para><para>JA: pg_verifybackup 実行メタデータです。</para></summary>
public sealed class PgVerifyBackupResult : PgBackupWalResult
{
    internal PgVerifyBackupResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) { }
}

/// <summary><para>EN: Executes PostgreSQL 13+ pg_verifybackup.</para><para>JA: PostgreSQL 13+ の pg_verifybackup を実行します。</para></summary>
public sealed class PgVerifyBackup
{
    private readonly IProcessRunner _runner;
    private readonly PostgreSqlExecutableVersionProvider _versions;

    /// <summary><para>EN: Creates a pg_verifybackup wrapper and validates tool availability.</para><para>JA: pg_verifybackup wrapper を作成し tool availability を検証します。</para></summary>
    public PgVerifyBackup(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }

    internal PgVerifyBackup(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.ExecutablePathRequired), nameof(executablePath));
        _ = PostgreSqlVersionCatalog.Get(version);
        BackupWalAvailability.EnsureTool("pg_verifybackup", version, PostgreSqlMajorVersion.V13);
        ExecutablePath = executablePath;
        Version = version;
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _versions = new PostgreSqlExecutableVersionProvider(_runner);
    }

    /// <summary><para>EN: Gets executable path.</para><para>JA: 実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath { get; }
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary><para>EN: Validates and executes pg_verifybackup.</para><para>JA: pg_verifybackup を検証して実行します。</para></summary>
    public async Task<PgVerifyBackupResult> ExecuteAsync(PgVerifyBackupOptions options, PgVerifyBackupInput input, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (input is null) throw new ArgumentNullException(nameof(input));
#else
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(input);
#endif
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        PostgreSqlExecutableVersion executableVersion = await _versions.ValidateVersionAsync(ExecutablePath, Version, cancellationToken).ConfigureAwait(false);
        PgVerifyBackupValidator.Validate(options, Version);
        var request = new ProcessRunRequest(ExecutablePath, PgVerifyBackupArgumentBuilder.Build(options, input), timeout: timeout, throwOnNonZeroExitCode: true, environmentVariables: BackupWalArgument.Environment(options.EnvironmentVariables));
        ProcessRunResult result = await _runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
        return new PgVerifyBackupResult(result.ExitCode, result.Duration, executableVersion.NumericVersion, executableVersion.RawVersion, result.StandardError);
    }
}
