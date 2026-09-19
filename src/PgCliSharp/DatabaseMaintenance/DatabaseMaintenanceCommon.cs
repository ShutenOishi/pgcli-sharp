using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Provides caller-owned standard input/output streams for database-management tools.</para>
/// <para>JA: データベース管理ツールへ渡す、呼び出し側所有の標準入力・標準出力ストリームを提供します。</para>
/// </summary>
public sealed class PgMaintenanceIo
{
    /// <summary>
    /// <para>EN: Creates an optional standard-input/standard-output stream pair. Null streams are not redirected by PgCliSharp.</para>
    /// <para>JA: 任意の標準入力・標準出力ストリームの組を作成します。null のストリームは PgCliSharp ではリダイレクトしません。</para>
    /// </summary>
    /// <param name="standardInput"><para>EN: Optional readable stream forwarded to the tool's standard input.</para><para>JA: ツールの標準入力へ転送する任意の読み取り可能ストリームです。</para></param>
    /// <param name="standardOutput"><para>EN: Optional writable stream receiving the tool's standard output.</para><para>JA: ツールの標準出力を受け取る任意の書き込み可能ストリームです。</para></param>
    public PgMaintenanceIo(Stream? standardInput = null, Stream? standardOutput = null)
    {
        if (standardInput is not null && !standardInput.CanRead)
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.InputStreamMustBeReadable), nameof(standardInput));
        if (standardOutput is not null && !standardOutput.CanWrite)
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.OutputStreamMustBeWritable), nameof(standardOutput));

        StandardInput = standardInput;
        StandardOutput = standardOutput;
    }

    /// <summary><para>EN: Gets the caller-owned standard-input stream, if any.</para><para>JA: 指定されている場合、呼び出し側所有の標準入力ストリームを取得します。</para></summary>
    public Stream? StandardInput { get; }

    /// <summary><para>EN: Gets the caller-owned standard-output stream, if any.</para><para>JA: 指定されている場合、呼び出し側所有の標準出力ストリームを取得します。</para></summary>
    public Stream? StandardOutput { get; }
}

/// <summary>
/// <para>EN: Common execution metadata returned by Phase 5 database-management tools.</para>
/// <para>JA: Phase 5 のデータベース管理ツールが返す共通実行メタデータです。</para>
/// </summary>
public class PgMaintenanceResult
{
    internal PgMaintenanceResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError)
    {
        ExitCode = exitCode;
        Duration = duration;
        ExecutableVersion = executableVersion;
        RawExecutableVersion = rawExecutableVersion;
        StandardError = standardError;
    }

    /// <summary><para>EN: Gets the process exit code.</para><para>JA: プロセス終了コードを取得します。</para></summary>
    public int ExitCode { get; }

    /// <summary><para>EN: Gets the process execution duration.</para><para>JA: プロセス実行時間を取得します。</para></summary>
    public TimeSpan Duration { get; }

    /// <summary><para>EN: Gets the parsed PostgreSQL executable version.</para><para>JA: 解析済み PostgreSQL 実行ファイルバージョンを取得します。</para></summary>
    public Version ExecutableVersion { get; }

    /// <summary><para>EN: Gets the raw numeric executable-version text.</para><para>JA: 実行ファイルの数値バージョン文字列を取得します。</para></summary>
    public string RawExecutableVersion { get; }

    /// <summary><para>EN: Gets original PostgreSQL standard-error text without translation.</para><para>JA: 翻訳していない PostgreSQL の元の標準エラーテキストを取得します。</para></summary>
    public string StandardError { get; }
}

/// <summary><para>EN: Specifies PostgreSQL 15+ createdb database-copy strategy.</para><para>JA: PostgreSQL 15 以降の createdb データベースコピー方式を指定します。</para></summary>
public enum PgCreateDbStrategy
{
    /// <summary><para>EN: Use WAL-logged database creation.</para><para>JA: WAL 記録を伴うデータベース作成を使用します。</para></summary>
    WalLog,
    /// <summary><para>EN: Copy the template database files.</para><para>JA: template database のファイルコピーを使用します。</para></summary>
    FileCopy,
}

/// <summary><para>EN: Specifies PostgreSQL 15+ createdb locale provider.</para><para>JA: PostgreSQL 15 以降の createdb locale provider を指定します。</para></summary>
public enum PgCreateDbLocaleProvider
{
    /// <summary><para>EN: Use the libc locale provider.</para><para>JA: libc locale provider を使用します。</para></summary>
    Libc,
    /// <summary><para>EN: Use the ICU locale provider.</para><para>JA: ICU locale provider を使用します。</para></summary>
    Icu,
    /// <summary><para>EN: Use the PostgreSQL 17+ builtin locale provider.</para><para>JA: PostgreSQL 17 以降の builtin locale provider を使用します。</para></summary>
    Builtin,
}

/// <summary><para>EN: Selects an explicit VACUUM index-cleanup policy.</para><para>JA: VACUUM の index cleanup 方針を明示的に指定します。</para></summary>
public enum PgVacuumIndexCleanup
{
    /// <summary><para>EN: Disable index cleanup (--no-index-cleanup).</para><para>JA: index cleanup を無効化します（--no-index-cleanup）。</para></summary>
    Disabled,
    /// <summary><para>EN: Force index cleanup (--force-index-cleanup).</para><para>JA: index cleanup を強制します（--force-index-cleanup）。</para></summary>
    Forced,
}

/// <summary><para>EN: Specifies pg_amcheck heap-page skip behavior.</para><para>JA: pg_amcheck の heap page skip 動作を指定します。</para></summary>
public enum PgAmcheckSkipMode
{
    /// <summary><para>EN: Do not skip pages.</para><para>JA: page を skip しません。</para></summary>
    None,
    /// <summary><para>EN: Skip all-visible pages.</para><para>JA: all-visible page を skip します。</para></summary>
    AllVisible,
    /// <summary><para>EN: Skip all-frozen pages.</para><para>JA: all-frozen page を skip します。</para></summary>
    AllFrozen,
}

/// <summary>
/// <para>EN: Represents pg_amcheck --install-missing with its optional schema argument.</para>
/// <para>JA: 任意の schema 引数を持つ pg_amcheck --install-missing を表します。</para>
/// </summary>
public sealed class PgAmcheckInstallMissing
{
    private PgAmcheckInstallMissing(string? schema) => Schema = schema;

    /// <summary><para>EN: Gets the installation schema. Null means use pg_amcheck's default schema.</para><para>JA: install 先 schema を取得します。null は pg_amcheck の既定 schema を使用することを表します。</para></summary>
    public string? Schema { get; }

    /// <summary><para>EN: Requests installation in pg_amcheck's default schema.</para><para>JA: pg_amcheck の既定 schema への install を要求します。</para></summary>
    public static PgAmcheckInstallMissing InDefaultSchema() => new PgAmcheckInstallMissing(null);

    /// <summary><para>EN: Requests installation in the specified schema.</para><para>JA: 指定した schema への install を要求します。</para></summary>
    /// <param name="schema"><para>EN: Non-empty target schema.</para><para>JA: 空でない install 先 schema です。</para></param>
    public static PgAmcheckInstallMissing InSchema(string schema)
    {
        if (string.IsNullOrWhiteSpace(schema))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.InvalidOptionValue), nameof(schema));
        return new PgAmcheckInstallMissing(schema);
    }
}

/// <summary><para>EN: Represents pg_isready's semantic process status.</para><para>JA: pg_isready の意味を持つプロセス状態を表します。</para></summary>
public enum PgIsReadyStatus
{
    /// <summary><para>EN: The server is accepting connections (exit code 0).</para><para>JA: server が接続を受け付けています（終了コード 0）。</para></summary>
    AcceptingConnections = 0,
    /// <summary><para>EN: The server is rejecting connections (exit code 1).</para><para>JA: server が接続を拒否しています（終了コード 1）。</para></summary>
    RejectingConnections = 1,
    /// <summary><para>EN: No response was received (exit code 2).</para><para>JA: 応答がありませんでした（終了コード 2）。</para></summary>
    NoResponse = 2,
    /// <summary><para>EN: No connection attempt was made (exit code 3).</para><para>JA: 接続試行が行われませんでした（終了コード 3）。</para></summary>
    NoAttempt = 3,
}
