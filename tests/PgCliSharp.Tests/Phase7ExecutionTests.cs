using System.Text;
using PgCliSharp.Internal.Execution;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Tests;

public sealed class Phase7ExecutionTests
{
    private static readonly string[] ExpectedArguments1 = new[] { "-D", "owned copy", "-n" };
    private static readonly string[] ExpectedArguments2 = new[] { "--help" };
    [Theory]
    [InlineData(0, PgCtlServerStatus.Running)]
    [InlineData(3, PgCtlServerStatus.NotRunning)]
    [InlineData(4, PgCtlServerStatus.UnavailableDataDirectory)]
    public async Task PgCtl_StatusCodesAreSemantic(int code, PgCtlServerStatus status)
    {
        var runner = new FakeRunner("pg_ctl", "18.1", Array.Empty<byte>(), normalExitCode: code);
        var ctl = new PgCtl("/fake/pg_ctl", PostgreSqlMajorVersion.V18, runner);
        PgCtlResult result = await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Status, DataDirectory = "cluster" });
        Assert.Equal(status, result.ServerStatus);
        Assert.Equal(code, result.ExitCode);
        Assert.False(runner.LastRequest!.ThrowOnNonZeroExitCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public async Task PgCtl_UnexpectedStatusCodesThrow(int code)
    {
        var runner = new FakeRunner("pg_ctl", "18.1", Array.Empty<byte>(), normalExitCode: code);
        var ctl = new PgCtl("/fake/pg_ctl", PostgreSqlMajorVersion.V18, runner);
        await Assert.ThrowsAsync<PgProcessExecutionException>(() => ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Status, DataDirectory = "cluster" }));
    }

    [Fact]
    public async Task PgCtl_NonStatusOperationUsesOrdinaryFailurePolicy()
    {
        var runner = new FakeRunner("pg_ctl", "18.1", Array.Empty<byte>());
        var ctl = new PgCtl("/fake/pg_ctl", PostgreSqlMajorVersion.V18, runner);
        PgCtlResult result = await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Stop, DataDirectory = "cluster" });
        Assert.True(runner.LastRequest!.ThrowOnNonZeroExitCode);
        Assert.Null(result.ServerStatus);
    }

    [Fact]
    public async Task InitDb_EnvironmentStreamsAndTimeoutAreForwarded()
    {
        byte[] expected = Encoding.UTF8.GetBytes("cluster ready\n");
        var runner = new FakeRunner("initdb", "18.1", expected);
        var tool = new InitDb("/fake/initdb", PostgreSqlMajorVersion.V18, runner);
        var options = new InitDbOptions();
        options.EnvironmentVariables["PGDATA"] = "environment cluster";
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();
        using var stdin = new MemoryStream(Encoding.UTF8.GetBytes("password\n"));
        PgServerResult result = await tool.ExecuteAsync(options, new PgServerIo(stdin, stdout, stderr), TimeSpan.FromSeconds(7));
        Assert.Equal(expected, stdout.ToArray());
        Assert.Same(stdin, runner.LastRequest!.StandardInput);
        Assert.Same(stderr, runner.LastRequest.StandardError);
        Assert.Equal(TimeSpan.FromSeconds(7), runner.LastRequest.Timeout);
        Assert.Equal("environment cluster", runner.LastRequest.EnvironmentVariables!["PGDATA"]);
        Assert.Equal(new Version(18, 1), result.ExecutableVersion);
        Assert.True(stdin.CanRead);
        Assert.True(stdout.CanWrite);
    }

    [Fact]
    public async Task PgRewind_InvalidSourceCombinationPreventsEvenVersionProbe()
    {
        var runner = new FakeRunner("pg_rewind", "18.1", Array.Empty<byte>());
        var tool = new PgRewind("/fake/pg_rewind", PostgreSqlMajorVersion.V18, runner);
        await Assert.ThrowsAsync<PgInvalidOptionCombinationException>(() => tool.ExecuteAsync(new PgRewindOptions { TargetDataDirectory = "target", SourceDataDirectory = "source", SourceConnectionString = "host=localhost" }));
        Assert.Equal(0, runner.InvocationCount);
    }

    [Fact]
    public async Task PgChecksums_VersionMismatchPreventsNormalExecution()
    {
        var runner = new FakeRunner("pg_checksums", "17.9", Array.Empty<byte>());
        var tool = new PgChecksums("/fake/pg_checksums", PostgreSqlMajorVersion.V18, runner);
        await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(() => tool.ExecuteAsync(new PgChecksumsOptions { DataDirectory = "cluster" }));
        Assert.Equal(1, runner.InvocationCount);
    }

    [Fact]
    public async Task PgUpgrade_CancellationReachesFiniteRunner()
    {
        var runner = new FakeRunner("pg_upgrade", "18.1", Array.Empty<byte>(), blockUntilCancelled: true);
        var tool = new PgUpgrade("/fake/pg_upgrade", PostgreSqlMajorVersion.V18, runner);
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => tool.ExecuteAsync(new PgUpgradeOptions { OldDataDirectory = "old", NewDataDirectory = "new", OldBinaryDirectory = "oldbin", CheckOnly = true }, cancellationToken: cancellation.Token));
        Assert.Equal(2, runner.InvocationCount);
    }

    [Fact]
    public async Task PgResetWal_DryRunArgumentsReachExecutionAndHelpIsExplicit()
    {
        var runner = new FakeRunner("pg_resetwal", "10.23", Array.Empty<byte>());
        var tool = new PgResetWal("/fake/pg_resetwal", PostgreSqlMajorVersion.V10, runner);
        await tool.ExecuteAsync(new PgResetWalOptions { DataDirectory = "owned copy", DryRun = true });
        Assert.Equal(ExpectedArguments1, runner.LastRequest!.Arguments);
        await tool.GetHelpAsync();
        Assert.Equal(ExpectedArguments2, runner.LastRequest!.Arguments);
    }

    private sealed class FakeRunner : IProcessRunner
    {
        private readonly string _tool;
        private readonly string _version;
        private readonly byte[] _output;
        private readonly bool _blockUntilCancelled;
        private readonly int _normalExitCode;

        internal FakeRunner(
            string tool,
            string version,
            byte[] output,
            bool blockUntilCancelled = false,
            int normalExitCode = 0)
        {
            _tool = tool;
            _version = version;
            _output = output;
            _blockUntilCancelled = blockUntilCancelled;
            _normalExitCode = normalExitCode;
        }

        internal int InvocationCount { get; private set; }
        internal ProcessRunRequest? LastRequest { get; private set; }

        public async Task<ProcessRunResult> RunAsync(ProcessRunRequest request, CancellationToken cancellationToken)
        {
            InvocationCount++;

            if (request.Arguments.Count == 1 && request.Arguments[0] == "--version")
            {
                byte[] version = Encoding.UTF8.GetBytes($"{_tool} (PostgreSQL) {_version}\n");
                Assert.NotNull(request.StandardOutput);
#if NET8_0_OR_GREATER
                await request.StandardOutput!.WriteAsync(version.AsMemory(), cancellationToken);
#else
                await request.StandardOutput!.WriteAsync(version, 0, version.Length, cancellationToken);
#endif
                return new ProcessRunResult(0, TimeSpan.Zero, string.Empty);
            }

            LastRequest = request;

            if (_blockUntilCancelled)
                await Task.Delay(Timeout.Infinite, cancellationToken);

            if (request.StandardOutput is not null)
            {
#if NET8_0_OR_GREATER
                await request.StandardOutput.WriteAsync(_output.AsMemory(), cancellationToken);
#else
                await request.StandardOutput.WriteAsync(_output, 0, _output.Length, cancellationToken);
#endif
            }

            return new ProcessRunResult(_normalExitCode, TimeSpan.FromMilliseconds(20), "phase7 diagnostic");
        }
    }
}
