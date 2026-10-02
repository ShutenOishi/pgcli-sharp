using System.Globalization;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgRewindImplementation
{
    internal static void Validate(PgRewindOptions o, PostgreSqlMajorVersion v)
    {
        ServerArgument.Text(o.TargetDataDirectory, "target-pgdata", v, true, false);
        ServerArgument.Text(o.SourceDataDirectory, "source-pgdata", v, true, false);
        ServerArgument.Text(o.SourceConnectionString, "source-server", v, true, true);
        if (o.WriteRecoveryConfiguration) MaintenanceAvailability.Ensure(PgRewindOptionAvailabilityCatalog.WriteRecoveryConf, v);
        if (o.NoEnsureShutdown) MaintenanceAvailability.Ensure(PgRewindOptionAvailabilityCatalog.NoEnsureShutdown, v);
        ServerArgument.Text(o.ConfigurationFile, "config-file", v, true, false);
        if (o.ConfigurationFile is not null) MaintenanceAvailability.Ensure(PgRewindOptionAvailabilityCatalog.ConfigFile, v);
        if (o.RestoreTargetWal) MaintenanceAvailability.Ensure(PgRewindOptionAvailabilityCatalog.RestoreTargetWal, v);
        if (o.NoSync) MaintenanceAvailability.Ensure(PgRewindOptionAvailabilityCatalog.NoSync, v);
        if (o.SyncMethod.HasValue && !ServerArgument.Defined(o.SyncMethod.Value)) ServerArgument.Invalid(v, "sync-method", o.SyncMethod);
        if (o.SyncMethod.HasValue) MaintenanceAvailability.Ensure(PgRewindOptionAvailabilityCatalog.SyncMethod, v);
        ServerArgument.Required(o.TargetDataDirectory, "--target-pgdata", v);
        if (o.SourceDataDirectory is null && o.SourceConnectionString is null) ServerArgument.Invalid(v, "--source-pgdata/--source-server");
        if (o.SourceDataDirectory is not null && o.SourceConnectionString is not null) ServerArgument.Conflict(v, "--source-pgdata", "--source-server");
        if (o.WriteRecoveryConfiguration && o.SourceConnectionString is null) ServerArgument.Conflict(v, "--write-recovery-conf", "--source-server");
        ServerArgument.Sync(o.SyncMethod, v);
    }

    internal static IReadOnlyList<string> Build(PgRewindOptions o, PostgreSqlMajorVersion v)
    {
        var args = new List<string>();
        ServerArgument.Value(args, "--target-pgdata", o.TargetDataDirectory);
        ServerArgument.Value(args, "--source-pgdata", o.SourceDataDirectory);
        ServerArgument.Value(args, "--source-server", o.SourceConnectionString);
        ServerArgument.Flag(args, "--write-recovery-conf", o.WriteRecoveryConfiguration);
        ServerArgument.Flag(args, "--no-ensure-shutdown", o.NoEnsureShutdown);
        ServerArgument.Value(args, "--config-file", o.ConfigurationFile);
        ServerArgument.Flag(args, "--restore-target-wal", o.RestoreTargetWal);
        ServerArgument.Flag(args, "--dry-run", o.DryRun);
        ServerArgument.Flag(args, "--no-sync", o.NoSync);
        ServerArgument.Flag(args, "--progress", o.Progress);
        ServerArgument.Flag(args, "--debug", o.Debug);
        if (o.SyncMethod.HasValue) ServerArgument.Value(args, "--sync-method", ServerArgument.SyncName(o.SyncMethod.Value));
        return args;
    }
}
