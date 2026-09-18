using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class PgDumpAllExecutionTests
{
    [Fact]
    public async Task ExecuteAsync_Stdout_IsPreservedAsBytes()
    {
        byte[] expected = { 0, 65, 66, 255, 10 };
        var runner = new FakeRunner("18.6", expected);
        var dumpAll = new PgDumpAll(
            "/fake/pg_dumpall",
            PostgreSqlMajorVersion.V18,
            runner);
        using var output = new MemoryStream();

        PgDumpAllResult result = await dumpAll.ExecuteAsync(
            new PgDumpAllOptions(),
            PgDumpAllOutput.ToStream(output));

        Assert.Equal(expected, output.ToArray());
        Assert.Equal(new Version(18, 6), result.ExecutableVersion);
        Assert.Equal(2, runner.InvocationCount);
    }

    [Fact]
    public async Task ExecuteAsync_FilterStandardInput_IsForwarded()
    {
        byte[] filterBytes = Encoding.UTF8.GetBytes(
            "exclude database temp_*\n");
        using var filter = new MemoryStream(filterBytes);
        var options = new PgDumpAllOptions();
        options.Filters.Add(PgDumpAllFilterSource.FromStandardInput(filter));

        var runner = new FakeRunner("17.6", Array.Empty<byte>());
        var dumpAll = new PgDumpAll(
            "/fake/pg_dumpall",
            PostgreSqlMajorVersion.V17,
            runner);

        await dumpAll.ExecuteAsync(
            options,
            PgDumpAllOutput.ToStream(Stream.Null));

        Assert.Equal(filterBytes, runner.CapturedStandardInput);
        Assert.Equal("-", ValueAfter(runner.LastRequest!.Arguments, "--filter"));
    }

    [Fact]
    public async Task ExecuteAsync_ConnectionArgumentsEnvironmentAndTimeout_AreForwarded()
    {
        var options = new PgDumpAllOptions
        {
            ConnectionString = "sslmode=require",
            InitialDatabase = "postgres",
            Host = "db-host",
        };
        options.EnvironmentVariables["PGAPPNAME"] = "phase2-dumpall";
        TimeSpan timeout = TimeSpan.FromSeconds(8);
        var runner = new FakeRunner("18.6", Array.Empty<byte>());
        var dumpAll = new PgDumpAll(
            "/fake/pg_dumpall",
            PostgreSqlMajorVersion.V18,
            runner);

        await dumpAll.ExecuteAsync(
            options,
            PgDumpAllOutput.ToFile("cluster.sql"),
            timeout);

        Assert.Equal(
            "sslmode=require",
            ValueAfter(runner.LastRequest!.Arguments, "--dbname"));
        Assert.Equal(
            "postgres",
            ValueAfter(runner.LastRequest.Arguments, "--database"));
        Assert.Equal(timeout, runner.LastRequest.Timeout);
        Assert.Equal(
            "phase2-dumpall",
            runner.LastRequest.EnvironmentVariables!["PGAPPNAME"]);
        Assert.Null(runner.LastRequest.StandardOutput);
    }

    [Fact]
    public async Task ExecuteAsync_ExecutableVersionMismatch_PreventsDump()
    {
        var runner = new FakeRunner("17.11", Array.Empty<byte>());
        var dumpAll = new PgDumpAll(
            "/fake/pg_dumpall",
            PostgreSqlMajorVersion.V18,
            runner);

        await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(
            () => dumpAll.ExecuteAsync(
                new PgDumpAllOptions(),
                PgDumpAllOutput.ToStream(Stream.Null)));

        Assert.Equal(1, runner.InvocationCount);
        Assert.Null(runner.LastRequest);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_IsForwarded()
    {
        var runner = new FakeRunner(
            "18.6",
            Array.Empty<byte>(),
            blockUntilCancelled: true);
        var dumpAll = new PgDumpAll(
            "/fake/pg_dumpall",
            PostgreSqlMajorVersion.V18,
            runner);
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => dumpAll.ExecuteAsync(
                new PgDumpAllOptions(),
                PgDumpAllOutput.ToStream(Stream.Null),
                cancellationToken: cancellation.Token));

        Assert.Equal(2, runner.InvocationCount);
    }

    private static string ValueAfter(
        IReadOnlyList<string> arguments,
        string option)
    {
        int index = arguments.ToList().IndexOf(option);
        Assert.True(index >= 0 && index + 1 < arguments.Count);
        return arguments[index + 1];
    }

    private sealed class FakeRunner : IProcessRunner
    {
        private readonly string _version;
        private readonly byte[] _output;
        private readonly bool _blockUntilCancelled;

        internal FakeRunner(
            string version,
            byte[] output,
            bool blockUntilCancelled = false)
        {
            _version = version;
            _output = output;
            _blockUntilCancelled = blockUntilCancelled;
        }

        internal int InvocationCount { get; private set; }

        internal ProcessRunRequest? LastRequest { get; private set; }

        internal byte[]? CapturedStandardInput { get; private set; }

        public async Task<ProcessRunResult> RunAsync(
            ProcessRunRequest request,
            CancellationToken cancellationToken)
        {
            InvocationCount++;

            if (request.Arguments.Count == 1 &&
                request.Arguments[0] == "--version")
            {
                byte[] versionBytes = Encoding.UTF8.GetBytes(
                    $"pg_dumpall (PostgreSQL) {_version}\n");
                Assert.NotNull(request.StandardOutput);
#if NET8_0_OR_GREATER
                await request.StandardOutput!.WriteAsync(
                    versionBytes.AsMemory(),
                    cancellationToken);
#else
                await request.StandardOutput!.WriteAsync(
                    versionBytes,
                    0,
                    versionBytes.Length,
                    cancellationToken);
#endif
                return new ProcessRunResult(0, TimeSpan.Zero, string.Empty);
            }

            LastRequest = request;

            if (request.StandardInput is not null)
            {
                using var captured = new MemoryStream();
                await request.StandardInput.CopyToAsync(
                    captured,
                    81920,
                    cancellationToken);
                CapturedStandardInput = captured.ToArray();
            }

            if (_blockUntilCancelled)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            if (request.StandardOutput is not null)
            {
#if NET8_0_OR_GREATER
                await request.StandardOutput.WriteAsync(
                    _output.AsMemory(),
                    cancellationToken);
#else
                await request.StandardOutput.WriteAsync(
                    _output,
                    0,
                    _output.Length,
                    cancellationToken);
#endif
            }

            return new ProcessRunResult(
                0,
                TimeSpan.FromMilliseconds(20),
                "dumpall diagnostic");
        }
    }
}
