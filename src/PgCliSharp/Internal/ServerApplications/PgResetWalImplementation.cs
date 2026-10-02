using System.Globalization;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgResetWalImplementation
{
    internal static void Validate(PgResetWalOptions o, PostgreSqlMajorVersion v)
    {
        ServerArgument.Text(o.DataDirectory, "pgdata", v, true, false);
        if (o.WalSegmentSizeMegabytes.HasValue) MaintenanceAvailability.Ensure(PgResetWalOptionAvailabilityCatalog.WalSegsize, v);
        if (o.CharSignedness.HasValue && !ServerArgument.Defined(o.CharSignedness.Value)) ServerArgument.Invalid(v, "char-signedness", o.CharSignedness);
        if (o.CharSignedness.HasValue) MaintenanceAvailability.Ensure(PgResetWalOptionAvailabilityCatalog.CharSignedness, v);
        ServerArgument.Required(o.DataDirectory, "-D", v);
        ServerArgument.WalSize(o.WalSegmentSizeMegabytes, v);
        if (o.TransactionIdEpoch == uint.MaxValue) ServerArgument.Invalid(v, "-e", o.TransactionIdEpoch);
        if (o.OldestTransactionId < 3) ServerArgument.Invalid(v, "-u", o.OldestTransactionId);
        if (o.NextTransactionId.HasValue && o.NextTransactionId.Value < (v >= PostgreSqlMajorVersion.V12 ? 3u : 1u))
            ServerArgument.Invalid(v, "-x", o.NextTransactionId);
        if (o.NextObjectId == 0) ServerArgument.Invalid(v, "-o", o.NextObjectId);
        if (o.MultiTransactionOffset == uint.MaxValue && v < PostgreSqlMajorVersion.V15) ServerArgument.Invalid(v, "-O", o.MultiTransactionOffset);
        if (o.MultiTransactionIds is not null && (o.MultiTransactionIds.Oldest == 0 || (o.MultiTransactionIds.Next == 0 && v < PostgreSqlMajorVersion.V15)))
            ServerArgument.Invalid(v, "-m");
        if (o.CommitTimestampIds is not null)
        {
            uint minimum = v >= PostgreSqlMajorVersion.V17 ? 3u : 2u;
            if ((o.CommitTimestampIds.Oldest != 0 && o.CommitTimestampIds.Oldest < minimum) ||
                (o.CommitTimestampIds.Newest != 0 && o.CommitTimestampIds.Newest < minimum)) ServerArgument.Invalid(v, "-c");
        }
    }

    internal static IReadOnlyList<string> Build(PgResetWalOptions o, PostgreSqlMajorVersion v)
    {
        var args = new List<string>();
        ServerArgument.Value(args, "-D", o.DataDirectory);
        ServerArgument.Value(args, "-c", o.CommitTimestampIds?.ToArgument());
        ServerArgument.Value(args, "-e", o.TransactionIdEpoch);
        ServerArgument.Flag(args, "-f", o.Force);
        ServerArgument.Value(args, "-l", o.NextWalFile?.Value);
        ServerArgument.Value(args, "-m", o.MultiTransactionIds?.ToArgument());
        ServerArgument.Flag(args, "-n", o.DryRun);
        ServerArgument.Value(args, "-o", o.NextObjectId);
        ServerArgument.Value(args, "-O", o.MultiTransactionOffset);
        ServerArgument.Value(args, "-u", o.OldestTransactionId);
        ServerArgument.Value(args, "-x", o.NextTransactionId);
        ServerArgument.Value(args, "--wal-segsize", o.WalSegmentSizeMegabytes);
        if (o.CharSignedness.HasValue) ServerArgument.Value(args, "--char-signedness", ServerArgument.CharName(o.CharSignedness.Value));
        return args;
    }
}
