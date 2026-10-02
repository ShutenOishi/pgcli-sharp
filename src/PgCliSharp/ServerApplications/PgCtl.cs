using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.ServerApplications;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Typed pg_ctl options for PostgreSQL 10-18. </para><para>JA: PostgreSQL 10〜18 の型付き pg_ctl オプションです。</para></summary>
public sealed class PgCtlOptions
{
    /// <summary><para>EN: Cluster configuration directory; null uses PGDATA. </para><para>JA: クラスタ設定ディレクトリ。null は PGDATA を使用します。</para></summary>
    public string? DataDirectory { get; set; }
    /// <summary><para>EN: Required operation. Windows service operations require Windows. </para><para>JA: 必須の操作です。Windows サービス操作は Windows が必要です。</para></summary>
    public PgCtlCommand? Command { get; set; }
    /// <summary><para>EN: Server log file; required for Start/Restart so detached server pipes can close. </para><para>JA: サーバーログファイル。独立したサーバーのパイプを閉じられるよう Start/Restart では必須です。</para></summary>
    public string? LogFile { get; set; }
    /// <summary><para>EN: Shutdown policy; null preserves upstream fast. </para><para>JA: 停止方式。null は既定の fast を維持します。</para></summary>
    public PgCtlShutdownMode? ShutdownMode { get; set; }
    /// <summary><para>EN: Ordered -o fragments interpreted by upstream postgres/initdb invocation. Trusted input only; not a PgCliSharp command tail. </para><para>JA: upstream が postgres/initdb 起動時に解釈する順序付き -o 断片です。信頼できる入力のみ指定してください。PgCliSharp の任意の引数末尾ではありません。</para></summary>
    public IList<string> ForwardedOptions { get; } = new List<string>();
    /// <summary><para>EN: Suppress informational messages. </para><para>JA: 情報メッセージを抑制します。</para></summary>
    public bool Silent { get; set; }
    /// <summary><para>EN: Upstream wait timeout in seconds, nonnegative; null uses PGCTLTIMEOUT/default 60. </para><para>JA: upstream の待機時間。0以上の秒数で、null は PGCTLTIMEOUT／既定60秒です。</para></summary>
    public int? WaitTimeoutSeconds { get; set; }
    /// <summary><para>EN: Allow server core dumps on supported platforms. </para><para>JA: 対応する環境でサーバーのコアダンプを許可します。</para></summary>
    public bool CoreFiles { get; set; }
    /// <summary><para>EN: Explicit wait/no-wait policy; null leaves upstream default. </para><para>JA: 明示的な待機／非待機方針。null は upstream の既定動作です。</para></summary>
    public bool? Wait { get; set; }
    /// <summary><para>EN: Explicit postgres/initdb executable for -p; null uses upstream sibling lookup. </para><para>JA: -p に渡す postgres/initdb の明示パス。null は upstream の同一ディレクトリ検索です。</para></summary>
    public string? ServerExecutablePath { get; set; }
    /// <summary><para>EN: Required signal in Kill mode. </para><para>JA: Kill 操作で必須のシグナルです。</para></summary>
    public PgCtlSignal? Signal { get; set; }
    /// <summary><para>EN: Positive process ID for Kill mode. </para><para>JA: Kill 操作で使用する正のプロセス ID です。</para></summary>
    public int? ProcessId { get; set; }
    /// <summary><para>EN: Windows service name (-N). </para><para>JA: Windows サービス名です（-N）。</para></summary>
    public string? ServiceName { get; set; }
    /// <summary><para>EN: Windows service account password (-P); do not log the option value. </para><para>JA: Windows サービスアカウントのパスワードです（-P）。値をログへ記録しないでください。</para></summary>
    public string? ServicePassword { get; set; }
    /// <summary><para>EN: Windows service account name (-U). </para><para>JA: Windows サービスアカウント名です（-U）。</para></summary>
    public string? ServiceUsername { get; set; }
    /// <summary><para>EN: Windows service start policy (-S): automatic or on demand. </para><para>JA: Windows サービス開始方針です（-S）。自動または要求時です。</para></summary>
    public PgCtlServiceStart? ServiceStart { get; set; }
    /// <summary><para>EN: Windows event-log source (-e). </para><para>JA: Windows イベントログのソースです（-e）。</para></summary>
    public string? EventSource { get; set; }
    /// <summary><para>EN: Process-only environment overrides; omitted keys inherit the calling process. </para><para>JA: プロセス専用の環境変数上書きです。省略キーは呼び出し側プロセスを継承します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Executes pg_ctl directly. Cluster state and filesystem suitability remain upstream checks. </para><para>JA: pg_ctl を直接実行します。クラスタ状態とファイルシステム適合性は upstream が検証します。</para></summary>
public sealed partial class PgCtl
{
    private static readonly string[] HelpArguments = { "--help" };
    private readonly MaintenanceExecutor _executor;
    /// <summary><para>EN: Creates a wrapper with an explicit executable and expected CLI major version. </para><para>JA: 明示的な実行ファイルと期待する CLI メジャーバージョンでラッパーを作成します。</para></summary>
    public PgCtl(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }
    internal PgCtl(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner, "pg_ctl", PostgreSqlMajorVersion.V10);
    /// <summary><para>EN: Gets the explicit tool executable path. </para><para>JA: 明示的なツール実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;
    /// <summary><para>EN: Gets the expected CLI major version. </para><para>JA: 期待する CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;
    /// <summary><para>EN: Validates options before the version probe, then executes. Cancellation/timeout is best effort and does not roll back cluster mutations or guarantee a stopped server. </para><para>JA: バージョン確認前にオプションを検証して実行します。キャンセル／タイムアウトは最善努力で、クラスタ変更の取り消しやサーバー停止を保証しません。</para></summary>
    public async Task<PgCtlResult> ExecuteAsync(PgCtlOptions options, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        options = PgCliSharp.Internal.Configuration.OptionsSnapshot.Copy(options);
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PgCtlImplementation.Validate(options, Version);
        IReadOnlyList<string> arguments = PgCtlImplementation.Build(options, Version);
        bool statusCommand = options.Command == PgCtlCommand.Status;
        MaintenanceExecutionInfo info = await _executor.RunAsync(arguments, io?.ToMaintenanceIo(), options.EnvironmentVariables, timeout, cancellationToken, !statusCommand).ConfigureAwait(false);
        int code = info.Process.ExitCode;
        if (statusCommand && code is not (0 or 3 or 4))
            throw new PgProcessExecutionException(ExecutablePath, code, info.Process.StandardError);
        return new PgCtlResult(info, statusCommand ? (PgCtlServerStatus?)code : null);
    }
    /// <summary><para>EN: Streams upstream help to caller-owned output after validating the executable version. </para><para>JA: 実行ファイルのバージョンを確認し、upstream のヘルプを呼び出し側所有の出力へ転送します。</para></summary>
    public async Task<PgServerResult> GetHelpAsync(PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        MaintenanceExecutionInfo info = await _executor.RunAsync(HelpArguments, io?.ToMaintenanceIo(), new Dictionary<string, string>(), timeout, cancellationToken).ConfigureAwait(false);
        return new PgServerResult(info);
    }
}
