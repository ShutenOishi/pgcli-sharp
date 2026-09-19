using PgCliSharp.Internal.BackupWal;

namespace PgCliSharp.Internal.PgVerifyBackup;

internal static class PgVerifyBackupOptionAvailabilityCatalog
{
    internal static readonly BackupWalOptionAvailabilityInfo Progress = BackupWalAvailability.Since("--progress", PostgreSqlMajorVersion.V16);
    internal static readonly BackupWalOptionAvailabilityInfo Format = BackupWalAvailability.Since("--format", PostgreSqlMajorVersion.V18);

    internal static readonly IReadOnlyList<BackupWalOptionAvailabilityInfo> All = new[] { Progress, Format };
}

internal static class PgVerifyBackupArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgVerifyBackupOptions options, PgVerifyBackupInput input)
    {
        var args = new List<string>();

        BackupWalArgument.AddFlag(args, "--exit-on-error", options.ExitOnError);

        if (options.Format.HasValue)
            BackupWalArgument.AddValue(args, "--format", options.Format.Value == PgVerifyBackupFormat.Plain ? "plain" : "tar");

        foreach (string ignored in options.IgnoredPaths)
            BackupWalArgument.AddValue(args, "--ignore", ignored);

        BackupWalArgument.AddValue(args, "--manifest-path", options.ManifestPath);
        BackupWalArgument.AddFlag(args, "--no-parse-wal", options.NoParseWal);
        BackupWalArgument.AddFlag(args, "--progress", options.Progress);
        BackupWalArgument.AddFlag(args, "--quiet", options.Quiet);
        BackupWalArgument.AddFlag(args, "--skip-checksums", options.SkipChecksums);
        BackupWalArgument.AddValue(args, "--wal-directory", options.WalDirectory);
        args.Add(input.Path);

        return args;
    }
}

internal static class PgVerifyBackupValidator
{
    internal static void Validate(PgVerifyBackupOptions options, PostgreSqlMajorVersion version)
    {
        if (options.Progress)
            BackupWalAvailability.Ensure(PgVerifyBackupOptionAvailabilityCatalog.Progress, version);
        if (options.Format.HasValue)
            BackupWalAvailability.Ensure(PgVerifyBackupOptionAvailabilityCatalog.Format, version);

        if (options.Progress && options.Quiet)
            throw new PgInvalidOptionCombinationException(version, "--progress", "--quiet");

        if (options.Format == PgVerifyBackupFormat.Tar && !options.NoParseWal)
            throw new PgInvalidOptionCombinationException(version, "--format=tar", "--no-parse-wal");

        BackupWalArgument.ValidateNonEmpty(options.IgnoredPaths, "--ignore", version);
    }
}
