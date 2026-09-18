namespace PgCliSharp.Internal.Localization;

internal static class MessageKeys
{
    internal const string ExecutableStartFailed = nameof(ExecutableStartFailed);
    internal const string ExecutableVersionMismatch = nameof(ExecutableVersionMismatch);
    internal const string ExecutableVersionOutputInvalid = nameof(ExecutableVersionOutputInvalid);
    internal const string ProcessExitedWithError = nameof(ProcessExitedWithError);
    internal const string ProcessTimedOut = nameof(ProcessTimedOut);
    internal const string OptionNotSupported = nameof(OptionNotSupported);
    internal const string InvalidOptionCombination = nameof(InvalidOptionCombination);

    internal static readonly string[] All =
    {
        ExecutableStartFailed,
        ExecutableVersionMismatch,
        ExecutableVersionOutputInvalid,
        ProcessExitedWithError,
        ProcessTimedOut,
        OptionNotSupported,
        InvalidOptionCombination,
    };
}
