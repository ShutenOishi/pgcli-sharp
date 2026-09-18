using PgCliSharp.Internal.PgDump;
using PgCliSharp.Internal.PgDumpAll;
using PgCliSharp.Internal.PgRestore;

namespace PgCliSharp.Tests;

public sealed class BackupRestoreTrioTests
{
    [Fact]
    public void PgDumpCustomArchive_AndPgRestoreCustomInput_AgreeOnFormatAndPath()
    {
        var dumpOptions = new PgDumpOptions
        {
            Format = PgDumpFormat.Custom,
        };

        IReadOnlyList<string> dumpArguments = PgDumpArgumentBuilder.Build(
            dumpOptions,
            PgDumpOutput.ToFile("backup.dump"),
            PostgreSqlMajorVersion.V18);

        var restoreOptions = new PgRestoreOptions
        {
            ArchiveFormat = PgRestoreArchiveFormat.Custom,
        };

        IReadOnlyList<string> restoreArguments = PgRestoreArgumentBuilder.Build(
            restoreOptions,
            PgRestoreInput.FromFile("backup.dump"),
            PgRestoreOutput.ToDatabase("appdb"));

        Assert.Equal("custom", ValueAfter(dumpArguments, "--format"));
        Assert.Equal("backup.dump", ValueAfter(dumpArguments, "--file"));
        Assert.Equal("custom", ValueAfter(restoreArguments, "--format"));
        Assert.Equal("backup.dump", restoreArguments[restoreArguments.Count - 1]);
    }

    [Fact]
    public void DirectoryArchive_UsesDirectoryDestinationAndDirectoryRestoreInput()
    {
        var dumpOptions = new PgDumpOptions
        {
            Format = PgDumpFormat.Directory,
            Jobs = 2,
        };

        IReadOnlyList<string> dumpArguments = PgDumpArgumentBuilder.Build(
            dumpOptions,
            PgDumpOutput.ToDirectory("backup-dir"),
            PostgreSqlMajorVersion.V18);

        var restoreOptions = new PgRestoreOptions
        {
            ArchiveFormat = PgRestoreArchiveFormat.Directory,
            Jobs = 2,
        };

        IReadOnlyList<string> restoreArguments = PgRestoreArgumentBuilder.Build(
            restoreOptions,
            PgRestoreInput.FromDirectory("backup-dir"),
            PgRestoreOutput.ToDatabase("appdb"));

        Assert.Equal("directory", ValueAfter(dumpArguments, "--format"));
        Assert.Equal("backup-dir", ValueAfter(dumpArguments, "--file"));
        Assert.Equal("directory", ValueAfter(restoreArguments, "--format"));
        Assert.Equal("backup-dir", restoreArguments[restoreArguments.Count - 1]);
        Assert.Equal("2", ValueAfter(restoreArguments, "--jobs"));
    }

    [Fact]
    public void PgDumpAll_RemainsSqlScriptOutputWithoutArchiveFormat()
    {
        IReadOnlyList<string> arguments = PgDumpAllArgumentBuilder.Build(
            new PgDumpAllOptions(),
            PgDumpAllOutput.ToFile("cluster.sql"));

        Assert.Equal("cluster.sql", ValueAfter(arguments, "--file"));
        Assert.DoesNotContain("--format", arguments);
    }

    private static string ValueAfter(
        IReadOnlyList<string> arguments,
        string option)
    {
        int index = arguments.ToList().IndexOf(option);
        Assert.True(index >= 0 && index + 1 < arguments.Count);
        return arguments[index + 1];
    }
}
