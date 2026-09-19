using PgCliSharp.Internal.BackupWal;

namespace PgCliSharp.Internal.PgCombineBackup;

internal static class PgCombineBackupOptionAvailabilityCatalog
{
    internal static readonly BackupWalOptionAvailabilityInfo Link = BackupWalAvailability.Since("--link", PostgreSqlMajorVersion.V18);
    internal static readonly IReadOnlyList<BackupWalOptionAvailabilityInfo> All = new[] { Link };
}

internal static class PgCombineBackupArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgCombineBackupOptions options, PostgreSqlMajorVersion version)
    {
        var args = new List<string>();

        BackupWalArgument.AddFlag(args, "--debug", options.Debug);
        BackupWalArgument.AddFlag(args, "--dry-run", options.DryRun);
        BackupWalArgument.AddFlag(args, "--no-sync", options.NoSync);
        BackupWalArgument.AddValue(args, "--output", options.OutputDirectory);

        foreach (PgTablespaceMapping mapping in options.TablespaceMappings)
            BackupWalArgument.AddValue(args, "--tablespace-mapping", mapping);

        switch (options.CopyMethod)
        {
            case PgCombineBackupCopyMethod.Default:
                break;
            case PgCombineBackupCopyMethod.Clone:
                args.Add("--clone");
                break;
            case PgCombineBackupCopyMethod.Copy:
                args.Add("--copy");
                break;
            case PgCombineBackupCopyMethod.CopyFileRange:
                args.Add("--copy-file-range");
                break;
            case PgCombineBackupCopyMethod.Link:
                args.Add("--link");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(options.CopyMethod));
        }

        if (options.ManifestChecksums.HasValue)
            BackupWalArgument.AddValue(args, "--manifest-checksums", BackupWalArgument.Checksum(options.ManifestChecksums.Value));

        BackupWalArgument.AddFlag(args, "--no-manifest", options.NoManifest);

        if (options.SyncMethod.HasValue)
            BackupWalArgument.AddValue(args, "--sync-method", BackupWalArgument.Sync(options.SyncMethod.Value));

        foreach (string input in options.InputDirectories)
            args.Add(input);

        return args;
    }
}

internal static class PgCombineBackupValidator
{
    internal static void Validate(PgCombineBackupOptions options, PostgreSqlMajorVersion version)
    {
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
            throw new PgInvalidOptionValueException(version, "--output", options.OutputDirectory);

        if (options.InputDirectories.Count == 0)
            throw new PgInvalidOptionValueException(version, "backup-directories", "<empty>");

        BackupWalArgument.ValidateNonEmpty(options.InputDirectories, "backup-directories", version);
        BackupWalArgument.ValidateNonEmpty(options.TablespaceMappings.Select(x => x?.ToString() ?? string.Empty), "--tablespace-mapping", version);

        if (options.NoManifest && options.ManifestChecksums.HasValue)
            throw new PgInvalidOptionCombinationException(version, "--no-manifest", "--manifest-checksums");

        if (options.CopyMethod == PgCombineBackupCopyMethod.Link)
            BackupWalAvailability.Ensure(PgCombineBackupOptionAvailabilityCatalog.Link, version);

#if NETSTANDARD2_0
        if (!Enum.IsDefined(typeof(PgCombineBackupCopyMethod), options.CopyMethod))
#else
        if (!Enum.IsDefined(options.CopyMethod))
#endif
            throw new PgInvalidOptionValueException(version, "copy-method", options.CopyMethod);
    }
}
