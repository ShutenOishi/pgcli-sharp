using System.Globalization;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.ServerApplications;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Caller-owned redirected streams for finite server-tool execution. The wrapper never disposes these streams. </para><para>JA: 有限のサーバーツール実行に使用する呼び出し側所有のストリームです。ラッパーは破棄しません。</para></summary>
public sealed class PgServerIo
{
    private readonly PgMaintenanceIo _io;
    /// <summary><para>EN: Creates optional readable stdin and writable stdout/stderr. Null stderr captures diagnostics in the result. </para><para>JA: 任意の読み取り可能 stdin と書き込み可能 stdout/stderr を設定します。stderr が null の場合は結果に診断を取得します。</para></summary>
    public PgServerIo(Stream? standardInput = null, Stream? standardOutput = null, Stream? standardError = null) =>
        _io = new PgMaintenanceIo(standardInput, standardOutput, standardError);
    internal PgMaintenanceIo ToMaintenanceIo() => _io;
    /// <summary><para>EN: Gets the caller-owned StandardInput stream, if supplied. </para><para>JA: 指定された呼び出し側所有の標準入力ストリームを取得します。</para></summary>
    public Stream? StandardInput => _io.StandardInput;
    /// <summary><para>EN: Gets the caller-owned StandardOutput stream, if supplied. </para><para>JA: 指定された呼び出し側所有の標準出力ストリームを取得します。</para></summary>
    public Stream? StandardOutput => _io.StandardOutput;
    /// <summary><para>EN: Gets the caller-owned StandardError stream, if supplied. </para><para>JA: 指定された呼び出し側所有の標準エラーストリームを取得します。</para></summary>
    public Stream? StandardError => _io.StandardError;
}

/// <summary><para>EN: Execution metadata. Success is the tool process result, not proof of cluster health or rollback safety. </para><para>JA: 実行メタデータです。成功はツールプロセスの結果で、クラスタの正常性や取り消し安全性の証明ではありません。</para></summary>
public class PgServerResult
{
    internal PgServerResult(MaintenanceExecutionInfo info)
    {
        ExitCode = info.Process.ExitCode;
        Duration = info.Process.Duration;
        ExecutableVersion = info.ExecutableVersion.NumericVersion;
        RawExecutableVersion = info.ExecutableVersion.RawVersion;
        StandardError = info.Process.StandardError;
    }
    /// <summary><para>EN: Gets the tool exit code. </para><para>JA: ツールの終了コードを取得します。</para></summary>
    public int ExitCode { get; }
    /// <summary><para>EN: Gets tool execution duration. </para><para>JA: ツールの実行時間を取得します。</para></summary>
    public TimeSpan Duration { get; }
    /// <summary><para>EN: Gets the actual numeric CLI version. </para><para>JA: 実際の CLI の数値バージョンを取得します。</para></summary>
    public Version ExecutableVersion { get; }
    /// <summary><para>EN: Gets the original numeric CLI version text. </para><para>JA: 元の CLI の数値バージョン文字列を取得します。</para></summary>
    public string RawExecutableVersion { get; }
    /// <summary><para>EN: Gets original upstream stderr; empty when externally streamed. </para><para>JA: 元の upstream stderr を取得します。外部ストリーム転送時は空文字です。</para></summary>
    public string StandardError { get; }
}

/// <summary><para>EN: pg_ctl result with semantic status only for the Status operation. </para><para>JA: Status 操作の場合のみ意味を持つ状態を含む pg_ctl の結果です。</para></summary>
public sealed class PgCtlResult : PgServerResult
{
    internal PgCtlResult(MaintenanceExecutionInfo info, PgCtlServerStatus? status) : base(info) => ServerStatus = status;
    /// <summary><para>EN: Gets Running/NotRunning/UnavailableDataDirectory for Status; null for other operations. </para><para>JA: Status の稼働／停止／ディレクトリ利用不可を取得します。他の操作では null です。</para></summary>
    public PgCtlServerStatus? ServerStatus { get; }
}

/// <summary><para>EN: initdb authentication; platform/build-specific facilities remain upstream checks. </para><para>JA: initdb の認証方式です。環境・ビルド固有の機能は upstream が検証します。</para></summary>
public enum PgInitDbAuthentication
{
    /// <summary><para>EN: Trust local users. </para><para>JA: ローカルユーザーを信頼します。</para></summary>
    Trust,
    /// <summary><para>EN: Reject connections. </para><para>JA: 接続を拒否します。</para></summary>
    Reject,
    /// <summary><para>EN: SCRAM-SHA-256 authentication. </para><para>JA: SCRAM-SHA-256 認証です。</para></summary>
    ScramSha256,
    /// <summary><para>EN: MD5 authentication. </para><para>JA: MD5 認証です。</para></summary>
    Md5,
    /// <summary><para>EN: Password authentication. </para><para>JA: パスワード認証です。</para></summary>
    Password,
    /// <summary><para>EN: Local peer authentication. </para><para>JA: ローカルの peer 認証です。</para></summary>
    Peer,
    /// <summary><para>EN: Host ident authentication. </para><para>JA: ホストの ident 認証です。</para></summary>
    Ident,
    /// <summary><para>EN: RADIUS authentication. </para><para>JA: RADIUS 認証です。</para></summary>
    Radius,
    /// <summary><para>EN: GSSAPI host authentication. </para><para>JA: GSSAPI ホスト認証です。</para></summary>
    Gss,
    /// <summary><para>EN: SSPI host authentication. </para><para>JA: SSPI ホスト認証です。</para></summary>
    Sspi,
    /// <summary><para>EN: PAM authentication. </para><para>JA: PAM 認証です。</para></summary>
    Pam,
    /// <summary><para>EN: BSD authentication. </para><para>JA: BSD 認証です。</para></summary>
    Bsd,
    /// <summary><para>EN: LDAP authentication. </para><para>JA: LDAP 認証です。</para></summary>
    Ldap,
    /// <summary><para>EN: TLS certificate host authentication. </para><para>JA: TLS 証明書によるホスト認証です。</para></summary>
    Cert,
}

/// <summary><para>EN: initdb locale provider. </para><para>JA: initdb のロケールプロバイダーです。</para></summary>
public enum PgInitDbLocaleProvider
{
    /// <summary><para>EN: System libc (15+). </para><para>JA: システムの libc（15以降）。</para></summary>
    Libc,
    /// <summary><para>EN: ICU (15+, requires ICU build). </para><para>JA: ICU（15以降。ICU 対応ビルドが必要）。</para></summary>
    Icu,
    /// <summary><para>EN: PostgreSQL builtin (17+). </para><para>JA: PostgreSQL 組み込み（17以降）。</para></summary>
    Builtin,
}

/// <summary><para>EN: Finite builtin locales. </para><para>JA: 有限の組み込みロケールです。</para></summary>
public enum PgBuiltinLocale
{
    /// <summary><para>EN: C locale (17+). </para><para>JA: C ロケール（17以降）。</para></summary>
    C,
    /// <summary><para>EN: C.UTF-8 locale (17+). </para><para>JA: C.UTF-8 ロケール（17以降）。</para></summary>
    CUtf8,
    /// <summary><para>EN: PG_UNICODE_FAST (18+). </para><para>JA: PG_UNICODE_FAST（18以降）。</para></summary>
    UnicodeFast,
}

/// <summary><para>EN: Cluster char signedness (18+). </para><para>JA: クラスタの char 符号（18以降）です。</para></summary>
public enum PgCharSignedness
{
    /// <summary><para>EN: Signed char. </para><para>JA: 符号付き char。</para></summary>
    SignedValue,
    /// <summary><para>EN: Unsigned char. </para><para>JA: 符号なし char。</para></summary>
    UnsignedValue,
}

/// <summary><para>EN: Mutually exclusive upgrade file-transfer mode. </para><para>JA: 排他的なアップグレードのファイル転送方式です。</para></summary>
public enum PgUpgradeTransferMode
{
    /// <summary><para>EN: Copy files; explicit --copy only from 16, omission before 16. </para><para>JA: コピー。明示 --copy は16以降、それより前は省略。</para></summary>
    Copy,
    /// <summary><para>EN: Hard link; old cluster must not be reused after new server writes. </para><para>JA: ハードリンク。新サーバー書き込み後は旧クラスタを再利用できません。</para></summary>
    Link,
    /// <summary><para>EN: Filesystem clone (12+). </para><para>JA: ファイルシステムのクローン（12以降）。</para></summary>
    Clone,
    /// <summary><para>EN: Optimized copy_file_range (17+). </para><para>JA: 最適化された copy_file_range（17以降）。</para></summary>
    CopyFileRange,
    /// <summary><para>EN: Destructively move directories (18+). </para><para>JA: ディレクトリを破壊的に移動（18以降）。</para></summary>
    Swap,
}

/// <summary><para>EN: Checksum operation on a shut-down cluster. </para><para>JA: 停止したクラスタのチェックサム操作です。</para></summary>
public enum PgChecksumsMode
{
    /// <summary><para>EN: Check without modifying files. </para><para>JA: ファイルを変更せず検査します。</para></summary>
    Check,
    /// <summary><para>EN: Enable checksums by rewriting files. </para><para>JA: ファイルを書き換えて有効化します。</para></summary>
    Enable,
    /// <summary><para>EN: Disable checksums in the control file. </para><para>JA: 制御ファイルで無効化します。</para></summary>
    Disable,
}

/// <summary><para>EN: pg_ctl operation. </para><para>JA: pg_ctl の操作です。</para></summary>
public enum PgCtlCommand
{
    /// <summary><para>EN: Initialize a cluster. </para><para>JA: クラスタを初期化します。</para></summary>
    InitDb,
    /// <summary><para>EN: Start a server; explicit LogFile required. </para><para>JA: サーバーを起動します。LogFile は必須です。</para></summary>
    Start,
    /// <summary><para>EN: Stop a server. </para><para>JA: サーバーを停止します。</para></summary>
    Stop,
    /// <summary><para>EN: Restart a server; explicit LogFile required. </para><para>JA: サーバーを再起動します。LogFile は必須です。</para></summary>
    Restart,
    /// <summary><para>EN: Reload configuration. </para><para>JA: 設定を再読み込みします。</para></summary>
    Reload,
    /// <summary><para>EN: Report server status. </para><para>JA: サーバー状態を表示します。</para></summary>
    Status,
    /// <summary><para>EN: Promote a standby. </para><para>JA: スタンバイを昇格します。</para></summary>
    Promote,
    /// <summary><para>EN: Rotate logs (12+). </para><para>JA: ログをローテーションします（12以降）。</para></summary>
    LogRotate,
    /// <summary><para>EN: Send a signal to a positive process ID. </para><para>JA: 正のプロセス ID にシグナルを送ります。</para></summary>
    Kill,
    /// <summary><para>EN: Register a Windows service. </para><para>JA: Windows サービスを登録します。</para></summary>
    Register,
    /// <summary><para>EN: Unregister a Windows service. </para><para>JA: Windows サービスを登録解除します。</para></summary>
    Unregister,
    /// <summary><para>EN: Run under the Windows service manager (internal upstream mode). </para><para>JA: Windows サービスマネージャーの下で実行します（upstream 内部モード）。</para></summary>
    RunService,
}

/// <summary><para>EN: Shutdown policy; upstream default is fast. </para><para>JA: 停止方式です。upstream の既定値は fast です。</para></summary>
public enum PgCtlShutdownMode
{
    /// <summary><para>EN: Wait for clients. </para><para>JA: クライアントを待機します。</para></summary>
    Smart,
    /// <summary><para>EN: Disconnect clients and roll back transactions. </para><para>JA: クライアントを切断しトランザクションを取り消します。</para></summary>
    Fast,
    /// <summary><para>EN: Immediate shutdown; crash recovery needed. </para><para>JA: 即時停止。クラッシュリカバリーが必要です。</para></summary>
    Immediate,
}

/// <summary><para>EN: Windows service start policy. </para><para>JA: Windows サービスの開始方針です。</para></summary>
public enum PgCtlServiceStart
{
    /// <summary><para>EN: Start automatically. </para><para>JA: 自動的に開始します。</para></summary>
    Automatic,
    /// <summary><para>EN: Start on demand. </para><para>JA: 要求時に開始します。</para></summary>
    Demand,
}

/// <summary><para>EN: Supported pg_ctl kill signal names. </para><para>JA: pg_ctl kill で対応するシグナル名です。</para></summary>
public enum PgCtlSignal
{
    /// <summary><para>EN: Abrt signal. </para><para>JA: Abrt シグナルです。</para></summary>
    Abrt,
    /// <summary><para>EN: Hup signal. </para><para>JA: Hup シグナルです。</para></summary>
    Hup,
    /// <summary><para>EN: Int signal. </para><para>JA: Int シグナルです。</para></summary>
    Interrupt,
    /// <summary><para>EN: Kill signal. </para><para>JA: Kill シグナルです。</para></summary>
    Kill,
    /// <summary><para>EN: Quit signal. </para><para>JA: Quit シグナルです。</para></summary>
    Quit,
    /// <summary><para>EN: Term signal. </para><para>JA: Term シグナルです。</para></summary>
    Term,
    /// <summary><para>EN: Usr1 signal. </para><para>JA: Usr1 シグナルです。</para></summary>
    Usr1,
    /// <summary><para>EN: Usr2 signal. </para><para>JA: Usr2 シグナルです。</para></summary>
    Usr2,
}

/// <summary><para>EN: Semantic pg_ctl Status exit codes; other errors remain exceptions. </para><para>JA: pg_ctl Status の意味を持つ終了コードです。他のエラーは例外です。</para></summary>
public enum PgCtlServerStatus
{
    /// <summary><para>EN: Server is running. </para><para>JA: サーバーは稼働中です。</para></summary>
    Running = 0,
    /// <summary><para>EN: Server is not running. </para><para>JA: サーバーは停止中です。</para></summary>
    NotRunning = 3,
    /// <summary><para>EN: Data directory cannot be accessed. </para><para>JA: データディレクトリへアクセスできません。</para></summary>
    UnavailableDataDirectory = 4,
}

/// <summary><para>EN: Structured server parameter assignment; values are not shell-escaped and are passed as one initdb argument. </para><para>JA: 構造化されたサーバーパラメーター代入です。値はシェルエスケープせず initdb の1引数として渡します。</para></summary>
public sealed class PgServerSetting
{
    /// <summary><para>EN: Creates a nonempty name and value assignment; empty values are allowed. </para><para>JA: 空でない名前と値の代入を作成します。空の値は利用できます。</para></summary>
    public PgServerSetting(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name) || ServerArgument.Contains(name, '=') || ServerArgument.Contains(name, '\0'))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.InvalidOptionValue), nameof(name));
#if NETSTANDARD2_0
        if (value is null) throw new ArgumentNullException(nameof(value));
#else
        ArgumentNullException.ThrowIfNull(value);
#endif
        if (ServerArgument.Contains(value, '\0'))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.InvalidOptionValue), nameof(value));
        Name = name;
        Value = value;
    }
    /// <summary><para>EN: Gets the parameter name. </para><para>JA: パラメーター名を取得します。</para></summary>
    public string Name { get; }
    /// <summary><para>EN: Gets the parameter value. </para><para>JA: パラメーター値を取得します。</para></summary>
    public string Value { get; }
    internal string ToArgument() => Name + "=" + Value;
}

/// <summary><para>EN: Oldest,newest commit-timestamp ID pair; version-specific ranges are checked before execution. </para><para>JA: 最古・最新のコミット時刻 ID の組です。版固有の範囲は実行前に検証します。</para></summary>
public sealed class PgCommitTimestampIds
{
    /// <summary><para>EN: Creates the ordered numeric pair. </para><para>JA: 順序付きの数値の組を作成します。</para></summary>
    public PgCommitTimestampIds(uint oldest, uint newest)
    {
        Oldest = oldest;
        Newest = newest;
    }
    /// <summary><para>EN: Gets the oldest ID. </para><para>JA: Oldest ID を取得します。</para></summary>
    public uint Oldest { get; }
    /// <summary><para>EN: Gets the newest ID. </para><para>JA: Newest ID を取得します。</para></summary>
    public uint Newest { get; }
    internal string ToArgument() => Oldest.ToString(CultureInfo.InvariantCulture) + "," + Newest.ToString(CultureInfo.InvariantCulture);
}

/// <summary><para>EN: Next,oldest multitransaction ID pair; version-specific ranges are checked before execution. </para><para>JA: 次・最古のマルチトランザクション ID の組です。版固有の範囲は実行前に検証します。</para></summary>
public sealed class PgMultiTransactionIds
{
    /// <summary><para>EN: Creates the ordered numeric pair. </para><para>JA: 順序付きの数値の組を作成します。</para></summary>
    public PgMultiTransactionIds(uint next, uint oldest)
    {
        Next = next;
        Oldest = oldest;
    }
    /// <summary><para>EN: Gets the next ID. </para><para>JA: Next ID を取得します。</para></summary>
    public uint Next { get; }
    /// <summary><para>EN: Gets the oldest ID. </para><para>JA: Oldest ID を取得します。</para></summary>
    public uint Oldest { get; }
    internal string ToArgument() => Next.ToString(CultureInfo.InvariantCulture) + "," + Oldest.ToString(CultureInfo.InvariantCulture);
}

/// <summary><para>EN: Validated 24-digit hexadecimal WAL segment file name. </para><para>JA: 検証済みの24桁16進数 WAL セグメントファイル名です。</para></summary>
public sealed class PgWalSegmentName
{
    /// <summary><para>EN: Creates and normalizes a WAL file name to uppercase; requires exactly 24 hexadecimal digits. </para><para>JA: WAL ファイル名を作成して大文字へ正規化します。厳密に24桁の16進数が必要です。</para></summary>
    public PgWalSegmentName(string value)
    {
        if (value is null || value.Length != 24 || value.Any(c => !Uri.IsHexDigit(c)))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.InvalidOptionValue), nameof(value));
        Value = value.ToUpperInvariant();
    }
    /// <summary><para>EN: Gets the normalized file name. </para><para>JA: 正規化されたファイル名を取得します。</para></summary>
    public string Value { get; }
}
/// <summary><para>EN: Filesystem synchronization method for server tools. </para><para>JA: サーバーツールのファイルシステム同期方式です。</para></summary>
public enum PgFileSyncMethod
{
    /// <summary><para>EN: Synchronize individual files (default). </para><para>JA: 個々のファイルを同期します（既定）。</para></summary>
    Fsync,
    /// <summary><para>EN: Synchronize whole filesystems; Linux only. </para><para>JA: ファイルシステム全体を同期します。Linux 専用です。</para></summary>
    Syncfs,
}
