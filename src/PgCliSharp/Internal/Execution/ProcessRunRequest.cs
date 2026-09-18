namespace PgCliSharp.Internal.Execution;

internal sealed class ProcessRunRequest
{
    internal ProcessRunRequest(
        string executablePath,
        IEnumerable<string> arguments,
        Stream? standardOutput = null,
        TimeSpan? timeout = null,
        bool throwOnNonZeroExitCode = true,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException("An executable path is required.", nameof(executablePath));
        }

        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Timeout must be greater than zero.");
        }

        ExecutablePath = executablePath;
        Arguments = arguments.ToArray();
        StandardOutput = standardOutput;
        Timeout = timeout;
        ThrowOnNonZeroExitCode = throwOnNonZeroExitCode;
        EnvironmentVariables = environmentVariables;
    }

    internal string ExecutablePath { get; }

    internal IReadOnlyList<string> Arguments { get; }

    internal Stream? StandardOutput { get; }

    internal TimeSpan? Timeout { get; }

    internal bool ThrowOnNonZeroExitCode { get; }

    internal IReadOnlyDictionary<string, string>? EnvironmentVariables { get; }
}
