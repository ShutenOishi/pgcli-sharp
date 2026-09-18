using System.Globalization;

namespace PgCliSharp.Internal.PgRestore;

internal static class PgRestoreArgumentBuilder
{
    internal static IReadOnlyList<string> Build(
        PgRestoreOptions options,
        PgRestoreInput input,
        PgRestoreOutput output)
    {
        var arguments = new List<string>();

        if (output.Kind == PgRestoreOutputKind.Database)
        {
            AddValue(arguments, "--dbname", output.Database);
        }
        else
        {
            AddValue(
                arguments,
                "--file",
                output.Kind == PgRestoreOutputKind.StandardOutput ? "-" : output.Path);
        }

        AddValue(arguments, "--host", options.Host);
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

        if (options.Clean) arguments.Add("--clean");
        if (options.Create) arguments.Add("--create");

        switch (options.ContentMode)
        {
            case PgRestoreContentMode.DataOnly:
                arguments.Add("--data-only");
                break;
            case PgRestoreContentMode.SchemaOnly:
                arguments.Add("--schema-only");
                break;
            case PgRestoreContentMode.StatisticsOnly:
                arguments.Add("--statistics-only");
                break;
        }

        if (options.ExitOnError) arguments.Add("--exit-on-error");

        if (options.ArchiveFormat.HasValue)
        {
            AddValue(
                arguments,
                "--format",
                FormatArchive(options.ArchiveFormat.Value));
        }

        AddRepeatable(arguments, "--function", options.Functions);
        AddRepeatable(arguments, "--index", options.Indexes);
        AddValue(arguments, "--jobs", options.Jobs);

        if (options.Mode == PgRestoreMode.List) arguments.Add("--list");
        if (options.NoPrivileges) arguments.Add("--no-privileges");
        if (options.NoOwner) arguments.Add("--no-owner");
        if (options.NoReconnect) arguments.Add("--no-reconnect");

        AddRepeatable(arguments, "--schema", options.Schemas);
        AddRepeatable(arguments, "--exclude-schema", options.ExcludedSchemas);
        AddValue(arguments, "--superuser", options.Superuser);
        AddRepeatable(arguments, "--table", options.Tables);
        AddRepeatable(arguments, "--trigger", options.Triggers);
        AddValue(arguments, "--use-list", options.UseListFile);

        for (int index = 0; index < options.Verbosity; index++)
        {
            arguments.Add("--verbose");
        }

        if (options.TransactionMode is not null)
        {
            switch (options.TransactionMode.Kind)
            {
                case PgRestoreTransactionModeKind.SingleTransaction:
                    arguments.Add("--single-transaction");
                    break;
                case PgRestoreTransactionModeKind.Batch:
                    AddValue(
                        arguments,
                        "--transaction-size",
                        options.TransactionMode.ObjectCount);
                    break;
            }
        }

        if (options.DisableTriggers) arguments.Add("--disable-triggers");
        if (options.EnableRowSecurity) arguments.Add("--enable-row-security");
        if (options.IfExists) arguments.Add("--if-exists");
        if (options.NoDataForFailedTables) arguments.Add("--no-data-for-failed-tables");
        if (options.NoTableAccessMethod) arguments.Add("--no-table-access-method");
        if (options.NoTablespaces) arguments.Add("--no-tablespaces");

        foreach (PgRestoreSection section in options.Sections)
        {
            AddValue(arguments, "--section", FormatSection(section));
        }

        if (options.StrictNames) arguments.Add("--strict-names");
        if (options.UseSetSessionAuthorization) arguments.Add("--use-set-session-authorization");
        if (options.NoComments) arguments.Add("--no-comments");
        if (options.NoData) arguments.Add("--no-data");
        if (options.NoPolicies) arguments.Add("--no-policies");
        if (options.NoPublications) arguments.Add("--no-publications");
        if (options.NoSchema) arguments.Add("--no-schema");
        if (options.NoSecurityLabels) arguments.Add("--no-security-labels");
        if (options.NoStatistics) arguments.Add("--no-statistics");
        if (options.NoSubscriptions) arguments.Add("--no-subscriptions");
        if (options.Statistics) arguments.Add("--statistics");

        foreach (PgRestoreFilterSource filter in options.Filters)
        {
            AddValue(
                arguments,
                "--filter",
                filter.Kind == PgRestoreFilterSourceKind.StandardInput
                    ? "-"
                    : filter.Path);
        }

        if (options.RestrictKey is not null)
        {
            AddValue(arguments, "--restrict-key", options.RestrictKey.Value);
        }

        if (input.Kind != PgRestoreInputKind.StandardInput)
        {
            arguments.Add(input.Path!);
        }

        return arguments;
    }

    private static string FormatArchive(PgRestoreArchiveFormat format)
    {
        return format switch
        {
            PgRestoreArchiveFormat.Custom => "custom",
            PgRestoreArchiveFormat.Directory => "directory",
            PgRestoreArchiveFormat.Tar => "tar",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
    }

    private static string FormatSection(PgRestoreSection section)
    {
        return section switch
        {
            PgRestoreSection.PreData => "pre-data",
            PgRestoreSection.Data => "data",
            PgRestoreSection.PostData => "post-data",
            _ => throw new ArgumentOutOfRangeException(nameof(section), section, null),
        };
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
