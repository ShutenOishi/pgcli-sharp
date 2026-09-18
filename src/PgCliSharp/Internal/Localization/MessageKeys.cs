namespace PgCliSharp.Internal.Localization;

internal static class MessageKeys
{
    internal const string ExecutableStartFailed = nameof(ExecutableStartFailed);
    internal const string ExecutableVersionMismatch = nameof(ExecutableVersionMismatch);
    internal const string ExecutableVersionOutputInvalid = nameof(ExecutableVersionOutputInvalid);
    internal const string ProcessExitedWithError = nameof(ProcessExitedWithError);
    internal const string ProcessTimedOut = nameof(ProcessTimedOut);
    internal const string OptionNotSupported = nameof(OptionNotSupported);
    internal const string OptionRequiresExecutableVersion = nameof(OptionRequiresExecutableVersion);
    internal const string InvalidOptionCombination = nameof(InvalidOptionCombination);
    internal const string InvalidCompressionLevel = nameof(InvalidCompressionLevel);
    internal const string InvalidCompressionCombination = nameof(InvalidCompressionCombination);
    internal const string InvalidRestrictKey = nameof(InvalidRestrictKey);
    internal const string OutputPathRequired = nameof(OutputPathRequired);
    internal const string InvalidOptionValue = nameof(InvalidOptionValue);
    internal const string ExecutablePathRequired = nameof(ExecutablePathRequired);
    internal const string OutputStreamMustBeWritable = nameof(OutputStreamMustBeWritable);
    internal const string FilterPathRequired = nameof(FilterPathRequired);
    internal const string FilterInputMustBeReadable = nameof(FilterInputMustBeReadable);
    internal const string CompressionNoneDetailsNotAllowed = nameof(CompressionNoneDetailsNotAllowed);
    internal const string CompressionLongRequiresZstd = nameof(CompressionLongRequiresZstd);

    internal static readonly string[] All =
    {
        ExecutableStartFailed,
        ExecutableVersionMismatch,
        ExecutableVersionOutputInvalid,
        ProcessExitedWithError,
        ProcessTimedOut,
        OptionNotSupported,
        OptionRequiresExecutableVersion,
        InvalidOptionCombination,
        InvalidCompressionLevel,
        InvalidCompressionCombination,
        InvalidRestrictKey,
        OutputPathRequired,
        InvalidOptionValue,
        ExecutablePathRequired,
        OutputStreamMustBeWritable,
        FilterPathRequired,
        FilterInputMustBeReadable,
        CompressionNoneDetailsNotAllowed,
        CompressionLongRequiresZstd,
    };
}
