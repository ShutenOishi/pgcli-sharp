using System.Globalization;
using System.Text.RegularExpressions;

namespace PgCliSharp.Internal.Versioning;

internal static class PostgreSqlVersionParser
{
    private static readonly Regex VersionRegex = new Regex(
        @"PostgreSQL\)?\s+(?<version>(?<major>\d+)(?:\.(?<minor>\d+))?(?:\.(?<patch>\d+))?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static bool TryParse(string output, out PostgreSqlExecutableVersion? executableVersion)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            executableVersion = null;
            return false;
        }

        Match match = VersionRegex.Match(output);
        if (!match.Success ||
            !int.TryParse(
                match.Groups["major"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int major))
        {
            executableVersion = null;
            return false;
        }

        int minor = ParseOptionalPart(match.Groups["minor"]);
        int patch = ParseOptionalPart(match.Groups["patch"]);

        Version numericVersion = match.Groups["patch"].Success
            ? new Version(major, minor, patch)
            : new Version(major, minor);

        executableVersion = new PostgreSqlExecutableVersion(
            major,
            numericVersion,
            match.Groups["version"].Value);
        return true;
    }

    private static int ParseOptionalPart(Group group)
    {
        if (!group.Success)
        {
            return 0;
        }

        return int.Parse(group.Value, NumberStyles.None, CultureInfo.InvariantCulture);
    }
}
