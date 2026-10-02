namespace PgCliSharp.Internal.Configuration;

internal static class ConfigurationGuard
{
    internal static void NotNull(object? value, string parameterName)
    {
#if NETSTANDARD2_0
        if (value is null) throw new ArgumentNullException(parameterName);
#else
        ArgumentNullException.ThrowIfNull(value, parameterName);
#endif
    }
}
