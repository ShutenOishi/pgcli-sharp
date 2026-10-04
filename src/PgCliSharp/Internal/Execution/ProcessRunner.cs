using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace PgCliSharp.Internal.Execution;

internal sealed class ProcessRunner : IProcessRunner
{
    public Task<ProcessRunResult> RunAsync(
        ProcessRunRequest request,
        CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }
#else
        ArgumentNullException.ThrowIfNull(request);
#endif

        cancellationToken.ThrowIfCancellationRequested();

        return RunWithProcessAsync(request, cancellationToken);
    }

    private static readonly TimeSpan AbnormalCleanupGracePeriod =
        TimeSpan.FromSeconds(2);

    private static async Task<ProcessRunResult> RunWithProcessAsync(
        ProcessRunRequest request,
        CancellationToken cancellationToken)
    {
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
        catch (Exception exception) when (
            exception is Win32Exception ||
            exception is InvalidOperationException)
        {
            throw new PgExecutableStartException(request.ExecutablePath, exception);
        }

        var stopwatch = Stopwatch.StartNew();
        using var outputCancellation = new CancellationTokenSource();

        Task<string> standardErrorTask = ReadOrStreamStandardErrorAsync(
            process,
            request.StandardError,
            outputCancellation.Token);
        Stream standardOutput = request.StandardOutput ?? Stream.Null;
        Task standardOutputTask = process.StandardOutput.BaseStream.CopyToAsync(
            standardOutput,
            81920,
            outputCancellation.Token);

        using var inputCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        if (request.Timeout.HasValue)
        {
            inputCancellation.CancelAfter(request.Timeout.Value);
        }

        Task standardInputTask = request.StandardInput is null
            ? Task.CompletedTask
            : CopyStandardInputAsync(
                request.StandardInput,
                process.StandardInput,
                inputCancellation.Token);

        Task waitTask = ProcessCompatibility.WaitForExitAsync(process);
        Task cancellationTask = cancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.Infinite, cancellationToken)
            : Task.Delay(Timeout.Infinite, CancellationToken.None);
        Task timeoutTask = request.Timeout.HasValue
            ? Task.Delay(request.Timeout.Value, CancellationToken.None)
            : Task.Delay(Timeout.Infinite, CancellationToken.None);
        Task<Exception> ioFaultTask = WaitForIoFaultAsync(
            standardOutputTask,
            standardErrorTask,
            standardInputTask);

        Task completedTask = await Task.WhenAny(
                waitTask,
                cancellationTask,
                timeoutTask,
                ioFaultTask)
            .ConfigureAwait(false);

        if (completedTask != waitTask)
        {
            Exception? ioFailure = completedTask == ioFaultTask
                ? await ioFaultTask.ConfigureAwait(false)
                : null;

            TryTerminateProcessTree(process);
            TryCloseStandardInput(process);
            inputCancellation.Cancel();
            outputCancellation.Cancel();

            await AwaitBoundedCleanupAsync(
                    waitTask,
                    standardOutputTask,
                    standardErrorTask,
                    standardInputTask)
                .ConfigureAwait(false);
            stopwatch.Stop();

            cancellationToken.ThrowIfCancellationRequested();

            if (timeoutTask.IsCompleted)
            {
                throw new PgProcessTimeoutException(
                    request.ExecutablePath,
                    request.Timeout!.Value);
            }

            if (ioFailure is not null)
            {
                ExceptionDispatchInfo.Capture(ioFailure).Throw();
            }

            throw new InvalidOperationException(
                "Process execution ended without a process, timeout, cancellation, or I/O outcome.");
        }

        inputCancellation.Cancel();

        Task<string> drainTask = DrainAfterExitAsync(
            standardOutputTask,
            standardErrorTask,
            standardInputTask,
            inputCancellation.Token);

        completedTask = await Task.WhenAny(
                drainTask,
                cancellationTask,
                timeoutTask)
            .ConfigureAwait(false);

        if (completedTask != drainTask)
        {
            TryCloseStandardInput(process);
            outputCancellation.Cancel();

            await AwaitBoundedCleanupAsync(
                    drainTask,
                    standardOutputTask,
                    standardErrorTask,
                    standardInputTask)
                .ConfigureAwait(false);
            stopwatch.Stop();

            cancellationToken.ThrowIfCancellationRequested();

            throw new PgProcessTimeoutException(
                request.ExecutablePath,
                request.Timeout!.Value);
        }

        string standardError = await drainTask.ConfigureAwait(false);
        stopwatch.Stop();

        var result = new ProcessRunResult(
            process.ExitCode,
            stopwatch.Elapsed,
            standardError);

        ThrowForNonZeroExitCodeIfRequested(request, result);
        return result;
    }

    private static async Task<string> DrainAfterExitAsync(
        Task standardOutputTask,
        Task<string> standardErrorTask,
        Task standardInputTask,
        CancellationToken inputCancellationToken)
    {
        await standardOutputTask.ConfigureAwait(false);
        string standardError = await standardErrorTask.ConfigureAwait(false);
        await ObserveInputTerminationAsync(
                standardInputTask,
                inputCancellationToken)
            .ConfigureAwait(false);
        return standardError;
    }

    private static async Task<Exception> WaitForIoFaultAsync(params Task[] tasks)
    {
        var remaining = new List<Task>(tasks);

        while (remaining.Count > 0)
        {
            Task completed = await Task.WhenAny(remaining).ConfigureAwait(false);
            remaining.Remove(completed);

            try
            {
                await completed.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                continue;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        var never = new TaskCompletionSource<Exception>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        return await never.Task.ConfigureAwait(false);
    }

    private static async Task AwaitBoundedCleanupAsync(params Task[] tasks)
    {
        Task cleanupTask = Task.WhenAll(
            tasks.Select(ObserveCleanupTaskAsync));

        Task completed = await Task.WhenAny(
                cleanupTask,
                Task.Delay(AbnormalCleanupGracePeriod))
            .ConfigureAwait(false);

        if (completed == cleanupTask)
        {
            await cleanupTask.ConfigureAwait(false);
        }
    }

    private static async Task ObserveCleanupTaskAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch
        {
            // The primary control/I/O outcome is reported by the caller.
        }
    }

    private static async Task<string> ReadOrStreamStandardErrorAsync(
        Process process,
        Stream? destination,
        CancellationToken cancellationToken)
    {
        if (destination is null)
        {
            #if NETSTANDARD2_0
            return await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
#else
            return await process.StandardError.ReadToEndAsync(
                cancellationToken).ConfigureAwait(false);
#endif
        }

        await process.StandardError.BaseStream.CopyToAsync(
                destination,
                81920,
                cancellationToken)
            .ConfigureAwait(false);
        return string.Empty;
    }

    private static async Task CopyStandardInputAsync(
        Stream source,
        StreamWriter destination,
        CancellationToken cancellationToken)
    {
        await source.CopyToAsync(
                destination.BaseStream,
                81920,
                cancellationToken)
            .ConfigureAwait(false);
        await destination.BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        destination.Close();
    }

    private static async Task ObserveInputTerminationAsync(
        Task inputTask,
        CancellationToken cancellationToken)
    {
        try
        {
            await inputTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (IOException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static ProcessStartInfo CreateStartInfo(ProcessRunRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = request.StandardInput is not null,
        };

        ProcessCompatibility.SetArguments(startInfo, request.Arguments);

        if (request.EnvironmentVariables is not null)
        {
            foreach (KeyValuePair<string, string> pair in request.EnvironmentVariables)
            {
                startInfo.EnvironmentVariables[pair.Key] = pair.Value;
            }
        }

        return startInfo;
    }

    private static void TryCloseStandardInput(Process process)
    {
        try
        {
            process.StandardInput.Close();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException ||
            exception is ObjectDisposedException)
        {
        }
    }

    private static void TryTerminateProcessTree(Process process) =>
        ProcessCompatibility.TryTerminateProcessTree(process);

    private static void ThrowForNonZeroExitCodeIfRequested(
        ProcessRunRequest request,
        ProcessRunResult result)
    {
        if (request.ThrowOnNonZeroExitCode && result.ExitCode != 0)
        {
            throw new PgProcessExecutionException(
                request.ExecutablePath,
                result.ExitCode,
                result.StandardError);
        }
    }
}
