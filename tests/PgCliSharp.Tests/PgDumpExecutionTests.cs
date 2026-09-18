using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class PgDumpExecutionTests
{
    [Fact]
    public async Task ExecuteAsync_BinaryStdout_IsPreservedWithoutTextConversion()
    {
        byte[] expected = { 0, 1, 2, 255, 128 };
        var runner = new PgDumpFakeRunner("18.6", expected);
        var pgDump = new PgDump(
            "/fake/pg_dump",
            PostgreSqlMajorVersion.V18,
            runner);
        using var output = new MemoryStream();

        PgDumpResult result = await pgDump.ExecuteAsync(
            new PgDumpOptions { Format = PgDumpFormat.Custom },
            PgDumpOutput.ToStream(output));

        Assert.Equal(expected, output.ToArray());
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(new Version(18, 6), result.ExecutableVersion);
        Assert.Equal(2, runner.InvocationCount);
    }

    [Fact]
    public async Task ExecuteAsync_FilterStandardInput_IsForwardedAsBytes()
    {
        byte[] filterBytes = Encoding.UTF8.GetBytes("include table public.*\n");
        using var filterStream = new MemoryStream(filterBytes);
        var options = new PgDumpOptions();
        options.Filters.Add(PgDumpFilterSource.FromStandardInput(filterStream));

        var runner = new PgDumpFakeRunner("17.6", Array.Empty<byte>());
        var pgDump = new PgDump(
            "/fake/pg_dump",
            PostgreSqlMajorVersion.V17,
            runner);

        await pgDump.ExecuteAsync(
            options,
            PgDumpOutput.ToStream(Stream.Null));

        Assert.Equal(filterBytes, runner.CapturedStandardInput);
        Assert.NotNull(runner.LastDumpRequest);
        Assert.Contains("--filter", runner.LastDumpRequest!.Arguments);
        Assert.Contains("-", runner.LastDumpRequest.Arguments);
    }

    [Fact]
    public async Task ExecuteAsync_ExecutableVersionMismatch_PreventsDumpExecution()
    {
        var runner = new PgDumpFakeRunner("17.9", Array.Empty<byte>());
        var pgDump = new PgDump(
            "/fake/pg_dump",
            PostgreSqlMajorVersion.V18,
            runner);

        await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(
            () => pgDump.ExecuteAsync(
                new PgDumpOptions(),
                PgDumpOutput.ToStream(Stream.Null)));

        Assert.Equal(1, runner.InvocationCount);
        Assert.Null(runner.LastDumpRequest);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_IsForwarded()
    {
        var runner = new PgDumpFakeRunner(
            "18.6",
            Array.Empty<byte>(),
            blockDumpUntilCancelled: true);
        var pgDump = new PgDump(
            "/fake/pg_dump",
            PostgreSqlMajorVersion.V18,
            runner);
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => pgDump.ExecuteAsync(
                new PgDumpOptions(),
                PgDumpOutput.ToStream(Stream.Null),
                cancellationToken: cancellation.Token));

        Assert.Equal(2, runner.InvocationCount);
    }

    [Fact]
    public async Task ExecuteAsync_Timeout_IsForwardedToProcessRequest()
    {
        var runner = new PgDumpFakeRunner("18.6", Array.Empty<byte>());
        var pgDump = new PgDump(
            "/fake/pg_dump",
            PostgreSqlMajorVersion.V18,
            runner);
        TimeSpan timeout = TimeSpan.FromSeconds(7);

        await pgDump.ExecuteAsync(
            new PgDumpOptions(),
            PgDumpOutput.ToStream(Stream.Null),
            timeout);

        Assert.NotNull(runner.LastDumpRequest);
        Assert.Equal(timeout, runner.LastDumpRequest!.Timeout);
    }

    [Fact]
    public async Task ExecuteAsync_RepeatableArgumentsRemainSeparateTokens()
    {
        var options = new PgDumpOptions();
        options.Schemas.Add("schema one");
        options.Schemas.Add("schema two");

        var runner = new PgDumpFakeRunner("18.6", Array.Empty<byte>());
        var pgDump = new PgDump(
            "/fake/pg_dump",
            PostgreSqlMajorVersion.V18,
            runner);

        await pgDump.ExecuteAsync(
            options,
            PgDumpOutput.ToStream(Stream.Null));

        Assert.NotNull(runner.LastDumpRequest);
        IReadOnlyList<string> arguments = runner.LastDumpRequest!.Arguments;
        Assert.Contains("schema one", arguments);
        Assert.Contains("schema two", arguments);
    }

    private sealed class PgDumpFakeRunner : IProcessRunner
    {
        private readonly byte[] _dumpOutput;
        private readonly string _version;
        private readonly bool _blockDumpUntilCancelled;

        internal PgDumpFakeRunner(
            string version,
            byte[] dumpOutput,
            bool blockDumpUntilCancelled = false)
        {
            _version = version;
            _dumpOutput = dumpOutput;
            _blockDumpUntilCancelled = blockDumpUntilCancelled;
        }

        internal int InvocationCount { get; private set; }

        internal ProcessRunRequest? LastDumpRequest { get; private set; }

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
                    $"pg_dump (PostgreSQL) {_version}\n");

                Assert.NotNull(request.StandardOutput);
                await request.StandardOutput!.WriteAsync(
                    versionBytes,
                    0,
                    versionBytes.Length,
                    cancellationToken);

                return new ProcessRunResult(0, TimeSpan.Zero, string.Empty);
            }

            LastDumpRequest = request;

            if (request.StandardInput is not null)
            {
                using var captured = new MemoryStream();
                await request.StandardInput.CopyToAsync(
                    captured,
                    81920,
                    cancellationToken);
                CapturedStandardInput = captured.ToArray();
            }

            if (_blockDumpUntilCancelled)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            if (request.StandardOutput is not null)
            {
                await request.StandardOutput.WriteAsync(
                    _dumpOutput,
                    0,
                    _dumpOutput.Length,
                    cancellationToken);
            }

            return new ProcessRunResult(
                0,
                TimeSpan.FromMilliseconds(25),
                "diagnostic");
        }
    }
}
