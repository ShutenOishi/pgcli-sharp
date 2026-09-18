using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class PgRestoreExecutionTests
{
    [Fact]
    public async Task ExecuteAsync_ScriptStdout_IsPreservedAsBytes()
    {
        byte[] expected = { 0, 1, 2, 255, 128 };
        var runner = new FakeRunner("pg_restore", "18.6", expected);
        var restore = new PgRestore(
            "/fake/pg_restore",
            PostgreSqlMajorVersion.V18,
            runner);
        using var output = new MemoryStream();

        PgRestoreResult result = await restore.ExecuteAsync(
            new PgRestoreOptions(),
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToStream(output));

        Assert.Equal(expected, output.ToArray());
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(new Version(18, 6), result.ExecutableVersion);
        Assert.Equal(2, runner.InvocationCount);
        Assert.NotNull(runner.LastRequest);
        Assert.Equal("-", ValueAfter(runner.LastRequest!.Arguments, "--file"));
    }

    [Fact]
    public async Task ExecuteAsync_ArchiveStandardInput_IsForwarded()
    {
        byte[] archiveBytes = { 9, 8, 7, 6 };
        using var archive = new MemoryStream(archiveBytes);
        var runner = new FakeRunner("pg_restore", "18.6", Array.Empty<byte>());
        var restore = new PgRestore(
            "/fake/pg_restore",
            PostgreSqlMajorVersion.V18,
            runner);

        await restore.ExecuteAsync(
            new PgRestoreOptions(),
            PgRestoreInput.FromStandardInput(archive),
            PgRestoreOutput.ToStream(Stream.Null));

        Assert.Equal(archiveBytes, runner.CapturedStandardInput);
        Assert.DoesNotContain("backup.dump", runner.LastRequest!.Arguments);
    }

    [Fact]
    public async Task ExecuteAsync_FilterStandardInput_IsForwarded()
    {
        byte[] filterBytes = Encoding.UTF8.GetBytes("include table public.t\n");
        using var filter = new MemoryStream(filterBytes);
        var options = new PgRestoreOptions();
        options.Filters.Add(PgRestoreFilterSource.FromStandardInput(filter));

        var runner = new FakeRunner("pg_restore", "17.6", Array.Empty<byte>());
        var restore = new PgRestore(
            "/fake/pg_restore",
            PostgreSqlMajorVersion.V17,
            runner);

        await restore.ExecuteAsync(
            options,
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToStream(Stream.Null));

        Assert.Equal(filterBytes, runner.CapturedStandardInput);
        Assert.Equal("-", ValueAfter(runner.LastRequest!.Arguments, "--filter"));
    }

    [Fact]
    public async Task ExecuteAsync_EnvironmentAndTimeout_AreForwarded()
    {
        var options = new PgRestoreOptions();
        options.EnvironmentVariables["PGAPPNAME"] = "phase2-test";
        TimeSpan timeout = TimeSpan.FromSeconds(7);
        var runner = new FakeRunner("pg_restore", "18.6", Array.Empty<byte>());
        var restore = new PgRestore(
            "/fake/pg_restore",
            PostgreSqlMajorVersion.V18,
            runner);

        await restore.ExecuteAsync(
            options,
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToDatabase("appdb"),
            timeout);

        Assert.Equal(timeout, runner.LastRequest!.Timeout);
        Assert.Equal(
            "phase2-test",
            runner.LastRequest.EnvironmentVariables!["PGAPPNAME"]);
        Assert.Null(runner.LastRequest.StandardOutput);
    }

    [Fact]
    public async Task ExecuteAsync_ExecutableVersionMismatch_PreventsRestore()
    {
        var runner = new FakeRunner("pg_restore", "17.11", Array.Empty<byte>());
        var restore = new PgRestore(
            "/fake/pg_restore",
            PostgreSqlMajorVersion.V18,
            runner);

        await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(
            () => restore.ExecuteAsync(
                new PgRestoreOptions(),
                PgRestoreInput.FromFile("backup.dump"),
                PgRestoreOutput.ToStream(Stream.Null)));

        Assert.Equal(1, runner.InvocationCount);
        Assert.Null(runner.LastRequest);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_IsForwarded()
    {
        var runner = new FakeRunner(
            "pg_restore",
            "18.6",
            Array.Empty<byte>(),
            blockUntilCancelled: true);
        var restore = new PgRestore(
            "/fake/pg_restore",
            PostgreSqlMajorVersion.V18,
            runner);
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => restore.ExecuteAsync(
                new PgRestoreOptions(),
                PgRestoreInput.FromFile("backup.dump"),
                PgRestoreOutput.ToDatabase("appdb"),
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
        private readonly string _tool;
        private readonly string _version;
        private readonly byte[] _output;
        private readonly bool _blockUntilCancelled;

        internal FakeRunner(
            string tool,
            string version,
            byte[] output,
            bool blockUntilCancelled = false)
        {
            _tool = tool;
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
                    $"{_tool} (PostgreSQL) {_version}\n");
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
                "restore diagnostic");
        }
    }
}
