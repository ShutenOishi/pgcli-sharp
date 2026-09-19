using System.Globalization;
using PgCliSharp.Internal.BackupWal;

namespace PgCliSharp.Internal.PgRecvLogical;

internal static class PgRecvLogicalOptionAvailabilityCatalog
{
    internal static readonly BackupWalOptionAvailabilityInfo TwoPhase = BackupWalAvailability.Since("--two-phase", PostgreSqlMajorVersion.V15);
    internal static readonly BackupWalOptionAvailabilityInfo EnableTwoPhase = BackupWalAvailability.Since("--enable-two-phase", PostgreSqlMajorVersion.V18);
    internal static readonly BackupWalOptionAvailabilityInfo EnableFailover = BackupWalAvailability.Since("--enable-failover", PostgreSqlMajorVersion.V18);

    internal static readonly IReadOnlyList<BackupWalOptionAvailabilityInfo> All = new[] { TwoPhase, EnableTwoPhase, EnableFailover };
}

internal static class PgRecvLogicalArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgRecvLogicalOptions options, PgRecvLogicalOutput? output, PostgreSqlMajorVersion version)
    {
        var args = new List<string>();

        if (output is not null) BackupWalArgument.AddValue(args, "--file", output.Value);

        if (options.FsyncInterval.HasValue)
            BackupWalArgument.AddValue(args, "--fsync-interval", ((long)options.FsyncInterval.Value.TotalSeconds).ToString(CultureInfo.InvariantCulture));

        BackupWalArgument.AddFlag(args, "--no-loop", options.NoLoop);
        BackupWalArgument.AddFlag(args, "--verbose", options.Verbose);

        if (options.EnableTwoPhase)
            args.Add((int)version >= 18 ? "--enable-two-phase" : "--two-phase");

        BackupWalArgument.AddFlag(args, "--enable-failover", options.EnableFailover);
        BackupWalArgument.AddValue(args, "--dbname", options.Database);
        BackupWalArgument.AddValue(args, "--host", options.Host);
        BackupWalArgument.AddValue(args, "--port", options.Port);
        BackupWalArgument.AddValue(args, "--username", options.Username);
        BackupWalArgument.AddPassword(args, options.PasswordPrompt);
        BackupWalArgument.AddValue(args, "--startpos", options.StartPosition);
        BackupWalArgument.AddValue(args, "--endpos", options.EndPosition);

        foreach (string pluginOption in options.PluginOptions)
            BackupWalArgument.AddValue(args, "--option", pluginOption);

        BackupWalArgument.AddValue(args, "--plugin", options.Plugin);

        if (options.StatusInterval.HasValue)
            BackupWalArgument.AddValue(args, "--status-interval", ((long)options.StatusInterval.Value.TotalSeconds).ToString(CultureInfo.InvariantCulture));

        BackupWalArgument.AddValue(args, "--slot", options.Slot);

        if ((options.Action & PgRecvLogicalAction.CreateSlot) != 0) args.Add("--create-slot");
        if ((options.Action & PgRecvLogicalAction.Start) != 0) args.Add("--start");
        if ((options.Action & PgRecvLogicalAction.DropSlot) != 0) args.Add("--drop-slot");

        BackupWalArgument.AddFlag(args, "--if-not-exists", options.IfNotExists);

        return args;
    }
}

internal static class PgRecvLogicalValidator
{
    internal static void Validate(PgRecvLogicalOptions options, PgRecvLogicalOutput? output, PostgreSqlMajorVersion version)
    {
        BackupWalArgument.ValidatePort(options.Port, version);
        BackupWalArgument.ValidatePositive(options.FsyncInterval, "--fsync-interval", version, allowZero: true);
        BackupWalArgument.ValidatePositive(options.StatusInterval, "--status-interval", version, allowZero: true);

        if (options.FsyncInterval.HasValue && options.FsyncInterval.Value.Ticks % TimeSpan.TicksPerSecond != 0)
            throw new PgInvalidOptionValueException(version, "--fsync-interval", options.FsyncInterval.Value);
        if (options.StatusInterval.HasValue && options.StatusInterval.Value.Ticks % TimeSpan.TicksPerSecond != 0)
            throw new PgInvalidOptionValueException(version, "--status-interval", options.StatusInterval.Value);

        PgRecvLogicalAction validMask = PgRecvLogicalAction.CreateSlot | PgRecvLogicalAction.Start | PgRecvLogicalAction.DropSlot;
        if (options.Action == PgRecvLogicalAction.None || (options.Action & ~validMask) != 0)
            throw new PgInvalidOptionValueException(version, "action", options.Action);

        bool create = (options.Action & PgRecvLogicalAction.CreateSlot) != 0;
        bool start = (options.Action & PgRecvLogicalAction.Start) != 0;
        bool drop = (options.Action & PgRecvLogicalAction.DropSlot) != 0;

        if (drop && (create || start))
            throw new PgInvalidOptionCombinationException(version, "--drop-slot", create ? "--create-slot" : "--start");

        if (string.IsNullOrWhiteSpace(options.Slot))
            throw new PgInvalidOptionValueException(version, "--slot", options.Slot);

        if (start && output is null)
            throw new PgInvalidOptionCombinationException(version, "--start", "--file");

        if (!start && output is not null)
            throw new PgInvalidOptionCombinationException(version, "--file", "--start");

        if (!drop || (int)version < 18)
        {
            if (string.IsNullOrWhiteSpace(options.Database))
                throw new PgInvalidOptionValueException(version, "--dbname", options.Database);
        }

        if (options.StartPosition is not null && (create || drop))
            throw new PgInvalidOptionCombinationException(version, "--startpos", create ? "--create-slot" : "--drop-slot");

        if (options.EndPosition is not null && !start)
            throw new PgInvalidOptionCombinationException(version, "--endpos", "--start");

        if (options.EnableTwoPhase)
        {
            BackupWalAvailability.Ensure(PgRecvLogicalOptionAvailabilityCatalog.TwoPhase, version);
            if (!create)
                throw new PgInvalidOptionCombinationException(version, (int)version >= 18 ? "--enable-two-phase" : "--two-phase", "--create-slot");
        }

        if (options.EnableFailover)
        {
            BackupWalAvailability.Ensure(PgRecvLogicalOptionAvailabilityCatalog.EnableFailover, version);
            if (!create)
                throw new PgInvalidOptionCombinationException(version, "--enable-failover", "--create-slot");
        }

        if (options.IfNotExists && !create)
            throw new PgInvalidOptionCombinationException(version, "--if-not-exists", "--create-slot");

        BackupWalArgument.ValidateNonEmpty(options.PluginOptions, "--option", version);
    }
}
