using PgCliSharp.Internal.PgDump;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Tests;

public sealed class PgDumpValidatorTests
{
    public static IEnumerable<object[]> MajorAvailabilityCases()
    {
        yield return Case(PostgreSqlMajorVersion.V12, "--oids", o => o.IncludeOids = true);
        yield return Case(PostgreSqlMajorVersion.V15, "--no-synchronized-snapshots", o => o.NoSynchronizedSnapshots = true);
        yield return Case(PostgreSqlMajorVersion.V10, "--no-comments", o => o.NoComments = true);
        yield return Case(PostgreSqlMajorVersion.V11, "--extra-float-digits", o => o.ExtraFloatDigits = 0);
        yield return Case(PostgreSqlMajorVersion.V12, "--include-foreign-data", o => o.IncludedForeignData.Add("server*"));
        yield return Case(PostgreSqlMajorVersion.V13, "--extension", o => o.Extensions.Add("ext*"));
        yield return Case(PostgreSqlMajorVersion.V14, "--no-table-access-method", o => o.NoTableAccessMethod = true);
        yield return Case(PostgreSqlMajorVersion.V15, "--table-and-children", o => o.TablesAndChildren.Add("public.parent"));
        yield return Case(PostgreSqlMajorVersion.V16, "--filter", o => o.Filters.Add(PgDumpFilterSource.FromFile("filters.txt")));
        yield return Case(PostgreSqlMajorVersion.V16, "--sync-method", o => o.SyncMethod = PgDumpSyncMethod.Fsync);
        yield return Case(PostgreSqlMajorVersion.V17, "--no-data", o => o.NoData = true);
        yield return Case(
            PostgreSqlMajorVersion.V15,
            "--compress=method[:detail]",
            o => o.Compression = PgDumpCompression.ForMethod(PgDumpCompressionMethod.Gzip));
    }

    [Theory]
    [MemberData(nameof(MajorAvailabilityCases))]
    public void Validate_OptionOutsideMajorAvailability_Throws(
        PostgreSqlMajorVersion version,
        string expectedOption,
        Action<PgDumpOptions> configure)
    {
        var options = new PgDumpOptions();
        configure(options);

        PgUnsupportedOptionException exception =
            Assert.Throws<PgUnsupportedOptionException>(
                () => Validate(options, version));

        Assert.Equal(expectedOption, exception.OptionName);
        Assert.Equal(version, exception.SelectedVersion);
    }

    [Theory]
    [InlineData(PostgreSqlMajorVersion.V13, 21, "13.22")]
    [InlineData(PostgreSqlMajorVersion.V14, 18, "14.19")]
    [InlineData(PostgreSqlMajorVersion.V15, 13, "15.14")]
    [InlineData(PostgreSqlMajorVersion.V16, 9, "16.10")]
    [InlineData(PostgreSqlMajorVersion.V17, 5, "17.6")]
    public void Validate_RestrictKeyBeforeSecurityBackport_Throws(
        PostgreSqlMajorVersion major,
        int minor,
        string expectedMinimum)
    {
        var options = new PgDumpOptions
        {
            RestrictKey = new PgDumpRestrictKey("SafeKey123"),
        };

        var actual = new PostgreSqlExecutableVersion(
            (int)major,
            new Version((int)major, minor),
            $"{(int)major}.{minor}");

        PgUnsupportedOptionException exception =
            Assert.Throws<PgUnsupportedOptionException>(
                () => PgDumpValidator.Validate(
                    options,
                    PgDumpOutput.ToStream(Stream.Null),
                    major,
                    actual));

        Assert.Equal("--restrict-key", exception.OptionName);
        Assert.Equal(expectedMinimum, exception.MinimumExecutableVersion!.ToString());
    }

    [Theory]
    [InlineData(PostgreSqlMajorVersion.V13, 22)]
    [InlineData(PostgreSqlMajorVersion.V14, 19)]
    [InlineData(PostgreSqlMajorVersion.V15, 14)]
    [InlineData(PostgreSqlMajorVersion.V16, 10)]
    [InlineData(PostgreSqlMajorVersion.V17, 6)]
    [InlineData(PostgreSqlMajorVersion.V18, 0)]
    public void Validate_RestrictKeyAtMinimumVersion_IsAccepted(
        PostgreSqlMajorVersion major,
        int minor)
    {
        var options = new PgDumpOptions
        {
            RestrictKey = new PgDumpRestrictKey("SafeKey123"),
        };

        var actual = new PostgreSqlExecutableVersion(
            (int)major,
            new Version((int)major, minor),
            $"{(int)major}.{minor}");

        PgDumpValidator.Validate(
            options,
            PgDumpOutput.ToStream(Stream.Null),
            major,
            actual);
    }

    [Fact]
    public void Validate_DirectoryFormatRequiresDirectoryOutput()
    {
        var options = new PgDumpOptions
        {
            Format = PgDumpFormat.Directory,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_JobsOutsideDirectoryFormat_Throws()
    {
        var options = new PgDumpOptions
        {
            Jobs = 2,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_TarWithCompression_Throws()
    {
        var options = new PgDumpOptions
        {
            Format = PgDumpFormat.Tar,
            Compression = PgDumpCompression.FromLevel(6),
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_TarWithExplicitNoCompression_IsAccepted()
    {
        var options = new PgDumpOptions
        {
            Format = PgDumpFormat.Tar,
            Compression = PgDumpCompression.FromLevel(0),
        };

        PgDumpValidator.Validate(
            options,
            PgDumpOutput.ToStream(Stream.Null),
            PostgreSqlMajorVersion.V18,
            Version(PostgreSqlMajorVersion.V18));
    }

    [Theory]
    [InlineData("data-schema")]
    [InlineData("data-clean")]
    [InlineData("if-exists")]
    [InlineData("on-conflict")]
    [InlineData("foreign-schema")]
    [InlineData("foreign-parallel")]
    [InlineData("statistics-data")]
    [InlineData("statistics-no-statistics")]
    public void Validate_IncompatibleCombinations_Throw(string scenario)
    {
        var options = new PgDumpOptions();
        PgDumpOutput output = PgDumpOutput.ToStream(Stream.Null);

        switch (scenario)
        {
            case "data-schema":
                options.DataOnly = true;
                options.SchemaOnly = true;
                break;
            case "data-clean":
                options.DataOnly = true;
                options.Clean = true;
                break;
            case "if-exists":
                options.IfExists = true;
                break;
            case "on-conflict":
                options.OnConflictDoNothing = true;
                break;
            case "foreign-schema":
                options.SchemaOnly = true;
                options.IncludedForeignData.Add("server");
                break;
            case "foreign-parallel":
                options.Format = PgDumpFormat.Directory;
                options.Jobs = 2;
                options.IncludedForeignData.Add("server");
                output = PgDumpOutput.ToDirectory("dumpdir");
                break;
            case "statistics-data":
                options.DataOnly = true;
                options.Statistics = true;
                break;
            case "statistics-no-statistics":
                options.Statistics = true;
                options.NoStatistics = true;
                break;
        }

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => PgDumpValidator.Validate(
                options,
                output,
                PostgreSqlMajorVersion.V18,
                Version(PostgreSqlMajorVersion.V18)));
    }

    [Fact]
    public void Validate_RepeatableFilterAllowsOneStandardInput()
    {
        using var input = new MemoryStream(new byte[] { 1, 2, 3 });
        var options = new PgDumpOptions();
        options.Filters.Add(PgDumpFilterSource.FromFile("one.filter"));
        options.Filters.Add(PgDumpFilterSource.FromStandardInput(input));
        options.Filters.Add(PgDumpFilterSource.FromFile("two.filter"));

        PgDumpValidator.Validate(
            options,
            PgDumpOutput.ToStream(Stream.Null),
            PostgreSqlMajorVersion.V17,
            Version(PostgreSqlMajorVersion.V17));

        Assert.Same(input, PgDumpValidator.GetStandardInput(options));
    }

    [Fact]
    public void Validate_MultipleStandardInputFilters_Throws()
    {
        using var input1 = new MemoryStream(new byte[] { 1 });
        using var input2 = new MemoryStream(new byte[] { 2 });
        var options = new PgDumpOptions();
        options.Filters.Add(PgDumpFilterSource.FromStandardInput(input1));
        options.Filters.Add(PgDumpFilterSource.FromStandardInput(input2));

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V17));
    }

    [Fact]
    public void Validate_ValueRanges_AreChecked()
    {
        var options = new PgDumpOptions
        {
            ExtraFloatDigits = 4,
        };

        PgInvalidOptionValueException exception =
            Assert.Throws<PgInvalidOptionValueException>(
                () => Validate(options, PostgreSqlMajorVersion.V18));

        Assert.Equal("--extra-float-digits", exception.OptionName);
    }

    private static object[] Case(
        PostgreSqlMajorVersion version,
        string optionName,
        Action<PgDumpOptions> configure)
    {
        return new object[] { version, optionName, configure };
    }

    private static void Validate(
        PgDumpOptions options,
        PostgreSqlMajorVersion version)
    {
        PgDumpValidator.Validate(
            options,
            PgDumpOutput.ToStream(Stream.Null),
            version,
            Version(version));
    }

    private static PostgreSqlExecutableVersion Version(
        PostgreSqlMajorVersion version)
    {
        return new PostgreSqlExecutableVersion(
            (int)version,
            new Version((int)version, 99),
            $"{(int)version}.99");
    }
}
