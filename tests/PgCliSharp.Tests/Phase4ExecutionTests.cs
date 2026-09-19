using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class Phase4ExecutionTests
{
    [Fact]
    public async Task PgBaseBackup_TarStdout_IsBinarySafeAndForwardsEnvironmentAndTimeout()
    {
        byte[] expected = { 0, 1, 2, 255, 10 };
        var runner = new FakeRunner("pg_basebackup", "18.6", expected);
        var tool = new PgBaseBackup("/fake/pg_basebackup", PostgreSqlMajorVersion.V18, runner);
        var options = new PgBaseBackupOptions();
        options.EnvironmentVariables["PGAPPNAME"] = "phase4-basebackup";
        using var output = new MemoryStream();
        TimeSpan timeout = TimeSpan.FromSeconds(9);

        PgBaseBackupResult result = await tool.ExecuteAsync(
            options,
            PgBaseBackupDestination.ToStream(output),
            timeout);

        Assert.Equal(expected, output.ToArray());
        Assert.Equal(new Version(18, 6), result.ExecutableVersion);
        Assert.Equal(2, runner.InvocationCount);
        Assert.NotNull(runner.LastRequest);
        Assert.Equal(timeout, runner.LastRequest!.Timeout);
        Assert.Equal("phase4-basebackup", runner.LastRequest.EnvironmentVariables!["PGAPPNAME"]);
        Assert.Equal("-", ValueAfter(runner.LastRequest.Arguments, "--pgdata"));
        Assert.Equal("tar", ValueAfter(runner.LastRequest.Arguments, "--format"));
    }

    [Fact]
    public async Task PgRecvLogical_Stdout_IsBinarySafe()
    {
        byte[] expected = Encoding.UTF8.GetBytes("logical row\n");
        var runner = new FakeRunner("pg_recvlogical", "18.2", expected);
        var tool = new PgRecvLogical("/fake/pg_recvlogical", PostgreSqlMajorVersion.V18, runner);
        var options = new PgRecvLogicalOptions
        {
            Action = PgRecvLogicalAction.Start,
            Database = "appdb",
            Slot = "slot1",
        };
        using var output = new MemoryStream();

        PgRecvLogicalResult result = await tool.ExecuteAsync(
            options,
            PgRecvLogicalOutput.ToStream(output));

        Assert.Equal(expected, output.ToArray());
        Assert.Equal(new Version(18, 2), result.ExecutableVersion);
        Assert.Equal("-", ValueAfter(runner.LastRequest!.Arguments, "--file"));
    }

    [Fact]
    public async Task PgReceiveWal_ExecutableVersionMismatch_PreventsNormalExecution()
    {
        var runner = new FakeRunner("pg_receivewal", "17.9", Array.Empty<byte>());
        var tool = new PgReceiveWal("/fake/pg_receivewal", PostgreSqlMajorVersion.V18, runner);

        await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(
            () => tool.ExecuteAsync(new PgReceiveWalOptions { Directory = "wal" }));

        Assert.Equal(1, runner.InvocationCount);
        Assert.Null(runner.LastRequest);
    }

    [Fact]
    public async Task PgVerifyBackup_ExecutesPositionalInputAndTypedFormat()
    {
        var runner = new FakeRunner("pg_verifybackup", "18.1", Array.Empty<byte>());
        var tool = new PgVerifyBackup("/fake/pg_verifybackup", PostgreSqlMajorVersion.V18, runner);
        var options = new PgVerifyBackupOptions
        {
            Format = PgVerifyBackupFormat.Tar,
            NoParseWal = true,
        };
        options.IgnoredPaths.Add("pg_wal/archive_status");

        await tool.ExecuteAsync(options, new PgVerifyBackupInput("backup.tar"));

        Assert.Equal("tar", ValueAfter(runner.LastRequest!.Arguments, "--format"));
        Assert.Equal("pg_wal/archive_status", ValueAfter(runner.LastRequest.Arguments, "--ignore"));
        Assert.Equal("backup.tar", runner.LastRequest.Arguments[^1]);
    }

    [Fact]
    public async Task PgCombineBackup_PreservesInputOrder()
    {
        var runner = new FakeRunner("pg_combinebackup", "18.0", Array.Empty<byte>());
        var tool = new PgCombineBackup("/fake/pg_combinebackup", PostgreSqlMajorVersion.V18, runner);
        var options = new PgCombineBackupOptions
        {
            OutputDirectory = "combined",
            CopyMethod = PgCombineBackupCopyMethod.Copy,
        };
        options.InputDirectories.Add("full");
        options.InputDirectories.Add("inc1");
        options.InputDirectories.Add("inc2");

        await tool.ExecuteAsync(options);

        Assert.Equal("combined", ValueAfter(runner.LastRequest!.Arguments, "--output"));
        Assert.Contains("--copy", runner.LastRequest.Arguments);
        Assert.Equal(new[] { "full", "inc1", "inc2" }, runner.LastRequest.Arguments.Skip(runner.LastRequest.Arguments.Count - 3));
    }

    [Fact]
    public async Task PgReceiveWal_CancellationToken_IsForwarded()
    {
        var runner = new FakeRunner("pg_receivewal", "18.1", Array.Empty<byte>(), blockUntilCancelled: true);
        var tool = new PgReceiveWal("/fake/pg_receivewal", PostgreSqlMajorVersion.V18, runner);
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => tool.ExecuteAsync(
                new PgReceiveWalOptions { Directory = "wal" },
                cancellationToken: cancellation.Token));

        Assert.Equal(2, runner.InvocationCount);
    }

    [Fact]
    public async Task PgCombineBackup_EnvironmentAndTimeout_AreForwarded()
    {
        var runner = new FakeRunner("pg_combinebackup", "17.3", Array.Empty<byte>());
        var tool = new PgCombineBackup("/fake/pg_combinebackup", PostgreSqlMajorVersion.V17, runner);
        var options = new PgCombineBackupOptions { OutputDirectory = "combined" };
        options.InputDirectories.Add("full");
        options.EnvironmentVariables["PG_COLOR"] = "never";
        TimeSpan timeout = TimeSpan.FromSeconds(4);

        await tool.ExecuteAsync(options, timeout);

        Assert.Equal(timeout, runner.LastRequest!.Timeout);
        Assert.Equal("never", runner.LastRequest.EnvironmentVariables!["PG_COLOR"]);
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

        internal FakeRunner(string tool, string version, byte[] output, bool blockUntilCancelled = false)
        {
            _tool = tool;
            _version = version;
            _output = output;
            _blockUntilCancelled = blockUntilCancelled;
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

            return new ProcessRunResult(0, TimeSpan.FromMilliseconds(20), "phase4 diagnostic");
        }
    }
}
