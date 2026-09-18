using PgCliSharp.Internal.PgDumpAll;

namespace PgCliSharp.Tests;

public sealed class PgDumpAllOptionAvailabilityCatalogTests
{
    [Fact]
    public void Catalog_HasUniqueOptionNames()
    {
        Assert.Equal(
            PgDumpAllOptionAvailabilityCatalog.All.Count,
            PgDumpAllOptionAvailabilityCatalog.All
                .Select(item => item.OptionName)
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void Catalog_ContainsAllVersionVaryingOptions()
    {
        string[] expected =
        {
            "--oids",
            "--encoding",
            "--load-via-partition-root",
            "--no-comments",
            "--exclude-database",
            "--extra-float-digits",
            "--on-conflict-do-nothing",
            "--rows-per-insert",
            "--restrict-key",
            "--no-toast-compression",
            "--no-table-access-method",
            "--filter",
            "--no-data",
            "--no-policies",
            "--no-schema",
            "--no-statistics",
            "--statistics",
            "--statistics-only",
            "--sequence-data",
        };

        Assert.Equal(
            expected.OrderBy(value => value),
            PgDumpAllOptionAvailabilityCatalog.All
                .Select(item => item.OptionName)
                .OrderBy(value => value));
    }

    [Fact]
    public void Oids_IsLimitedToPostgreSql10And11()
    {
        Assert.Equal(PostgreSqlMajorVersion.V10, PgDumpAllOptionAvailabilityCatalog.Oids.Since);
        Assert.Equal(PostgreSqlMajorVersion.V11, PgDumpAllOptionAvailabilityCatalog.Oids.Until);
    }

    [Theory]
    [InlineData(PostgreSqlMajorVersion.V13, 13, 22)]
    [InlineData(PostgreSqlMajorVersion.V14, 14, 19)]
    [InlineData(PostgreSqlMajorVersion.V15, 15, 14)]
    [InlineData(PostgreSqlMajorVersion.V16, 16, 10)]
    [InlineData(PostgreSqlMajorVersion.V17, 17, 6)]
    [InlineData(PostgreSqlMajorVersion.V18, 18, 0)]
    public void RestrictKey_UsesExactSecurityBackportBoundary(
        PostgreSqlMajorVersion major,
        int expectedMajor,
        int expectedMinor)
    {
        Assert.True(
            PgDumpAllOptionAvailabilityCatalog.RestrictKey.MinimumVersions
                .TryGetValue(major, out Version? minimum));
        Assert.Equal(new Version(expectedMajor, expectedMinor), minimum);
    }
}
