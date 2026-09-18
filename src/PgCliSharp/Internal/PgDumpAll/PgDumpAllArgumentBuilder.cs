using System.Globalization;

namespace PgCliSharp.Internal.PgDumpAll;

internal static class PgDumpAllArgumentBuilder
{
    internal static IReadOnlyList<string> Build(
        PgDumpAllOptions options,
        PgDumpAllOutput output)
    {
        var arguments = new List<string>();

        AddValue(arguments, "--dbname", options.ConnectionString);
        AddValue(arguments, "--host", options.Host);
        AddValue(arguments, "--database", options.InitialDatabase);
        AddValue(arguments, "--port", options.Port);
        AddValue(arguments, "--username", options.Username);

        switch (options.PasswordPrompt)
        {
            case PgPasswordPromptMode.NeverPrompt:
                arguments.Add("--no-password");
                break;
            case PgPasswordPromptMode.ForcePrompt:
                arguments.Add("--password");
                break;
        }

        AddValue(arguments, "--role", options.Role);

        switch (options.ContentMode)
        {
            case PgDumpAllContentMode.DataOnly:
                arguments.Add("--data-only");
                break;
            case PgDumpAllContentMode.SchemaOnly:
                arguments.Add("--schema-only");
                break;
            case PgDumpAllContentMode.StatisticsOnly:
                arguments.Add("--statistics-only");
                break;
        }

        if (options.Clean) arguments.Add("--clean");
        AddValue(arguments, "--encoding", options.Encoding);

        if (output.Kind == PgDumpAllOutputKind.File)
        {
            AddValue(arguments, "--file", output.Path);
        }

        switch (options.Scope)
        {
            case PgDumpAllScope.GlobalsOnly:
                arguments.Add("--globals-only");
                break;
            case PgDumpAllScope.RolesOnly:
                arguments.Add("--roles-only");
                break;
            case PgDumpAllScope.TablespacesOnly:
                arguments.Add("--tablespaces-only");
                break;
        }

        if (options.IncludeOids) arguments.Add("--oids");
        if (options.NoOwner) arguments.Add("--no-owner");
        AddValue(arguments, "--superuser", options.Superuser);

        for (int index = 0; index < options.Verbosity; index++)
        {
            arguments.Add("--verbose");
        }

        if (options.NoPrivileges) arguments.Add("--no-privileges");
        if (options.BinaryUpgrade) arguments.Add("--binary-upgrade");
        if (options.ColumnInserts) arguments.Add("--column-inserts");
        if (options.DisableDollarQuoting) arguments.Add("--disable-dollar-quoting");
        if (options.DisableTriggers) arguments.Add("--disable-triggers");

        AddRepeatable(
            arguments,
            "--exclude-database",
            options.ExcludedDatabases);

        AddValue(arguments, "--extra-float-digits", options.ExtraFloatDigits);
        if (options.IfExists) arguments.Add("--if-exists");
        if (options.Inserts) arguments.Add("--inserts");

        if (options.LockWaitTimeout.HasValue)
        {
            long milliseconds = checked(
                (long)Math.Ceiling(
                    options.LockWaitTimeout.Value.TotalMilliseconds));
            AddValue(
                arguments,
                "--lock-wait-timeout",
                milliseconds);
        }

        if (options.NoTableAccessMethod) arguments.Add("--no-table-access-method");
        if (options.NoTablespaces) arguments.Add("--no-tablespaces");
        if (options.QuoteAllIdentifiers) arguments.Add("--quote-all-identifiers");
        if (options.LoadViaPartitionRoot) arguments.Add("--load-via-partition-root");
        if (options.UseSetSessionAuthorization) arguments.Add("--use-set-session-authorization");
        if (options.NoComments) arguments.Add("--no-comments");
        if (options.NoData) arguments.Add("--no-data");
        if (options.NoPolicies) arguments.Add("--no-policies");
        if (options.NoPublications) arguments.Add("--no-publications");
        if (options.NoRolePasswords) arguments.Add("--no-role-passwords");
        if (options.NoSchema) arguments.Add("--no-schema");
        if (options.NoSecurityLabels) arguments.Add("--no-security-labels");
        if (options.NoStatistics) arguments.Add("--no-statistics");
        if (options.NoSubscriptions) arguments.Add("--no-subscriptions");
        if (options.NoSync) arguments.Add("--no-sync");
        if (options.NoToastCompression) arguments.Add("--no-toast-compression");
        if (options.NoUnloggedTableData) arguments.Add("--no-unlogged-table-data");
        if (options.OnConflictDoNothing) arguments.Add("--on-conflict-do-nothing");

        AddValue(arguments, "--rows-per-insert", options.RowsPerInsert);

        if (options.Statistics) arguments.Add("--statistics");
        if (options.SequenceData) arguments.Add("--sequence-data");

        foreach (PgDumpAllFilterSource filter in options.Filters)
        {
            AddValue(
                arguments,
                "--filter",
                filter.Kind == PgDumpAllFilterSourceKind.StandardInput
                    ? "-"
                    : filter.Path);
        }

        if (options.RestrictKey is not null)
        {
            AddValue(
                arguments,
                "--restrict-key",
                options.RestrictKey.Value);
        }

        return arguments;
    }

    private static void AddRepeatable(
        ICollection<string> arguments,
        string option,
        IEnumerable<string> values)
    {
        foreach (string value in values)
        {
            AddValue(arguments, option, value);
        }
    }

    private static void AddValue(
        ICollection<string> arguments,
        string option,
        object? value)
    {
        if (value is null)
        {
            return;
        }

        string formatted = value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value.ToString()!;

        arguments.Add(option);
        arguments.Add(formatted);
    }
}
