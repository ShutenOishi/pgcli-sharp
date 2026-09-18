using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Tests;

public sealed class PostgreSqlVersionParserTests
{
    [Theory]
    [InlineData("pg_dump (PostgreSQL) 10.23", 10, "10.23")]
    [InlineData("pg_restore (PostgreSQL) 14.24", 14, "14.24")]
    [InlineData("psql (PostgreSQL) 18.6 (Ubuntu 18.6-1)", 18, "18.6")]
    public void TryParse_OfficialStyleVersionOutput_ParsesMajorAndNumericVersion(
        string output,
        int expectedMajor,
        string expectedVersion)
    {
        bool parsed = PostgreSqlVersionParser.TryParse(output, out PostgreSqlExecutableVersion? version);

        Assert.True(parsed);
        Assert.NotNull(version);
        Assert.Equal(expectedMajor, version.Major);
        Assert.Equal(expectedVersion, version.NumericVersion.ToString());
        Assert.Equal(expectedVersion, version.RawVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a PostgreSQL version")]
    [InlineData("PostgreSQL unknown")]
    public void TryParse_InvalidOutput_ReturnsFalse(string output)
    {
        bool parsed = PostgreSqlVersionParser.TryParse(output, out PostgreSqlExecutableVersion? version);

        Assert.False(parsed);
        Assert.Null(version);
    }
}
