using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgRestore;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Executes a caller-selected pg_restore executable with typed, version-aware archive input and restore options.</para>
/// <para>JA: 呼び出し側が選択した pg_restore 実行ファイルを、型付きかつバージョン対応のアーカイブ入力・復元オプションで実行します。</para>
/// </summary>
public sealed class PgRestore
{
    private readonly IProcessRunner _processRunner;
    private readonly PostgreSqlExecutableVersionProvider _versionProvider;

    /// <summary><para>EN: Initializes a pg_restore wrapper for an explicit executable and expected PostgreSQL CLI major version.</para><para>JA: 明示的な pg_restore 実行ファイルと期待する PostgreSQL CLI メジャーバージョンでラッパーを初期化します。</para></summary>
    /// <param name="executablePath"><para>EN: pg_restore executable path.</para><para>JA: pg_restore 実行ファイルのパスです。</para></param>
    /// <param name="version"><para>EN: Expected CLI major version.</para><para>JA: 期待する CLI メジャーバージョンです。</para></param>
    public PgRestore(
        string executablePath,
        PostgreSqlMajorVersion version)
        : this(executablePath, version, new ProcessRunner())
    {
    }

    internal PgRestore(
        string executablePath,
        PostgreSqlMajorVersion version,
        IProcessRunner processRunner)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.ExecutablePathRequired),
                nameof(executablePath));
        }

        _ = PostgreSqlVersionCatalog.Get(version);

        ExecutablePath = executablePath;
        Version = version;
        _processRunner =
            processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _versionProvider = new PostgreSqlExecutableVersionProvider(_processRunner);
    }

    /// <summary><para>EN: Gets the selected pg_restore executable path.</para><para>JA: 選択した pg_restore 実行ファイルのパスを取得します。</para></summary>
    public string ExecutablePath { get; }

    /// <summary><para>EN: Gets the expected PostgreSQL CLI major version.</para><para>JA: 期待する PostgreSQL CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary><para>EN: Validates the executable, archive input, output mode, and options, then executes pg_restore.</para><para>JA: 実行ファイル、アーカイブ入力、出力モード、オプションを検証して pg_restore を実行します。</para></summary>
    /// <param name="options"><para>EN: Typed pg_restore options.</para><para>JA: 型付き pg_restore オプションです。</para></param>
    /// <param name="input"><para>EN: Archive file, directory, or standard-input source.</para><para>JA: アーカイブファイル、ディレクトリ、または標準入力です。</para></param>
    /// <param name="output"><para>EN: Direct database or generated SQL/list destination.</para><para>JA: データベース直接復元先、または生成 SQL・一覧の出力先です。</para></param>
    /// <param name="timeout"><para>EN: Optional process timeout. Null means no PgCliSharp timeout.</para><para>JA: 任意のプロセスタイムアウトです。null の場合、PgCliSharp 側ではタイムアウトを設定しません。</para></param>
    /// <param name="cancellationToken"><para>EN: Token used to cancel execution and terminate the process tree.</para><para>JA: 実行をキャンセルし、プロセスツリーを終了するためのトークンです。</para></param>
    /// <returns><para>EN: Structured execution metadata.</para><para>JA: 構造化された実行メタデータです。</para></returns>
    public async Task<PgRestoreResult> ExecuteAsync(
        PgRestoreOptions options,
        PgRestoreInput input,
        PgRestoreOutput output,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }
#else
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
#endif

        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        PostgreSqlExecutableVersion executableVersion =
            await _versionProvider.ValidateVersionAsync(
                    ExecutablePath,
                    Version,
                    cancellationToken)
                .ConfigureAwait(false);

        PgRestoreValidator.Validate(
            options,
            input,
            output,
            Version,
            executableVersion);

        IReadOnlyList<string> arguments =
            PgRestoreArgumentBuilder.Build(options, input, output);

        IReadOnlyDictionary<string, string>? environmentVariables =
            options.EnvironmentVariables.Count == 0
                ? null
                : new Dictionary<string, string>(
                    options.EnvironmentVariables,
                    StringComparer.Ordinal);

        var request = new ProcessRunRequest(
            ExecutablePath,
            arguments,
            output.Kind == PgRestoreOutputKind.StandardOutput
                ? output.StandardOutput
                : null,
            timeout,
            throwOnNonZeroExitCode: true,
            environmentVariables: environmentVariables,
            standardInput: PgRestoreValidator.GetStandardInput(options, input));

        ProcessRunResult result =
            await _processRunner.RunAsync(request, cancellationToken)
                .ConfigureAwait(false);

        return new PgRestoreResult(
            result.ExitCode,
            result.Duration,
            executableVersion.NumericVersion,
            executableVersion.RawVersion,
            result.StandardError);
    }
}
