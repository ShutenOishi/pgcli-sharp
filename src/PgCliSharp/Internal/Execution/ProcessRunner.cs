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
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

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
        var standardError = new StringBuilder();
        Stream standardOutput = request.StandardOutput ?? Stream.Null;

        Command command = Cli.Wrap(request.ExecutablePath)
            .WithArguments(request.Arguments)
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStream(standardOutput))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(standardError));

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

            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

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
            standardError.ToString());

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
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();
        Stream standardOutput = request.StandardOutput ?? Stream.Null;
        Task standardOutputTask = process.StandardOutput.BaseStream.CopyToAsync(
            standardOutput,
            81920);

        Task waitTask = process.WaitForExitAsync();
        Task cancellationTask = cancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.Infinite, cancellationToken)
            : Task.Delay(Timeout.Infinite);
        Task timeoutTask = request.Timeout.HasValue
            ? Task.Delay(request.Timeout.Value)
            : Task.Delay(Timeout.Infinite);

        Task completedTask = await Task.WhenAny(
                waitTask,
                cancellationTask,
                timeoutTask)
            .ConfigureAwait(false);

        if (completedTask != waitTask)
        {
            TryTerminateProcessTree(process);
            await waitTask.ConfigureAwait(false);
            await standardOutputTask.ConfigureAwait(false);
            _ = await standardErrorTask.ConfigureAwait(false);
            stopwatch.Stop();

            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            throw new PgProcessTimeoutException(
                request.ExecutablePath,
                request.Timeout!.Value);
        }

        await standardOutputTask.ConfigureAwait(false);
        string standardError = await standardErrorTask.ConfigureAwait(false);
        stopwatch.Stop();

        var result = new ProcessRunResult(
            process.ExitCode,
            stopwatch.Elapsed,
            standardError);

        ThrowForNonZeroExitCodeIfRequested(request, result);
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
            RedirectStandardOutput = true,
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
