using System.Collections.Concurrent;

namespace PgCliSharp.Tests;

// Models a caller whose context is occupied until execution has completed.
internal sealed class QueuedSynchronizationContext : SynchronizationContext
{
    private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _callbacks = new();
    private int _posts;

    internal int PostCount => Volatile.Read(ref _posts);

    public override void Post(SendOrPostCallback d, object? state)
    {
        Interlocked.Increment(ref _posts);
        _callbacks.Enqueue((d, state));
    }

    internal void Release()
    {
        while (_callbacks.TryDequeue(out var callback))
            callback.Callback(callback.State);
    }
}
