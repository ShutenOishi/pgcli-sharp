using System.Collections.ObjectModel;
using System.Globalization;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.BackupWal;

internal sealed class BackupWalOptionAvailabilityInfo
{
    internal BackupWalOptionAvailabilityInfo(string optionName, PostgreSqlMajorVersion since, PostgreSqlMajorVersion until)
    {
        OptionName = optionName;
        Since = since;
        Until = until;
    }

    internal string OptionName { get; }
    internal PostgreSqlMajorVersion Since { get; }
    internal PostgreSqlMajorVersion Until { get; }
    internal IReadOnlyDictionary<PostgreSqlMajorVersion, Version> MinimumVersions { get; } =
        new ReadOnlyDictionary<PostgreSqlMajorVersion, Version>(new Dictionary<PostgreSqlMajorVersion, Version>());
}

internal static class BackupWalAvailability
{
    internal static BackupWalOptionAvailabilityInfo Since(string optionName, PostgreSqlMajorVersion since) =>
        new BackupWalOptionAvailabilityInfo(optionName, since, PostgreSqlMajorVersion.V18);

    internal static BackupWalOptionAvailabilityInfo Only(string optionName, PostgreSqlMajorVersion since, PostgreSqlMajorVersion until) =>
        new BackupWalOptionAvailabilityInfo(optionName, since, until);

    internal static void Ensure(BackupWalOptionAvailabilityInfo info, PostgreSqlMajorVersion version)
    {
        if ((int)version < (int)info.Since || (int)version > (int)info.Until)
        {
            throw new PgUnsupportedOptionException(version, info.OptionName, info.Since, info.Until);
        }
    }

    internal static void EnsureTool(string tool, PostgreSqlMajorVersion version, PostgreSqlMajorVersion since)
    {
        if ((int)version < (int)since)
        {
            throw new PgUnsupportedToolException(tool, version, since);
        }
    }
}

internal static class BackupWalArgument
{
    internal static void AddValue(ICollection<string> args, string name, object? value)
    {
        if (value is null) return;
        string text = value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value.ToString()!;
        args.Add(name);
        args.Add(text);
    }

    internal static void AddFlag(ICollection<string> args, string name, bool enabled)
    {
        if (enabled) args.Add(name);
    }

    internal static void AddPassword(ICollection<string> args, PgPasswordPromptMode mode)
    {
        switch (mode)
        {
            case PgPasswordPromptMode.Default:
                break;
            case PgPasswordPromptMode.NeverPrompt:
                args.Add("--no-password");
                break;
            case PgPasswordPromptMode.ForcePrompt:
                args.Add("--password");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    internal static string Checksum(PgBackupManifestChecksum value) => value switch
    {
        PgBackupManifestChecksum.None => "NONE",
        PgBackupManifestChecksum.Crc32C => "CRC32C",
        PgBackupManifestChecksum.Sha224 => "SHA224",
        PgBackupManifestChecksum.Sha256 => "SHA256",
        PgBackupManifestChecksum.Sha384 => "SHA384",
        PgBackupManifestChecksum.Sha512 => "SHA512",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static string Sync(PgBackupSyncMethod value) => value switch
    {
        PgBackupSyncMethod.Fsync => "fsync",
        PgBackupSyncMethod.Syncfs => "syncfs",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    internal static IReadOnlyDictionary<string, string>? Environment(IDictionary<string, string> source) =>
        source.Count == 0 ? null : new Dictionary<string, string>(source, StringComparer.Ordinal);

    internal static void ValidatePort(int? port, PostgreSqlMajorVersion version)
    {
        if (port.HasValue && (port.Value < 1 || port.Value > 65535))
            throw new PgInvalidOptionValueException(version, "--port", port.Value);
    }

    internal static void ValidatePositive(TimeSpan? value, string option, PostgreSqlMajorVersion version, bool allowZero = false)
    {
        if (value.HasValue && (allowZero ? value.Value < TimeSpan.Zero : value.Value <= TimeSpan.Zero))
            throw new PgInvalidOptionValueException(version, option, value.Value);
    }

    internal static void ValidateNonEmpty(IEnumerable<string> values, string option, PostgreSqlMajorVersion version)
    {
        foreach (string value in values)
            if (string.IsNullOrWhiteSpace(value))
                throw new PgInvalidOptionValueException(version, option, value);
    }
}
