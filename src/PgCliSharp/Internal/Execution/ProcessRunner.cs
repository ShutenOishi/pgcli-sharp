using System.ComponentModel;
using System.Diagnostics;
#if NETSTANDARD2_0
using System.Text;
using CliWrap;
#endif

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

#if NETSTANDARD2_0
        return RunWithCliWrapAsync(request, cancellationToken);
#else
        return RunWithProcessAsync(request, cancellationToken);
#endif
    }

#if NETSTANDARD2_0
    private static async Task<ProcessRunResult> RunWithCliWrapAsync(
        ProcessRunRequest request,
        CancellationToken cancellationToken)
    {
        var standardError = request.StandardError is null ? new StringBuilder() : null;
        Stream standardOutput = request.StandardOutput ?? Stream.Null;

        Command command = Cli.Wrap(request.ExecutablePath)
            .WithArguments(request.Arguments)
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStream(standardOutput))
            .WithStandardErrorPipe(
                request.StandardError is null
                    ? PipeTarget.ToStringBuilder(standardError!)
                    : PipeTarget.ToStream(request.StandardError));

        if (request.StandardInput is not null)
        {
            command = command.WithStandardInputPipe(
                PipeSource.FromStream(request.StandardInput));
        }

        if (request.EnvironmentVariables is not null)
        {
            command = command.WithEnvironmentVariables(
                builder =>
                {
                    foreach (KeyValuePair<string, string> pair in request.EnvironmentVariables)
                    {
                        builder.Set(pair.Key, pair.Value);
                    }
                });
        }

        using var timeoutSource = request.Timeout.HasValue
            ? new CancellationTokenSource(request.Timeout.Value)
            : null;
        using var executionSource = timeoutSource is null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutSource.Token);

        CommandTask<CommandResult> commandTask;
        try
        {
            commandTask = command.ExecuteAsync(executionSource.Token);
        }
        catch (Exception exception) when (
            exception is Win32Exception ||
            exception is InvalidOperationException)
        {
            throw new PgExecutableStartException(request.ExecutablePath, exception);
        }

        var stopwatch = Stopwatch.StartNew();

        CommandResult commandResult;
        try
        {
            commandResult = await commandTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (executionSource.IsCancellationRequested)
        {
            stopwatch.Stop();

            cancellationToken.ThrowIfCancellationRequested();

            if (timeoutSource is not null && timeoutSource.IsCancellationRequested)
            {
                throw new PgProcessTimeoutException(
                    request.ExecutablePath,
                    request.Timeout!.Value);
            }

            throw;
        }

        stopwatch.Stop();

        var result = new ProcessRunResult(
            commandResult.ExitCode,
            stopwatch.Elapsed,
            standardError?.ToString() ?? string.Empty);

        ThrowForNonZeroExitCodeIfRequested(request, result);
        return result;
    }
#else
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

        Task<string> standardErrorTask = ReadOrStreamStandardErrorAsync(
            process,
            request.StandardError);
        Stream standardOutput = request.StandardOutput ?? Stream.Null;
        Task standardOutputTask = process.StandardOutput.BaseStream.CopyToAsync(
            standardOutput,
            81920,
            CancellationToken.None);

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

        Task waitTask = process.WaitForExitAsync(CancellationToken.None);
        Task cancellationTask = cancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.Infinite, cancellationToken)
            : Task.Delay(Timeout.Infinite, CancellationToken.None);
        Task timeoutTask = request.Timeout.HasValue
            ? Task.Delay(request.Timeout.Value, CancellationToken.None)
            : Task.Delay(Timeout.Infinite, CancellationToken.None);

        Task completedTask = await Task.WhenAny(
                waitTask,
                cancellationTask,
                timeoutTask)
            .ConfigureAwait(false);

        if (completedTask != waitTask)
        {
            TryTerminateProcessTree(process);
            inputCancellation.Cancel();
            await waitTask.ConfigureAwait(false);
            await standardOutputTask.ConfigureAwait(false);
            _ = await standardErrorTask.ConfigureAwait(false);
            await ObserveInputTerminationAsync(
                    standardInputTask,
                    inputCancellation.Token)
                .ConfigureAwait(false);
            stopwatch.Stop();

            cancellationToken.ThrowIfCancellationRequested();

            throw new PgProcessTimeoutException(
                request.ExecutablePath,
                request.Timeout!.Value);
        }

        inputCancellation.Cancel();
        await standardOutputTask.ConfigureAwait(false);
        string standardError = await standardErrorTask.ConfigureAwait(false);
        await ObserveInputTerminationAsync(
                standardInputTask,
                inputCancellation.Token)
            .ConfigureAwait(false);
        stopwatch.Stop();

        var result = new ProcessRunResult(
            process.ExitCode,
            stopwatch.Elapsed,
            standardError);

        ThrowForNonZeroExitCodeIfRequested(request, result);
        return result;
    }

    private static async Task<string> ReadOrStreamStandardErrorAsync(
        Process process,
        Stream? destination)
    {
        if (destination is null)
        {
            return await process.StandardError.ReadToEndAsync(
                CancellationToken.None).ConfigureAwait(false);
        }

        await process.StandardError.BaseStream.CopyToAsync(
                destination,
                81920,
                CancellationToken.None)
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

        foreach (string argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (request.EnvironmentVariables is not null)
        {
            foreach (KeyValuePair<string, string> pair in request.EnvironmentVariables)
            {
                startInfo.EnvironmentVariables[pair.Key] = pair.Value;
            }
        }

        return startInfo;
    }

    private static void TryTerminateProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception exception) when (
            exception is InvalidOperationException ||
            exception is Win32Exception ||
            exception is NotSupportedException)
        {
            // Best effort: cancellation/timeout still reports the original control-flow outcome.
        }
    }
#endif

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
