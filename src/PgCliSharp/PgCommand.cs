using System.Collections.ObjectModel;

namespace PgCliSharp;

/// <summary><para>EN: Selects copyable shell syntax; PgCliSharp execution itself never uses a shell.</para><para>JA: コピー用のシェル構文を選択します。PgCliSharp 自体の実行はシェルを使用しません。</para></summary>
public enum PgCommandLineStyle
{
    /// <summary><para>EN: POSIX shell quoting, including an env prefix when needed.</para><para>JA: 必要に応じた env 接頭辞を含む POSIX シェルの引用構文です。</para></summary>
    PosixShell,
    /// <summary><para>EN: PowerShell 7.5+ quoting/invocation with Standard native argument passing; process environment overrides are restored afterward.</para><para>JA: Standard ネイティブ引数渡しを使用する PowerShell 7.5 以降の引用・呼び出し構文です。プロセスの環境変数上書きは終了後に復元します。</para></summary>
    PowerShell,
}

/// <summary><para>EN: Immutable offline command description. Raw tokens/environment may contain secrets; string rendering redacts them by default. Stream contents and shell redirections are not serialized.</para><para>JA: 不変のオフラインコマンド記述です。生の引数・環境変数には秘密情報が含まれ得るため、文字列表示は既定で伏せます。ストリーム内容やシェルのリダイレクトは文字列化しません。</para></summary>
public sealed class PgCommand
{
    internal PgCommand(string executablePath, IEnumerable<string> arguments, IDictionary<string, string> environmentVariables,
        bool usesStandardInput = false, bool usesStandardOutput = false, bool usesStandardError = false,
        bool requiresExecutableVersionCheck = false)
    {
        ExecutablePath = executablePath;
        Arguments = new ReadOnlyCollection<string>(arguments.ToArray());
        EnvironmentVariables = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(environmentVariables, StringComparer.Ordinal));
        UsesStandardInput = usesStandardInput;
        UsesStandardOutput = usesStandardOutput;
        UsesStandardError = usesStandardError;
        RequiresExecutableVersionCheck = requiresExecutableVersionCheck;
    }
    /// <summary><para>EN: Gets the caller-selected executable path, without checking its existence.</para><para>JA: 存在確認を行わず、呼び出し側が選択した実行ファイルパスを取得します。</para></summary>
    public string ExecutablePath { get; }
    /// <summary><para>EN: Gets exact argument tokens, potentially containing secrets. Treat this explicit raw-data access as sensitive.</para><para>JA: 秘密情報を含み得る正確な引数トークンを取得します。この明示的な生データ参照は機密として扱ってください。</para></summary>
    public IReadOnlyList<string> Arguments { get; }
    /// <summary><para>EN: Gets exact process environment overrides, potentially containing passwords. Inherited environment is not captured.</para><para>JA: パスワードを含み得る正確なプロセス環境変数の上書き値を取得します。継承する環境変数は取得しません。</para></summary>
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; }
    /// <summary><para>EN: Gets whether input must be supplied separately; stream bytes are never embedded in the command line.</para><para>JA: 入力を別途供給する必要があるか取得します。ストリームのバイト列はコマンド文字列に埋め込みません。</para></summary>
    public bool UsesStandardInput { get; }
    /// <summary><para>EN: Gets whether caller-owned stdout routing was requested; no shell redirection is generated.</para><para>JA: 呼び出し側所有の stdout 出力先が要求されたか取得します。シェルのリダイレクトは生成しません。</para></summary>
    public bool UsesStandardOutput { get; }
    /// <summary><para>EN: Gets whether caller-owned stderr routing was requested; no shell redirection is generated.</para><para>JA: 呼び出し側所有の stderr 出力先が要求されたか取得します。シェルのリダイレクトは生成しません。</para></summary>
    public bool UsesStandardError { get; }
    /// <summary><para>EN: Gets whether patch-sensitive options need an exact executable-version check. No executable was probed.</para><para>JA: パッチ版依存オプションに実行ファイルの正確なバージョン確認が必要か取得します。実行ファイルの確認は行っていません。</para></summary>
    public bool RequiresExecutableVersionCheck { get; }

    /// <summary><para>EN: Renders explicitly selected shell syntax. All argument and environment values are redacted unless includeSensitiveValues is true; only that explicit form is suitable for copying as the actual invocation. Caller-owned streams require separate routing.</para><para>JA: 明示選択したシェル構文で表示します。includeSensitiveValues が true の場合のみ実際の引数・環境変数を出力し、それ以外はすべて伏せます。実際の呼び出しとしてコピーするには明示指定が必要です。呼び出し側所有のストリームは別途接続してください。</para></summary>
    public string ToCommandLine(PgCommandLineStyle style, bool includeSensitiveValues = false)
    {
        if (style is not (PgCommandLineStyle.PosixShell or PgCommandLineStyle.PowerShell))
            throw new ArgumentOutOfRangeException(nameof(style));
        Func<string, string> quote = style == PgCommandLineStyle.PosixShell
            ? value => "'" + value.Replace("'", "'\"'\"'") + "'"
            : value => "'" + value.Replace("'", "''").Replace("‘", "‘‘").Replace("’", "’’") + "'";
        string invocation = (style == PgCommandLineStyle.PowerShell ? "& " : string.Empty) + quote(ExecutablePath);
        if (Arguments.Count > 0)
            invocation += " " + string.Join(" ", Arguments.Select(value => quote(includeSensitiveValues ? value : "***")));
        if (EnvironmentVariables.Count == 0) return invocation;
        KeyValuePair<string, string>[] environment = EnvironmentVariables.ToArray();
        if (style == PgCommandLineStyle.PosixShell)
            return "env " + string.Join(" ", environment.Select(pair => quote(pair.Key + "=" + (includeSensitiveValues ? pair.Value : "***")))) + " " + invocation;

        // A child script scope plus try/finally preserves the invoking PowerShell process's environment.
        string saved = string.Join(" ", environment.Select(pair =>
            "$pgcliSavedEnvironment[" + quote(pair.Key) + "] = [Environment]::GetEnvironmentVariable(" + quote(pair.Key) + ", 'Process');"));
        string assignments = string.Join(" ", environment.Select(pair =>
            "[Environment]::SetEnvironmentVariable(" + quote(pair.Key) + ", " + quote(includeSensitiveValues ? pair.Value : "***") + ", 'Process');"));
        return "& { $pgcliSavedEnvironment = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal); try { " + saved + " " + assignments + " " + invocation +
            " } finally { foreach ($pgcliEnvironmentName in $pgcliSavedEnvironment.Keys) { if ($null -eq $pgcliSavedEnvironment[$pgcliEnvironmentName]) { [Environment]::SetEnvironmentVariable($pgcliEnvironmentName, [NullString]::Value, 'Process') } else { [Environment]::SetEnvironmentVariable($pgcliEnvironmentName, $pgcliSavedEnvironment[$pgcliEnvironmentName], 'Process') } } } }";
    }

    /// <summary><para>EN: Gets a POSIX-shaped diagnostic string with all argument/environment values redacted; it is not the actual invocation.</para><para>JA: すべての引数・環境変数値を伏せた POSIX 形式の診断文字列を取得します。実際の呼び出しではありません。</para></summary>
    public override string ToString() => ToCommandLine(PgCommandLineStyle.PosixShell);
}
