using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
#if NETSTANDARD2_0
using CliWrap;
#endif

namespace PgCliSharp.Internal.Execution;

internal sealed class ProcessSessionStartRequest
{
    internal ProcessSessionStartRequest(
        string executablePath,
        IEnumerable<string> arguments,
        Stream standardOutput,
        Stream standardError,
        TimeSpan? timeout = null,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException("An executable path is required.", nameof(executablePath));
#if NETSTANDARD2_0
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));
        if (standardOutput is null) throw new ArgumentNullException(nameof(standardOutput));
        if (standardError is null) throw new ArgumentNullException(nameof(standardError));
#else
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(standardOutput);
        ArgumentNullException.ThrowIfNull(standardError);
#endif
        if (!standardOutput.CanWrite)
            throw new ArgumentException("Standard output stream must be writable.", nameof(standardOutput));
        if (!standardError.CanWrite)
            throw new ArgumentException("Standard error stream must be writable.", nameof(standardError));
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Timeout must be greater than zero.");

        ExecutablePath = executablePath;
        Arguments = arguments.ToArray();
        StandardOutput = standardOutput;
        StandardError = standardError;
        Timeout = timeout;
        EnvironmentVariables = environmentVariables;
    }

    internal string ExecutablePath { get; }
    internal IReadOnlyList<string> Arguments { get; }
    internal Stream StandardOutput { get; }
    internal Stream StandardError { get; }
    internal TimeSpan? Timeout { get; }
    internal IReadOnlyDictionary<string, string>? EnvironmentVariables { get; }
}

internal sealed class ProcessSessionResult
{
    internal ProcessSessionResult(int exitCode, TimeSpan duration)
    {
        ExitCode = exitCode;
        Duration = duration;
    }

    internal int ExitCode { get; }
    internal TimeSpan Duration { get; }
}

internal interface IProcessSession : IDisposable
{
    Stream StandardInput { get; }
    Task<ProcessSessionResult> Completion { get; }
    void CompleteInput();
    void Cancel();
}

internal interface IProcessSessionRunner
{
    IProcessSession Start(ProcessSessionStartRequest request, CancellationToken cancellationToken);
}

internal sealed class ProcessSessionRunner : IProcessSessionRunner
{
    public IProcessSession Start(
        ProcessSessionStartRequest request,
        CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        if (request is null) throw new ArgumentNullException(nameof(request));
#else
        ArgumentNullException.ThrowIfNull(request);
#endif
        cancellationToken.ThrowIfCancellationRequested();

#if NETSTANDARD2_0
        return StartWithCliWrap(request, cancellationToken);
#else
        return StartWithProcess(request, cancellationToken);
#endif
    }

#if NETSTANDARD2_0
    private static CliWrapProcessSession StartWithCliWrap(
        ProcessSessionStartRequest request,
        CancellationToken cancellationToken)
    {
        var inputPipe = new SessionInputPipe();

        Command command = Cli.Wrap(request.ExecutablePath)
            .WithArguments(request.Arguments)
            .WithValidation(CommandResultValidation.None)
            .WithStandardInputPipe(inputPipe.Source)
            .WithStandardOutputPipe(PipeTarget.ToStream(request.StandardOutput))
            .WithStandardErrorPipe(PipeTarget.ToStream(request.StandardError));

        if (request.EnvironmentVariables is not null)
        {
            command = command.WithEnvironmentVariables(
                builder =>
                {
                    foreach (KeyValuePair<string, string> pair in request.EnvironmentVariables)
                        builder.Set(pair.Key, pair.Value);
                });
        }

        return new CliWrapProcessSession(
            request,
            command,
            inputPipe,
            cancellationToken);
    }
#else
    private static ModernProcessSession StartWithProcess(
        ProcessSessionStartRequest request,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (string argument in request.Arguments)
            startInfo.ArgumentList.Add(argument);

        if (request.EnvironmentVariables is not null)
        {
            foreach (KeyValuePair<string, string> pair in request.EnvironmentVariables)
                startInfo.EnvironmentVariables[pair.Key] = pair.Value;
        }

        var process = new Process { StartInfo = startInfo };

        try
        {
            if (!process.Start())
                throw new InvalidOperationException("The process could not be started.");
        }
        catch (Exception exception) when (
            exception is Win32Exception ||
            exception is InvalidOperationException)
        {
            process.Dispose();
            throw new PgExecutableStartException(request.ExecutablePath, exception);
        }

        return new ModernProcessSession(request, process, cancellationToken);
    }

    private sealed class ModernProcessSession : IProcessSession
    {
        private static readonly TimeSpan AbnormalCleanupGracePeriod =
            TimeSpan.FromSeconds(2);

        private readonly ProcessSessionStartRequest _request;
        private readonly Process _process;
        private readonly CancellationTokenSource _manualCancellation =
            new CancellationTokenSource();
        private readonly CancellationTokenSource _ioCancellation =
            new CancellationTokenSource();
        private readonly Task _standardOutputTask;
        private readonly Task _standardErrorTask;
        private int _inputCompleted;
        private int _disposed;

        internal ModernProcessSession(
            ProcessSessionStartRequest request,
            Process process,
            CancellationToken cancellationToken)
        {
            _request = request;
            _process = process;
            StandardInput = process.StandardInput.BaseStream;
            _standardOutputTask = process.StandardOutput.BaseStream.CopyToAsync(
                request.StandardOutput,
                81920,
                _ioCancellation.Token);
            _standardErrorTask = process.StandardError.BaseStream.CopyToAsync(
                request.StandardError,
                81920,
                _ioCancellation.Token);
            Completion = CompleteAsync(cancellationToken);
        }

        public Stream StandardInput { get; }

        public Task<ProcessSessionResult> Completion { get; }

        public void CompleteInput()
        {
            if (Interlocked.Exchange(ref _inputCompleted, 1) != 0)
                return;

            try
            {
                _process.StandardInput.Close();
            }
            catch (Exception exception) when (
                exception is InvalidOperationException ||
                exception is ObjectDisposedException ||
                exception is IOException)
            {
            }
        }

        public void Cancel()
        {
            try
            {
                _manualCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            CompleteInput();
            Cancel();
        }

        private async Task<ProcessSessionResult> CompleteAsync(
            CancellationToken cancellationToken)
        {
            using var lifetimeCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    _manualCancellation.Token);

            var stopwatch = Stopwatch.StartNew();
            Task waitTask = _process.WaitForExitAsync(CancellationToken.None);
            Task cancellationTask = lifetimeCancellation.Token.CanBeCanceled
                ? Task.Delay(Timeout.Infinite, lifetimeCancellation.Token)
                : Task.Delay(Timeout.Infinite, CancellationToken.None);
            Task timeoutTask = _request.Timeout.HasValue
                ? Task.Delay(_request.Timeout.Value, CancellationToken.None)
                : Task.Delay(Timeout.Infinite, CancellationToken.None);
            Task<Exception> ioFaultTask = WaitForIoFaultAsync(
                _standardOutputTask,
                _standardErrorTask);

            try
            {
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

                    TryTerminateProcessTree(_process);
                    CompleteInput();
                    _ioCancellation.Cancel();

                    await AwaitBoundedCleanupAsync(
                            waitTask,
                            _standardOutputTask,
                            _standardErrorTask)
                        .ConfigureAwait(false);
                    stopwatch.Stop();

                    if (lifetimeCancellation.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new OperationCanceledException();
                    }

                    if (timeoutTask.IsCompleted)
                    {
                        throw new PgProcessTimeoutException(
                            _request.ExecutablePath,
                            _request.Timeout!.Value);
                    }

                    if (ioFailure is not null)
                    {
                        ExceptionDispatchInfo.Capture(ioFailure).Throw();
                    }

                    throw new InvalidOperationException(
                        "Process session ended without a process, timeout, cancellation, or I/O outcome.");
                }

                Task drainTask = Task.WhenAll(
                    _standardOutputTask,
                    _standardErrorTask);

                completedTask = await Task.WhenAny(
                        drainTask,
                        cancellationTask,
                        timeoutTask)
                    .ConfigureAwait(false);

                if (completedTask != drainTask)
                {
                    CompleteInput();
                    _ioCancellation.Cancel();

                    await AwaitBoundedCleanupAsync(
                            drainTask,
                            _standardOutputTask,
                            _standardErrorTask)
                        .ConfigureAwait(false);
                    stopwatch.Stop();

                    if (lifetimeCancellation.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new OperationCanceledException();
                    }

                    throw new PgProcessTimeoutException(
                        _request.ExecutablePath,
                        _request.Timeout!.Value);
                }

                await drainTask.ConfigureAwait(false);
                stopwatch.Stop();

                return new ProcessSessionResult(
                    _process.ExitCode,
                    stopwatch.Elapsed);
            }
            finally
            {
                _ioCancellation.Cancel();
                _ioCancellation.Dispose();
                _manualCancellation.Dispose();
                _process.Dispose();
            }
        }

        private static async Task<Exception> WaitForIoFaultAsync(
            params Task[] tasks)
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
                // The primary control/I/O outcome is reported by Completion.
            }
        }

        private static void TryTerminateProcessTree(Process process)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (Exception exception) when (
                exception is InvalidOperationException ||
                exception is Win32Exception ||
                exception is NotSupportedException)
            {
                // Best effort. Completion still reports the primary outcome.
            }
        }
    }
#endif

#if NETSTANDARD2_0
    private sealed class CliWrapProcessSession : IProcessSession
    {
        private readonly ProcessSessionStartRequest _request;
        private readonly SessionInputPipe _inputPipe;
        private readonly CancellationTokenSource _manualCancellation = new CancellationTokenSource();
        private readonly CancellationTokenSource? _timeoutCancellation;
        private readonly CancellationTokenSource _lifetimeCancellation;
        private readonly CancellationTokenSource _forcefulCancellation;
        private int _disposed;

        internal CliWrapProcessSession(
            ProcessSessionStartRequest request,
            Command command,
            SessionInputPipe inputPipe,
            CancellationToken cancellationToken)
        {
            _request = request;
            _inputPipe = inputPipe;
            StandardInput = inputPipe.Writer;
            _timeoutCancellation = request.Timeout.HasValue
                ? new CancellationTokenSource(request.Timeout.Value)
                : null;
            _lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _manualCancellation.Token);
            _forcefulCancellation = _timeoutCancellation is null
                ? CancellationTokenSource.CreateLinkedTokenSource(
                    _lifetimeCancellation.Token)
                : CancellationTokenSource.CreateLinkedTokenSource(
                    _lifetimeCancellation.Token,
                    _timeoutCancellation.Token);

            CommandTask<CommandResult> commandTask;
            try
            {
                commandTask = command.ExecuteAsync(_forcefulCancellation.Token);
            }
            catch (Exception exception) when (
                exception is Win32Exception ||
                exception is InvalidOperationException)
            {
                DisposeSources();
                throw new PgExecutableStartException(request.ExecutablePath, exception);
            }

            Completion = CompleteAsync(commandTask);
        }

        public Stream StandardInput { get; }

        public Task<ProcessSessionResult> Completion { get; }

        public void CompleteInput() => _inputPipe.Complete();

        public void Cancel()
        {
            try
            {
                _manualCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            CompleteInput();
            Cancel();
        }

        private async Task<ProcessSessionResult> CompleteAsync(
            CommandTask<CommandResult> commandTask)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                CommandResult result = await commandTask.ConfigureAwait(false);
                stopwatch.Stop();
                return new ProcessSessionResult(result.ExitCode, stopwatch.Elapsed);
            }
            catch (OperationCanceledException) when (_forcefulCancellation.IsCancellationRequested)
            {
                stopwatch.Stop();

                if (_lifetimeCancellation.IsCancellationRequested)
                    throw new OperationCanceledException();

                if (_timeoutCancellation is not null &&
                    _timeoutCancellation.IsCancellationRequested)
                {
                    throw new PgProcessTimeoutException(
                        _request.ExecutablePath,
                        _request.Timeout!.Value);
                }

                throw;
            }
            finally
            {
                DisposeSources();
            }
        }

        private void DisposeSources()
        {
            _inputPipe.Dispose();
            _forcefulCancellation.Dispose();
            _lifetimeCancellation.Dispose();
            _timeoutCancellation?.Dispose();
            _manualCancellation.Dispose();
        }
    }

    private sealed class SessionInputPipe : IDisposable
    {
        private const int SegmentSize = 64 * 1024;
        private const int MaxBufferedSegments = 16;

        private readonly ConcurrentQueue<byte[]> _queue =
            new ConcurrentQueue<byte[]>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _availableSlots =
            new SemaphoreSlim(MaxBufferedSegments, MaxBufferedSegments);
        private readonly CancellationTokenSource _writeCancellation =
            new CancellationTokenSource();
        private readonly object _gate = new object();
        private int _completed;
        private int _disposed;

        internal SessionInputPipe()
        {
            Source = PipeSource.Create(PumpAsync);
            Writer = new SessionInputStream(this);
        }

        internal PipeSource Source { get; }
        internal Stream Writer { get; }

        internal void Enqueue(byte[] buffer, int offset, int count)
        {
            int position = offset;
            int remaining = count;

            while (remaining > 0)
            {
                int segmentLength = Math.Min(SegmentSize, remaining);
                WaitForSlot();
                EnqueueAcquiredSegment(buffer, position, segmentLength);
                position += segmentLength;
                remaining -= segmentLength;
            }
        }

        internal async Task EnqueueAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            int position = offset;
            int remaining = count;

            while (remaining > 0)
            {
                int segmentLength = Math.Min(SegmentSize, remaining);
                await WaitForSlotAsync(cancellationToken).ConfigureAwait(false);
                EnqueueAcquiredSegment(buffer, position, segmentLength);
                position += segmentLength;
                remaining -= segmentLength;
            }
        }

        internal void Complete()
        {
            bool shouldSignal;

            lock (_gate)
            {
                if (_completed != 0)
                    return;

                _completed = 1;
                shouldSignal = true;
            }

            CancelWriters();
            if (shouldSignal)
                _signal.Release();
        }

        public void Dispose()
        {
            bool shouldSignal;

            lock (_gate)
            {
                if (_disposed != 0)
                    return;

                _disposed = 1;
                _completed = 1;
                shouldSignal = true;
            }

            CancelWriters();
            if (shouldSignal)
                _signal.Release();
        }

        private void WaitForSlot()
        {
            try
            {
                _availableSlots.Wait(_writeCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                ThrowForClosedInput();
                throw;
            }
        }

        private async Task WaitForSlotAsync(CancellationToken cancellationToken)
        {
            using var linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    _writeCancellation.Token);

            try
            {
                await _availableSlots.WaitAsync(linkedCancellation.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (
                !cancellationToken.IsCancellationRequested &&
                _writeCancellation.IsCancellationRequested)
            {
                ThrowForClosedInput();
                throw;
            }
        }

        private void EnqueueAcquiredSegment(
            byte[] buffer,
            int offset,
            int count)
        {
            var copy = new byte[count];
            Buffer.BlockCopy(buffer, offset, copy, 0, count);

            lock (_gate)
            {
                if (_disposed != 0)
                {
                    _availableSlots.Release();
                    throw new ObjectDisposedException(nameof(SessionInputPipe));
                }

                if (_completed != 0)
                {
                    _availableSlots.Release();
                    throw new InvalidOperationException(
                        "Standard input has already been completed.");
                }

                _queue.Enqueue(copy);
            }

            _signal.Release();
        }

        private async Task PumpAsync(
            Stream destination,
            CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);

                    while (_queue.TryDequeue(out byte[]? data))
                    {
                        try
                        {
                            await destination.WriteAsync(
                                    data,
                                    0,
                                    data.Length,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }
                        finally
                        {
                            _availableSlots.Release();
                        }
                    }

                    if (Volatile.Read(ref _completed) != 0 && _queue.IsEmpty)
                        return;
                }
            }
            finally
            {
                CancelWriters();

                while (_queue.TryDequeue(out _))
                {
                    _availableSlots.Release();
                }
            }
        }

        private void CancelWriters()
        {
            try
            {
                _writeCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void ThrowForClosedInput()
        {
            lock (_gate)
            {
                if (_disposed != 0)
                    throw new ObjectDisposedException(nameof(SessionInputPipe));

                if (_completed != 0)
                {
                    throw new InvalidOperationException(
                        "Standard input has already been completed.");
                }
            }
        }

        private sealed class SessionInputStream : Stream
        {
            private readonly SessionInputPipe _owner;
            private int _disposed;

            internal SessionInputStream(SessionInputPipe owner) => _owner = owner;

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => Volatile.Read(ref _disposed) == 0;
            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                ValidateBuffer(buffer, offset, count);
                ThrowIfDisposed();
                _owner.Enqueue(buffer, offset, count);
            }

            public override async Task WriteAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken)
            {
                ValidateBuffer(buffer, offset, count);
                ThrowIfDisposed();
                cancellationToken.ThrowIfCancellationRequested();
                await _owner.EnqueueAsync(
                        buffer,
                        offset,
                        count,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && Interlocked.Exchange(ref _disposed, 1) == 0)
                    _owner.Complete();
                base.Dispose(disposing);
            }

            public override int Read(byte[] buffer, int offset, int count) =>
                throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin) =>
                throw new NotSupportedException();

            public override void SetLength(long value) =>
                throw new NotSupportedException();

            private static void ValidateBuffer(byte[] buffer, int offset, int count)
            {
                if (buffer is null) throw new ArgumentNullException(nameof(buffer));
                if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
                if (buffer.Length - offset < count)
                    throw new ArgumentException(
                        "Offset and count exceed the buffer length.");
            }

            private void ThrowIfDisposed()
            {
                if (Volatile.Read(ref _disposed) != 0)
                    throw new ObjectDisposedException(nameof(SessionInputStream));
            }
        }
    }
#endif
}
