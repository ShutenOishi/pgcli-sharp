using System.Globalization;

namespace PgCliSharp.Tests;

internal static class RealPostgreSqlTestEnvironment
{
    internal static bool TryGet(out string binaryDirectory, out PostgreSqlMajorVersion version)
    {
        string? binary = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_BIN");
        string? majorText = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_MAJOR");
        bool required = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_REQUIRED") == "true";
        binaryDirectory = string.Empty;
        version = default;
        if (!required && string.IsNullOrWhiteSpace(binary) && string.IsNullOrWhiteSpace(majorText))
            return false;
        if (string.IsNullOrWhiteSpace(binary) ||
            !int.TryParse(majorText, NumberStyles.None, CultureInfo.InvariantCulture, out int major) || major < 10 || major > 18)
            throw new InvalidOperationException("Real PostgreSQL configuration is missing or invalid.");
        binaryDirectory = binary!;
        version = (PostgreSqlMajorVersion)major;
        return true;
    }
}
