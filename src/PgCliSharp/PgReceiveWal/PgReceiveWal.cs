using PgCliSharp.Internal.BackupWal;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgReceiveWal;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary><para>EN: Specifies pg_receivewal operation mode.</para><para>JA: pg_receivewal の動作モードを指定します。</para></summary>
public enum PgReceiveWalAction
{
    /// <summary><para>EN: Receive WAL continuously.</para><para>JA: WAL を継続受信します。</para></summary>
    Receive,
    /// <summary><para>EN: Create a physical replication slot and exit.</para><para>JA: physical replication slot を作成して終了します。</para></summary>
    CreateSlot,
    /// <summary><para>EN: Drop a replication slot and exit.</para><para>JA: replication slot を削除して終了します。</para></summary>
    DropSlot,
}

/// <summary><para>EN: Specifies pg_receivewal compression.</para><para>JA: pg_receivewal の圧縮方式を指定します。</para></summary>
public enum PgReceiveWalCompressionMethod
{
    /// <summary><para>EN: gzip compression.</para><para>JA: gzip 圧縮です。</para></summary>
    Gzip,
    /// <summary><para>EN: LZ4 compression.</para><para>JA: LZ4 圧縮です。</para></summary>
    Lz4,
    /// <summary><para>EN: No compression.</para><para>JA: 圧縮しません。</para></summary>
    None,
}

/// <summary><para>EN: Version-aware pg_receivewal compression specification.</para><para>JA: バージョン差を考慮した pg_receivewal 圧縮指定です。</para></summary>
public sealed class PgReceiveWalCompression
{
    private PgReceiveWalCompression(int? legacyLevel, PgReceiveWalCompressionMethod? method, int? level)
    {
        LegacyLevel = legacyLevel;
        Method = method;
        Level = level;
    }

    /// <summary><para>EN: Gets legacy numeric gzip level.</para><para>JA: 従来の数値 gzip level を取得します。</para></summary>
    public int? LegacyLevel { get; }
    /// <summary><para>EN: Gets method-based compression.</para><para>JA: 方式指定圧縮を取得します。</para></summary>
    public PgReceiveWalCompressionMethod? Method { get; }
    /// <summary><para>EN: Gets optional method-specific level.</para><para>JA: 任意の方式別 level を取得します。</para></summary>
    public int? Level { get; }
    /// <summary><para>EN: Gets whether legacy level syntax is used.</para><para>JA: 従来の level 構文を使用するか取得します。</para></summary>
    public bool IsLegacyLevel => LegacyLevel.HasValue;

    /// <summary><para>EN: Creates numeric gzip compression accepted by PostgreSQL 10+.</para><para>JA: PostgreSQL 10+ で受理される数値 gzip 圧縮を作成します。</para></summary>
    public static PgReceiveWalCompression FromLevel(int level)
    {
        if (level < 0 || level > 9) throw new ArgumentOutOfRangeException(nameof(level));
        return new PgReceiveWalCompression(level, null, null);
    }

    /// <summary><para>EN: Creates method-based compression used by newer pg_receivewal versions.</para><para>JA: 新しい pg_receivewal で使用する方式指定圧縮を作成します。</para></summary>
    public static PgReceiveWalCompression ForMethod(PgReceiveWalCompressionMethod method, int? level = null)
    {
#if NETSTANDARD2_0
        if (!Enum.IsDefined(typeof(PgReceiveWalCompressionMethod), method))
#else
        if (!Enum.IsDefined(method))
#endif
            throw new ArgumentOutOfRangeException(nameof(method));
        if (method == PgReceiveWalCompressionMethod.None && level.HasValue)
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.CompressionNoneDetailsNotAllowed), nameof(level));
        if (method == PgReceiveWalCompressionMethod.Gzip && level.HasValue && level.Value != -1 && (level.Value < 1 || level.Value > 9))
            throw new ArgumentOutOfRangeException(nameof(level));
        if (method == PgReceiveWalCompressionMethod.Lz4 && level.HasValue && (level.Value < 0 || level.Value > 12))
            throw new ArgumentOutOfRangeException(nameof(level));
        return new PgReceiveWalCompression(null, method, level);
    }
}

/// <summary><para>EN: Typed pg_receivewal options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き pg_receivewal オプションです。</para></summary>
public sealed class PgReceiveWalOptions
{
    /// <summary><para>EN: Target WAL directory for receive mode.</para><para>JA: receive mode の WAL 出力ディレクトリです。</para></summary>
    public string? Directory { get; set; }
    /// <summary><para>EN: libpq connection string.</para><para>JA: libpq connection string です。</para></summary>
    public string? ConnectionString { get; set; }
    /// <summary><para>EN: End WAL position (PostgreSQL 11+).</para><para>JA: WAL 終了位置です（PostgreSQL 11+）。</para></summary>
    public PgLogSequenceNumber? EndPosition { get; set; }
    /// <summary><para>EN: Server host or socket directory.</para><para>JA: server host または socket directory です。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Server port.</para><para>JA: server port です。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: User name.</para><para>JA: user 名です。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Exit after connection loss instead of reconnecting.</para><para>JA: 接続断後に再接続せず終了します。</para></summary>
    public bool NoLoop { get; set; }
    /// <summary><para>EN: Password prompting mode.</para><para>JA: password prompt mode です。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Status interval; zero disables periodic status.</para><para>JA: status 間隔です。0 は定期 status を無効化します。</para></summary>
    public TimeSpan? StatusInterval { get; set; }
    /// <summary><para>EN: Replication slot name.</para><para>JA: replication slot 名です。</para></summary>
    public string? Slot { get; set; }
    /// <summary><para>EN: Verbose logging.</para><para>JA: verbose logging です。</para></summary>
    public bool Verbose { get; set; }
    /// <summary><para>EN: WAL-file compression.</para><para>JA: WAL file 圧縮です。</para></summary>
    public PgReceiveWalCompression? Compression { get; set; }
    /// <summary><para>EN: Receive/create/drop action.</para><para>JA: receive/create/drop action です。</para></summary>
    public PgReceiveWalAction Action { get; set; } = PgReceiveWalAction.Receive;
    /// <summary><para>EN: Do not fail if create-slot finds an existing slot.</para><para>JA: create-slot で slot が既存でも失敗しません。</para></summary>
    public bool IfNotExists { get; set; }
    /// <summary><para>EN: Flush WAL synchronously after each write.</para><para>JA: 各 write 後に WAL を同期 flush します。</para></summary>
    public bool Synchronous { get; set; }
    /// <summary><para>EN: Disable filesystem sync (PostgreSQL 11+).</para><para>JA: filesystem sync を無効化します（PostgreSQL 11+）。</para></summary>
    public bool NoSync { get; set; }
    /// <summary><para>EN: Process-only environment variables.</para><para>JA: process 専用環境変数です。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: pg_receivewal execution metadata.</para><para>JA: pg_receivewal 実行メタデータです。</para></summary>
public sealed class PgReceiveWalResult : PgBackupWalResult
{
    internal PgReceiveWalResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) { }
}

/// <summary><para>EN: Executes pg_receivewal using typed PostgreSQL 10-18 options.</para><para>JA: PostgreSQL 10〜18 の型付きオプションで pg_receivewal を実行します。</para></summary>
public sealed class PgReceiveWal
{
    private readonly IProcessRunner _runner;
    private readonly PostgreSqlExecutableVersionProvider _versions;

    /// <summary><para>EN: Creates a pg_receivewal wrapper.</para><para>JA: pg_receivewal wrapper を作成します。</para></summary>
    public PgReceiveWal(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }

    internal PgReceiveWal(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner)
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

    /// <summary><para>EN: Validates and executes pg_receivewal.</para><para>JA: pg_receivewal を検証して実行します。</para></summary>
    public async Task<PgReceiveWalResult> ExecuteAsync(PgReceiveWalOptions options, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        PostgreSqlExecutableVersion executableVersion = await _versions.ValidateVersionAsync(ExecutablePath, Version, cancellationToken).ConfigureAwait(false);
        PgReceiveWalValidator.Validate(options, Version);
        var request = new ProcessRunRequest(ExecutablePath, PgReceiveWalArgumentBuilder.Build(options, Version), timeout: timeout, throwOnNonZeroExitCode: true, environmentVariables: BackupWalArgument.Environment(options.EnvironmentVariables));
        ProcessRunResult result = await _runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
        return new PgReceiveWalResult(result.ExitCode, result.Duration, executableVersion.NumericVersion, executableVersion.RawVersion, result.StandardError);
    }
}
