using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Createdb;

internal static class CreatedbOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo Strategy = MaintenanceAvailability.Since("--strategy", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo LocaleProvider = MaintenanceAvailability.Since("--locale-provider", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo IcuLocale = MaintenanceAvailability.Since("--icu-locale", PostgreSqlMajorVersion.V15);
    internal static readonly MaintenanceOptionAvailabilityInfo IcuRules = MaintenanceAvailability.Since("--icu-rules", PostgreSqlMajorVersion.V16);
    internal static readonly MaintenanceOptionAvailabilityInfo BuiltinLocale = MaintenanceAvailability.Since("--builtin-locale", PostgreSqlMajorVersion.V17);

    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All =
        new[] { Strategy, LocaleProvider, IcuLocale, IcuRules, BuiltinLocale };
}

internal static class CreatedbArgumentBuilder
{
    internal static IReadOnlyList<string> Build(CreateDbOptions options, PostgreSqlMajorVersion version)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddValue(args, "--owner", options.Owner);
        MaintenanceArgument.AddValue(args, "--tablespace", options.Tablespace);
        MaintenanceArgument.AddValue(args, "--template", options.Template);
        MaintenanceArgument.AddValue(args, "--encoding", options.Encoding);

        if (options.Strategy.HasValue)
            MaintenanceArgument.AddValue(args, "--strategy", options.Strategy.Value == PgCreateDbStrategy.WalLog ? "wal_log" : "file_copy");

        MaintenanceArgument.AddValue(args, "--lc-collate", options.LcCollate);
        MaintenanceArgument.AddValue(args, "--lc-ctype", options.LcCtype);
        MaintenanceArgument.AddValue(args, "--locale", options.Locale);
        MaintenanceArgument.AddValue(args, "--maintenance-db", options.MaintenanceDatabase);

        if (options.LocaleProvider.HasValue)
            MaintenanceArgument.AddValue(args, "--locale-provider", options.LocaleProvider.Value switch
            {
                PgCreateDbLocaleProvider.Libc => "libc",
                PgCreateDbLocaleProvider.Icu => "icu",
                PgCreateDbLocaleProvider.Builtin => "builtin",
                _ => throw new ArgumentOutOfRangeException(nameof(options)),
            });

        MaintenanceArgument.AddValue(args, "--builtin-locale", options.BuiltinLocale);
        MaintenanceArgument.AddValue(args, "--icu-locale", options.IcuLocale);
        MaintenanceArgument.AddValue(args, "--icu-rules", options.IcuRules);

        if (options.DatabaseName is not null) args.Add(options.DatabaseName);
        if (options.Description is not null) args.Add(options.Description);
        return args;
    }
}

internal static class CreatedbValidator
{
    internal static void Validate(CreateDbOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        if (options.Strategy.HasValue) MaintenanceAvailability.Ensure(CreatedbOptionAvailabilityCatalog.Strategy, version);
        if (options.LocaleProvider.HasValue) MaintenanceAvailability.Ensure(CreatedbOptionAvailabilityCatalog.LocaleProvider, version);
        if (options.IcuLocale is not null) MaintenanceAvailability.Ensure(CreatedbOptionAvailabilityCatalog.IcuLocale, version);
        if (options.IcuRules is not null) MaintenanceAvailability.Ensure(CreatedbOptionAvailabilityCatalog.IcuRules, version);
        if (options.BuiltinLocale is not null) MaintenanceAvailability.Ensure(CreatedbOptionAvailabilityCatalog.BuiltinLocale, version);

        if (options.LocaleProvider == PgCreateDbLocaleProvider.Builtin && (int)version < 17)
            throw new PgUnsupportedOptionException(version, "--locale-provider=builtin", PostgreSqlMajorVersion.V17, PostgreSqlMajorVersion.V18);

        if (options.Description is not null && string.IsNullOrWhiteSpace(options.DatabaseName))
            throw new PgInvalidOptionCombinationException(version, "description", "database-name");

        ValidateNonBlank(options.DatabaseName, "database-name", version);
        ValidateNonBlank(options.Description, "description", version);
    }

    private static void ValidateNonBlank(string? value, string option, PostgreSqlMajorVersion version)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
            throw new PgInvalidOptionValueException(version, option, value);
    }
}
