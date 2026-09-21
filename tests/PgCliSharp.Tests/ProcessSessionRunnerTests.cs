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
