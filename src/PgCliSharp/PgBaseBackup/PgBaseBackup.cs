using PgCliSharp.Internal.BackupWal;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgBaseBackup;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary><para>EN: Specifies pg_basebackup local output format.</para><para>JA: pg_basebackup のローカル出力形式を指定します。</para></summary>
public enum PgBaseBackupFormat
{
    /// <summary><para>EN: Plain directory format.</para><para>JA: plain ディレクトリ形式です。</para></summary>
    Plain,
    /// <summary><para>EN: Tar archive format.</para><para>JA: tar アーカイブ形式です。</para></summary>
    Tar,
}

/// <summary><para>EN: Specifies checkpoint behavior.</para><para>JA: checkpoint 動作を指定します。</para></summary>
public enum PgBaseBackupCheckpointMode
{
    /// <summary><para>EN: Request a fast checkpoint.</para><para>JA: fast checkpoint を要求します。</para></summary>
    Fast,
    /// <summary><para>EN: Spread checkpoint I/O over time.</para><para>JA: checkpoint I/O を時間分散します。</para></summary>
    Spread,
}

/// <summary><para>EN: Specifies how WAL is included in a base backup.</para><para>JA: base backup に WAL を含める方式を指定します。</para></summary>
public enum PgBaseBackupWalMethod
{
    /// <summary><para>EN: Do not include WAL.</para><para>JA: WAL を含めません。</para></summary>
    None,
    /// <summary><para>EN: Fetch required WAL after backup.</para><para>JA: backup 後に必要な WAL を取得します。</para></summary>
    Fetch,
    /// <summary><para>EN: Stream WAL in parallel.</para><para>JA: WAL を並列ストリーミングします。</para></summary>
    Stream,
}

/// <summary><para>EN: Identifies a pg_basebackup destination.</para><para>JA: pg_basebackup の出力先を表します。</para></summary>
public enum PgBaseBackupDestinationKind
{
    /// <summary><para>EN: Local directory.</para><para>JA: ローカルディレクトリです。</para></summary>
    Directory,
    /// <summary><para>EN: Tar output through standard output.</para><para>JA: 標準出力へ tar を出力します。</para></summary>
    StandardOutput,
    /// <summary><para>EN: PostgreSQL 15+ server backup target.</para><para>JA: PostgreSQL 15 以降の server backup target です。</para></summary>
    ServerTarget,
}

/// <summary><para>EN: Represents a pg_basebackup destination.</para><para>JA: pg_basebackup の出力先を表します。</para></summary>
public sealed class PgBaseBackupDestination
{
    private PgBaseBackupDestination(PgBaseBackupDestinationKind kind, string? value, Stream? stream)
    {
        Kind = kind;
        Value = value;
        StandardOutput = stream;
    }

    /// <summary><para>EN: Gets the destination kind.</para><para>JA: 出力先種別を取得します。</para></summary>
    public PgBaseBackupDestinationKind Kind { get; }
    /// <summary><para>EN: Gets the directory or server-target value.</para><para>JA: ディレクトリまたは server-target 値を取得します。</para></summary>
    public string? Value { get; }
    /// <summary><para>EN: Gets the caller-owned output stream for stdout destinations.</para><para>JA: stdout 出力時の呼び出し側所有ストリームを取得します。</para></summary>
    public Stream? StandardOutput { get; }

    /// <summary><para>EN: Creates a local directory destination.</para><para>JA: ローカルディレクトリ出力先を作成します。</para></summary>
    public static PgBaseBackupDestination ToDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.OutputPathRequired), nameof(path));
        return new PgBaseBackupDestination(PgBaseBackupDestinationKind.Directory, path, null);
    }

    /// <summary><para>EN: Creates tar output streamed through stdout.</para><para>JA: stdout 経由で tar をストリーミングする出力先を作成します。</para></summary>
    public static PgBaseBackupDestination ToStream(Stream output)
    {
#if NETSTANDARD2_0
        if (output is null) throw new ArgumentNullException(nameof(output));
#else
        ArgumentNullException.ThrowIfNull(output);
#endif
        if (!output.CanWrite)
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.OutputStreamMustBeWritable), nameof(output));
        return new PgBaseBackupDestination(PgBaseBackupDestinationKind.StandardOutput, "-", output);
    }

    /// <summary><para>EN: Creates a PostgreSQL 15+ server-side backup target.</para><para>JA: PostgreSQL 15 以降の server-side backup target を作成します。</para></summary>
    public static PgBaseBackupDestination ToServerTarget(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.OutputPathRequired), nameof(target));
        return new PgBaseBackupDestination(PgBaseBackupDestinationKind.ServerTarget, target, null);
    }
}

/// <summary><para>EN: Specifies pg_basebackup compression algorithm.</para><para>JA: pg_basebackup の圧縮アルゴリズムを指定します。</para></summary>
public enum PgBaseBackupCompressionMethod
{
    /// <summary><para>EN: gzip.</para><para>JA: gzip です。</para></summary>
    Gzip,
    /// <summary><para>EN: LZ4.</para><para>JA: LZ4 です。</para></summary>
    Lz4,
    /// <summary><para>EN: Zstandard.</para><para>JA: Zstandard です。</para></summary>
    Zstd,
    /// <summary><para>EN: No compression.</para><para>JA: 圧縮しません。</para></summary>
    None,
}

/// <summary><para>EN: Specifies where pg_basebackup compression is performed.</para><para>JA: pg_basebackup の圧縮を実行する場所を指定します。</para></summary>
public enum PgBaseBackupCompressionLocation
{
    /// <summary><para>EN: Let PostgreSQL choose the context default.</para><para>JA: PostgreSQL の文脈依存既定値を使用します。</para></summary>
    Unspecified,
    /// <summary><para>EN: Compress in the pg_basebackup client.</para><para>JA: pg_basebackup client 側で圧縮します。</para></summary>
    Client,
    /// <summary><para>EN: Compress on the PostgreSQL server.</para><para>JA: PostgreSQL server 側で圧縮します。</para></summary>
    Server,
}

/// <summary><para>EN: Represents version-aware pg_basebackup compression.</para><para>JA: バージョン差を考慮した pg_basebackup 圧縮指定を表します。</para></summary>
public sealed class PgBaseBackupCompression
{
    private PgBaseBackupCompression(int? legacyLevel, PgBaseBackupCompressionMethod? method, int? level, PgBaseBackupCompressionLocation location)
    {
        LegacyLevel = legacyLevel;
        Method = method;
        Level = level;
        Location = location;
    }

    /// <summary><para>EN: Gets the PostgreSQL 10+ numeric gzip level.</para><para>JA: PostgreSQL 10 以降の数値 gzip level を取得します。</para></summary>
    public int? LegacyLevel { get; }
    /// <summary><para>EN: Gets the PostgreSQL 15+ method.</para><para>JA: PostgreSQL 15 以降の方式を取得します。</para></summary>
    public PgBaseBackupCompressionMethod? Method { get; }
    /// <summary><para>EN: Gets an optional method-specific level.</para><para>JA: 任意の方式別 level を取得します。</para></summary>
    public int? Level { get; }
    /// <summary><para>EN: Gets the compression location.</para><para>JA: 圧縮場所を取得します。</para></summary>
    public PgBaseBackupCompressionLocation Location { get; }
    /// <summary><para>EN: Gets whether numeric legacy syntax is used.</para><para>JA: 従来の数値構文を使用するか取得します。</para></summary>
    public bool IsLegacyLevel => LegacyLevel.HasValue;

    /// <summary><para>EN: Creates PostgreSQL 10+ numeric gzip compression (0 disables compression).</para><para>JA: PostgreSQL 10 以降の数値 gzip 圧縮を作成します（0 は圧縮なし）。</para></summary>
    public static PgBaseBackupCompression FromLevel(int level)
    {
        if (level < 0 || level > 9)
            throw new ArgumentOutOfRangeException(nameof(level));
        return new PgBaseBackupCompression(level, null, null, PgBaseBackupCompressionLocation.Unspecified);
    }

    /// <summary><para>EN: Creates PostgreSQL 15+ method/location compression.</para><para>JA: PostgreSQL 15 以降の方式・場所指定圧縮を作成します。</para></summary>
    public static PgBaseBackupCompression ForMethod(PgBaseBackupCompressionMethod method, int? level = null, PgBaseBackupCompressionLocation location = PgBaseBackupCompressionLocation.Unspecified)
    {
#if NETSTANDARD2_0
        if (!Enum.IsDefined(typeof(PgBaseBackupCompressionMethod), method) || !Enum.IsDefined(typeof(PgBaseBackupCompressionLocation), location))
#else
        if (!Enum.IsDefined(method) || !Enum.IsDefined(location))
#endif
            throw new ArgumentOutOfRangeException(nameof(method));

        if (method == PgBaseBackupCompressionMethod.None && level.HasValue)
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.CompressionNoneDetailsNotAllowed), nameof(level));
        if (method == PgBaseBackupCompressionMethod.Gzip && level.HasValue && level.Value != -1 && (level.Value < 1 || level.Value > 9))
            throw new ArgumentOutOfRangeException(nameof(level));
        if (method == PgBaseBackupCompressionMethod.Lz4 && level.HasValue && (level.Value < 0 || level.Value > 12))
            throw new ArgumentOutOfRangeException(nameof(level));

        return new PgBaseBackupCompression(null, method, level, location);
    }
}

/// <summary><para>EN: Typed pg_basebackup options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き pg_basebackup オプションです。</para></summary>
public sealed class PgBaseBackupOptions
{
    /// <summary><para>EN: Explicit local format; null uses upstream default.</para><para>JA: 明示的なローカル形式です。null は上流既定値です。</para></summary>
    public PgBaseBackupFormat? Format { get; set; }
    /// <summary><para>EN: Checkpoint mode.</para><para>JA: checkpoint mode です。</para></summary>
    public PgBaseBackupCheckpointMode? Checkpoint { get; set; }
    /// <summary><para>EN: Create the named replication slot (PostgreSQL 11+).</para><para>JA: 指定した replication slot を作成します（PostgreSQL 11+）。</para></summary>
    public bool CreateSlot { get; set; }
    /// <summary><para>EN: Maximum transfer rate such as 100M.</para><para>JA: 100M などの最大転送速度です。</para></summary>
    public string? MaxRate { get; set; }
    /// <summary><para>EN: Write recovery configuration.</para><para>JA: recovery configuration を書き込みます。</para></summary>
    public bool WriteRecoveryConf { get; set; }
    /// <summary><para>EN: Replication slot name.</para><para>JA: replication slot 名です。</para></summary>
    public string? Slot { get; set; }
    /// <summary><para>EN: Tablespace mappings in caller order.</para><para>JA: 呼び出し順の tablespace mapping です。</para></summary>
    public IList<PgTablespaceMapping> TablespaceMappings { get; } = new List<PgTablespaceMapping>();
    /// <summary><para>EN: WAL inclusion method.</para><para>JA: WAL inclusion method です。</para></summary>
    public PgBaseBackupWalMethod? WalMethod { get; set; }
    /// <summary><para>EN: Compression specification.</para><para>JA: 圧縮指定です。</para></summary>
    public PgBaseBackupCompression? Compression { get; set; }
    /// <summary><para>EN: Backup label.</para><para>JA: backup label です。</para></summary>
    public string? Label { get; set; }
    /// <summary><para>EN: Preserve partially-created directories on error.</para><para>JA: エラー時に作成途中のディレクトリを保持します。</para></summary>
    public bool NoClean { get; set; }
    /// <summary><para>EN: Skip final filesystem synchronization.</para><para>JA: 最終ファイルシステム同期を省略します。</para></summary>
    public bool NoSync { get; set; }
    /// <summary><para>EN: libpq connection string.</para><para>JA: libpq connection string です。</para></summary>
    public string? ConnectionString { get; set; }
    /// <summary><para>EN: Server host or socket directory.</para><para>JA: server host または socket directory です。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Server port.</para><para>JA: server port です。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Database user name.</para><para>JA: database user 名です。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Password prompting mode.</para><para>JA: password prompt mode です。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Status update interval; zero disables periodic status.</para><para>JA: status 更新間隔です。0 は定期 status を無効化します。</para></summary>
    public TimeSpan? StatusInterval { get; set; }
    /// <summary><para>EN: Enable verbose logging.</para><para>JA: verbose logging を有効にします。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: Show progress.</para><para>JA: progress を表示します。</para></summary>
    public bool Progress { get; set; }
    /// <summary><para>EN: Alternate WAL directory for plain output.</para><para>JA: plain 出力用の別 WAL directory です。</para></summary>
    public string? WalDirectory { get; set; }
    /// <summary><para>EN: Do not create a temporary WAL slot.</para><para>JA: 一時 WAL slot を作成しません。</para></summary>
    public bool NoSlot { get; set; }
    /// <summary><para>EN: Disable checksum verification (PostgreSQL 11+).</para><para>JA: checksum 検証を無効にします（PostgreSQL 11+）。</para></summary>
    public bool NoVerifyChecksums { get; set; }
    /// <summary><para>EN: Disable backup size estimation (PostgreSQL 13+).</para><para>JA: backup size 推定を無効にします（PostgreSQL 13+）。</para></summary>
    public bool NoEstimateSize { get; set; }
    /// <summary><para>EN: Do not write a backup manifest (PostgreSQL 13+).</para><para>JA: backup manifest を書きません（PostgreSQL 13+）。</para></summary>
    public bool NoManifest { get; set; }
    /// <summary><para>EN: Force manifest path encoding (PostgreSQL 13+).</para><para>JA: manifest path encoding を強制します（PostgreSQL 13+）。</para></summary>
    public bool ManifestForceEncode { get; set; }
    /// <summary><para>EN: Manifest checksum algorithm (PostgreSQL 13+).</para><para>JA: manifest checksum algorithm です（PostgreSQL 13+）。</para></summary>
    public PgBackupManifestChecksum? ManifestChecksums { get; set; }
    /// <summary><para>EN: Prior backup manifest for PostgreSQL 17+ incremental backup.</para><para>JA: PostgreSQL 17+ incremental backup 用の prior backup manifest です。</para></summary>
    public string? IncrementalManifest { get; set; }
    /// <summary><para>EN: Filesystem sync method (PostgreSQL 17+).</para><para>JA: filesystem sync method です（PostgreSQL 17+）。</para></summary>
    public PgBackupSyncMethod? SyncMethod { get; set; }
    /// <summary><para>EN: Environment variables passed only to the process.</para><para>JA: process にのみ渡す環境変数です。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: pg_basebackup execution metadata.</para><para>JA: pg_basebackup 実行メタデータです。</para></summary>
public sealed class PgBaseBackupResult : PgBackupWalResult
{
    internal PgBaseBackupResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) { }
}

/// <summary><para>EN: Executes pg_basebackup with typed PostgreSQL 10-18 options.</para><para>JA: 型付き PostgreSQL 10〜18 オプションで pg_basebackup を実行します。</para></summary>
public sealed class PgBaseBackup
{
    private readonly IProcessRunner _runner;
    private readonly PostgreSqlExecutableVersionProvider _versions;

    /// <summary><para>EN: Creates an explicit pg_basebackup wrapper.</para><para>JA: 明示的な pg_basebackup wrapper を作成します。</para></summary>
    public PgBaseBackup(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }

    internal PgBaseBackup(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.ExecutablePathRequired), nameof(executablePath));
        _ = PostgreSqlVersionCatalog.Get(version);
        ExecutablePath = executablePath;
        Version = version;
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _versions = new PostgreSqlExecutableVersionProvider(_runner);
    }

    /// <summary><para>EN: Gets the selected executable path.</para><para>JA: 選択した実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath { get; }
    /// <summary><para>EN: Gets the expected PostgreSQL major version.</para><para>JA: 期待する PostgreSQL major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary><para>EN: Validates and executes pg_basebackup.</para><para>JA: pg_basebackup を検証して実行します。</para></summary>
    public async Task<PgBaseBackupResult> ExecuteAsync(PgBaseBackupOptions options, PgBaseBackupDestination destination, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (destination is null) throw new ArgumentNullException(nameof(destination));
#else
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(destination);
#endif
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

        PostgreSqlExecutableVersion executableVersion = await _versions.ValidateVersionAsync(ExecutablePath, Version, cancellationToken).ConfigureAwait(false);
        PgBaseBackupValidator.Validate(options, destination, Version);
        IReadOnlyList<string> arguments = PgBaseBackupArgumentBuilder.Build(options, destination, Version);

        var request = new ProcessRunRequest(
            ExecutablePath,
            arguments,
            destination.Kind == PgBaseBackupDestinationKind.StandardOutput ? destination.StandardOutput : null,
            timeout,
            throwOnNonZeroExitCode: true,
            environmentVariables: BackupWalArgument.Environment(options.EnvironmentVariables));

        ProcessRunResult result = await _runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
        return new PgBaseBackupResult(result.ExitCode, result.Duration, executableVersion.NumericVersion, executableVersion.RawVersion, result.StandardError);
    }
}
