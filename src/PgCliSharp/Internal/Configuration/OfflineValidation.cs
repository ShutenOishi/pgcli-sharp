using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.Configuration;

internal static class OfflineValidation
{
    internal static T Configure<T>(Action<T> configureOptions) where T : new()
    {
        ConfigurationGuard.NotNull(configureOptions, nameof(configureOptions));
        var options = new T();
        configureOptions(options);
        return options;
    }

    internal static PostgreSqlExecutableVersion? ExactVersion(string executablePath, PostgreSqlMajorVersion selectedVersion, Version? executableVersion)
    {
        if (executableVersion is null) return null;
        if (executableVersion.Major != (int)selectedVersion)
            throw new PgExecutableVersionMismatchException(executablePath, selectedVersion, executableVersion.Major);
        return new PostgreSqlExecutableVersion(executableVersion.Major, executableVersion, executableVersion.ToString());
    }

    internal static PgValidationResult Check(Action validate, Func<string, IEnumerable<string>> properties, bool deferred)
    {
        try
        {
            validate();
            return new PgValidationResult(null, deferred);
        }
        catch (PgOptionValidationException exception)
        {
            PgValidationErrorCode code = exception is PgUnsupportedOptionException ? PgValidationErrorCode.UnsupportedOption : PgValidationErrorCode.InvalidValue;
            return Failure(code, exception.Message, new[] { exception.OptionName }, properties, deferred);
        }
        catch (PgInvalidOptionCombinationException exception)
        {
            return Failure(PgValidationErrorCode.InvalidCombination, exception.Message, exception.OptionNames, properties, deferred);
        }
    }

    private static PgValidationResult Failure(PgValidationErrorCode code, string message, IEnumerable<string> options, Func<string, IEnumerable<string>> properties, bool deferred)
    {
        string[] names = options.ToArray();
        IEnumerable<string> propertyNames = names.SelectMany(name => name.Split('/'))
            .Select(name => name.Split('=')[0])
            .Select(name => name.Length > 0 && name[0] == '-' ? name : "--" + name)
            .SelectMany(properties).Distinct(StringComparer.Ordinal);
        return new PgValidationResult(new PgValidationError(code, message, names, propertyNames), deferred);
    }
}
