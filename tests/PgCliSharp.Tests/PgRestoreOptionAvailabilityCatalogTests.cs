using PgCliSharp.Internal.PgRestore;

namespace PgCliSharp.Tests;

public sealed class PgRestoreOptionAvailabilityCatalogTests
{
    [Fact]
    public void Catalog_HasUniqueOptionNames()
    {
        Assert.Equal(
            PgRestoreOptionAvailabilityCatalog.All.Count,
            PgRestoreOptionAvailabilityCatalog.All
                .Select(item => item.OptionName)
                .Distinct(StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void Catalog_ContainsAllVersionVaryingOptions()
    {
        string[] expected =
        {
            "--no-comments",
            "--restrict-key",
            "--no-table-access-method",
            "--transaction-size",
            "--filter",
            "--no-data",
            "--no-policies",
            "--no-schema",
            "--no-statistics",
            "--statistics",
            "--statistics-only",
        };

        Assert.Equal(
            expected.OrderBy(value => value),
            PgRestoreOptionAvailabilityCatalog.All
                .Select(item => item.OptionName)
                .OrderBy(value => value));
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
            PgRestoreOptionAvailabilityCatalog.RestrictKey.MinimumVersions
                .TryGetValue(major, out Version? minimum));
        Assert.Equal(new Version(expectedMajor, expectedMinor), minimum);
    }
}
