using System.Runtime.InteropServices;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class ProcessSessionRunnerTests
{
    [Fact]
    public async Task Session_AllowsWritesAfterStart_AndExplicitEof()
    {
        byte[] first = { 0, 1, 2, 255 };
        byte[] second = { 10, 11, 12 };
        using var output = new MemoryStream();
        using var error = new MemoryStream();

        (string executable, string[] arguments) = GetBinaryEchoCommand();
        var request = new ProcessSessionStartRequest(
            executable,
            arguments,
            output,
            error);
        var runner = new ProcessSessionRunner();

        using IProcessSession session = runner.Start(
            request,
            CancellationToken.None);

#if NET8_0_OR_GREATER
        await session.StandardInput.WriteAsync(
            first.AsMemory(),
            CancellationToken.None);
        await session.StandardInput.WriteAsync(
            second.AsMemory(),
            CancellationToken.None);
#else
        await session.StandardInput.WriteAsync(
            first,
            0,
            first.Length,
            CancellationToken.None);
        await session.StandardInput.WriteAsync(
            second,
            0,
            second.Length,
            CancellationToken.None);
#endif

        session.CompleteInput();

        ProcessSessionResult result = await session.Completion;

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(first.Concat(second).ToArray(), output.ToArray());
        Assert.Empty(error.ToArray());
    }

    [Fact]
    public async Task Session_Timeout_TerminatesProcessAndThrowsTimeout()
    {
        (string executable, string[] arguments) = GetLongRunningCommand();
        var request = new ProcessSessionStartRequest(
            executable,
            arguments,
            Stream.Null,
            Stream.Null,
            TimeSpan.FromMilliseconds(150));
        var runner = new ProcessSessionRunner();

        using IProcessSession session = runner.Start(
            request,
            CancellationToken.None);

        PgProcessTimeoutException exception =
            await Assert.ThrowsAsync<PgProcessTimeoutException>(
                () => session.Completion);

        Assert.Equal(executable, exception.ExecutablePath);
    }

    [Fact]
    public async Task Session_ManualCancellation_TerminatesProcess()
    {
        (string executable, string[] arguments) = GetLongRunningCommand();
        var request = new ProcessSessionStartRequest(
            executable,
            arguments,
            Stream.Null,
            Stream.Null);
        var runner = new ProcessSessionRunner();

        using IProcessSession session = runner.Start(
            request,
            CancellationToken.None);

        await Task.Delay(100);
        session.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => session.Completion);
    }

#if NET8_0_OR_GREATER
    [Fact]
    public async Task Session_OutputWriteFault_TerminatesProducerAndPropagatesOriginalFailure()
    {
        (string executable, string[] arguments) = GetContinuousOutputCommand();
        using var output = new ThrowingWriteStream();
        var request = new ProcessSessionStartRequest(
            executable,
            arguments,
            output,
            Stream.Null);
        var runner = new ProcessSessionRunner();

        using IProcessSession session = runner.Start(
            request,
            CancellationToken.None);

        Task completed = await Task.WhenAny(
            session.Completion,
            Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(session.Completion, completed);
        await Assert.ThrowsAsync<IOException>(
            () => session.Completion);
        Assert.False(output.IsDisposed);
    }
#endif

#if NET48
    [Fact]
    public async Task Session_LegacyInputBuffer_AppliesBackpressureAndHonorsWriteCancellation()
    {
        (string executable, string[] arguments) = GetLongRunningCommand();
        var request = new ProcessSessionStartRequest(
            executable,
            arguments,
            Stream.Null,
            Stream.Null);
        var runner = new ProcessSessionRunner();

        using IProcessSession session = runner.Start(
            request,
            CancellationToken.None);
        using var writeCancellation = new CancellationTokenSource(
            TimeSpan.FromMilliseconds(250));

        byte[] payload = new byte[4 * 1024 * 1024];

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => session.StandardInput.WriteAsync(
                payload,
                0,
                payload.Length,
                writeCancellation.Token));

        session.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => session.Completion);
    }

    [Fact]
    public async Task Session_LegacyInputBuffer_ReleasesBlockedWriterWhenSessionEnds()
    {
        (string executable, string[] arguments) = GetLongRunningCommand();
        var request = new ProcessSessionStartRequest(
            executable,
            arguments,
            Stream.Null,
            Stream.Null);
        var runner = new ProcessSessionRunner();

        using IProcessSession session = runner.Start(
            request,
            CancellationToken.None);

        byte[] payload = new byte[4 * 1024 * 1024];
        Task writeTask = session.StandardInput.WriteAsync(
            payload,
            0,
            payload.Length,
            CancellationToken.None);

        await Task.Delay(250);
        Assert.False(writeTask.IsCompleted);

        session.Cancel();

        Task completedWrite = await Task.WhenAny(
            writeTask,
            Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(writeTask, completedWrite);
        await Assert.ThrowsAnyAsync<Exception>(() => writeTask);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => session.Completion);
    }
#endif

    private static (string Executable, string[] Arguments) GetBinaryEchoCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string powerShell = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "WindowsPowerShell",
                "v1.0",
                "powershell.exe");
            const string Script =
                "[Console]::OpenStandardInput().CopyTo([Console]::OpenStandardOutput())";
            return (
                powerShell,
                new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-Command", Script });
        }

        return ("/bin/cat", Array.Empty<string>());
    }

    private static (string Executable, string[] Arguments) GetContinuousOutputCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string powerShell = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "WindowsPowerShell",
                "v1.0",
                "powershell.exe");
            const string Script =
                "while ($true) { [Console]::Out.Write(('x' * 4096)) }";
            return (
                powerShell,
                new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-Command", Script });
        }

        return (
            "/bin/sh",
            new[] { "-c", "while :; do printf 'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\\n'; done" });
    }

    private sealed class ThrowingWriteStream : Stream
    {
        internal bool IsDisposed { get; private set; }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !IsDisposed;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new IOException("Injected session output write failure.");

        public override Task WriteAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) =>
            Task.FromException(
                new IOException("Injected session output write failure."));

#if NET8_0_OR_GREATER
        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException(
                new IOException("Injected session output write failure."));
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();
        public override void SetLength(long value) =>
            throw new NotSupportedException();
    }

    private static (string Executable, string[] Arguments) GetLongRunningCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string windowsDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.Windows);
            return (
                Path.Combine(windowsDirectory, "System32", "ping.exe"),
                new[] { "127.0.0.1", "-n", "6" });
        }

        return ("/bin/sleep", new[] { "5" });
    }
}
