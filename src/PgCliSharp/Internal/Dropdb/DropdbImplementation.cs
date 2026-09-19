using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Dropdb;

internal static class DropdbOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo Force = MaintenanceAvailability.Since("--force", PostgreSqlMajorVersion.V13);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new[] { Force };
}

internal static class DropdbArgumentBuilder
{
    internal static IReadOnlyList<string> Build(DropDbOptions options)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddFlag(args, "--interactive", options.Interactive);
        MaintenanceArgument.AddFlag(args, "--if-exists", options.IfExists);
        MaintenanceArgument.AddValue(args, "--maintenance-db", options.MaintenanceDatabase);
        MaintenanceArgument.AddFlag(args, "--force", options.Force);
        args.Add(options.DatabaseName!);
        return args;
    }
}

internal static class DropdbValidator
{
    internal static void Validate(DropDbOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        if (string.IsNullOrWhiteSpace(options.DatabaseName))
            throw new PgInvalidOptionValueException(version, "database-name", options.DatabaseName);
        if (options.Force) MaintenanceAvailability.Ensure(DropdbOptionAvailabilityCatalog.Force, version);
    }
}
