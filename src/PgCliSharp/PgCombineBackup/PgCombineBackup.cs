using PgCliSharp.Internal.BackupWal;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgCombineBackup;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary><para>EN: Specifies how pg_combinebackup copies unchanged file content.</para><para>JA: pg_combinebackup が未変更ファイル内容をコピーする方式を指定します。</para></summary>
public enum PgCombineBackupCopyMethod
{
    /// <summary><para>EN: Use pg_combinebackup default selection.</para><para>JA: pg_combinebackup の既定選択を使用します。</para></summary>
    Default,
    /// <summary><para>EN: Clone files where the platform supports it.</para><para>JA: platform 対応時に file clone を使用します。</para></summary>
    Clone,
    /// <summary><para>EN: Use ordinary file copy.</para><para>JA: 通常の file copy を使用します。</para></summary>
    Copy,
    /// <summary><para>EN: Use copy_file_range where supported.</para><para>JA: 対応環境で copy_file_range を使用します。</para></summary>
    CopyFileRange,
    /// <summary><para>EN: Use PostgreSQL 18+ hard links.</para><para>JA: PostgreSQL 18+ の hard link を使用します。</para></summary>
    Link,
}

/// <summary><para>EN: Typed pg_combinebackup options for PostgreSQL 17-18.</para><para>JA: PostgreSQL 17〜18 の型付き pg_combinebackup オプションです。</para></summary>
public sealed class PgCombineBackupOptions
{
    /// <summary><para>EN: Input backup directories, ordered oldest/full to newest/incremental.</para><para>JA: oldest/full から newest/incremental の順に並べた input backup directory です。</para></summary>
    public IList<string> InputDirectories { get; } = new List<string>();
    /// <summary><para>EN: Required combined backup output directory.</para><para>JA: 必須の combined backup output directory です。</para></summary>
    public string? OutputDirectory { get; set; }
    /// <summary><para>EN: Enable debug logging.</para><para>JA: debug logging を有効化します。</para></summary>
    public bool Debug { get; set; }
    /// <summary><para>EN: Validate planned work without writing output.</para><para>JA: output を書かずに予定処理を検証します。</para></summary>
    public bool DryRun { get; set; }
    /// <summary><para>EN: Skip final filesystem synchronization.</para><para>JA: 最終 filesystem sync を省略します。</para></summary>
    public bool NoSync { get; set; }
    /// <summary><para>EN: Tablespace mappings.</para><para>JA: tablespace mapping です。</para></summary>
    public IList<PgTablespaceMapping> TablespaceMappings { get; } = new List<PgTablespaceMapping>();
    /// <summary><para>EN: File-copy method.</para><para>JA: file copy method です。</para></summary>
    public PgCombineBackupCopyMethod CopyMethod { get; set; }
    /// <summary><para>EN: Output manifest checksum algorithm.</para><para>JA: output manifest checksum algorithm です。</para></summary>
    public PgBackupManifestChecksum? ManifestChecksums { get; set; }
    /// <summary><para>EN: Do not generate an output manifest.</para><para>JA: output manifest を生成しません。</para></summary>
    public bool NoManifest { get; set; }
    /// <summary><para>EN: Filesystem synchronization method.</para><para>JA: filesystem sync method です。</para></summary>
    public PgBackupSyncMethod? SyncMethod { get; set; }
    /// <summary><para>EN: Process-only environment variables.</para><para>JA: process 専用環境変数です。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: pg_combinebackup execution metadata.</para><para>JA: pg_combinebackup 実行メタデータです。</para></summary>
public sealed class PgCombineBackupResult : PgBackupWalResult
{
    internal PgCombineBackupResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) { }
}

/// <summary><para>EN: Executes PostgreSQL 17+ pg_combinebackup.</para><para>JA: PostgreSQL 17+ の pg_combinebackup を実行します。</para></summary>
public sealed class PgCombineBackup
{
    private readonly IProcessRunner _runner;
    private readonly PostgreSqlExecutableVersionProvider _versions;

    /// <summary><para>EN: Creates a pg_combinebackup wrapper and validates tool availability.</para><para>JA: pg_combinebackup wrapper を作成し tool availability を検証します。</para></summary>
    public PgCombineBackup(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }

    internal PgCombineBackup(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.ExecutablePathRequired), nameof(executablePath));
        _ = PostgreSqlVersionCatalog.Get(version);
        BackupWalAvailability.EnsureTool("pg_combinebackup", version, PostgreSqlMajorVersion.V17);
        ExecutablePath = executablePath;
        Version = version;
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _versions = new PostgreSqlExecutableVersionProvider(_runner);
    }

    /// <summary><para>EN: Gets executable path.</para><para>JA: 実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath { get; }
    /// <summary><para>EN: Gets expected major version.</para><para>JA: 期待する major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary><para>EN: Validates and executes pg_combinebackup.</para><para>JA: pg_combinebackup を検証して実行します。</para></summary>
    public async Task<PgCombineBackupResult> ExecuteAsync(PgCombineBackupOptions options, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        PostgreSqlExecutableVersion executableVersion = await _versions.ValidateVersionAsync(ExecutablePath, Version, cancellationToken).ConfigureAwait(false);
        PgCombineBackupValidator.Validate(options, Version);
        var request = new ProcessRunRequest(ExecutablePath, PgCombineBackupArgumentBuilder.Build(options, Version), timeout: timeout, throwOnNonZeroExitCode: true, environmentVariables: BackupWalArgument.Environment(options.EnvironmentVariables));
        ProcessRunResult result = await _runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
        return new PgCombineBackupResult(result.ExitCode, result.Duration, executableVersion.NumericVersion, executableVersion.RawVersion, result.StandardError);
    }
}
