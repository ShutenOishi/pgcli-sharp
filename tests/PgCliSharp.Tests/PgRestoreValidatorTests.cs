using PgCliSharp.Internal.PgRestore;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Tests;

public sealed class PgRestoreValidatorTests
{
    [Theory]
    [InlineData(PostgreSqlMajorVersion.V10, "no-comments")]
    [InlineData(PostgreSqlMajorVersion.V14, "no-table-access-method")]
    [InlineData(PostgreSqlMajorVersion.V16, "transaction-size")]
    [InlineData(PostgreSqlMajorVersion.V16, "filter")]
    [InlineData(PostgreSqlMajorVersion.V17, "statistics")]
    public void Validate_OptionBeforeMajorAvailability_Throws(
        PostgreSqlMajorVersion version,
        string scenario)
    {
        var options = new PgRestoreOptions();

        switch (scenario)
        {
            case "no-comments":
                options.NoComments = true;
                break;
            case "no-table-access-method":
                options.NoTableAccessMethod = true;
                break;
            case "transaction-size":
                options.TransactionMode = PgRestoreTransactionMode.Batch(10);
                break;
            case "filter":
                options.Filters.Add(PgRestoreFilterSource.FromFile("restore.filter"));
                break;
            case "statistics":
                options.Statistics = true;
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
        var options = new PgRestoreOptions
        {
            RestrictKey = new PgRestoreRestrictKey("SafeKey123"),
        };

        PgUnsupportedOptionException exception =
            Assert.Throws<PgUnsupportedOptionException>(
                () => PgRestoreValidator.Validate(
                    options,
                    PgRestoreInput.FromFile("backup.dump"),
                    PgRestoreOutput.ToStream(Stream.Null),
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
        var options = new PgRestoreOptions
        {
            RestrictKey = new PgRestoreRestrictKey("SafeKey123"),
        };

        PgRestoreValidator.Validate(
            options,
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToStream(Stream.Null),
            version,
            Executable(version, minor));
    }

    [Fact]
    public void Validate_CreateWithSingleTransaction_IsAcceptedOnPg11()
    {
        var options = new PgRestoreOptions
        {
            Create = true,
            TransactionMode = PgRestoreTransactionMode.SingleTransaction,
        };

        Validate(options, PostgreSqlMajorVersion.V11);
    }

    [Fact]
    public void Validate_CreateWithSingleTransaction_IsRejectedFromPg12()
    {
        var options = new PgRestoreOptions
        {
            Create = true,
            TransactionMode = PgRestoreTransactionMode.SingleTransaction,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V12));
    }

    [Fact]
    public void Validate_ParallelOptionIgnoredForScriptOutput_IsNotOverRejected()
    {
        using var archive = new MemoryStream(new byte[] { 1, 2, 3 });
        var options = new PgRestoreOptions
        {
            Jobs = 4,
            ArchiveFormat = PgRestoreArchiveFormat.Tar,
        };

        PgRestoreValidator.Validate(
            options,
            PgRestoreInput.FromStandardInput(archive),
            PgRestoreOutput.ToStream(Stream.Null),
            PostgreSqlMajorVersion.V18,
            Executable(PostgreSqlMajorVersion.V18, 6));
    }

    [Fact]
    public void Validate_ParallelDirectRestoreFromStandardInput_Throws()
    {
        using var archive = new MemoryStream(new byte[] { 1, 2, 3 });
        var options = new PgRestoreOptions { Jobs = 2 };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => PgRestoreValidator.Validate(
                options,
                PgRestoreInput.FromStandardInput(archive),
                PgRestoreOutput.ToDatabase("appdb"),
                PostgreSqlMajorVersion.V18,
                Executable(PostgreSqlMajorVersion.V18, 6)));
    }

    [Fact]
    public void Validate_SingleTransactionWithMultipleJobs_Throws()
    {
        var options = new PgRestoreOptions
        {
            Jobs = 2,
            TransactionMode = PgRestoreTransactionMode.SingleTransaction,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_IfExistsWithoutClean_Throws()
    {
        var options = new PgRestoreOptions { IfExists = true };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_DataOnlyWithClean_Throws()
    {
        var options = new PgRestoreOptions
        {
            ContentMode = PgRestoreContentMode.DataOnly,
            Clean = true,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_NoReconnectNoOp_IsAccepted()
    {
        var options = new PgRestoreOptions { NoReconnect = true };

        Validate(options, PostgreSqlMajorVersion.V18);
    }

    [Fact]
    public void Validate_FilterAndArchiveCannotBothConsumeStandardInput()
    {
        using var archive = new MemoryStream(new byte[] { 1 });
        using var filter = new MemoryStream(new byte[] { 2 });
        var options = new PgRestoreOptions();
        options.Filters.Add(PgRestoreFilterSource.FromStandardInput(filter));

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => PgRestoreValidator.Validate(
                options,
                PgRestoreInput.FromStandardInput(archive),
                PgRestoreOutput.ToStream(Stream.Null),
                PostgreSqlMajorVersion.V17,
                Executable(PostgreSqlMajorVersion.V17, 6)));
    }

    [Fact]
    public void Validate_MultipleFilterStandardInputs_Throws()
    {
        using var first = new MemoryStream(new byte[] { 1 });
        using var second = new MemoryStream(new byte[] { 2 });
        var options = new PgRestoreOptions();
        options.Filters.Add(PgRestoreFilterSource.FromStandardInput(first));
        options.Filters.Add(PgRestoreFilterSource.FromStandardInput(second));

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V17, 6));
    }

    [Theory]
    [InlineData("port")]
    [InlineData("jobs")]
    [InlineData("verbosity")]
    public void Validate_InvalidScalarValue_Throws(string scenario)
    {
        var options = new PgRestoreOptions();

        switch (scenario)
        {
            case "port":
                options.Port = 70000;
                break;
            case "jobs":
                options.Jobs = 0;
                break;
            case "verbosity":
                options.Verbosity = -1;
                break;
        }

        Assert.Throws<PgInvalidOptionValueException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void Validate_Pg18StatisticsConflicts_AreRejected()
    {
        var options = new PgRestoreOptions
        {
            ContentMode = PgRestoreContentMode.DataOnly,
            Statistics = true,
        };

        Assert.Throws<PgInvalidOptionCombinationException>(
            () => Validate(options, PostgreSqlMajorVersion.V18));
    }

    private static void Validate(
        PgRestoreOptions options,
        PostgreSqlMajorVersion version,
        int minor = 99)
    {
        PgRestoreValidator.Validate(
            options,
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToStream(Stream.Null),
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
