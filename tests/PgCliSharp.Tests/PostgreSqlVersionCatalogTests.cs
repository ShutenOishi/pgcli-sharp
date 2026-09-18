namespace PgCliSharp.Tests;

public sealed class PostgreSqlVersionCatalogTests
{
    [Fact]
    public void All_ContainsPostgreSql10Through18InOrder()
    {
        Assert.Equal(9, PostgreSqlVersionCatalog.All.Count);

        for (int major = 10; major <= 18; major++)
        {
            PostgreSqlVersionInfo info = PostgreSqlVersionCatalog.All[major - 10];
            Assert.Equal(major, (int)info.Version);
        }
    }

    [Fact]
    public void LifecycleStatus_AsOf2026September18_MatchesUpstreamPolicy()
    {
        var asOf = new DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero);

        foreach (PostgreSqlVersionInfo info in PostgreSqlVersionCatalog.All)
        {
            PostgreSqlUpstreamLifecycleStatus expected = (int)info.Version <= 13
                ? PostgreSqlUpstreamLifecycleStatus.EndOfLife
                : PostgreSqlUpstreamLifecycleStatus.Supported;

            Assert.Equal(expected, info.GetLifecycleStatus(asOf));
        }
    }

    [Fact]
    public void Get_UnknownEnumValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PostgreSqlVersionCatalog.Get((PostgreSqlMajorVersion)19));
    }
}
