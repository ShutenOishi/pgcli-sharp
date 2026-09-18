using PgCliSharp.Internal.PgDump;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Tests;

public sealed class PgDumpOptionAvailabilityCatalogTests
{
    [Fact]
    public void All_HasUniqueOptionNamesAndExpectedVersionBoundaries()
    {
        Assert.Equal(
            PgDumpOptionAvailabilityCatalog.All.Count,
            PgDumpOptionAvailabilityCatalog.All
                .Select(info => info.OptionName)
                .Distinct(StringComparer.Ordinal)
                .Count());

        Assert.Equal(PostgreSqlMajorVersion.V10, PgDumpOptionAvailabilityCatalog.Oids.Since);
        Assert.Equal(PostgreSqlMajorVersion.V11, PgDumpOptionAvailabilityCatalog.Oids.Until);
        Assert.Equal(PostgreSqlMajorVersion.V10, PgDumpOptionAvailabilityCatalog.NoSynchronizedSnapshots.Since);
        Assert.Equal(PostgreSqlMajorVersion.V14, PgDumpOptionAvailabilityCatalog.NoSynchronizedSnapshots.Until);
        Assert.Equal(PostgreSqlMajorVersion.V11, PgDumpOptionAvailabilityCatalog.LoadViaPartitionRoot.Since);
        Assert.Equal(PostgreSqlMajorVersion.V12, PgDumpOptionAvailabilityCatalog.ExtraFloatDigits.Since);
        Assert.Equal(PostgreSqlMajorVersion.V13, PgDumpOptionAvailabilityCatalog.IncludeForeignData.Since);
        Assert.Equal(PostgreSqlMajorVersion.V14, PgDumpOptionAvailabilityCatalog.Extension.Since);
        Assert.Equal(PostgreSqlMajorVersion.V15, PgDumpOptionAvailabilityCatalog.NoTableAccessMethod.Since);
        Assert.Equal(PostgreSqlMajorVersion.V16, PgDumpOptionAvailabilityCatalog.MethodCompression.Since);
        Assert.Equal(PostgreSqlMajorVersion.V17, PgDumpOptionAvailabilityCatalog.Filter.Since);
        Assert.Equal(PostgreSqlMajorVersion.V18, PgDumpOptionAvailabilityCatalog.StatisticsOnly.Since);
    }

    [Theory]
    [InlineData(PostgreSqlMajorVersion.V13, "13.22")]
    [InlineData(PostgreSqlMajorVersion.V14, "14.19")]
    [InlineData(PostgreSqlMajorVersion.V15, "15.14")]
    [InlineData(PostgreSqlMajorVersion.V16, "16.10")]
    [InlineData(PostgreSqlMajorVersion.V17, "17.6")]
    [InlineData(PostgreSqlMajorVersion.V18, "18.0")]
    public void RestrictKey_RecordsSecurityBackportMinimums(
        PostgreSqlMajorVersion major,
        string expected)
    {
        Assert.True(
            PgDumpOptionAvailabilityCatalog.RestrictKey.MinimumVersions.TryGetValue(
                major,
                out Version? minimum));
        Assert.NotNull(minimum);
        Assert.Equal(expected, minimum!.ToString());
    }

    [Fact]
    public void Ensure_UsesExactExecutableVersionForPatchAvailability()
    {
        var tooOld = new PostgreSqlExecutableVersion(
            17,
            new Version(17, 5),
            "17.5");

        PgUnsupportedOptionException exception =
            Assert.Throws<PgUnsupportedOptionException>(
                () => PgDumpOptionAvailability.Ensure(
                    PgDumpOptionAvailabilityCatalog.RestrictKey,
                    PostgreSqlMajorVersion.V17,
                    tooOld));

        Assert.Equal(new Version(17, 6), exception.MinimumExecutableVersion);
        Assert.Equal(new Version(17, 5), exception.ActualExecutableVersion);
    }
}
