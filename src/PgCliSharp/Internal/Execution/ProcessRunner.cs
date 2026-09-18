using System.ComponentModel;
using System.Diagnostics;

namespace PgCliSharp.Internal.Execution;

internal sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessRunResult> RunAsync(
        ProcessRunRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        ProcessStartInfo startInfo = CreateStartInfo(request);
        using var process = new Process
        {
            StartInfo = startInfo,
        };

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("The process could not be started.");
            }
        }
        catch (Exception exception) when (exception is Win32Exception || exception is InvalidOperationException)
        {
            throw new PgExecutableStartException(request.ExecutablePath, exception);
        }

        var stopwatch = Stopwatch.StartNew();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();
        Task standardOutputTask = request.StandardOutput is null
            ? Task.CompletedTask
            : process.StandardOutput.BaseStream.CopyToAsync(request.StandardOutput, 81920);

        Task waitTask = WaitForExitAsync(process);
        Task cancellationTask = cancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.Infinite, cancellationToken)
            : Task.Delay(Timeout.Infinite);
        Task timeoutTask = request.Timeout.HasValue
            ? Task.Delay(request.Timeout.Value)
            : Task.Delay(Timeout.Infinite);

        Task completedTask = await Task.WhenAny(waitTask, cancellationTask, timeoutTask).ConfigureAwait(false);

        if (completedTask != waitTask)
        {
            TryTerminate(process);
            await waitTask.ConfigureAwait(false);
            await standardOutputTask.ConfigureAwait(false);
            _ = await standardErrorTask.ConfigureAwait(false);
            stopwatch.Stop();

            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            throw new PgProcessTimeoutException(request.ExecutablePath, request.Timeout!.Value);
        }

        await standardOutputTask.ConfigureAwait(false);
        string standardError = await standardErrorTask.ConfigureAwait(false);
        stopwatch.Stop();

        var result = new ProcessRunResult(process.ExitCode, stopwatch.Elapsed, standardError);
        if (request.ThrowOnNonZeroExitCode && result.ExitCode != 0)
        {
            throw new PgProcessExecutionException(
                request.ExecutablePath,
                result.ExitCode,
                result.StandardError);
        }

        return result;
    }

    private static ProcessStartInfo CreateStartInfo(ProcessRunRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = request.StandardOutput is not null,
        };

#if NETSTANDARD2_0
        startInfo.Arguments = ArgumentEscaper.JoinForProcessStartInfo(request.Arguments);
#else
        foreach (string argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
#endif

        if (request.EnvironmentVariables is not null)
        {
            foreach (KeyValuePair<string, string> pair in request.EnvironmentVariables)
            {
                startInfo.EnvironmentVariables[pair.Key] = pair.Value;
            }
        }

        return startInfo;
    }

    private static Task WaitForExitAsync(Process process)
    {
#if NETSTANDARD2_0
        return Task.Run(process.WaitForExit);
#else
        return process.WaitForExitAsync();
#endif
    }

    private static void TryTerminate(Process process)
    {
        try
        {
            if (process.HasExited)
            {
                return;
            }

#if NETSTANDARD2_0
            process.Kill();
#else
            process.Kill(entireProcessTree: true);
#endif
        }
        catch (Exception exception) when (
            exception is InvalidOperationException ||
            exception is Win32Exception ||
            exception is NotSupportedException)
        {
            // Best effort: cancellation/timeout still reports the original control-flow outcome.
        }
    }
}
