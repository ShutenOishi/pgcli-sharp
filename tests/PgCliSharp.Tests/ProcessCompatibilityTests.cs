using System.Diagnostics;
using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class ProcessCompatibilityTests
{
    [Fact]
    public async Task NativeArguments_PreserveEmptyQuotesBackslashesUnicodeAndEnvironment()
    {
        string[] values = { "", "plain", "two words", "tab\there", "\"", "a\\\"b", "folder with spaces\\", "日本語🙂", "';&|$()", "line\nbreak" };
        (string executable, string[] arguments) = ManagedTestProcess.Command(values.Prepend("arguments").ToArray());
        using var output = new MemoryStream();
        var request = new ProcessRunRequest(executable, arguments, output, TimeSpan.FromSeconds(15),
            environmentVariables: new Dictionary<string, string> { ["PGCLI_TEST_VALUE"] = "環境 value" });
        ProcessRunResult result = await new ProcessRunner().RunAsync(request, CancellationToken.None);
        string[] actual = Encoding.UTF8.GetString(output.ToArray()).Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(values.Select(value => Convert.ToBase64String(Encoding.UTF8.GetBytes(value)))
            .Concat(new[] { "environment:" + Convert.ToBase64String(Encoding.UTF8.GetBytes("環境 value")) }), actual);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public async Task ExitWait_ObservesAnAlreadyExitedOwnedProcess()
    {
        (string executable, string[] arguments) = ManagedTestProcess.Command("exit");
        var info = new ProcessStartInfo { FileName = executable, UseShellExecute = false, CreateNoWindow = true };
        ProcessCompatibility.SetArguments(info, arguments);
        using var process = new Process { StartInfo = info };
        Assert.True(process.Start());
        try
        {
            Assert.True(process.WaitForExit(10000));
            Task completion = ProcessCompatibility.WaitForExitAsync(process);
            Assert.Same(completion, await Task.WhenAny(completion, Task.Delay(5000)));
            await completion;
            Assert.Equal(17, process.ExitCode);
        }
        finally
        {
            if (!process.HasExited) ProcessCompatibility.TryTerminateProcessTree(process);
        }
    }

    [Fact]
    public async Task EarlySessionExit_DoesNotRequireExplicitInputEof()
    {
        (string executable, string[] arguments) = ManagedTestProcess.Command("exit");
        using var output = new MemoryStream();
        using var error = new MemoryStream();
        using IProcessSession session = new ProcessSessionRunner().Start(
            new ProcessSessionStartRequest(executable, arguments, output, error, TimeSpan.FromSeconds(15)), CancellationToken.None);
        Assert.Same(session.Completion, await Task.WhenAny(session.Completion, Task.Delay(5000)));
        Assert.Equal(17, (await session.Completion).ExitCode);
        Assert.Contains("fixture-exit", Encoding.UTF8.GetString(error.ToArray()));
        Assert.True(output.CanWrite);
        Assert.True(error.CanWrite);
    }

    [Fact]
    public async Task FiniteNonzeroExit_MapsStatusAndCapturedStderr()
    {
        (string executable, string[] arguments) = ManagedTestProcess.Command("exit");
        var runner = new ProcessRunner();
        ProcessRunResult result = await runner.RunAsync(new ProcessRunRequest(executable, arguments, throwOnNonZeroExitCode: false), CancellationToken.None);
        Assert.Equal(17, result.ExitCode);
        Assert.Contains("fixture-exit", result.StandardError);
        PgProcessExecutionException failure = await Assert.ThrowsAsync<PgProcessExecutionException>(() =>
            runner.RunAsync(new ProcessRunRequest(executable, arguments), CancellationToken.None));
        Assert.Equal(17, failure.ExitCode);
    }

    [Fact]
    public async Task EarlySessionExit_ReleasesAProducerBlockedOnInput()
    {
        string gate = Path.Combine(Path.GetTempPath(), "pgcli-exit-gate-" + Guid.NewGuid().ToString("N"));
        (string executable, string[] arguments) = ManagedTestProcess.Command("gate-exit", gate);
        using IProcessSession session = new ProcessSessionRunner().Start(
            new ProcessSessionStartRequest(executable, arguments, Stream.Null, Stream.Null, TimeSpan.FromSeconds(15)), CancellationToken.None);
        Task write = session.StandardInput.WriteAsync(new byte[4 * 1024 * 1024], 0, 4 * 1024 * 1024, CancellationToken.None);
        try
        {
            Assert.False(write.IsCompleted);
            File.WriteAllText(gate, string.Empty);
            Assert.Same(session.Completion, await Task.WhenAny(session.Completion, Task.Delay(5000)));
            Assert.Equal(17, (await session.Completion).ExitCode);
            Assert.Same(write, await Task.WhenAny(write, Task.Delay(5000)));
            await Assert.ThrowsAnyAsync<Exception>(() => write);
        }
        finally
        {
            session.Cancel();
            File.Delete(gate);
        }
    }
}
