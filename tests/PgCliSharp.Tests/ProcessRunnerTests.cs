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
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => runner.RunAsync(request, cancellation.Token));
    }

    private static (string Executable, string[] Arguments) GetLongRunningCommand()
    {
        if (OperatingSystem.IsWindows())
        {
            string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            return (
                Path.Combine(windowsDirectory, "System32", "ping.exe"),
                new[] { "127.0.0.1", "-n", "6" });
        }

        return ("/bin/sleep", new[] { "5" });
    }
}
