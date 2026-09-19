using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Createuser;

internal static class CreateuserOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo AdduserAlias = MaintenanceAvailability.Range("--adduser", PostgreSqlMajorVersion.V10, PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo NoAdduserAlias = MaintenanceAvailability.Range("--no-adduser", PostgreSqlMajorVersion.V10, PostgreSqlMajorVersion.V12);
    internal static readonly MaintenanceOptionAvailabilityInfo MemberOf = MaintenanceAvailability.Since("--member-of", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo WithAdmin = MaintenanceAvailability.Since("--with-admin", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo WithMember = MaintenanceAvailability.Since("--with-member", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo ValidUntil = MaintenanceAvailability.Since("--valid-until", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo BypassRls = MaintenanceAvailability.Since("--bypassrls", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo NoBypassRls = MaintenanceAvailability.Since("--no-bypassrls", PostgreSqlMajorVersion.V16);

    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new[]
    {
        AdduserAlias, NoAdduserAlias, MemberOf, WithAdmin, WithMember, ValidUntil, BypassRls, NoBypassRls,
    };
}

internal static class CreateuserArgumentBuilder
{
    internal static IReadOnlyList<string> Build(CreateUserOptions options, PostgreSqlMajorVersion version)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddNullableBoolean(args, options.CanCreateDatabase, "--createdb", "--no-createdb");
        MaintenanceArgument.AddNullableBoolean(args, options.IsSuperuser, "--superuser", "--no-superuser");
        MaintenanceArgument.AddNullableBoolean(args, options.CanCreateRole, "--createrole", "--no-createrole");
        MaintenanceArgument.AddNullableBoolean(args, options.Inherit, "--inherit", "--no-inherit");
        MaintenanceArgument.AddNullableBoolean(args, options.Login, "--login", "--no-login");
        MaintenanceArgument.AddNullableBoolean(args, options.Replication, "--replication", "--no-replication");
        MaintenanceArgument.AddFlag(args, "--interactive", options.Interactive);
        MaintenanceArgument.AddValue(args, "--connection-limit", options.ConnectionLimit);
        MaintenanceArgument.AddFlag(args, "--pwprompt", options.PromptForRolePassword);
        MaintenanceArgument.AddFlag(args, "--encrypted", options.Encrypted);

        string membershipOption = (int)version >= 16 ? "--member-of" : "--role";
        MaintenanceArgument.AddRepeatable(args, membershipOption, options.MemberOfRoles);
        MaintenanceArgument.AddRepeatable(args, "--with-admin", options.AdminOfRoles);
        MaintenanceArgument.AddRepeatable(args, "--with-member", options.Members);
        MaintenanceArgument.AddValue(args, "--valid-until", options.ValidUntil);

        if (options.BypassRls.HasValue)
            args.Add(options.BypassRls.Value ? "--bypassrls" : "--no-bypassrls");

        if (options.RoleName is not null) args.Add(options.RoleName);
        return args;
    }
}

internal static class CreateuserValidator
{
    internal static void Validate(CreateUserOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);

        if (options.ConnectionLimit.HasValue && options.ConnectionLimit.Value < -1)
            throw new PgInvalidOptionValueException(version, "--connection-limit", options.ConnectionLimit.Value);

        if (options.AdminOfRoles.Count > 0) MaintenanceAvailability.Ensure(CreateuserOptionAvailabilityCatalog.WithAdmin, version);
        if (options.Members.Count > 0) MaintenanceAvailability.Ensure(CreateuserOptionAvailabilityCatalog.WithMember, version);
        if (options.ValidUntil is not null) MaintenanceAvailability.Ensure(CreateuserOptionAvailabilityCatalog.ValidUntil, version);
        if (options.BypassRls.HasValue)
            MaintenanceAvailability.Ensure(
                options.BypassRls.Value ? CreateuserOptionAvailabilityCatalog.BypassRls : CreateuserOptionAvailabilityCatalog.NoBypassRls,
                version);

        MaintenanceArgument.ValidateNonEmpty(options.MemberOfRoles, (int)version >= 16 ? "--member-of" : "--role", version);
        MaintenanceArgument.ValidateNonEmpty(options.AdminOfRoles, "--with-admin", version);
        MaintenanceArgument.ValidateNonEmpty(options.Members, "--with-member", version);

        if (options.RoleName is not null && string.IsNullOrWhiteSpace(options.RoleName))
            throw new PgInvalidOptionValueException(version, "role-name", options.RoleName);
        if (options.ValidUntil is not null && string.IsNullOrWhiteSpace(options.ValidUntil))
            throw new PgInvalidOptionValueException(version, "--valid-until", options.ValidUntil);
    }
}
