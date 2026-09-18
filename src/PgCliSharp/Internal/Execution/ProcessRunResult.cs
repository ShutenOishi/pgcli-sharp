namespace PgCliSharp.Internal.Execution;

internal sealed class ProcessRunResult
{
    internal ProcessRunResult(int exitCode, TimeSpan duration, string standardError)
    {
        ExitCode = exitCode;
        Duration = duration;
        StandardError = standardError;
    }

    internal int ExitCode { get; }

    internal TimeSpan Duration { get; }

    internal string StandardError { get; }
}
