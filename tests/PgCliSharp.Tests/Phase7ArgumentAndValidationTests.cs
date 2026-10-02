using PgCliSharp.Internal.ServerApplications;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Tests;

public sealed class Phase7ArgumentAndValidationTests
{
    [Theory]
    [InlineData(PostgreSqlMajorVersion.V10)]
    [InlineData(PostgreSqlMajorVersion.V17)]
    [InlineData(PostgreSqlMajorVersion.V18)]
    public void InitDb_ChecksumDefaultIsNotOverridden(PostgreSqlMajorVersion version)
    {
        var o = new InitDbOptions { DataDirectory = "cluster" };
        InitDbImplementation.Validate(o, version);
        Assert.DoesNotContain("--data-checksums", InitDbImplementation.Build(o, version));
        Assert.DoesNotContain("--no-data-checksums", InitDbImplementation.Build(o, version));
        o.DataChecksums = false;
        Assert.Equal(version == PostgreSqlMajorVersion.V18, InitDbImplementation.Build(o, version).Contains("--no-data-checksums"));
        o.DataChecksums = true;
        Assert.Contains("--data-checksums", InitDbImplementation.Build(o, version));
    }

    [Fact]
    public void InitDb_OrderedSettingsAndWhitespaceStaySingleTokens()
    {
        var o = new InitDbOptions { DataDirectory = "cluster with spaces", NoLocale = true };
        o.Settings.Add(new PgServerSetting("shared_buffers", "128 MB"));
        o.Settings.Add(new PgServerSetting("shared_buffers", "256 MB"));
        InitDbImplementation.Validate(o, PostgreSqlMajorVersion.V18);
        Assert.Equal(new[] { "--pgdata", "cluster with spaces", "--no-locale", "--set", "shared_buffers=128 MB", "--set", "shared_buffers=256 MB" }, InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(1025)]
    public void WalSizeRejectsNonPowerOfTwoAndOutOfRange(int size)
    {
        Assert.Throws<PgInvalidOptionValueException>(() => InitDbImplementation.Validate(new InitDbOptions { DataDirectory = "cluster", WalSegmentSizeMegabytes = size }, PostgreSqlMajorVersion.V18));
        Assert.Throws<PgInvalidOptionValueException>(() => PgResetWalImplementation.Validate(new PgResetWalOptions { DataDirectory = "cluster", WalSegmentSizeMegabytes = size }, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void InitDb_AuthenticationAndLocaleDependenciesAreValidated()
    {
        Assert.Throws<PgInvalidOptionValueException>(() => InitDbImplementation.Validate(new InitDbOptions { DataDirectory = "cluster", HostAuthentication = PgInitDbAuthentication.Peer }, PostgreSqlMajorVersion.V18));
        Assert.Throws<PgInvalidOptionCombinationException>(() => InitDbImplementation.Validate(new InitDbOptions { DataDirectory = "cluster", PasswordPrompt = true, PasswordFile = "pw" }, PostgreSqlMajorVersion.V18));
        Assert.Throws<PgInvalidOptionCombinationException>(() => InitDbImplementation.Validate(new InitDbOptions { DataDirectory = "cluster", IcuRules = "rules" }, PostgreSqlMajorVersion.V18));
        Assert.Throws<PgUnsupportedOptionException>(() => InitDbImplementation.Validate(new InitDbOptions { DataDirectory = "cluster", LocaleProvider = PgInitDbLocaleProvider.Builtin, BuiltinLocale = PgBuiltinLocale.C }, PostgreSqlMajorVersion.V16));
        Assert.Throws<PgInvalidOptionValueException>(() => InitDbImplementation.Validate(new InitDbOptions { DataDirectory = "cluster", LocaleProvider = PgInitDbLocaleProvider.Builtin, BuiltinLocale = PgBuiltinLocale.UnicodeFast }, PostgreSqlMajorVersion.V17));
    }

    [Fact]
    public void PgCtl_KillAndLogRotateHaveExplicitCommandSemantics()
    {
        var kill = new PgCtlOptions { Command = PgCtlCommand.Kill, Signal = PgCtlSignal.Usr1, ProcessId = 123 };
        PgCtlImplementation.Validate(kill, PostgreSqlMajorVersion.V10);
        Assert.Equal(new[] { "kill", "USR1", "123" }, PgCtlImplementation.Build(kill, PostgreSqlMajorVersion.V10));
        Assert.Throws<PgInvalidOptionValueException>(() => PgCtlImplementation.Validate(new PgCtlOptions { Command = PgCtlCommand.Kill, Signal = PgCtlSignal.Kill, ProcessId = 0 }, PostgreSqlMajorVersion.V18));
        Assert.Throws<PgUnsupportedOptionException>(() => PgCtlImplementation.Validate(new PgCtlOptions { Command = PgCtlCommand.LogRotate, DataDirectory = "cluster" }, PostgreSqlMajorVersion.V11));
        PgCtlImplementation.Validate(new PgCtlOptions { Command = PgCtlCommand.LogRotate, DataDirectory = "cluster" }, PostgreSqlMajorVersion.V12);
    }

    [Fact]
    public void PgCtl_StartRequiresLogFileToReleaseDetachedServerPipes()
    {
        Assert.Throws<PgInvalidOptionValueException>(() => PgCtlImplementation.Validate(new PgCtlOptions { Command = PgCtlCommand.Start, DataDirectory = "cluster" }, PostgreSqlMajorVersion.V18));
        var o = new PgCtlOptions { Command = PgCtlCommand.Start, DataDirectory = "cluster", LogFile = "server.log", Wait = false, ShutdownMode = PgCtlShutdownMode.Fast };
        o.ForwardedOptions.Add("-p 55432");
        o.ForwardedOptions.Add("-c listen_addresses=''");
        PgCtlImplementation.Validate(o, PostgreSqlMajorVersion.V18);
        Assert.Equal(new[] { "--pgdata", "cluster", "--log", "server.log", "--mode", "fast", "--options", "-p 55432", "--options", "-c listen_addresses=''", "--no-wait", "start" }, PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18));
    }

    [Theory]
    [InlineData(PostgreSqlMajorVersion.V10, false)]
    [InlineData(PostgreSqlMajorVersion.V15, false)]
    [InlineData(PostgreSqlMajorVersion.V16, true)]
    public void PgUpgrade_CopyUsesHistoricalOmissionBefore16(PostgreSqlMajorVersion version, bool emitsCopy)
    {
        var o = UpgradeOptions();
        o.TransferMode = PgUpgradeTransferMode.Copy;
        PgUpgradeImplementation.Validate(o, version);
        Assert.Equal(emitsCopy, PgUpgradeImplementation.Build(o, version).Contains("--copy"));
    }

    [Fact]
    public void PgUpgrade_TransferModesAndRepeatableOptionsHaveVersionBoundaries()
    {
        var o = UpgradeOptions();
        o.TransferMode = PgUpgradeTransferMode.Clone;
        Assert.Throws<PgUnsupportedOptionException>(() => PgUpgradeImplementation.Validate(o, PostgreSqlMajorVersion.V11));
        PgUpgradeImplementation.Validate(o, PostgreSqlMajorVersion.V12);
        o.TransferMode = PgUpgradeTransferMode.Swap;
        Assert.Throws<PgUnsupportedOptionException>(() => PgUpgradeImplementation.Validate(o, PostgreSqlMajorVersion.V17));
        o.OldServerOptions.Add("-c first=1");
        o.OldServerOptions.Add("-c second=2");
        PgUpgradeImplementation.Validate(o, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--swap", args);
        Assert.True(args.ToList().IndexOf("-c first=1") < args.ToList().IndexOf("-c second=2"));
    }

    [Fact]
    public void PgRewind_RequiresExactlyOneSourceAndRecoveryConfRequiresServer()
    {
        var o = new PgRewindOptions { TargetDataDirectory = "target" };
        Assert.Throws<PgInvalidOptionValueException>(() => PgRewindImplementation.Validate(o, PostgreSqlMajorVersion.V18));
        o.SourceDataDirectory = "source";
        o.SourceConnectionString = "host=localhost";
        Assert.Throws<PgInvalidOptionCombinationException>(() => PgRewindImplementation.Validate(o, PostgreSqlMajorVersion.V18));
        o.SourceConnectionString = null;
        o.WriteRecoveryConfiguration = true;
        Assert.Throws<PgInvalidOptionCombinationException>(() => PgRewindImplementation.Validate(o, PostgreSqlMajorVersion.V18));
        o.SourceConnectionString = "host=localhost";
        o.SourceDataDirectory = null;
        Assert.Throws<PgUnsupportedOptionException>(() => PgRewindImplementation.Validate(o, PostgreSqlMajorVersion.V12));
        PgRewindImplementation.Validate(o, PostgreSqlMajorVersion.V13);
    }

    [Fact]
    public void PgChecksums_FileNodeIsRestrictedToCheckMode()
    {
        var o = new PgChecksumsOptions { DataDirectory = "cluster", Mode = PgChecksumsMode.Enable, FileNode = 0 };
        Assert.Throws<PgInvalidOptionCombinationException>(() => PgChecksumsImplementation.Validate(o, PostgreSqlMajorVersion.V18));
        o.Mode = PgChecksumsMode.Check;
        PgChecksumsImplementation.Validate(o, PostgreSqlMajorVersion.V18);
        Assert.Equal(new[] { "--pgdata", "cluster", "--check", "--filenode", "0" }, PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgResetWal_StructuredValuesUseStableShortSpellings()
    {
        var o = new PgResetWalOptions { DataDirectory = "cluster", DryRun = true, NextWalFile = new PgWalSegmentName("00000001000000000000000a"), CommitTimestampIds = new PgCommitTimestampIds(2, 10), MultiTransactionIds = new PgMultiTransactionIds(12, 3) };
        PgResetWalImplementation.Validate(o, PostgreSqlMajorVersion.V10);
        Assert.Equal(new[] { "-D", "cluster", "-c", "2,10", "-l", "00000001000000000000000A", "-m", "12,3", "-n" }, PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V10));
        Assert.Throws<PgInvalidOptionValueException>(() => PgResetWalImplementation.Validate(o, PostgreSqlMajorVersion.V17));
    }

    [Fact]
    public void PgResetWal_NumericSemanticsFollowTheHistoricalParser()
    {
        var o = new PgResetWalOptions { DataDirectory = "cluster", NextTransactionId = 2 };
        PgResetWalImplementation.Validate(o, PostgreSqlMajorVersion.V11);
        Assert.Throws<PgInvalidOptionValueException>(() => PgResetWalImplementation.Validate(o, PostgreSqlMajorVersion.V12));
        o.NextTransactionId = null;
        o.MultiTransactionIds = new PgMultiTransactionIds(0, 1);
        o.MultiTransactionOffset = uint.MaxValue;
        Assert.Throws<PgInvalidOptionValueException>(() => PgResetWalImplementation.Validate(o, PostgreSqlMajorVersion.V14));
        PgResetWalImplementation.Validate(o, PostgreSqlMajorVersion.V15);
    }

    [Fact]
    public void UndefinedEnumsAndMalformedStructuredValuesFail()
    {
        Assert.Throws<PgInvalidOptionValueException>(() => PgChecksumsImplementation.Validate(new PgChecksumsOptions { DataDirectory = "cluster", Mode = (PgChecksumsMode)99 }, PostgreSqlMajorVersion.V18));
        Assert.Throws<ArgumentException>(() => new PgWalSegmentName("123"));
        Assert.Throws<ArgumentException>(() => new PgServerSetting("a=b", "v"));
    }

    private static PgUpgradeOptions UpgradeOptions() => new PgUpgradeOptions { OldDataDirectory = "old", NewDataDirectory = "new", OldBinaryDirectory = "old bin" };
}
