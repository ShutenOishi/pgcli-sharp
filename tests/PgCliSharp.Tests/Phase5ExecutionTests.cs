using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class Phase5ExecutionTests
{
    [Fact]
    public async Task CreateDb_ForwardsOutputEnvironmentAndTimeout()
    {
        byte[] expected = Encoding.UTF8.GetBytes("CREATE DATABASE\n");
        var runner = new FakeRunner("createdb", "18.2", expected);
        var tool = new CreateDb("/fake/createdb", PostgreSqlMajorVersion.V18, runner);
        var options = new CreateDbOptions { DatabaseName = "appdb" };
        options.EnvironmentVariables["PGAPPNAME"] = "phase5-createdb";
        using var output = new MemoryStream();
        TimeSpan timeout = TimeSpan.FromSeconds(7);

        PgMaintenanceResult result = await tool.ExecuteAsync(
            options,
            new PgMaintenanceIo(standardOutput: output),
            timeout);

        Assert.Equal(expected, output.ToArray());
        Assert.Equal(new Version(18, 2), result.ExecutableVersion);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, runner.InvocationCount);
        Assert.NotNull(runner.LastRequest);
        Assert.Equal(timeout, runner.LastRequest!.Timeout);
        Assert.Equal("phase5-createdb", runner.LastRequest.EnvironmentVariables!["PGAPPNAME"]);
        Assert.Equal("appdb", runner.LastRequest.Arguments[runner.LastRequest.Arguments.Count - 1]);
    }

    [Fact]
    public async Task CreateUser_InteractiveInput_IsForwarded()
    {
        var runner = new FakeRunner("createuser", "18.1", Array.Empty<byte>());
        var tool = new CreateUser("/fake/createuser", PostgreSqlMajorVersion.V18, runner);
        var options = new CreateUserOptions { Interactive = true };
        using var input = new MemoryStream(Encoding.UTF8.GetBytes("role1\ny\n"));
        using var output = new MemoryStream();

        await tool.ExecuteAsync(options, new PgMaintenanceIo(input, output));

        Assert.Same(input, runner.LastRequest!.StandardInput);
        Assert.Same(output, runner.LastRequest.StandardOutput);
        Assert.Contains("--interactive", runner.LastRequest.Arguments);
    }

    [Fact]
    public async Task PgAmcheck_ExecutableVersionMismatch_PreventsNormalExecution()
    {
        var runner = new FakeRunner("pg_amcheck", "17.9", Array.Empty<byte>());
        var tool = new PgAmcheck("/fake/pg_amcheck", PostgreSqlMajorVersion.V18, runner);

        await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(
            () => tool.ExecuteAsync(new PgAmcheckOptions { DatabaseName = "appdb" }));

        Assert.Equal(1, runner.InvocationCount);
        Assert.Null(runner.LastRequest);
    }

    [Theory]
    [InlineData(0, PgIsReadyStatus.AcceptingConnections)]
    [InlineData(1, PgIsReadyStatus.RejectingConnections)]
    [InlineData(2, PgIsReadyStatus.NoResponse)]
    [InlineData(3, PgIsReadyStatus.NoAttempt)]
    public async Task PgIsReady_ExitCodesZeroThroughThree_AreSemanticStatuses(int exitCode, PgIsReadyStatus expected)
    {
        var runner = new FakeRunner("pg_isready", "18.0", Array.Empty<byte>(), normalExitCode: exitCode);
        var tool = new PgIsReady("/fake/pg_isready", PostgreSqlMajorVersion.V18, runner);

        PgIsReadyResult result = await tool.ExecuteAsync(new PgIsReadyOptions { ConnectTimeoutSeconds = 0 });

        Assert.Equal(exitCode, result.ExitCode);
        Assert.Equal(expected, result.Status);
        Assert.False(runner.LastRequest!.ThrowOnNonZeroExitCode);
    }

    [Fact]
    public async Task PgIsReady_ExitCodeOutsideSemanticRange_IsExecutionFailure()
    {
        var runner = new FakeRunner("pg_isready", "18.0", Array.Empty<byte>(), normalExitCode: 4);
        var tool = new PgIsReady("/fake/pg_isready", PostgreSqlMajorVersion.V18, runner);

        await Assert.ThrowsAsync<PgProcessExecutionException>(
            () => tool.ExecuteAsync(new PgIsReadyOptions()));

        Assert.False(runner.LastRequest!.ThrowOnNonZeroExitCode);
    }

    [Fact]
    public async Task ClusterDb_CancellationToken_IsForwarded()
    {
        var runner = new FakeRunner("clusterdb", "18.1", Array.Empty<byte>(), blockUntilCancelled: true);
        var tool = new ClusterDb("/fake/clusterdb", PostgreSqlMajorVersion.V18, runner);
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => tool.ExecuteAsync(new ClusterDbOptions(), cancellationToken: cancellation.Token));

        Assert.Equal(2, runner.InvocationCount);
    }

    [Fact]
    public async Task DropDb_ArgumentsReachExecutionInDeterministicOrder()
    {
        var runner = new FakeRunner("dropdb", "18.3", Array.Empty<byte>());
        var tool = new DropDb("/fake/dropdb", PostgreSqlMajorVersion.V18, runner);
        var options = new DropDbOptions
        {
            Host = "localhost",
            Port = 5432,
            Username = "admin",
            IfExists = true,
            Force = true,
            DatabaseName = "old_db",
        };

        await tool.ExecuteAsync(options);

        IReadOnlyList<string> args = runner.LastRequest!.Arguments;
        Assert.Equal("localhost", ValueAfter(args, "--host"));
        Assert.Equal("5432", ValueAfter(args, "--port"));
        Assert.Equal("admin", ValueAfter(args, "--username"));
        Assert.Contains("--if-exists", args);
        Assert.Contains("--force", args);
        Assert.Equal("old_db", args[args.Count - 1]);
    }

    [Fact]
    public async Task VacuumDb_EnvironmentAndTextOutput_AreForwarded()
    {
        byte[] expected = Encoding.UTF8.GetBytes("vacuuming database appdb\n");
        var runner = new FakeRunner("vacuumdb", "18.1", expected);
        var tool = new VacuumDb("/fake/vacuumdb", PostgreSqlMajorVersion.V18, runner);
        var options = new VacuumDbOptions { DatabaseName = "appdb", Analyze = true };
        options.EnvironmentVariables["PG_COLOR"] = "never";
        using var output = new MemoryStream();

        await tool.ExecuteAsync(options, new PgMaintenanceIo(standardOutput: output));

        Assert.Equal(expected, output.ToArray());
        Assert.Equal("never", runner.LastRequest!.EnvironmentVariables!["PG_COLOR"]);
        Assert.Contains("--analyze", runner.LastRequest.Arguments);
    }

    private static string ValueAfter(IReadOnlyList<string> args, string option)
    {
        int index = args.ToList().IndexOf(option);
        Assert.True(index >= 0 && index + 1 < args.Count);
        return args[index + 1];
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

            return new ProcessRunResult(_normalExitCode, TimeSpan.FromMilliseconds(20), "phase5 diagnostic");
        }
    }
}
