using System.Globalization;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgUpgradeImplementation
{
    internal static void Validate(PgUpgradeOptions o, PostgreSqlMajorVersion v)
    {
        ServerArgument.Text(o.OldDataDirectory, "old-datadir", v, true, false);
        ServerArgument.Text(o.NewDataDirectory, "new-datadir", v, true, false);
        ServerArgument.Text(o.OldBinaryDirectory, "old-bindir", v, true, false);
        ServerArgument.Text(o.NewBinaryDirectory, "new-bindir", v, true, false);
        foreach (string value in o.OldServerOptions)
        {
            if (value is null) ServerArgument.Invalid(v, "old-options");
            ServerArgument.Text(value, "old-options", v);
        }
        foreach (string value in o.NewServerOptions)
        {
            if (value is null) ServerArgument.Invalid(v, "new-options");
            ServerArgument.Text(value, "new-options", v);
        }
        ServerArgument.Text(o.Username, "username", v, true, false);
        if (o.TransferMode.HasValue && !ServerArgument.Defined(o.TransferMode.Value)) ServerArgument.Invalid(v, "link", o.TransferMode);
        ServerArgument.Text(o.SocketDirectory, "socketdir", v, true, false);
        if (o.SocketDirectory is not null) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.Socketdir, v);
        if (o.NoSync) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.NoSync, v);
        if (o.SyncMethod.HasValue && !ServerArgument.Defined(o.SyncMethod.Value)) ServerArgument.Invalid(v, "sync-method", o.SyncMethod);
        if (o.SyncMethod.HasValue) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.SyncMethod, v);
        if (o.NoStatistics) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.NoStatistics, v);
        if (o.CharSignedness.HasValue && !ServerArgument.Defined(o.CharSignedness.Value)) ServerArgument.Invalid(v, "set-char-signedness", o.CharSignedness);
        if (o.CharSignedness.HasValue) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.SetCharSignedness, v);
        ServerArgument.Directory(o.OldDataDirectory, "PGDATAOLD", o.EnvironmentVariables, "--old-datadir", v);
        ServerArgument.Directory(o.NewDataDirectory, "PGDATANEW", o.EnvironmentVariables, "--new-datadir", v);
        ServerArgument.Directory(o.OldBinaryDirectory, "PGBINOLD", o.EnvironmentVariables, "--old-bindir", v);
        MaintenanceArgument.ValidatePositive(o.Jobs, "--jobs", v);
        MaintenanceArgument.ValidatePort(o.OldPort, v);
        MaintenanceArgument.ValidatePort(o.NewPort, v);
        if (o.TransferMode == PgUpgradeTransferMode.Clone) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.Clone, v);
        if (o.TransferMode == PgUpgradeTransferMode.CopyFileRange) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.CopyFileRange, v);
        if (o.TransferMode == PgUpgradeTransferMode.Swap) MaintenanceAvailability.Ensure(PgUpgradeOptionAvailabilityCatalog.Swap, v);
        ServerArgument.Sync(o.SyncMethod, v);
    }

    internal static IReadOnlyList<string> Build(PgUpgradeOptions o, PostgreSqlMajorVersion v)
    {
        var args = new List<string>();
        ServerArgument.Value(args, "--old-datadir", o.OldDataDirectory);
        ServerArgument.Value(args, "--new-datadir", o.NewDataDirectory);
        ServerArgument.Value(args, "--old-bindir", o.OldBinaryDirectory);
        ServerArgument.Value(args, "--new-bindir", o.NewBinaryDirectory);
        foreach (string value in o.OldServerOptions) ServerArgument.Value(args, "--old-options", value);
        foreach (string value in o.NewServerOptions) ServerArgument.Value(args, "--new-options", value);
        ServerArgument.Value(args, "--old-port", o.OldPort);
        ServerArgument.Value(args, "--new-port", o.NewPort);
        ServerArgument.Value(args, "--username", o.Username);
        ServerArgument.Flag(args, "--check", o.CheckOnly);
        if (o.TransferMode.HasValue)
        {
            string? mode = o.TransferMode.Value switch
            {
                PgUpgradeTransferMode.Copy => v >= PostgreSqlMajorVersion.V16 ? "--copy" : null,
                PgUpgradeTransferMode.Link => "--link",
                PgUpgradeTransferMode.Clone => "--clone",
                PgUpgradeTransferMode.CopyFileRange => "--copy-file-range",
                _ => "--swap",
            };
            if (mode is not null) args.Add(mode);
        }
        ServerArgument.Flag(args, "--retain", o.Retain);
        ServerArgument.Value(args, "--jobs", o.Jobs);
        ServerArgument.Value(args, "--socketdir", o.SocketDirectory);
        ServerArgument.Flag(args, "--verbose", o.Verbose);
        ServerArgument.Flag(args, "--no-sync", o.NoSync);
        if (o.SyncMethod.HasValue) ServerArgument.Value(args, "--sync-method", ServerArgument.SyncName(o.SyncMethod.Value));
        ServerArgument.Flag(args, "--no-statistics", o.NoStatistics);
        if (o.CharSignedness.HasValue) ServerArgument.Value(args, "--set-char-signedness", ServerArgument.CharName(o.CharSignedness.Value));
        return args;
    }
}
