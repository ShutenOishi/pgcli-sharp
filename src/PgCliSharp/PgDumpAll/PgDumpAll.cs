using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.PgDumpAll;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Executes a caller-selected pg_dumpall executable with typed PostgreSQL 10-18 options.</para>
/// <para>JA: 呼び出し側が選択した pg_dumpall 実行ファイルを PostgreSQL 10〜18 対応の型付きオプションで実行します。</para>
/// </summary>
public sealed class PgDumpAll
{
    private readonly IProcessRunner _processRunner;
    private readonly PostgreSqlExecutableVersionProvider _versionProvider;

    /// <summary><para>EN: Initializes pg_dumpall for an explicit executable and expected CLI major version.</para><para>JA: 明示的な実行ファイルと期待する CLI メジャーバージョンで pg_dumpall を初期化します。</para></summary>
    /// <param name="executablePath"><para>EN: pg_dumpall executable path.</para><para>JA: pg_dumpall 実行ファイルのパスです。</para></param>
    /// <param name="version"><para>EN: Expected CLI major version.</para><para>JA: 期待する CLI メジャーバージョンです。</para></param>
    public PgDumpAll(
        string executablePath,
        PostgreSqlMajorVersion version)
        : this(executablePath, version, new ProcessRunner())
    {
    }

    internal PgDumpAll(
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

    /// <summary><para>EN: Gets the selected pg_dumpall executable path.</para><para>JA: 選択した pg_dumpall 実行ファイルのパスを取得します。</para></summary>
    public string ExecutablePath { get; }

    /// <summary><para>EN: Gets the expected PostgreSQL CLI major version.</para><para>JA: 期待する PostgreSQL CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion Version { get; }

    /// <summary><para>EN: Validates the executable and typed options, then executes pg_dumpall.</para><para>JA: 実行ファイルと型付きオプションを検証して pg_dumpall を実行します。</para></summary>
    /// <param name="options"><para>EN: Typed pg_dumpall options.</para><para>JA: 型付き pg_dumpall オプションです。</para></param>
    /// <param name="output"><para>EN: SQL-script stream or file destination.</para><para>JA: SQL スクリプトのストリームまたはファイル出力先です。</para></param>
    /// <param name="timeout"><para>EN: Optional process timeout. Null means no PgCliSharp timeout.</para><para>JA: 任意のプロセスタイムアウトです。null の場合、PgCliSharp 側ではタイムアウトを設定しません。</para></param>
    /// <param name="cancellationToken"><para>EN: Token used to cancel execution and terminate the process tree.</para><para>JA: 実行をキャンセルし、プロセスツリーを終了するためのトークンです。</para></param>
    /// <returns><para>EN: Structured execution metadata.</para><para>JA: 構造化された実行メタデータです。</para></returns>
    public async Task<PgDumpAllResult> ExecuteAsync(
        PgDumpAllOptions options,
        PgDumpAllOutput output,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
#if NETSTANDARD2_0
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }
#else
        ArgumentNullException.ThrowIfNull(options);
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

        PgDumpAllValidator.Validate(
            options,
            output,
            Version,
            executableVersion);

        IReadOnlyList<string> arguments =
            PgDumpAllArgumentBuilder.Build(options, output);

        IReadOnlyDictionary<string, string>? environmentVariables =
            options.EnvironmentVariables.Count == 0
                ? null
                : new Dictionary<string, string>(
                    options.EnvironmentVariables,
                    StringComparer.Ordinal);

        var request = new ProcessRunRequest(
            ExecutablePath,
            arguments,
            output.Kind == PgDumpAllOutputKind.StandardOutput
                ? output.StandardOutput
                : null,
            timeout,
            throwOnNonZeroExitCode: true,
            environmentVariables: environmentVariables,
            standardInput: PgDumpAllValidator.GetStandardInput(options));

        ProcessRunResult result =
            await _processRunner.RunAsync(request, cancellationToken)
                .ConfigureAwait(false);

        return new PgDumpAllResult(
            result.ExitCode,
            result.Duration,
            executableVersion.NumericVersion,
            executableVersion.RawVersion,
            result.StandardError);
    }
}
