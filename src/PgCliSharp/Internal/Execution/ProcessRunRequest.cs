namespace PgCliSharp.Internal.Execution;

internal sealed class ProcessRunRequest
{
    internal ProcessRunRequest(
        string executablePath,
        IEnumerable<string> arguments,
        Stream? standardOutput = null,
        TimeSpan? timeout = null,
        bool throwOnNonZeroExitCode = true,
        IReadOnlyDictionary<string, string>? environmentVariables = null,
        Stream? standardInput = null)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException("An executable path is required.", nameof(executablePath));
        }

#if NETSTANDARD2_0
        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }
#else
        ArgumentNullException.ThrowIfNull(arguments);
#endif

        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Timeout must be greater than zero.");
        }

        if (standardInput is not null && !standardInput.CanRead)
        {
            throw new ArgumentException("Standard input stream must be readable.", nameof(standardInput));
        }

        if (standardOutput is not null && !standardOutput.CanWrite)
        {
            throw new ArgumentException("Standard output stream must be writable.", nameof(standardOutput));
        }

        ExecutablePath = executablePath;
        Arguments = arguments.ToArray();
        StandardOutput = standardOutput;
        Timeout = timeout;
        ThrowOnNonZeroExitCode = throwOnNonZeroExitCode;
        EnvironmentVariables = environmentVariables;
        StandardInput = standardInput;
    }

    internal string ExecutablePath { get; }

    internal IReadOnlyList<string> Arguments { get; }

    internal Stream? StandardOutput { get; }

    internal TimeSpan? Timeout { get; }

    internal bool ThrowOnNonZeroExitCode { get; }

    internal IReadOnlyDictionary<string, string>? EnvironmentVariables { get; }

    internal Stream? StandardInput { get; }
}
