using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.PgDump;

internal static class PgDumpOptionAvailability
{
    internal static void EnsureMajor(
        PostgreSqlMajorVersion selectedVersion,
        string optionName,
        PostgreSqlMajorVersion? since = null,
        PostgreSqlMajorVersion? until = null)
    {
        int major = (int)selectedVersion;
        if ((since.HasValue && major < (int)since.Value) ||
            (until.HasValue && major > (int)until.Value))
        {
            throw new PgUnsupportedOptionException(
                selectedVersion,
                optionName,
                since,
                until);
        }
    }

    internal static void EnsureRestrictKey(
        PostgreSqlMajorVersion selectedVersion,
        PostgreSqlExecutableVersion executableVersion)
    {
        EnsureMajor(
            selectedVersion,
            "--restrict-key",
            PostgreSqlMajorVersion.V13,
            PostgreSqlMajorVersion.V18);

        Version minimum = selectedVersion switch
        {
            PostgreSqlMajorVersion.V13 => new Version(13, 22),
            PostgreSqlMajorVersion.V14 => new Version(14, 19),
            PostgreSqlMajorVersion.V15 => new Version(15, 14),
            PostgreSqlMajorVersion.V16 => new Version(16, 10),
            PostgreSqlMajorVersion.V17 => new Version(17, 6),
            PostgreSqlMajorVersion.V18 => new Version(18, 0),
            _ => throw new ArgumentOutOfRangeException(
                nameof(selectedVersion),
                selectedVersion,
                null),
        };

        if (executableVersion.NumericVersion < minimum)
        {
            throw new PgUnsupportedOptionException(
                selectedVersion,
                "--restrict-key",
                PostgreSqlMajorVersion.V13,
                PostgreSqlMajorVersion.V18,
                minimum,
                executableVersion.NumericVersion);
        }
    }
}
