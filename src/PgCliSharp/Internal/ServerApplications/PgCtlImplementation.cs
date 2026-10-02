using System.Globalization;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.ServerApplications;

internal static class PgCtlImplementation
{
    internal static readonly MaintenanceOptionAvailabilityInfo LogRotateAvailability = MaintenanceAvailability.Since("logrotate", PostgreSqlMajorVersion.V12);
    internal static void Validate(PgCtlOptions o, PostgreSqlMajorVersion v)
    {
        ServerArgument.Text(o.DataDirectory, "pgdata", v, true, false);
        if (o.Command.HasValue && !ServerArgument.Defined(o.Command.Value)) ServerArgument.Invalid(v, "command", o.Command);
        ServerArgument.Text(o.LogFile, "log", v, true, false);
        if (o.ShutdownMode.HasValue && !ServerArgument.Defined(o.ShutdownMode.Value)) ServerArgument.Invalid(v, "mode", o.ShutdownMode);
        foreach (string value in o.ForwardedOptions)
        {
            if (value is null) ServerArgument.Invalid(v, "options");
            ServerArgument.Text(value, "options", v);
        }
        ServerArgument.Text(o.ServerExecutablePath, "executable", v, true, false);
        if (o.Signal.HasValue && !ServerArgument.Defined(o.Signal.Value)) ServerArgument.Invalid(v, "signal", o.Signal);
        ServerArgument.Text(o.ServiceName, "service-name", v, false, false);
        ServerArgument.Text(o.ServicePassword, "service-password", v, false, true);
        ServerArgument.Text(o.ServiceUsername, "service-username", v, false, false);
        if (o.ServiceStart.HasValue && !ServerArgument.Defined(o.ServiceStart.Value)) ServerArgument.Invalid(v, "service-start", o.ServiceStart);
        ServerArgument.Text(o.EventSource, "event-source", v, false, false);
        if (!o.Command.HasValue) ServerArgument.Invalid(v, "command");
        if (o.Command == PgCtlCommand.LogRotate) MaintenanceAvailability.Ensure(LogRotateAvailability, v);
        if (o.Command is PgCtlCommand.Start or PgCtlCommand.Restart) ServerArgument.Required(o.LogFile, "--log", v);
        if (o.Command is not (PgCtlCommand.Kill or PgCtlCommand.Unregister))
            ServerArgument.Directory(o.DataDirectory, "PGDATA", o.EnvironmentVariables, "--pgdata", v);
        if (o.Command == PgCtlCommand.Kill)
        {
            if (!o.Signal.HasValue) ServerArgument.Invalid(v, "signal");
            if (!o.ProcessId.HasValue || o.ProcessId.Value <= 0) ServerArgument.Invalid(v, "pid", o.ProcessId);
        }
        else if (o.Signal.HasValue || o.ProcessId.HasValue) ServerArgument.Conflict(v, "signal/pid", "command=kill");
        if (o.WaitTimeoutSeconds < 0) ServerArgument.Invalid(v, "--timeout", o.WaitTimeoutSeconds);
        bool service = o.Command is PgCtlCommand.Register or PgCtlCommand.Unregister or PgCtlCommand.RunService;
        if ((service || o.ServiceStart.HasValue || o.ServiceName is not null || o.ServiceUsername is not null || o.ServicePassword is not null || o.EventSource is not null) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            ServerArgument.Invalid(v, "Windows service options");
    }

    internal static IReadOnlyList<string> Build(PgCtlOptions o, PostgreSqlMajorVersion v)
    {
        var args = new List<string>();
        ServerArgument.Value(args, "--pgdata", o.DataDirectory);
        ServerArgument.Value(args, "--log", o.LogFile);
        if (o.ShutdownMode.HasValue) ServerArgument.Value(args, "--mode", o.ShutdownMode.Value.ToString().ToLowerInvariant());
        foreach (string value in o.ForwardedOptions) ServerArgument.Value(args, "--options", value);
        ServerArgument.Flag(args, "--silent", o.Silent);
        ServerArgument.Value(args, "--timeout", o.WaitTimeoutSeconds);
        ServerArgument.Flag(args, "--core-files", o.CoreFiles);
        MaintenanceArgument.AddNullableBoolean(args, o.Wait, "--wait", "--no-wait");
        ServerArgument.Value(args, "-p", o.ServerExecutablePath);
        ServerArgument.Value(args, "-N", o.ServiceName);
        ServerArgument.Value(args, "-P", o.ServicePassword);
        ServerArgument.Value(args, "-U", o.ServiceUsername);
        if (o.ServiceStart.HasValue) ServerArgument.Value(args, "-S", o.ServiceStart == PgCtlServiceStart.Automatic ? "auto" : "demand");
        ServerArgument.Value(args, "-e", o.EventSource);
        args.Add(o.Command!.Value switch
        {
            PgCtlCommand.InitDb => "initdb",
            PgCtlCommand.LogRotate => "logrotate",
            PgCtlCommand.RunService => "runservice",
            _ => o.Command.Value.ToString().ToLowerInvariant(),
        });
        if (o.Command == PgCtlCommand.Kill)
        {
            args.Add(o.Signal == PgCtlSignal.Interrupt ? "INT" : o.Signal!.Value.ToString().ToUpperInvariant());
            args.Add(o.ProcessId!.Value.ToString(CultureInfo.InvariantCulture));
        }
        return args;
    }
}
