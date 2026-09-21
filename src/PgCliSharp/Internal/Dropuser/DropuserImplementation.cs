using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Dropuser;

internal static class DropuserOptionAvailabilityCatalog
{
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = Array.Empty<MaintenanceOptionAvailabilityInfo>();
}

internal static class DropuserArgumentBuilder
{
    internal static IReadOnlyList<string> Build(DropUserOptions options)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddFlag(args, "--interactive", options.Interactive);
        MaintenanceArgument.AddFlag(args, "--if-exists", options.IfExists);
        if (options.RoleName is not null) args.Add(options.RoleName);
        return args;
    }
}

internal static class DropuserValidator
{
    internal static void Validate(DropUserOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        if (!options.Interactive && string.IsNullOrWhiteSpace(options.RoleName))
            throw new PgInvalidOptionValueException(version, "role-name", options.RoleName);
        if (options.RoleName is not null && string.IsNullOrWhiteSpace(options.RoleName))
            throw new PgInvalidOptionValueException(version, "role-name", options.RoleName);
    }
}
