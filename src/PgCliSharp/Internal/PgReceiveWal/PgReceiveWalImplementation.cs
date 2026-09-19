using System.Globalization;
using PgCliSharp.Internal.BackupWal;

namespace PgCliSharp.Internal.PgReceiveWal;

internal static class PgReceiveWalOptionAvailabilityCatalog
{
    internal static readonly BackupWalOptionAvailabilityInfo EndPosition = BackupWalAvailability.Since("--endpos", PostgreSqlMajorVersion.V11);
    internal static readonly BackupWalOptionAvailabilityInfo NoSync = BackupWalAvailability.Since("--no-sync", PostgreSqlMajorVersion.V11);

    internal static readonly IReadOnlyList<BackupWalOptionAvailabilityInfo> All = new[] { EndPosition, NoSync };
}

internal static class PgReceiveWalArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgReceiveWalOptions options, PostgreSqlMajorVersion version)
    {
        var args = new List<string>();

        BackupWalArgument.AddValue(args, "--directory", options.Directory);
        BackupWalArgument.AddValue(args, "--dbname", options.ConnectionString);
        BackupWalArgument.AddValue(args, "--endpos", options.EndPosition);
        BackupWalArgument.AddValue(args, "--host", options.Host);
        BackupWalArgument.AddValue(args, "--port", options.Port);
        BackupWalArgument.AddValue(args, "--username", options.Username);
        BackupWalArgument.AddFlag(args, "--no-loop", options.NoLoop);
        BackupWalArgument.AddPassword(args, options.PasswordPrompt);

        if (options.StatusInterval.HasValue)
            BackupWalArgument.AddValue(args, "--status-interval", ((long)options.StatusInterval.Value.TotalSeconds).ToString(CultureInfo.InvariantCulture));

        BackupWalArgument.AddValue(args, "--slot", options.Slot);
        BackupWalArgument.AddFlag(args, "--verbose", options.Verbose);

        if (options.Compression is not null)
            BackupWalArgument.AddValue(args, "--compress", FormatCompression(options.Compression));

        if (options.Action == PgReceiveWalAction.CreateSlot) args.Add("--create-slot");
        if (options.Action == PgReceiveWalAction.DropSlot) args.Add("--drop-slot");

        BackupWalArgument.AddFlag(args, "--if-not-exists", options.IfNotExists);
        BackupWalArgument.AddFlag(args, "--synchronous", options.Synchronous);
        BackupWalArgument.AddFlag(args, "--no-sync", options.NoSync);

        return args;
    }

    internal static string FormatCompression(PgReceiveWalCompression compression)
    {
        if (compression.IsLegacyLevel)
            return compression.LegacyLevel!.Value.ToString(CultureInfo.InvariantCulture);

        string method = compression.Method switch
        {
            PgReceiveWalCompressionMethod.Gzip => "gzip",
            PgReceiveWalCompressionMethod.Lz4 => "lz4",
            PgReceiveWalCompressionMethod.None => "none",
            _ => throw new ArgumentOutOfRangeException(nameof(compression)),
        };

        return compression.Level.HasValue
            ? method + ":level=" + compression.Level.Value.ToString(CultureInfo.InvariantCulture)
            : method;
    }
}

internal static class PgReceiveWalValidator
{
    internal static void Validate(PgReceiveWalOptions options, PostgreSqlMajorVersion version)
    {
        BackupWalArgument.ValidatePort(options.Port, version);
        BackupWalArgument.ValidatePositive(options.StatusInterval, "--status-interval", version, allowZero: true);

        if (options.StatusInterval.HasValue && options.StatusInterval.Value.Ticks % TimeSpan.TicksPerSecond != 0)
            throw new PgInvalidOptionValueException(version, "--status-interval", options.StatusInterval.Value);

#if NETSTANDARD2_0
        if (!Enum.IsDefined(typeof(PgReceiveWalAction), options.Action))
#else
        if (!Enum.IsDefined(options.Action))
#endif
            throw new PgInvalidOptionValueException(version, "action", options.Action);

        if (options.EndPosition is not null)
            BackupWalAvailability.Ensure(PgReceiveWalOptionAvailabilityCatalog.EndPosition, version);
        if (options.NoSync)
            BackupWalAvailability.Ensure(PgReceiveWalOptionAvailabilityCatalog.NoSync, version);

        if (options.Compression is not null && !options.Compression.IsLegacyLevel && (int)version < 15)
            throw new PgUnsupportedOptionException(version, "--compress=method", PostgreSqlMajorVersion.V15, PostgreSqlMajorVersion.V18);

        if (options.Action == PgReceiveWalAction.Receive && string.IsNullOrWhiteSpace(options.Directory))
            throw new PgInvalidOptionValueException(version, "--directory", options.Directory);

        if (options.Action != PgReceiveWalAction.Receive && string.IsNullOrWhiteSpace(options.Slot))
            throw new PgInvalidOptionCombinationException(version, options.Action == PgReceiveWalAction.CreateSlot ? "--create-slot" : "--drop-slot", "--slot");

        if (options.IfNotExists && options.Action != PgReceiveWalAction.CreateSlot)
            throw new PgInvalidOptionCombinationException(version, "--if-not-exists", "--create-slot");

        if (options.Synchronous && options.NoSync)
            throw new PgInvalidOptionCombinationException(version, "--synchronous", "--no-sync");

        if (options.Compression is not null && options.Compression.Method.HasValue &&
            options.Compression.Method.Value == PgReceiveWalCompressionMethod.Lz4 && (int)version < 15)
            throw new PgUnsupportedOptionException(version, "--compress=lz4", PostgreSqlMajorVersion.V15, PostgreSqlMajorVersion.V18);
    }
}
