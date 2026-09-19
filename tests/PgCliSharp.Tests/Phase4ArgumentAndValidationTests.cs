using PgCliSharp.Internal.PgBaseBackup;
using PgCliSharp.Internal.PgCombineBackup;
using PgCliSharp.Internal.PgReceiveWal;
using PgCliSharp.Internal.PgRecvLogical;
using PgCliSharp.Internal.PgVerifyBackup;

namespace PgCliSharp.Tests;

public sealed class Phase4ArgumentAndValidationTests
{
    [Fact]
    public void PgBaseBackup_BuildsTypedTokensInStableOrder()
    {
        var options = new PgBaseBackupOptions
        {
            Format = PgBaseBackupFormat.Tar,
            WalMethod = PgBaseBackupWalMethod.Fetch,
            Slot = null,
            Compression = PgBaseBackupCompression.ForMethod(PgBaseBackupCompressionMethod.Gzip, 6, PgBaseBackupCompressionLocation.Client),
            ManifestChecksums = PgBackupManifestChecksum.Sha256,
            SyncMethod = PgBackupSyncMethod.Fsync,
            IncrementalManifest = "prior.json",
        };
        options.TablespaceMappings.Add(new PgTablespaceMapping("/old one", "/new one"));

        PgBaseBackupValidator.Validate(options, PgBaseBackupDestination.ToDirectory("backup"), PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgBaseBackupArgumentBuilder.Build(options, PgBaseBackupDestination.ToDirectory("backup"), PostgreSqlMajorVersion.V18);

        Assert.Equal("backup", ValueAfter(args, "--pgdata"));
        Assert.Equal("tar", ValueAfter(args, "--format"));
        Assert.Equal("fetch", ValueAfter(args, "--wal-method"));
        Assert.Equal("client-gzip:level=6", ValueAfter(args, "--compress"));
        Assert.Equal("SHA256", ValueAfter(args, "--manifest-checksums"));
        Assert.Equal("prior.json", ValueAfter(args, "--incremental"));
        Assert.Equal("fsync", ValueAfter(args, "--sync-method"));
        Assert.Equal("/old one=/new one", ValueAfter(args, "--tablespace-mapping"));
    }

    [Fact]
    public void PgBaseBackup_StreamImpliesTarWhenFormatIsUnset()
    {
        IReadOnlyList<string> args = PgBaseBackupArgumentBuilder.Build(
            new PgBaseBackupOptions(),
            PgBaseBackupDestination.ToStream(Stream.Null),
            PostgreSqlMajorVersion.V18);

        Assert.Equal("-", ValueAfter(args, "--pgdata"));
        Assert.Equal("tar", ValueAfter(args, "--format"));
    }

    [Fact]
    public void PgBaseBackup_TargetBeforePg15_IsRejected()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgBaseBackupValidator.Validate(
                new PgBaseBackupOptions(),
                PgBaseBackupDestination.ToServerTarget("blackhole"),
                PostgreSqlMajorVersion.V14));
    }

    [Fact]
    public void PgBaseBackup_SlotRequiresWalStreaming()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBaseBackupValidator.Validate(
                new PgBaseBackupOptions { Slot = "physical_slot", WalMethod = PgBaseBackupWalMethod.Fetch },
                PgBaseBackupDestination.ToDirectory("backup"),
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgBaseBackup_ProgressAndNoEstimateSizeConflict()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgBaseBackupValidator.Validate(
                new PgBaseBackupOptions { Progress = true, NoEstimateSize = true },
                PgBaseBackupDestination.ToDirectory("backup"),
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgReceiveWal_Pg10EndPositionIsRejected()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgReceiveWalValidator.Validate(
                new PgReceiveWalOptions
                {
                    Directory = "wal",
                    EndPosition = new PgLogSequenceNumber("0/100"),
                },
                PostgreSqlMajorVersion.V10));
    }

    [Fact]
    public void PgReceiveWal_CreateSlotRequiresSlot()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgReceiveWalValidator.Validate(
                new PgReceiveWalOptions { Action = PgReceiveWalAction.CreateSlot },
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgReceiveWal_SynchronousAndNoSyncConflict()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgReceiveWalValidator.Validate(
                new PgReceiveWalOptions { Directory = "wal", Synchronous = true, NoSync = true },
                PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgReceiveWal_CreateSlotArgumentsAreCanonical()
    {
        var options = new PgReceiveWalOptions
        {
            Action = PgReceiveWalAction.CreateSlot,
            Slot = "wal_slot",
            IfNotExists = true,
        };

        PgReceiveWalValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgReceiveWalArgumentBuilder.Build(options, PostgreSqlMajorVersion.V18);

        Assert.Contains("--create-slot", args);
        Assert.Contains("--if-not-exists", args);
        Assert.Equal("wal_slot", ValueAfter(args, "--slot"));
        Assert.DoesNotContain("--drop-slot", args);
    }

    [Fact]
    public void PgRecvLogical_CreateAndStartCanBeCombined()
    {
        var options = new PgRecvLogicalOptions
        {
            Action = PgRecvLogicalAction.CreateSlot | PgRecvLogicalAction.Start,
            Slot = "logical_slot",
            Database = "appdb",
            EnableTwoPhase = true,
        };

        PgRecvLogicalValidator.Validate(options, PgRecvLogicalOutput.ToStream(Stream.Null), PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgRecvLogicalArgumentBuilder.Build(options, PgRecvLogicalOutput.ToStream(Stream.Null), PostgreSqlMajorVersion.V18);

        Assert.Contains("--create-slot", args);
        Assert.Contains("--start", args);
        Assert.Contains("--enable-two-phase", args);
        Assert.Equal("-", ValueAfter(args, "--file"));
    }

    [Fact]
    public void PgRecvLogical_DropCannotCombineWithStart()
    {
        var options = new PgRecvLogicalOptions
        {
            Action = PgRecvLogicalAction.DropSlot | PgRecvLogicalAction.Start,
            Slot = "logical_slot",
        };

        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgRecvLogicalValidator.Validate(options, PgRecvLogicalOutput.ToStream(Stream.Null), PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgRecvLogical_TwoPhaseBeforePg15IsRejected()
    {
        var options = new PgRecvLogicalOptions
        {
            Action = PgRecvLogicalAction.CreateSlot,
            Slot = "logical_slot",
            Database = "appdb",
            EnableTwoPhase = true,
        };

        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgRecvLogicalValidator.Validate(options, null, PostgreSqlMajorVersion.V14));
    }

    [Fact]
    public void PgRecvLogical_Pg18DropDoesNotRequireDatabase()
    {
        var options = new PgRecvLogicalOptions
        {
            Action = PgRecvLogicalAction.DropSlot,
            Slot = "logical_slot",
        };

        PgRecvLogicalValidator.Validate(options, null, PostgreSqlMajorVersion.V18);
        Assert.Throws<PgInvalidOptionValueException>(() =>
            PgRecvLogicalValidator.Validate(options, null, PostgreSqlMajorVersion.V17));
    }

    [Fact]
    public void PgVerifyBackup_ProgressBeforePg16IsRejected()
    {
        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgVerifyBackupValidator.Validate(new PgVerifyBackupOptions { Progress = true }, PostgreSqlMajorVersion.V15));
    }

    [Fact]
    public void PgVerifyBackup_ProgressAndQuietConflict()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgVerifyBackupValidator.Validate(new PgVerifyBackupOptions { Progress = true, Quiet = true }, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgVerifyBackup_TarRequiresNoParseWal()
    {
        Assert.Throws<PgInvalidOptionCombinationException>(() =>
            PgVerifyBackupValidator.Validate(new PgVerifyBackupOptions { Format = PgVerifyBackupFormat.Tar }, PostgreSqlMajorVersion.V18));

        PgVerifyBackupValidator.Validate(
            new PgVerifyBackupOptions { Format = PgVerifyBackupFormat.Tar, NoParseWal = true },
            PostgreSqlMajorVersion.V18);
    }

    [Fact]
    public void PgCombineBackup_RequiresInputsAndOutput()
    {
        Assert.Throws<PgInvalidOptionValueException>(() =>
            PgCombineBackupValidator.Validate(new PgCombineBackupOptions(), PostgreSqlMajorVersion.V18));

        var outputOnly = new PgCombineBackupOptions { OutputDirectory = "combined" };
        Assert.Throws<PgInvalidOptionValueException>(() =>
            PgCombineBackupValidator.Validate(outputOnly, PostgreSqlMajorVersion.V18));
    }

    [Fact]
    public void PgCombineBackup_LinkIsPg18Only()
    {
        var options = new PgCombineBackupOptions
        {
            OutputDirectory = "combined",
            CopyMethod = PgCombineBackupCopyMethod.Link,
        };
        options.InputDirectories.Add("full");
        options.InputDirectories.Add("incremental");

        Assert.Throws<PgUnsupportedOptionException>(() =>
            PgCombineBackupValidator.Validate(options, PostgreSqlMajorVersion.V17));

        PgCombineBackupValidator.Validate(options, PostgreSqlMajorVersion.V18);
        IReadOnlyList<string> args = PgCombineBackupArgumentBuilder.Build(options, PostgreSqlMajorVersion.V18);
        Assert.Contains("--link", args);
        Assert.Equal(new[] { "full", "incremental" }, args.TakeLast(2));
    }

    [Fact]
    public void SharedLsnAndTablespaceMapping_AreTypedAndDeterministic()
    {
        Assert.Equal("A/1F", new PgLogSequenceNumber("a/1f").Value);
        Assert.Equal("/old\\=data=/new\\=data", new PgTablespaceMapping("/old=data", "/new=data").ToString());
        Assert.Throws<ArgumentException>(() => new PgLogSequenceNumber("not-an-lsn"));
    }

    private static string ValueAfter(IReadOnlyList<string> args, string option)
    {
        int index = args.ToList().IndexOf(option);
        Assert.True(index >= 0 && index + 1 < args.Count, $"Missing value for {option}.");
        return args[index + 1];
    }
}
