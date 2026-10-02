using System.Globalization;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.ServerApplications;

internal static class InitDbImplementation
{
    internal static void Validate(InitDbOptions o, PostgreSqlMajorVersion v)
    {
        ServerArgument.Text(o.DataDirectory, "pgdata", v, true, false);
        ServerArgument.Text(o.Encoding, "encoding", v, false, false);
        ServerArgument.Text(o.Locale, "locale", v, false, false);
        ServerArgument.Text(o.LcCollate, "lc-collate", v, false, false);
        ServerArgument.Text(o.LcCtype, "lc-ctype", v, false, false);
        ServerArgument.Text(o.LcMessages, "lc-messages", v, false, false);
        ServerArgument.Text(o.LcMonetary, "lc-monetary", v, false, false);
        ServerArgument.Text(o.LcNumeric, "lc-numeric", v, false, false);
        ServerArgument.Text(o.LcTime, "lc-time", v, false, false);
        ServerArgument.Text(o.TextSearchConfiguration, "text-search-config", v, false, false);
        if (o.Authentication.HasValue && !ServerArgument.Defined(o.Authentication.Value)) ServerArgument.Invalid(v, "auth", o.Authentication);
        if (o.LocalAuthentication.HasValue && !ServerArgument.Defined(o.LocalAuthentication.Value)) ServerArgument.Invalid(v, "auth-local", o.LocalAuthentication);
        if (o.HostAuthentication.HasValue && !ServerArgument.Defined(o.HostAuthentication.Value)) ServerArgument.Invalid(v, "auth-host", o.HostAuthentication);
        ServerArgument.Text(o.PasswordFile, "pwfile", v, true, false);
        ServerArgument.Text(o.Username, "username", v, true, false);
        if (o.NoInstructions) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.NoInstructions, v);
        ServerArgument.Text(o.WalDirectory, "waldir", v, true, false);
        if (o.WalSegmentSizeMegabytes.HasValue) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.WalSegsize, v);
        if (o.AllowGroupAccess) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.AllowGroupAccess, v);
        if (o.DiscardCaches) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.DiscardCaches, v);
        if (o.LocaleProvider.HasValue && !ServerArgument.Defined(o.LocaleProvider.Value)) ServerArgument.Invalid(v, "locale-provider", o.LocaleProvider);
        if (o.LocaleProvider.HasValue) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.LocaleProvider, v);
        ServerArgument.Text(o.IcuLocale, "icu-locale", v, false, false);
        if (o.IcuLocale is not null) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.IcuLocale, v);
        ServerArgument.Text(o.IcuRules, "icu-rules", v, false, false);
        if (o.IcuRules is not null) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.IcuRules, v);
        if (o.BuiltinLocale.HasValue && !ServerArgument.Defined(o.BuiltinLocale.Value)) ServerArgument.Invalid(v, "builtin-locale", o.BuiltinLocale);
        if (o.BuiltinLocale.HasValue) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.BuiltinLocale, v);
        if (o.Settings.Any(value => value is null)) ServerArgument.Invalid(v, "set");
        if (o.Settings.Count > 0) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.Set, v);
        if (o.SyncMethod.HasValue && !ServerArgument.Defined(o.SyncMethod.Value)) ServerArgument.Invalid(v, "sync-method", o.SyncMethod);
        if (o.SyncMethod.HasValue) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.SyncMethod, v);
        if (o.NoSyncDataFiles) MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.NoSyncDataFiles, v);
        ServerArgument.Text(o.InputDirectory, "input-directory", v, true, false);
        if (!o.ShowSettings) ServerArgument.Directory(o.DataDirectory, "PGDATA", o.EnvironmentVariables, "--pgdata", v);
        ServerArgument.WalSize(o.WalSegmentSizeMegabytes, v);
        if (o.PasswordPrompt && o.PasswordFile is not null) ServerArgument.Conflict(v, "--pwprompt", "--pwfile");
        if (o.NoLocale && o.Locale is not null) ServerArgument.Conflict(v, "--no-locale", "--locale");
        if (o.WalDirectory is not null && !Path.IsPathRooted(o.WalDirectory)) ServerArgument.Invalid(v, "--waldir", o.WalDirectory);
        PgInitDbAuthentication? local = o.LocalAuthentication ?? o.Authentication;
        PgInitDbAuthentication? host = o.HostAuthentication ?? o.Authentication;
        if (local is PgInitDbAuthentication.Ident or PgInitDbAuthentication.Gss or PgInitDbAuthentication.Sspi or PgInitDbAuthentication.Cert)
            ServerArgument.Invalid(v, "--auth-local", local);
        if (host == PgInitDbAuthentication.Peer) ServerArgument.Invalid(v, "--auth-host", host);
        if (NeedsPassword(local) && NeedsPassword(host) && !o.PasswordPrompt && o.PasswordFile is null)
            ServerArgument.Conflict(v, "--auth", "--pwfile/--pwprompt");
        if (o.LocaleProvider == PgInitDbLocaleProvider.Builtin)
        {
            MaintenanceAvailability.Ensure(InitDbOptionAvailabilityCatalog.BuiltinLocale, v);
            if (!o.BuiltinLocale.HasValue && o.Locale is null && !o.NoLocale) ServerArgument.Invalid(v, "--builtin-locale");
            string? locale = o.BuiltinLocale.HasValue ? ServerArgument.Builtin(o.BuiltinLocale.Value) : o.NoLocale ? "C" : o.Locale;
            if (locale is not ("C" or "C.UTF-8" or "C.UTF8" or "PG_UNICODE_FAST")) ServerArgument.Invalid(v, "--locale", locale);
            if (locale == "PG_UNICODE_FAST" && v < PostgreSqlMajorVersion.V18) ServerArgument.Invalid(v, "--locale", locale);
        }
        if (o.BuiltinLocale.HasValue && o.LocaleProvider != PgInitDbLocaleProvider.Builtin) ServerArgument.Conflict(v, "--builtin-locale", "--locale-provider=builtin");
        if ((o.IcuLocale is not null || o.IcuRules is not null) && o.LocaleProvider != PgInitDbLocaleProvider.Icu)
            ServerArgument.Conflict(v, "--icu-locale/--icu-rules", "--locale-provider=icu");
        // PostgreSQL 15-16 derive ICU locale from the environment when no explicit locale is provided.
        if (v >= PostgreSqlMajorVersion.V17 && o.LocaleProvider == PgInitDbLocaleProvider.Icu && o.IcuLocale is null && o.Locale is null && !o.NoLocale)
            ServerArgument.Invalid(v, "--icu-locale");
        ServerArgument.Sync(o.SyncMethod, v);
    }

    private static bool NeedsPassword(PgInitDbAuthentication? value) => value is PgInitDbAuthentication.Md5 or PgInitDbAuthentication.Password or PgInitDbAuthentication.ScramSha256;

    internal static IReadOnlyList<string> Build(InitDbOptions o, PostgreSqlMajorVersion v)
    {
        var args = new List<string>();
        ServerArgument.Value(args, "--pgdata", o.DataDirectory);
        ServerArgument.Value(args, "--encoding", o.Encoding);
        ServerArgument.Value(args, "--locale", o.Locale);
        ServerArgument.Value(args, "--lc-collate", o.LcCollate);
        ServerArgument.Value(args, "--lc-ctype", o.LcCtype);
        ServerArgument.Value(args, "--lc-messages", o.LcMessages);
        ServerArgument.Value(args, "--lc-monetary", o.LcMonetary);
        ServerArgument.Value(args, "--lc-numeric", o.LcNumeric);
        ServerArgument.Value(args, "--lc-time", o.LcTime);
        ServerArgument.Flag(args, "--no-locale", o.NoLocale);
        ServerArgument.Value(args, "--text-search-config", o.TextSearchConfiguration);
        if (o.Authentication.HasValue) ServerArgument.Value(args, "--auth", ServerArgument.Authentication(o.Authentication.Value));
        if (o.LocalAuthentication.HasValue) ServerArgument.Value(args, "--auth-local", ServerArgument.Authentication(o.LocalAuthentication.Value));
        if (o.HostAuthentication.HasValue) ServerArgument.Value(args, "--auth-host", ServerArgument.Authentication(o.HostAuthentication.Value));
        ServerArgument.Flag(args, "--pwprompt", o.PasswordPrompt);
        ServerArgument.Value(args, "--pwfile", o.PasswordFile);
        ServerArgument.Value(args, "--username", o.Username);
        ServerArgument.Flag(args, "--debug", o.Debug);
        ServerArgument.Flag(args, "--show", o.ShowSettings);
        ServerArgument.Flag(args, "--no-clean", o.NoClean);
        ServerArgument.Flag(args, "--no-sync", o.NoSync);
        ServerArgument.Flag(args, "--no-instructions", o.NoInstructions);
        ServerArgument.Flag(args, "--sync-only", o.SyncOnly);
        ServerArgument.Value(args, "--waldir", o.WalDirectory);
        ServerArgument.Value(args, "--wal-segsize", o.WalSegmentSizeMegabytes);
        if (o.DataChecksums == true) args.Add("--data-checksums");
        if (o.DataChecksums == false && v >= PostgreSqlMajorVersion.V18) args.Add("--no-data-checksums");
        ServerArgument.Flag(args, "--allow-group-access", o.AllowGroupAccess);
        ServerArgument.Flag(args, "--discard-caches", o.DiscardCaches);
        if (o.LocaleProvider.HasValue) ServerArgument.Value(args, "--locale-provider", o.LocaleProvider.Value.ToString().ToLowerInvariant());
        ServerArgument.Value(args, "--icu-locale", o.IcuLocale);
        ServerArgument.Value(args, "--icu-rules", o.IcuRules);
        if (o.BuiltinLocale.HasValue) ServerArgument.Value(args, "--builtin-locale", ServerArgument.Builtin(o.BuiltinLocale.Value));
        foreach (PgServerSetting value in o.Settings) ServerArgument.Value(args, "--set", value.ToArgument());
        if (o.SyncMethod.HasValue) ServerArgument.Value(args, "--sync-method", ServerArgument.SyncName(o.SyncMethod.Value));
        ServerArgument.Flag(args, "--no-sync-data-files", o.NoSyncDataFiles);
        ServerArgument.Value(args, "-L", o.InputDirectory);
        return args;
    }
}
