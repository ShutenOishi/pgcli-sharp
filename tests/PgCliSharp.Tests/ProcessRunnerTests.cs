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
    public async Task RunAsync_Timeout_TerminatesDescendantProcessTreeOnWindows()
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
            Stream.Null,
            TimeSpan.FromSeconds(2));
        var runner = new ProcessRunner();

        try
        {
            await Assert.ThrowsAsync<PgProcessTimeoutException>(
                () => runner.RunAsync(request, CancellationToken.None));

            Assert.True(
                File.Exists(pidFile),
                "The descendant process PID file was not created before timeout.");

            int childProcessId = int.Parse(
                File.ReadAllText(pidFile),
                System.Globalization.CultureInfo.InvariantCulture);

            bool descendantExited = HasProcessExited(childProcessId);
            Assert.True(
                descendantExited,
                "The descendant process remained alive after timeout termination.");
        }
        finally
        {
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
