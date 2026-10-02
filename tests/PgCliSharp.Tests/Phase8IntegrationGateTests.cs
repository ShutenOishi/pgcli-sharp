namespace PgCliSharp.Tests;

public sealed class Phase8IntegrationGateTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("/fixture/bin", null)]
    [InlineData(null, "18")]
    [InlineData("/fixture/bin", "9")]
    [InlineData("/fixture/bin", "19")]
    public void RequiredRealConfiguration_CannotSilentlyPassWithoutValidConfiguration(string? binary, string? major)
    {
        string? oldBinary = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_BIN");
        string? oldMajor = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_MAJOR");
        string? oldRequired = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_REQUIRED");
        try
        {
            Environment.SetEnvironmentVariable("PGCLI_REAL_PG_BIN", binary);
            Environment.SetEnvironmentVariable("PGCLI_REAL_PG_MAJOR", major);
            Environment.SetEnvironmentVariable("PGCLI_REAL_PG_REQUIRED", "true");
            Assert.Throws<InvalidOperationException>(() => RealPostgreSqlTestEnvironment.TryGet(out _, out _));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PGCLI_REAL_PG_BIN", oldBinary);
            Environment.SetEnvironmentVariable("PGCLI_REAL_PG_MAJOR", oldMajor);
            Environment.SetEnvironmentVariable("PGCLI_REAL_PG_REQUIRED", oldRequired);
        }
    }
}
