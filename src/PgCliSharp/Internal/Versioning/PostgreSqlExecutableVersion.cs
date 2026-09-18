namespace PgCliSharp.Internal.Versioning;

internal sealed class PostgreSqlExecutableVersion
{
    internal PostgreSqlExecutableVersion(int major, Version numericVersion, string rawVersion)
    {
        Major = major;
        NumericVersion = numericVersion;
        RawVersion = rawVersion;
    }

    internal int Major { get; }

    internal Version NumericVersion { get; }

    internal string RawVersion { get; }
}
