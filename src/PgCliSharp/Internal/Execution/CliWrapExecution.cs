#if NETSTANDARD2_0
using CliWrap;

namespace PgCliSharp.Internal.Execution;

internal static class CliWrapExecution
{
    internal static CommandTask<CommandResult> Start(Command command, CancellationToken cancellationToken)
    {
        // The dependency's async enumeration otherwise captures the caller's context.
        // Keep startup synchronous so start failures retain their existing semantics.
        SynchronizationContext? context = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            return command.ExecuteAsync(cancellationToken);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }
}
#endif
