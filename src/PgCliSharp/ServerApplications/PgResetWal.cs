using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.ServerApplications;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Typed pg_resetwal options for PostgreSQL 10-18. </para><para>JA: PostgreSQL 10〜18 の型付き pg_resetwal オプションです。</para></summary>
public sealed class PgResetWalOptions
{
    /// <summary><para>EN: Required data directory. This tool does not read PGDATA; it is last-resort repair and can destroy consistency. </para><para>JA: 必須のデータディレクトリ。このツールは PGDATA を読みません。最後の修復手段で、整合性を損なうおそれがあります。</para></summary>
    public string? DataDirectory { get; set; }
    /// <summary><para>EN: Oldest,newest commit-timestamp IDs; each is 0 or at least 2 through 16, at least 3 from 17. </para><para>JA: 最古・最新のコミット時刻 ID。各値は0または16まで2以上、17以降3以上です。</para></summary>
    public PgCommitTimestampIds? CommitTimestampIds { get; set; }
    /// <summary><para>EN: Transaction ID epoch; UInt32.MaxValue is reserved. </para><para>JA: トランザクション ID の epoch。UInt32.MaxValue は予約されています。</para></summary>
    public uint? TransactionIdEpoch { get; set; }
    /// <summary><para>EN: Force reset despite unsafe control-file state. </para><para>JA: 安全でない制御ファイル状態でもリセットを強制します。</para></summary>
    public bool Force { get; set; }
    /// <summary><para>EN: Next WAL file name: exactly 24 hexadecimal characters. </para><para>JA: 次の WAL ファイル名。厳密に24桁の16進数です。</para></summary>
    public PgWalSegmentName? NextWalFile { get; set; }
    /// <summary><para>EN: Next,oldest multitransaction IDs. Oldest must be positive; next zero is accepted from 15. </para><para>JA: 次・最古のマルチトランザクション ID。最古は正数、次の0は15以降で利用できます。</para></summary>
    public PgMultiTransactionIds? MultiTransactionIds { get; set; }
    /// <summary><para>EN: Display proposed values without changing WAL/control files. </para><para>JA: WAL／制御ファイルを変更せず予定値を表示します。</para></summary>
    public bool DryRun { get; set; }
    /// <summary><para>EN: Positive next object ID. </para><para>JA: 次の正のオブジェクト ID です。</para></summary>
    public uint? NextObjectId { get; set; }
    /// <summary><para>EN: Next multitransaction offset; UInt32.MaxValue accepted from 15. </para><para>JA: 次のマルチトランザクション offset。UInt32.MaxValue は15以降で利用できます。</para></summary>
    public uint? MultiTransactionOffset { get; set; }
    /// <summary><para>EN: Oldest transaction ID, at least 3. </para><para>JA: 最古のトランザクション ID。3以上です。</para></summary>
    public uint? OldestTransactionId { get; set; }
    /// <summary><para>EN: Next transaction ID: positive through 11, at least 3 from 12. </para><para>JA: 次のトランザクション ID。11までは正数、12以降は3以上です。</para></summary>
    public uint? NextTransactionId { get; set; }
    /// <summary><para>EN: WAL segment size: power of two, 1-1024 MiB (PostgreSQL 11+). </para><para>JA: WAL セグメントサイズ。1〜1024 MiB の2の累乗です（PostgreSQL 11 以降）。</para></summary>
    public int? WalSegmentSizeMegabytes { get; set; }
    /// <summary><para>EN: Override cluster char signedness (PostgreSQL 18+). </para><para>JA: クラスタの char 符号を上書きします（PostgreSQL 18 以降）。</para></summary>
    public PgCharSignedness? CharSignedness { get; set; }
    /// <summary><para>EN: Process-only environment overrides; omitted keys inherit the calling process. </para><para>JA: プロセス専用の環境変数上書きです。省略キーは呼び出し側プロセスを継承します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes pg_resetwal directly. Cluster state and filesystem suitability remain upstream checks. </para><para>JA: pg_resetwal を直接実行します。クラスタ状態とファイルシステム適合性は upstream が検証します。</para></summary>
public sealed partial class PgResetWal
{
    private static readonly string[] HelpArguments = { "--help" };
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a wrapper with an explicit executable and expected CLI major version. </para><para>JA: 明示的な実行ファイルと期待する CLI メジャーバージョンでラッパーを作成します。</para></summary>
    public PgResetWal(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal PgResetWal(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner, "pg_resetwal", PostgreSqlMajorVersion.V10);
    /// <summary><para>EN: Gets the explicit tool executable path. </para><para>JA: 明示的なツール実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets the expected CLI major version. </para><para>JA: 期待する CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates options before the version probe, then executes. Cancellation/timeout is best effort and does not roll back cluster mutations or guarantee a stopped server. </para><para>JA: バージョン確認前にオプションを検証して実行します。キャンセル／タイムアウトは最善努力で、クラスタ変更の取り消しやサーバー停止を保証しません。</para></summary>
    public async Task<PgServerResult> ExecuteAsync(PgResetWalOptions options, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        options = PgCliSharp.Internal.Configuration.OptionsSnapshot.Copy(options);
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgResetWalImplementation.Validate(options, Version);
        IReadOnlyList<string> arguments = PgResetWalImplementation.Build(options, Version);
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io?.ToMaintenanceIo(), options.EnvironmentVariables, timeout, cancellationToken).ConfigureAwait(false);
        return new PgServerResult(info);
    }
    /// <summary><para>EN: Streams upstream help to caller-owned output after validating the executable version. </para><para>JA: 実行ファイルのバージョンを確認し、upstream のヘルプを呼び出し側所有の出力へ転送します。</para></summary>
    public async Task<PgServerResult> GetHelpAsync(PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        MaintenanceExecutionInfo info = await _executor.RunAsync(HelpArguments, io?.ToMaintenanceIo(), new Dictionary<string, string>(), timeout, cancellationToken).ConfigureAwait(false);
        return new PgServerResult(info);
    }
}
