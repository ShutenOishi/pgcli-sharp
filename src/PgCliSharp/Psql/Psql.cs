using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Psql;

namespace PgCliSharp;

/// <summary><para>EN: Selects psql's initial output format.</para><para>JA: psql の初期出力形式を選択します。</para></summary>
public enum PsqlOutputFormat
{
    /// <summary><para>EN: Keep psql's default aligned output.</para><para>JA: psql の既定の aligned 出力を使用します。</para></summary>
    Aligned,
    /// <summary><para>EN: Use unaligned output.</para><para>JA: unaligned 出力を使用します。</para></summary>
    Unaligned,
    /// <summary><para>EN: Use HTML output.</para><para>JA: HTML 出力を使用します。</para></summary>
    Html,
    /// <summary><para>EN: Use PostgreSQL 12+ CSV output.</para><para>JA: PostgreSQL 12 以降の CSV 出力を使用します。</para></summary>
    Csv,
}

/// <summary><para>EN: Identifies an ordered psql startup action.</para><para>JA: 順序付き psql 起動 action の種類を表します。</para></summary>
public enum PsqlActionKind
{
    /// <summary><para>EN: Execute a command string with --command.</para><para>JA: --command で command 文字列を実行します。</para></summary>
    Command,
    /// <summary><para>EN: Read commands from a file with --file.</para><para>JA: --file で file から command を読み取ります。</para></summary>
    File,
}

/// <summary><para>EN: Represents one ordered psql --command or --file action.</para><para>JA: 順序を保持する psql --command または --file action を表します。</para></summary>
public sealed class PsqlAction
{
    private PsqlAction(PsqlActionKind kind, string value)
    {
        Kind = kind;
        Value = value;
    }

    /// <summary><para>EN: Gets the action kind.</para><para>JA: action の種類を取得します。</para></summary>
    public PsqlActionKind Kind { get; }

    /// <summary><para>EN: Gets the command text or file name.</para><para>JA: command 文字列または file 名を取得します。</para></summary>
    public string Value { get; }

    /// <summary><para>EN: Creates a --command action.</para><para>JA: --command action を作成します。</para></summary>
    /// <param name="command"><para>EN: Command text.</para><para>JA: command 文字列です。</para></param>
    public static PsqlAction Command(string command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));
        return new PsqlAction(PsqlActionKind.Command, command);
    }

    /// <summary><para>EN: Creates a --file action. Use "-" to read this action from standard input.</para><para>JA: --file action を作成します。標準入力から読む場合は "-" を使用します。</para></summary>
    /// <param name="fileName"><para>EN: File name or "-".</para><para>JA: file 名または "-" です。</para></param>
    public static PsqlAction File(string fileName)
    {
        if (fileName is null) throw new ArgumentNullException(nameof(fileName));
        return new PsqlAction(PsqlActionKind.File, fileName);
    }
}

/// <summary><para>EN: Represents a psql variable assignment while preserving unset versus empty value.</para><para>JA: unset と空文字値を区別して保持する psql variable assignment です。</para></summary>
public sealed class PsqlVariableAssignment
{
    private PsqlVariableAssignment(string name, string? value, bool hasValue)
    {
        Name = name;
        Value = value;
        HasValue = hasValue;
    }

    /// <summary><para>EN: Gets the variable name.</para><para>JA: variable 名を取得します。</para></summary>
    public string Name { get; }

    /// <summary><para>EN: Gets the value when one was supplied.</para><para>JA: 値が指定された場合、その値を取得します。</para></summary>
    public string? Value { get; }

    /// <summary><para>EN: Gets whether an equals sign/value was supplied. False means unset.</para><para>JA: 等号と値が指定されたか取得します。false は unset を表します。</para></summary>
    public bool HasValue { get; }

    /// <summary><para>EN: Creates an assignment that unsets a psql variable.</para><para>JA: psql variable を unset する assignment を作成します。</para></summary>
    public static PsqlVariableAssignment Unset(string name)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        return new PsqlVariableAssignment(name, null, false);
    }

    /// <summary><para>EN: Creates an assignment that sets a value; an empty string remains an explicit empty value.</para><para>JA: 値を設定する assignment を作成します。空文字は明示的な空値として保持されます。</para></summary>
    public static PsqlVariableAssignment Set(string name, string value)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new PsqlVariableAssignment(name, value, true);
    }
}

/// <summary><para>EN: Represents a psql literal separator or zero-byte separator.</para><para>JA: psql の通常 separator または zero-byte separator を表します。</para></summary>
public sealed class PsqlSeparator
{
    private PsqlSeparator(string? value, bool isZeroByte)
    {
        Value = value;
        IsZeroByte = isZeroByte;
    }

    /// <summary><para>EN: Gets the literal separator text, or null for zero byte.</para><para>JA: 通常 separator 文字列を取得します。zero byte の場合は null です。</para></summary>
    public string? Value { get; }

    /// <summary><para>EN: Gets whether the separator is a zero byte.</para><para>JA: separator が zero byte か取得します。</para></summary>
    public bool IsZeroByte { get; }

    /// <summary><para>EN: Creates a literal separator, including an explicitly empty separator.</para><para>JA: 明示的な空文字を含む通常 separator を作成します。</para></summary>
    public static PsqlSeparator Text(string value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        return new PsqlSeparator(value, false);
    }

    /// <summary><para>EN: Creates a zero-byte separator.</para><para>JA: zero-byte separator を作成します。</para></summary>
    public static PsqlSeparator ZeroByte() => new PsqlSeparator(null, true);
}

/// <summary><para>EN: psql's documented process exit status.</para><para>JA: psql が定義する process 終了 status です。</para></summary>
public enum PsqlExitStatus
{
    /// <summary><para>EN: psql finished normally.</para><para>JA: psql が正常終了しました。</para></summary>
    Success = 0,
    /// <summary><para>EN: psql encountered its own fatal error.</para><para>JA: psql 自身の fatal error が発生しました。</para></summary>
    FatalError = 1,
    /// <summary><para>EN: The server connection went bad in non-interactive mode.</para><para>JA: non-interactive mode で server 接続が失われました。</para></summary>
    ConnectionFailure = 2,
    /// <summary><para>EN: A script error occurred while ON_ERROR_STOP was active.</para><para>JA: ON_ERROR_STOP 有効時に script error が発生しました。</para></summary>
    ScriptError = 3,
}

/// <summary><para>EN: Typed psql options for PostgreSQL 10-18.</para><para>JA: PostgreSQL 10〜18 の型付き psql オプションです。</para></summary>
public sealed class PsqlOptions
{
    /// <summary><para>EN: Gets or sets the database name or connection string.</para><para>JA: database 名または connection string を取得または設定します。</para></summary>
    public string? Database { get; set; }
    /// <summary><para>EN: Gets or sets server host/socket directory.</para><para>JA: server host/socket directory を取得または設定します。</para></summary>
    public string? Host { get; set; }
    /// <summary><para>EN: Gets or sets server port.</para><para>JA: server port を取得または設定します。</para></summary>
    public int? Port { get; set; }
    /// <summary><para>EN: Gets or sets connection user.</para><para>JA: 接続 user を取得または設定します。</para></summary>
    public string? Username { get; set; }
    /// <summary><para>EN: Gets or sets password prompting policy.</para><para>JA: password prompt 方針を取得または設定します。</para></summary>
    public PgPasswordPromptMode PasswordPrompt { get; set; } = PgPasswordPromptMode.Default;
    /// <summary><para>EN: Gets or sets the initial output format.</para><para>JA: 初期出力形式を取得または設定します。</para></summary>
    public PsqlOutputFormat? OutputFormat { get; set; }
    /// <summary><para>EN: Gets ordered --command/--file actions.</para><para>JA: 順序を保持する --command/--file action を取得します。</para></summary>
    public IList<PsqlAction> Actions { get; } = new List<PsqlAction>();
    /// <summary><para>EN: Gets startup variable assignments.</para><para>JA: 起動時 variable assignment を取得します。</para></summary>
    public IList<PsqlVariableAssignment> Variables { get; } = new List<PsqlVariableAssignment>();
    /// <summary><para>EN: Gets repeatable --pset assignments.</para><para>JA: 繰り返し指定可能な --pset assignment を取得します。</para></summary>
    public IList<string> PrintSettings { get; } = new List<string>();
    /// <summary><para>EN: Gets or sets whether all nonempty input lines are echoed.</para><para>JA: 空でない入力行をすべて echo するか取得または設定します。</para></summary>
    public bool EchoAll { get; set; }
    /// <summary><para>EN: Gets or sets whether failed SQL commands are echoed.</para><para>JA: 失敗した SQL command を echo するか取得または設定します。</para></summary>
    public bool EchoErrors { get; set; }
    /// <summary><para>EN: Gets or sets whether sent SQL queries are echoed.</para><para>JA: 送信する SQL query を echo するか取得または設定します。</para></summary>
    public bool EchoQueries { get; set; }
    /// <summary><para>EN: Gets or sets whether hidden queries are echoed.</para><para>JA: hidden query を echo するか取得または設定します。</para></summary>
    public bool EchoHidden { get; set; }
    /// <summary><para>EN: Gets or sets the field separator.</para><para>JA: field separator を取得または設定します。</para></summary>
    public PsqlSeparator? FieldSeparator { get; set; }
    /// <summary><para>EN: Gets or sets the record separator.</para><para>JA: record separator を取得または設定します。</para></summary>
    public PsqlSeparator? RecordSeparator { get; set; }
    /// <summary><para>EN: Gets or sets whether all databases are listed then psql exits.</para><para>JA: 全 database を一覧表示して終了するか取得または設定します。</para></summary>
    public bool ListDatabases { get; set; }
    /// <summary><para>EN: Gets or sets a query-output log file.</para><para>JA: query 出力 log file を取得または設定します。</para></summary>
    public string? LogFile { get; set; }
    /// <summary><para>EN: Gets or sets whether Readline/history are disabled.</para><para>JA: Readline/history を無効化するか取得または設定します。</para></summary>
    public bool NoReadline { get; set; }
    /// <summary><para>EN: Gets or sets whether ordered actions run in one transaction.</para><para>JA: 順序付き action を単一 transaction で実行するか取得または設定します。</para></summary>
    public bool SingleTransaction { get; set; }
    /// <summary><para>EN: Gets or sets psql's query-output file.</para><para>JA: psql の query 出力 file を取得または設定します。</para></summary>
    public string? OutputFile { get; set; }
    /// <summary><para>EN: Gets or sets quiet mode.</para><para>JA: quiet mode を取得または設定します。</para></summary>
    public bool Quiet { get; set; }
    /// <summary><para>EN: Gets or sets single-step mode.</para><para>JA: single-step mode を取得または設定します。</para></summary>
    public bool SingleStep { get; set; }
    /// <summary><para>EN: Gets or sets single-line mode.</para><para>JA: single-line mode を取得または設定します。</para></summary>
    public bool SingleLine { get; set; }
    /// <summary><para>EN: Gets or sets tuples-only output.</para><para>JA: tuples-only 出力を取得または設定します。</para></summary>
    public bool TuplesOnly { get; set; }
    /// <summary><para>EN: Gets or sets HTML table attributes.</para><para>JA: HTML table attribute を取得または設定します。</para></summary>
    public string? TableAttributes { get; set; }
    /// <summary><para>EN: Gets or sets expanded output.</para><para>JA: expanded 出力を取得または設定します。</para></summary>
    public bool Expanded { get; set; }
    /// <summary><para>EN: Gets or sets whether startup psqlrc files are skipped.</para><para>JA: 起動時 psqlrc file を読み飛ばすか取得または設定します。</para></summary>
    public bool NoPsqlRc { get; set; }
    /// <summary><para>EN: Gets process-only environment variables.</para><para>JA: process 専用環境変数を取得します。</para></summary>
    public IDictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary><para>EN: Result of one finite psql execution.</para><para>JA: 1 回の有限な psql 実行結果です。</para></summary>
public sealed class PsqlResult : PgMaintenanceResult
{
    internal PsqlResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError, PsqlExitStatus status)
        : base(exitCode, duration, executableVersion, rawExecutableVersion, standardError) => Status = status;

    /// <summary><para>EN: Gets psql's documented exit status.</para><para>JA: psql が定義する終了 status を取得します。</para></summary>
    public PsqlExitStatus Status { get; }
}

/// <summary><para>EN: Executes finite psql invocations with typed PostgreSQL 10-18 options.</para><para>JA: PostgreSQL 10〜18 の型付きオプションで有限な psql 実行を行います。</para></summary>
public sealed class Psql
{
    private readonly MaintenanceExecutor _executor;

    /// <summary><para>EN: Creates a psql wrapper for an explicit executable path/version.</para><para>JA: 明示的な executable path/version の psql wrapper を作成します。</para></summary>
    public Psql(string executablePath, PostgreSqlMajorVersion version) : this(executablePath, version, new ProcessRunner()) { }

    internal Psql(string executablePath, PostgreSqlMajorVersion version, IProcessRunner runner) =>
        _executor = new MaintenanceExecutor(executablePath, version, runner);

    /// <summary><para>EN: Gets the selected executable path.</para><para>JA: 選択した executable path を取得します。</para></summary>
    public string ExecutablePath => _executor.ExecutablePath;

    /// <summary><para>EN: Gets the expected PostgreSQL CLI major version.</para><para>JA: 期待する PostgreSQL CLI major version を取得します。</para></summary>
    public PostgreSqlMajorVersion Version => _executor.Version;

    /// <summary><para>EN: Validates and executes a finite psql invocation. Known psql exit codes 0-3 are returned as typed status.</para><para>JA: 有限な psql 実行を検証して実行します。既知の psql 終了コード 0〜3 は型付き status として返します。</para></summary>
    public async Task<PsqlResult> ExecuteAsync(PsqlOptions options, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null) throw new ArgumentNullException(nameof(options));
#else
        ArgumentNullException.ThrowIfNull(options);
#endif
        PsqlValidator.Validate(options, Version);
        IReadOnlyList<string> arguments = PsqlArgumentBuilder.Build(options);
        MaintenanceExecutionInfo info = await _executor.RunAsync(
            arguments,
            io,
            options.EnvironmentVariables,
            timeout,
            cancellationToken,
            throwOnNonZeroExitCode: false).ConfigureAwait(false);

        if (info.Process.ExitCode < 0 || info.Process.ExitCode > 3)
            throw new PgProcessExecutionException(ExecutablePath, info.Process.ExitCode, info.Process.StandardError);

        return new PsqlResult(
            info.Process.ExitCode,
            info.Process.Duration,
            info.ExecutableVersion.NumericVersion,
            info.ExecutableVersion.RawVersion,
            info.Process.StandardError,
            (PsqlExitStatus)info.Process.ExitCode);
    }
}
