using System.Globalization;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgChecksumsImplementation
{
    internal static void Validate(PgChecksumsOptions o, PostgreSqlMajorVersion v)
    {
        ServerArgument.Text(o.DataDirectory, "pgdata", v, true, false);
        if (!ServerArgument.Defined(o.Mode)) ServerArgument.Invalid(v, "check", o.Mode);
        if (o.SyncMethod.HasValue && !ServerArgument.Defined(o.SyncMethod.Value)) ServerArgument.Invalid(v, "sync-method", o.SyncMethod);
        if (o.SyncMethod.HasValue) MaintenanceAvailability.Ensure(PgChecksumsOptionAvailabilityCatalog.SyncMethod, v);
        ServerArgument.Directory(o.DataDirectory, "PGDATA", o.EnvironmentVariables, "--pgdata", v);
        if (o.FileNode < 0 || (o.FileNode == 0 && v < PostgreSqlMajorVersion.V15)) ServerArgument.Invalid(v, "--filenode", o.FileNode);
        if (o.FileNode.HasValue && o.Mode != PgChecksumsMode.Check) ServerArgument.Conflict(v, "--filenode", "--check");
        ServerArgument.Sync(o.SyncMethod, v);
    }

    internal static IReadOnlyList<string> Build(PgChecksumsOptions o, PostgreSqlMajorVersion v)
    {
        var args = new List<string>();
        ServerArgument.Value(args, "--pgdata", o.DataDirectory);
        args.Add(o.Mode switch { PgChecksumsMode.Check => "--check", PgChecksumsMode.Enable => "--enable", _ => "--disable" });
        ServerArgument.Value(args, "--filenode", o.FileNode);
        ServerArgument.Flag(args, "--no-sync", o.NoSync);
        ServerArgument.Flag(args, "--progress", o.Progress);
        ServerArgument.Flag(args, "--verbose", o.Verbose);
        if (o.SyncMethod.HasValue) ServerArgument.Value(args, "--sync-method", ServerArgument.SyncName(o.SyncMethod.Value));
        return args;
    }
}
