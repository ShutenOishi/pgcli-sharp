using System.Text;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Tests;

public sealed class PostgreSqlExecutableVersionProviderTests
{
    [Fact]
    public async Task ValidateVersionAsync_MatchingVersion_ReturnsParsedVersionAndCachesByFileIdentity()
    {
        string executablePath = CreateTemporaryExecutableIdentity();

        try
        {
            var runner = new FakeProcessRunner("pg_dump (PostgreSQL) 18.6");
            var provider = new PostgreSqlExecutableVersionProvider(runner);

            PostgreSqlExecutableVersion first = await provider.ValidateVersionAsync(
                executablePath,
                PostgreSqlMajorVersion.V18,
                CancellationToken.None);
            PostgreSqlExecutableVersion second = await provider.ValidateVersionAsync(
                executablePath,
                PostgreSqlMajorVersion.V18,
                CancellationToken.None);

            Assert.Equal(18, first.Major);
            Assert.Same(first, second);
            Assert.Equal(1, runner.InvocationCount);
        }
        finally
        {
            File.Delete(executablePath);
        }
    }

    [Fact]
    public async Task ValidateVersionAsync_Mismatch_ThrowsStructuredException()
    {
        string executablePath = CreateTemporaryExecutableIdentity();

        try
        {
            var runner = new FakeProcessRunner("pg_dump (PostgreSQL) 17.11");
            var provider = new PostgreSqlExecutableVersionProvider(runner);

            PgExecutableVersionMismatchException exception =
                await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(
                    () => provider.ValidateVersionAsync(
                        executablePath,
                        PostgreSqlMajorVersion.V18,
                        CancellationToken.None));

            Assert.Equal(executablePath, exception.ExecutablePath);
            Assert.Equal(PostgreSqlMajorVersion.V18, exception.ExpectedVersion);
            Assert.Equal(17, exception.ActualMajorVersion);
        }
        finally
        {
            File.Delete(executablePath);
        }
    }

    [Fact]
    public async Task GetVersionAsync_UnparseableOutput_ThrowsStructuredException()
    {
        string executablePath = CreateTemporaryExecutableIdentity();

        try
        {
            var runner = new FakeProcessRunner("unexpected output");
            var provider = new PostgreSqlExecutableVersionProvider(runner);

            PgExecutableVersionParseException exception =
                await Assert.ThrowsAsync<PgExecutableVersionParseException>(
                    () => provider.GetVersionAsync(executablePath, CancellationToken.None));

            Assert.Equal(executablePath, exception.ExecutablePath);
            Assert.Contains("unexpected output", exception.VersionOutput, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(executablePath);
        }
    }

    private static string CreateTemporaryExecutableIdentity()
    {
        string path = Path.Combine(Path.GetTempPath(), "pgclisharp-" + Guid.NewGuid().ToString("N"));
        File.WriteAllBytes(path, new byte[] { 0x50, 0x47 });
        return path;
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        private readonly byte[] _versionOutput;

        internal FakeProcessRunner(string versionOutput)
        {
            _versionOutput = Encoding.UTF8.GetBytes(versionOutput);
        }

        internal int InvocationCount { get; private set; }

        public async Task<ProcessRunResult> RunAsync(
            ProcessRunRequest request,
            CancellationToken cancellationToken)
        {
            InvocationCount++;

            Assert.Single(request.Arguments);
            Assert.Equal("--version", request.Arguments[0]);
            Assert.NotNull(request.StandardOutput);

            await request.StandardOutput.WriteAsync(
                _versionOutput,
                0,
                _versionOutput.Length,
                cancellationToken);

            return new ProcessRunResult(0, TimeSpan.Zero, string.Empty);
        }
    }
}
