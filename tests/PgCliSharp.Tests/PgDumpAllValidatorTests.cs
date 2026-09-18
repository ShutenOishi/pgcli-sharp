using PgCliSharp.Internal.PgDumpAll;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Tests;

public sealed class PgDumpAllValidatorTests
{
    [Theory]
    [InlineData(PostgreSqlMajorVersion.V12, "oids")]
    [InlineData(PostgreSqlMajorVersion.V10, "encoding")]
    [InlineData(PostgreSqlMajorVersion.V11, "exclude-database")]
    [InlineData(PostgreSqlMajorVersion.V13, "toast")]
    [InlineData(PostgreSqlMajorVersion.V14, "table-am")]
    [InlineData(PostgreSqlMajorVersion.V16, "filter")]
    [InlineData(PostgreSqlMajorVersion.V17, "sequence")]
    public void Validate_MajorAvailabilityBoundaries_AreEnforced(
        PostgreSqlMajorVersion version,
        string scenario)
    {
        var options = new PgDumpAllOptions();

        switch (scenario)
        {
            case "oids":
                options.IncludeOids = true;
                break;
            case "encoding":
                options.Encoding = "UTF8";
                break;
            case "exclude-database":
                options.ExcludedDatabases.Add("legacy");
                break;
            case "toast":
                options.NoToastCompression = true;
                break;
            case "table-am":
                options.NoTableAccessMethod = true;
                break;
            case "filter":
                options.Filters.Add(PgDumpAllFilterSource.FromFile("dumpall.filter"));
                break;
            case "sequence":
                options.SequenceData = true;
                break;
        }

        Assert.Throws<PgUnsupportedOptionException>(
            () => Validate(options, version));
    }

    [Theory]
    [InlineData(PostgreSqlMajorVersion.V13, 21, "13.22")]
    [InlineData(PostgreSqlMajorVersion.V14, 18, "14.19")]
    [InlineData(PostgreSqlMajorVersion.V15, 13, "15.14")]
    [InlineData(PostgreSqlMajorVersion.V16, 9, "16.10")]
    [InlineData(PostgreSqlMajorVersion.V17, 5, "17.6")]
    public void Validate_RestrictKeyBeforeSecurityBackport_Throws(
        PostgreSqlMajorVersion version,
        int minor,
        string expectedMinimum)
    {
        var options = new PgDumpAllOptions
        {
            RestrictKey = new PgDumpAllRestrictKey("SafeKey123"),
        };

        PgUnsupportedOptionException exception =
            Assert.Throws<PgUnsupportedOptionException>(
                () => PgDumpAllValidator.Validate(
                    options,
                    PgDumpAllOutput.ToStream(Stream.Null),
                    version,
                    Executable(version, minor)));

        Assert.Equal(expectedMinimum, exception.MinimumExecutableVersion!.ToString());
    }

    [Theory]
    [InlineData(PostgreSqlMajorVersion.V13, 22)]
    [InlineData(PostgreSqlMajorVersion.V14, 19)]
    [InlineData(PostgreSqlMajorVersion.V15, 14)]
    [InlineData(PostgreSqlMajorVersion.V16, 10)]
    [InlineData(PostgreSqlMajorVersion.V17, 6)]
    [InlineData(PostgreSqlMajorVersion.V18, 0)]
    public void Validate_RestrictKeyAtSecurityBoundary_IsAccepted(
        PostgreSqlMajorVersion version,
        int minor)
    {
        var options = new PgDumpAllOptions
        {
            RestrictKey = new PgDumpAllRestrictKey("SafeKey123"),
        };

        PgDumpAllValidator.Validate(
            options,
            PgDumpAllOutput.ToStream(Stream.Null),
            version,
            Executable(version, minor));
    }

    [Theory]
    [InlineData(PgDumpAllScope.GlobalsOnly)]
    [InlineData(PgDumpAllScope.RolesOnly)]
    [InlineData(PgDumpAllScope.TablespacesOnly)]
    public void Validate_DatabaseExclusionWithGlobalOnlyScope_Throws(
        PgDumpAllScope scope)
    {
        var options = new PgDumpAllOptions { Scope = scope };
        options.ExcludedDatabases.Add("db*");

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_IfExistsWithoutClean_Throws()
    {
        var options = new PgDumpAllOptions { IfExists = true };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_OnConflictWithoutInsertMode_ThrowsForDatabaseDump()
    {
        var options = new PgDumpAllOptions
        {
            OnConflictDoNothing = true,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_OnConflictIgnoredForGlobalsOnly_IsNotOverRejected()
    {
        var options = new PgDumpAllOptions
        {
            Scope = PgDumpAllScope.GlobalsOnly,
            OnConflictDoNothing = true,
        };

        Validate(options, PostgreSqlMajorVersion.V18);
    }

    [Fact]
    public void Validate_Pg18DatabaseContentConflict_Throws()
    {
        var options = new PgDumpAllOptions
        {
            ContentMode = PgDumpAllContentMode.DataOnly,
            Statistics = true,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_Pg18DatabaseContentSwitchIgnoredForGlobalsOnly_IsAccepted()
    {
        var options = new PgDumpAllOptions
        {
            Scope = PgDumpAllScope.GlobalsOnly,
            ContentMode = PgDumpAllContentMode.DataOnly,
            Statistics = true,
        };

        Validate(options, PostgreSqlMajorVersion.V18);
    }

    [Theory]
    [InlineData("port")]
    [InlineData("verbosity")]
    [InlineData("extra-float-low")]
    [InlineData("extra-float-high")]
    [InlineData("rows")]
    [InlineData("lock-timeout")]
    public void Validate_InvalidScalarValue_Throws(string scenario)
    {
        var options = new PgDumpAllOptions();

        switch (scenario)
        {
            case "port":
                options.Port = 0;
                break;
            case "verbosity":
                options.Verbosity = -1;
                break;
            case "extra-float-low":
                options.ExtraFloatDigits = -16;
                break;
            case "extra-float-high":
                options.ExtraFloatDigits = 4;
                break;
            case "rows":
                options.RowsPerInsert = 0;
                break;
            case "lock-timeout":
                options.LockWaitTimeout = TimeSpan.FromMilliseconds(-1);
                break;
        }

        Assert.Throws<PgInvalidOptionValueException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_MultipleFilterStandardInputs_Throws()
    {
        using var first = new MemoryStream(new byte[] { 1 });
        using var second = new MemoryStream(new byte[] { 2 });
        var options = new PgDumpAllOptions();
        options.Filters.Add(PgDumpAllFilterSource.FromStandardInput(first));
        options.Filters.Add(PgDumpAllFilterSource.FromStandardInput(second));

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V17, 6));
    }

    private static void Validate(
        PgDumpAllOptions options,
        PostgreSqlMajorVersion version,
        int minor = 99)
    {
        PgDumpAllValidator.Validate(
            options,
            PgDumpAllOutput.ToStream(Stream.Null),
            version,
            Executable(version, minor));
    }

    private static PostgreSqlExecutableVersion Executable(
        PostgreSqlMajorVersion version,
        int minor)
    {
        return new PostgreSqlExecutableVersion(
            (int)version,
            new Version((int)version, minor),
            $"{(int)version}.{minor}");
    }
}
