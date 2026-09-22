using System.Diagnostics;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class ProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_Timeout_TerminatesProcessAndThrowsTimeoutException()
    {
        (string executable, string[] arguments) = GetLongRunningCommand();
        using var output = new MemoryStream();
        var request = new ProcessRunRequest(
            executable,
            arguments,
            output,
            TimeSpan.FromMilliseconds(150));
        var runner = new ProcessRunner();

        PgProcessTimeoutException exception =
            await Assert.ThrowsAsync<PgProcessTimeoutException>(
                () => runner.RunAsync(request, CancellationToken.None));

        Assert.Equal(executable, exception.ExecutablePath);
        Assert.Equal(TimeSpan.FromMilliseconds(150), exception.Timeout);
    }

    [Fact]
    public async Task RunAsync_Cancellation_TerminatesProcessAndPreservesCancellationSemantics()
    {
        (string executable, string[] arguments) = GetLongRunningCommand();
        using var output = new MemoryStream();
        var request = new ProcessRunRequest(executable, arguments, output);
        var runner = new ProcessRunner();
        using var cancellation = new CancellationTokenSource(
            TimeSpan.FromMilliseconds(150));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => runner.RunAsync(request, cancellation.Token));
    }

    [Fact]
    public async Task RunAsync_ArgumentWithSpaces_IsPassedAsOneToken()
    {
        string value = "value with spaces";
        string dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
            ?? "dotnet";
        using var output = new MemoryStream();
        var request = new ProcessRunRequest(
            dotnetHost,
            new[] { value },
            output,
            throwOnNonZeroExitCode: false);
        var runner = new ProcessRunner();

        ProcessRunResult result = await runner.RunAsync(
            request,
            CancellationToken.None);

        output.Position = 0;
        using var reader = new StreamReader(output);
        string standardOutput = await reader.ReadToEndAsync();
        string combinedOutput = standardOutput + Environment.NewLine + result.StandardError;

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains(value, combinedOutput);
    }

    [Fact]
    public async Task RunAsync_Cancellation_TerminatesDescendantProcessTreeOnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        string pidFile = Path.Combine(
            Path.GetTempPath(),
            "pgclisharp-child-" + Guid.NewGuid().ToString("N") + ".pid");

        string powerShell = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");

        string escapedPidFile = pidFile.Replace("'", "''");
        string script =
            "$child = Start-Process -FilePath $env:ComSpec " +
            "-ArgumentList '/d','/c','ping 127.0.0.1 -n 30 > nul' -PassThru; " +
            "[IO.File]::WriteAllText('" + escapedPidFile + "', $child.Id.ToString()); " +
            "Wait-Process -Id $child.Id";

        var request = new ProcessRunRequest(
            powerShell,
            new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-Command", script },
            Stream.Null);
        var runner = new ProcessRunner();
        using var cancellation = new CancellationTokenSource(
            TimeSpan.FromSeconds(30));

        Task<ProcessRunResult> runTask = runner.RunAsync(
            request,
            cancellation.Token);

        try
        {
            bool pidCreated = await WaitForFileAsync(
                pidFile,
                TimeSpan.FromSeconds(15));

            Assert.True(
                pidCreated,
                "The descendant process PID file was not created after the child process started.");

            int childProcessId = int.Parse(
                File.ReadAllText(pidFile),
                System.Globalization.CultureInfo.InvariantCulture);

            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => runTask);

            bool descendantExited = HasProcessExited(childProcessId);
            Assert.True(
                descendantExited,
                "The descendant process remained alive after cancellation.");
        }
        finally
        {
            cancellation.Cancel();

            if (File.Exists(pidFile))
            {
                File.Delete(pidFile);
            }
        }
    }

    [Fact]
    public async Task RunAsync_BinaryStandardOutput_PreservesBytesOnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        string powerShell = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");
        const string Script =
            "[Console]::OpenStandardOutput().Write([byte[]](0,1,2,255),0,4)";

        using var output = new MemoryStream();
        var request = new ProcessRunRequest(
            powerShell,
            new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-Command", Script },
            output);
        var runner = new ProcessRunner();

        ProcessRunResult result = await runner.RunAsync(
            request,
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(new byte[] { 0, 1, 2, 255 }, output.ToArray());
    }


    [Fact]
    public async Task RunAsync_BinaryStandardInput_IsForwardedByteForByte()
    {
        byte[] expected = { 0, 1, 2, 255, 128, 10 };
        using var input = new MemoryStream(expected);
        using var output = new MemoryStream();

        (string executable, string[] arguments) = GetBinaryEchoCommand();
        var request = new ProcessRunRequest(
            executable,
            arguments,
            standardOutput: output,
            standardInput: input);
        var runner = new ProcessRunner();

        ProcessRunResult result = await runner.RunAsync(
            request,
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expected, output.ToArray());
    }

#if NET8_0_OR_GREATER
    [Fact]
    public async Task RunAsync_OutputWriteFault_TerminatesProducerAndPropagatesOriginalFailure()
    {
        (string executable, string[] arguments) = GetContinuousOutputCommand();
        using var output = new ThrowingWriteStream();
        var request = new ProcessRunRequest(executable, arguments, output);
        var runner = new ProcessRunner();

        Task<ProcessRunResult> runTask = runner.RunAsync(
            request,
            CancellationToken.None);

        Task completed = await Task.WhenAny(
            runTask,
            Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(runTask, completed);
        await Assert.ThrowsAsync<IOException>(() => runTask);
        Assert.False(output.IsDisposed);
    }

    [Fact]
    public async Task RunAsync_InputReadFault_TerminatesConsumerAndPropagatesOriginalFailure()
    {
        (string executable, string[] arguments) = GetBinaryEchoCommand();
        using var input = new ThrowingReadStream();
        var request = new ProcessRunRequest(
            executable,
            arguments,
            standardOutput: Stream.Null,
            standardInput: input);
        var runner = new ProcessRunner();

        Task<ProcessRunResult> runTask = runner.RunAsync(
            request,
            CancellationToken.None);

        Task completed = await Task.WhenAny(
            runTask,
            Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(runTask, completed);
        await Assert.ThrowsAsync<IOException>(() => runTask);
        Assert.False(input.IsDisposed);
    }
#endif

    private static async Task<bool> WaitForFileAsync(
        string path,
        TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (File.Exists(path))
            {
                return true;
            }

            await Task.Delay(50);
        }

        return File.Exists(path);
    }

    private static bool HasProcessExited(int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            if (process.WaitForExit(2000))
            {
                return true;
            }

            try
            {
                process.Kill();
            }
            catch (InvalidOperationException)
            {
                return true;
            }

            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }


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
            throw new IOException("Injected output write failure.");

        public override Task WriteAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) =>
            Task.FromException(new IOException("Injected output write failure."));

#if NET8_0_OR_GREATER
        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException(
                new IOException("Injected output write failure."));
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

    private sealed class ThrowingReadStream : Stream
    {
        internal bool IsDisposed { get; private set; }

        public override bool CanRead => !IsDisposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new IOException("Injected input read failure.");

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) =>
            Task.FromException<int>(
                new IOException("Injected input read failure."));

#if NET8_0_OR_GREATER
        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<int>(
                new IOException("Injected input read failure."));
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;
            base.Dispose(disposing);
        }

        public override void Write(byte[] buffer, int offset, int count) =>
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
