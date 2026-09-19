using System.Globalization;
using PgCliSharp.Internal.BackupWal;

namespace PgCliSharp.Internal.PgBaseBackup;

internal static class PgBaseBackupOptionAvailabilityCatalog
{
    internal static readonly BackupWalOptionAvailabilityInfo CreateSlot = BackupWalAvailability.Since("--create-slot", PostgreSqlMajorVersion.V11);
    internal static readonly BackupWalOptionAvailabilityInfo NoVerifyChecksums = BackupWalAvailability.Since("--no-verify-checksums", PostgreSqlMajorVersion.V11);
    internal static readonly BackupWalOptionAvailabilityInfo NoEstimateSize = BackupWalAvailability.Since("--no-estimate-size", PostgreSqlMajorVersion.V13);
    internal static readonly BackupWalOptionAvailabilityInfo NoManifest = BackupWalAvailability.Since("--no-manifest", PostgreSqlMajorVersion.V13);
    internal static readonly BackupWalOptionAvailabilityInfo ManifestForceEncode = BackupWalAvailability.Since("--manifest-force-encode", PostgreSqlMajorVersion.V13);
    internal static readonly BackupWalOptionAvailabilityInfo ManifestChecksums = BackupWalAvailability.Since("--manifest-checksums", PostgreSqlMajorVersion.V13);
    internal static readonly BackupWalOptionAvailabilityInfo Target = BackupWalAvailability.Since("--target", PostgreSqlMajorVersion.V15);
    internal static readonly BackupWalOptionAvailabilityInfo Incremental = BackupWalAvailability.Since("--incremental", PostgreSqlMajorVersion.V17);
    internal static readonly BackupWalOptionAvailabilityInfo SyncMethod = BackupWalAvailability.Since("--sync-method", PostgreSqlMajorVersion.V17);

    internal static readonly IReadOnlyList<BackupWalOptionAvailabilityInfo> All = new[]
    {
        CreateSlot, NoVerifyChecksums, NoEstimateSize, NoManifest, ManifestForceEncode,
        ManifestChecksums, Target, Incremental, SyncMethod,
    };
}

internal static class PgBaseBackupArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgBaseBackupOptions options, PgBaseBackupDestination destination, PostgreSqlMajorVersion version)
    {
        var args = new List<string>();

        switch (destination.Kind)
        {
            case PgBaseBackupDestinationKind.Directory:
                BackupWalArgument.AddValue(args, "--pgdata", destination.Value);
                break;
            case PgBaseBackupDestinationKind.StandardOutput:
                BackupWalArgument.AddValue(args, "--pgdata", "-");
                break;
            case PgBaseBackupDestinationKind.ServerTarget:
                BackupWalArgument.AddValue(args, "--target", destination.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(destination));
        }

        PgBaseBackupFormat? format = options.Format;
        if (destination.Kind == PgBaseBackupDestinationKind.StandardOutput && !format.HasValue)
            format = PgBaseBackupFormat.Tar;

        if (format.HasValue)
            BackupWalArgument.AddValue(args, "--format", format.Value == PgBaseBackupFormat.Plain ? "plain" : "tar");

        if (options.Checkpoint.HasValue)
            BackupWalArgument.AddValue(args, "--checkpoint", options.Checkpoint.Value == PgBaseBackupCheckpointMode.Fast ? "fast" : "spread");

        BackupWalArgument.AddFlag(args, "--create-slot", options.CreateSlot);
        BackupWalArgument.AddValue(args, "--max-rate", options.MaxRate);
        BackupWalArgument.AddFlag(args, "--write-recovery-conf", options.WriteRecoveryConf);
        BackupWalArgument.AddValue(args, "--slot", options.Slot);

        foreach (PgTablespaceMapping mapping in options.TablespaceMappings)
            BackupWalArgument.AddValue(args, "--tablespace-mapping", mapping);

        if (options.WalMethod.HasValue)
            BackupWalArgument.AddValue(args, "--wal-method", options.WalMethod.Value switch
            {
                PgBaseBackupWalMethod.None => "none",
                PgBaseBackupWalMethod.Fetch => "fetch",
                PgBaseBackupWalMethod.Stream => "stream",
                _ => throw new ArgumentOutOfRangeException(nameof(options)),
            });

        if (options.Compression is not null)
            BackupWalArgument.AddValue(args, "--compress", FormatCompression(options.Compression));

        BackupWalArgument.AddValue(args, "--label", options.Label);
        BackupWalArgument.AddFlag(args, "--no-clean", options.NoClean);
        BackupWalArgument.AddFlag(args, "--no-sync", options.NoSync);
        BackupWalArgument.AddValue(args, "--dbname", options.ConnectionString);
        BackupWalArgument.AddValue(args, "--host", options.Host);
        BackupWalArgument.AddValue(args, "--port", options.Port);
        BackupWalArgument.AddValue(args, "--username", options.Username);
        BackupWalArgument.AddPassword(args, options.PasswordPrompt);

        if (options.StatusInterval.HasValue)
            BackupWalArgument.AddValue(args, "--status-interval", ((long)options.StatusInterval.Value.TotalSeconds).ToString(CultureInfo.InvariantCulture));

        BackupWalArgument.AddFlag(args, "--verbose", options.Verbose);
        BackupWalArgument.AddFlag(args, "--progress", options.Progress);
        BackupWalArgument.AddValue(args, "--waldir", options.WalDirectory);
        BackupWalArgument.AddFlag(args, "--no-slot", options.NoSlot);
        BackupWalArgument.AddFlag(args, "--no-verify-checksums", options.NoVerifyChecksums);
        BackupWalArgument.AddFlag(args, "--no-estimate-size", options.NoEstimateSize);
        BackupWalArgument.AddFlag(args, "--no-manifest", options.NoManifest);
        BackupWalArgument.AddFlag(args, "--manifest-force-encode", options.ManifestForceEncode);

        if (options.ManifestChecksums.HasValue)
            BackupWalArgument.AddValue(args, "--manifest-checksums", BackupWalArgument.Checksum(options.ManifestChecksums.Value));

        BackupWalArgument.AddValue(args, "--incremental", options.IncrementalManifest);

        if (options.SyncMethod.HasValue)
            BackupWalArgument.AddValue(args, "--sync-method", BackupWalArgument.Sync(options.SyncMethod.Value));

        return args;
    }

    internal static string FormatCompression(PgBaseBackupCompression compression)
    {
        if (compression.IsLegacyLevel)
            return compression.LegacyLevel!.Value.ToString(CultureInfo.InvariantCulture);

        string method = compression.Method switch
        {
            PgBaseBackupCompressionMethod.Gzip => "gzip",
            PgBaseBackupCompressionMethod.Lz4 => "lz4",
            PgBaseBackupCompressionMethod.Zstd => "zstd",
            PgBaseBackupCompressionMethod.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(compression)),
        };

        string prefix = compression.Location switch
        {
            PgBaseBackupCompressionLocation.Unspecified => string.Empty,
            PgBaseBackupCompressionLocation.Client => "client-",
            PgBaseBackupCompressionLocation.Server => "server-",
            _ => throw new ArgumentOutOfRangeException(nameof(compression)),
        };

        return compression.Level.HasValue
            ? prefix + method + ":level=" + compression.Level.Value.ToString(CultureInfo.InvariantCulture)
            : prefix + method;
    }
}

internal static class PgBaseBackupValidator
{
    internal static void Validate(PgBaseBackupOptions options, PgBaseBackupDestination destination, PostgreSqlMajorVersion version)
    {
        BackupWalArgument.ValidatePort(options.Port, version);
        BackupWalArgument.ValidatePositive(options.StatusInterval, "--status-interval", version, allowZero: true);

        if (options.StatusInterval.HasValue && options.StatusInterval.Value.Ticks % TimeSpan.TicksPerSecond != 0)
            throw new PgInvalidOptionValueException(version, "--status-interval", options.StatusInterval.Value);

        Ensure(options.CreateSlot, PgBaseBackupOptionAvailabilityCatalog.CreateSlot, version);
        Ensure(options.NoVerifyChecksums, PgBaseBackupOptionAvailabilityCatalog.NoVerifyChecksums, version);
        Ensure(options.NoEstimateSize, PgBaseBackupOptionAvailabilityCatalog.NoEstimateSize, version);
        Ensure(options.NoManifest, PgBaseBackupOptionAvailabilityCatalog.NoManifest, version);
        Ensure(options.ManifestForceEncode, PgBaseBackupOptionAvailabilityCatalog.ManifestForceEncode, version);
        Ensure(options.ManifestChecksums.HasValue, PgBaseBackupOptionAvailabilityCatalog.ManifestChecksums, version);
        Ensure(destination.Kind == PgBaseBackupDestinationKind.ServerTarget, PgBaseBackupOptionAvailabilityCatalog.Target, version);
        Ensure(!string.IsNullOrWhiteSpace(options.IncrementalManifest), PgBaseBackupOptionAvailabilityCatalog.Incremental, version);
        Ensure(options.SyncMethod.HasValue, PgBaseBackupOptionAvailabilityCatalog.SyncMethod, version);

        if (options.Compression is not null && !options.Compression.IsLegacyLevel && (int)version < 15)
            throw new PgUnsupportedOptionException(version, "--compress=method", PostgreSqlMajorVersion.V15, PostgreSqlMajorVersion.V18);

        if (destination.Kind == PgBaseBackupDestinationKind.ServerTarget && options.Format.HasValue)
            Reject(version, "--target", "--format");

        if (destination.Kind == PgBaseBackupDestinationKind.ServerTarget && options.WriteRecoveryConf)
            Reject(version, "--target", "--write-recovery-conf");

        PgBaseBackupFormat effectiveFormat = options.Format ??
            (destination.Kind == PgBaseBackupDestinationKind.StandardOutput ? PgBaseBackupFormat.Tar : PgBaseBackupFormat.Plain);

        if (destination.Kind == PgBaseBackupDestinationKind.StandardOutput && effectiveFormat != PgBaseBackupFormat.Tar)
            Reject(version, "stdout", "--format=plain");

        if (!string.IsNullOrWhiteSpace(options.Slot) && options.WalMethod != PgBaseBackupWalMethod.Stream)
            Reject(version, "--slot", "--wal-method=stream");

        if (options.NoSlot && !string.IsNullOrWhiteSpace(options.Slot))
            Reject(version, "--no-slot", "--slot");

        if (options.CreateSlot && string.IsNullOrWhiteSpace(options.Slot))
            Reject(version, "--create-slot", "--slot");

        if (options.CreateSlot && options.NoSlot)
            Reject(version, "--create-slot", "--no-slot");

        if (destination.Kind == PgBaseBackupDestinationKind.ServerTarget && options.WalMethod == PgBaseBackupWalMethod.Stream)
            Reject(version, "--target", "--wal-method=stream");

        if (destination.Kind == PgBaseBackupDestinationKind.StandardOutput && options.WalMethod == PgBaseBackupWalMethod.Stream)
            Reject(version, "stdout", "--wal-method=stream");

        if (!string.IsNullOrWhiteSpace(options.WalDirectory))
        {
            if (effectiveFormat != PgBaseBackupFormat.Plain)
                Reject(version, "--waldir", "--format=tar");
            if (!Path.IsPathRooted(options.WalDirectory))
                throw new PgInvalidOptionValueException(version, "--waldir", options.WalDirectory);
        }

        if (options.Progress && options.NoEstimateSize)
            Reject(version, "--progress", "--no-estimate-size");

        if (options.NoManifest && options.ManifestChecksums.HasValue)
            Reject(version, "--no-manifest", "--manifest-checksums");

        if (options.NoManifest && options.ManifestForceEncode)
            Reject(version, "--no-manifest", "--manifest-force-encode");

        if (options.Compression is not null)
        {
            PgBaseBackupCompressionLocation location = options.Compression.Location;
            if (destination.Kind == PgBaseBackupDestinationKind.ServerTarget && location == PgBaseBackupCompressionLocation.Client)
                Reject(version, "--target", "--compress=client-*");

            bool compressed = options.Compression.IsLegacyLevel
                ? options.Compression.LegacyLevel != 0
                : options.Compression.Method != PgBaseBackupCompressionMethod.None;

            PgBaseBackupCompressionLocation effectiveLocation = location;
            if (effectiveLocation == PgBaseBackupCompressionLocation.Unspecified)
                effectiveLocation = destination.Kind == PgBaseBackupDestinationKind.ServerTarget
                    ? PgBaseBackupCompressionLocation.Server
                    : PgBaseBackupCompressionLocation.Client;

            if (compressed && effectiveLocation == PgBaseBackupCompressionLocation.Client && effectiveFormat == PgBaseBackupFormat.Plain)
                Reject(version, "--compress", "--format=plain");
        }

        BackupWalArgument.ValidateNonEmpty(options.TablespaceMappings.Select(x => x?.ToString() ?? string.Empty), "--tablespace-mapping", version);
        if (options.MaxRate is not null && string.IsNullOrWhiteSpace(options.MaxRate))
            throw new PgInvalidOptionValueException(version, "--max-rate", options.MaxRate);
    }

    private static void Ensure(bool requested, BackupWalOptionAvailabilityInfo info, PostgreSqlMajorVersion version)
    {
        if (requested) BackupWalAvailability.Ensure(info, version);
    }

    private static void Reject(PostgreSqlMajorVersion version, params string[] names) =>
        throw new PgInvalidOptionCombinationException(version, names);
}
