using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgDump;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Executes a caller-selected pg_dump executable with typed, version-aware options.</para>
/// <para>JA: 呼び出し側が選択した pg_dump 実行ファイルを、型付きかつバージョン対応のオプションで実行します。</para>
/// </summary>
public sealed class PgDump
{
    private readonly IProcessRunner _processRunner;
    private readonly PostgreSqlExecutableVersionProvider _versionProvider;

    /// <summary>
    /// <para>EN: Initializes a pg_dump wrapper for an explicitly selected executable and expected PostgreSQL CLI major version.</para>
    /// <para>JA: 明示的に選択した pg_dump 実行ファイルと、期待する PostgreSQL CLI メジャーバージョンでラッパーを初期化します。</para>
    /// </summary>
    /// <param name="executablePath"><para>EN: Full or caller-resolved path to the pg_dump executable.</para><para>JA: pg_dump 実行ファイルの完全パス、または呼び出し側が解決したパスです。</para></param>
    /// <param name="version"><para>EN: Expected pg_dump major version. The executable is probed with --version before normal execution.</para><para>JA: 期待する pg_dump のメジャーバージョンです。通常実行の前に --version で実行ファイルを検証します。</para></param>
    public PgDump(
        string executablePath,
        PostgreSqlMajorVersion version)
        : this(executablePath, version, new ProcessRunner())
    {
    }

    internal PgDump(
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
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _versionProvider = new PostgreSqlExecutableVersionProvider(_processRunner);
    }

    /// <summary>
    /// <para>EN: Gets the explicitly selected pg_dump executable path.</para>
    /// <para>JA: 明示的に選択した pg_dump 実行ファイルのパスを取得します。</para>
    /// </summary>
    public string ExecutablePath { get; }

    /// <summary>
    /// <para>EN: Gets the expected PostgreSQL CLI major version.</para>
    /// <para>JA: 期待する PostgreSQL CLI メジャーバージョンを取得します。</para>
    /// </summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary>
    /// <para>EN: Validates the executable and options, then executes pg_dump. Standard output is streamed directly to the caller when requested and is never assumed to be text.</para>
    /// <para>JA: 実行ファイルとオプションを検証してから pg_dump を実行します。標準出力を指定した場合は呼び出し側へ直接ストリーミングし、テキストであるとは仮定しません。</para>
    /// </summary>
    /// <param name="options"><para>EN: Typed pg_dump options.</para><para>JA: 型付きの pg_dump オプションです。</para></param>
    /// <param name="output"><para>EN: Binary-safe output destination.</para><para>JA: バイナリセーフな出力先です。</para></param>
    /// <param name="timeout"><para>EN: Optional process timeout. Null means no PgCliSharp timeout.</para><para>JA: 任意のプロセスタイムアウトです。null の場合、PgCliSharp 側ではタイムアウトを設定しません。</para></param>
    /// <param name="cancellationToken"><para>EN: Token used to cancel execution and terminate the process tree.</para><para>JA: 実行をキャンセルし、プロセスツリーを終了するためのトークンです。</para></param>
    /// <returns><para>EN: Structured execution metadata. The dump payload remains in the selected output destination.</para><para>JA: 構造化された実行メタデータです。ダンプ本体は選択した出力先に保持されます。</para></returns>
    /// <exception cref="PgExecutableVersionMismatchException"><para>EN: The selected executable major version differs from <see cref="Version"/>.</para><para>JA: 選択した実行ファイルのメジャーバージョンが <see cref="Version"/> と異なる場合にスローされます。</para></exception>
    /// <exception cref="PgOptionValidationException"><para>EN: A typed option value or version availability rule is invalid.</para><para>JA: 型付きオプションの値またはバージョン利用可否ルールが無効な場合にスローされます。</para></exception>
    /// <exception cref="PgInvalidOptionCombinationException"><para>EN: The requested option combination or output mode is invalid.</para><para>JA: 指定されたオプションの組み合わせまたは出力モードが無効な場合にスローされます。</para></exception>
    /// <exception cref="PgProcessTimeoutException"><para>EN: The configured timeout expires.</para><para>JA: 設定したタイムアウトを超過した場合にスローされます。</para></exception>
    public async Task<PgDumpResult> ExecuteAsync(
        PgDumpOptions options,
        PgDumpOutput output,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }

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

        PgDumpValidator.Validate(
            options,
            output,
            Version,
            executableVersion);

        IReadOnlyList<string> arguments = PgDumpArgumentBuilder.Build(
            options,
            output,
            Version);

        Stream? standardOutput = output.Kind == PgDumpOutputKind.StandardOutput
            ? output.StandardOutput
            : null;

        Stream? standardInput = PgDumpValidator.GetStandardInput(options);

        IReadOnlyDictionary<string, string>? environmentVariables =
            options.EnvironmentVariables.Count == 0
                ? null
                : new Dictionary<string, string>(
                    options.EnvironmentVariables,
                    StringComparer.Ordinal);

        var request = new ProcessRunRequest(
            ExecutablePath,
            arguments,
            standardOutput,
            timeout,
            throwOnNonZeroExitCode: true,
            environmentVariables,
            standardInput);

        ProcessRunResult result = await _processRunner
            .RunAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new PgDumpResult(
            result.ExitCode,
            result.Duration,
            executableVersion.NumericVersion,
            executableVersion.RawVersion,
            result.StandardError);
    }
}
