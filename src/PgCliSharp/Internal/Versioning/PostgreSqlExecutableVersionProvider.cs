using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Internal.Versioning;

internal sealed class PostgreSqlExecutableVersionProvider
{
    private static readonly TimeSpan DefaultProbeTimeout = TimeSpan.FromSeconds(10);
    private static readonly string[] VersionArguments = { "--version" };

    private readonly IProcessRunner _processRunner;
    private readonly TimeSpan _probeTimeout;
    private readonly ConcurrentDictionary<string, PostgreSqlExecutableVersion> _cache =
        new ConcurrentDictionary<string, PostgreSqlExecutableVersion>(StringComparer.Ordinal);

    internal PostgreSqlExecutableVersionProvider(
        IProcessRunner processRunner,
        TimeSpan? probeTimeout = null)
    {
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _probeTimeout = probeTimeout ?? DefaultProbeTimeout;

        if (_probeTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(probeTimeout), probeTimeout, "Probe timeout must be greater than zero.");
        }
    }

    internal async Task<PostgreSqlExecutableVersion> GetVersionAsync(
        string executablePath,
        CancellationToken cancellationToken)
    {
        string? cacheKey = TryCreateCacheKey(executablePath);
        if (cacheKey is not null && _cache.TryGetValue(cacheKey, out PostgreSqlExecutableVersion? cachedVersion))
        {
            return cachedVersion;
        }

        using var standardOutput = new MemoryStream();
        var request = new ProcessRunRequest(
            executablePath,
            VersionArguments,
            standardOutput,
            _probeTimeout);

        ProcessRunResult result = await _processRunner
            .RunAsync(request, cancellationToken)
            .ConfigureAwait(false);

        string output = Encoding.UTF8.GetString(standardOutput.ToArray());
        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            output = output + Environment.NewLine + result.StandardError;
        }

        if (!PostgreSqlVersionParser.TryParse(output, out PostgreSqlExecutableVersion? parsedVersion) ||
            parsedVersion is null)
        {
            throw new PgExecutableVersionParseException(executablePath, output);
        }

        if (cacheKey is not null)
        {
            _cache[cacheKey] = parsedVersion;
        }

        return parsedVersion;
    }

    internal async Task<PostgreSqlExecutableVersion> ValidateVersionAsync(
        string executablePath,
        PostgreSqlMajorVersion expectedVersion,
        CancellationToken cancellationToken)
    {
        _ = PostgreSqlVersionCatalog.Get(expectedVersion);

        PostgreSqlExecutableVersion actualVersion = await GetVersionAsync(
                executablePath,
                cancellationToken)
            .ConfigureAwait(false);

        if (actualVersion.Major != (int)expectedVersion)
        {
            throw new PgExecutableVersionMismatchException(
                executablePath,
                expectedVersion,
                actualVersion.Major);
        }

        return actualVersion;
    }

    private static string? TryCreateCacheKey(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        try
        {
            string fullPath = Path.GetFullPath(executablePath);
            var info = new FileInfo(fullPath);
            if (!info.Exists)
            {
                return null;
            }

            return string.Concat(
                fullPath,
                "\n",
                info.Length.ToString(CultureInfo.InvariantCulture),
                "\n",
                info.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception exception) when (
            exception is ArgumentException ||
            exception is NotSupportedException ||
            exception is PathTooLongException ||
            exception is UnauthorizedAccessException ||
            exception is IOException)
        {
            return null;
        }
    }
}
