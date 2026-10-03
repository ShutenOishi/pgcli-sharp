using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class PsqlCompletionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(262144)]
    public async Task CompleteAsync_SignalsEofAndWaitsForBinaryDrain_WithoutDisposingCallerStreams(int size)
    {
        for (int iteration = 0; iteration < 10; iteration++)
            await VerifyBinaryCompletionAsync(size);
    }

    private static async Task VerifyBinaryCompletionAsync(int size)
    {
        string? traceDirectory = Environment.GetEnvironmentVariable("PGCLI_TEST_TRACE_DIRECTORY");
        string? trace = traceDirectory is null ? null : Path.Combine(traceDirectory, $"{typeof(object).Assembly.GetName().Version}-{size}-{Guid.NewGuid():N}");
        void Record(string stage)
        {
            if (trace is null) return;
            Directory.CreateDirectory(traceDirectory!);
            File.AppendAllText(trace + ".parent.log", $"{DateTime.UtcNow:O} {stage}\n");
        }
        using var output = new MemoryStream();
        using var error = new MemoryStream();
        (string executable, string[] arguments) = ManagedTestProcess.Command();
        Record("start-enter");
        IProcessSession process = new ProcessSessionRunner().Start(new ProcessSessionStartRequest(executable, arguments, output, error, TimeSpan.FromSeconds(15), trace is null ? null : new Dictionary<string, string> { ["PGCLI_TEST_TRACE_FILE"] = trace + ".child.log" }), CancellationToken.None);
        Record("start-returned");
        using var session = new PsqlSession(process, executable, new Version(18, 6), "18.6");
        byte[] payload = Enumerable.Range(0, size).Select(index => (byte)(index % 256)).ToArray();
        Record("write-enter");
#if NET8_0_OR_GREATER
        await session.StandardInput.WriteAsync(payload.AsMemory());
#else
        await session.StandardInput.WriteAsync(payload, 0, payload.Length);
#endif
        Record("write-returned");
        Record("complete-enter");
        Task<PsqlSessionResult> completion = session.CompleteAsync();
        Record("complete-returned");
        PsqlSessionResult result = await completion;
        Record("completion-finished");
        Assert.Equal(PsqlExitStatus.Success, result.Status);
        Assert.Same(result, await session.CompleteAsync());
        Assert.Equal(payload, output.ToArray());
        string stages = Encoding.UTF8.GetString(error.ToArray()).Replace("\r", string.Empty);
        Assert.Equal("ready\neof\ndrained\n", stages);
        Assert.True(output.CanWrite);
        Assert.True(error.CanWrite);
    }

    [Fact]
    public async Task CompleteAsync_CancellationTerminatesAndPreservesCallersToken()
    {
        var process = new ControlledSession();
        using var session = new PsqlSession(process, "/fake/psql", new Version(18, 6), "18.6");
        using var cancellation = new CancellationTokenSource();
        Task<PsqlSessionResult> finishing = session.CompleteAsync(cancellation.Token);
        Assert.True(process.InputCompleted);
        Assert.False(finishing.IsCompleted);
        cancellation.Cancel();
        OperationCanceledException failure = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => finishing);
        Assert.True(process.Canceled);
        Assert.Equal(cancellation.Token, failure.CancellationToken);
        Assert.True(finishing.IsCanceled);
    }

    [Fact]
    public async Task CompleteAsync_PreservesOriginalProcessFailure()
    {
        var process = new ControlledSession();
        using var session = new PsqlSession(process, "/fake/psql", new Version(18, 6), "18.6");
        Task<PsqlSessionResult> finishing = session.CompleteAsync();
        process.Exit(9);
        PgProcessExecutionException failure = await Assert.ThrowsAsync<PgProcessExecutionException>(() => finishing);
        Assert.Equal(9, failure.ExitCode);
    }

    private sealed class ControlledSession : IProcessSession
    {
        private readonly TaskCompletionSource<ProcessSessionResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Stream StandardInput => Stream.Null;
        public Task<ProcessSessionResult> Completion => _completion.Task;
        internal bool InputCompleted { get; private set; }
        internal bool Canceled { get; private set; }
        public void CompleteInput() => InputCompleted = true;
        public void Cancel() { Canceled = true; _completion.TrySetCanceled(); }
        public void Dispose() { }
        internal void Exit(int code) => _completion.SetResult(new ProcessSessionResult(code, TimeSpan.Zero));
    }
}
