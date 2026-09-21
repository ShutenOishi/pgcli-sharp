using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.Psql;

internal static class PsqlOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo Csv =
        MaintenanceAvailability.Since("--csv", PostgreSqlMajorVersion.V12);

    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new[] { Csv };
}

internal static class PsqlArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PsqlOptions options)
    {
        var args = new List<string>();

        MaintenanceArgument.AddFlag(args, "--echo-all", options.EchoAll);
        if (options.OutputFormat.HasValue)
        {
            switch (options.OutputFormat.Value)
            {
                case PsqlOutputFormat.Aligned:
                    break;
                case PsqlOutputFormat.Unaligned:
                    args.Add("--no-align");
                    break;
                case PsqlOutputFormat.Html:
                    args.Add("--html");
                    break;
                case PsqlOutputFormat.Csv:
                    args.Add("--csv");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options));
            }
        }

        MaintenanceArgument.AddFlag(args, "--echo-errors", options.EchoErrors);
        MaintenanceArgument.AddValue(args, "--dbname", options.Database);
        MaintenanceArgument.AddFlag(args, "--echo-queries", options.EchoQueries);
        MaintenanceArgument.AddFlag(args, "--echo-hidden", options.EchoHidden);
        AddSeparator(args, options.FieldSeparator, "--field-separator", "--field-separator-zero");
        MaintenanceArgument.AddValue(args, "--host", options.Host);
        MaintenanceArgument.AddFlag(args, "--list", options.ListDatabases);
        MaintenanceArgument.AddValue(args, "--log-file", options.LogFile);
        MaintenanceArgument.AddFlag(args, "--no-readline", options.NoReadline);
        MaintenanceArgument.AddFlag(args, "--single-transaction", options.SingleTransaction);
        MaintenanceArgument.AddValue(args, "--output", options.OutputFile);
        MaintenanceArgument.AddValue(args, "--port", options.Port);
        MaintenanceArgument.AddRepeatable(args, "--pset", options.PrintSettings);
        MaintenanceArgument.AddFlag(args, "--quiet", options.Quiet);
        AddSeparator(args, options.RecordSeparator, "--record-separator", "--record-separator-zero");
        MaintenanceArgument.AddFlag(args, "--single-step", options.SingleStep);
        MaintenanceArgument.AddFlag(args, "--single-line", options.SingleLine);
        MaintenanceArgument.AddFlag(args, "--tuples-only", options.TuplesOnly);
        MaintenanceArgument.AddValue(args, "--table-attr", options.TableAttributes);
        MaintenanceArgument.AddValue(args, "--username", options.Username);
        MaintenanceArgument.AddPassword(args, options.PasswordPrompt);

        foreach (PsqlVariableAssignment assignment in options.Variables)
        {
            string value = assignment.HasValue
                ? assignment.Name + "=" + assignment.Value
                : assignment.Name;
            MaintenanceArgument.AddValue(args, "--set", value);
        }

        MaintenanceArgument.AddFlag(args, "--expanded", options.Expanded);
        MaintenanceArgument.AddFlag(args, "--no-psqlrc", options.NoPsqlRc);

        foreach (PsqlAction action in options.Actions)
        {
            MaintenanceArgument.AddValue(
                args,
                action.Kind == PsqlActionKind.Command ? "--command" : "--file",
                action.Value);
        }

        return args;
    }

    private static void AddSeparator(ICollection<string> args, PsqlSeparator? separator, string textOption, string zeroOption)
    {
        if (separator is null) return;
        if (separator.IsZeroByte)
            args.Add(zeroOption);
        else
            MaintenanceArgument.AddValue(args, textOption, separator.Value);
    }
}

internal static class PsqlValidator
{
    internal static void Validate(PsqlOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);

        if (options.OutputFormat == PsqlOutputFormat.Csv)
            MaintenanceAvailability.Ensure(PsqlOptionAvailabilityCatalog.Csv, version);

        if (options.SingleTransaction && options.Actions.Count == 0)
            throw new PgInvalidOptionCombinationException(version, "--single-transaction", "--command/--file");

        MaintenanceArgument.ValidateNonEmpty(options.PrintSettings, "--pset", version);

        foreach (PsqlAction action in options.Actions)
        {
            if (action is null)
                throw new PgInvalidOptionValueException(version, "--command/--file", null);
            if (action.Kind == PsqlActionKind.File && string.IsNullOrWhiteSpace(action.Value))
                throw new PgInvalidOptionValueException(version, "--file", action.Value);
        }

        foreach (PsqlVariableAssignment assignment in options.Variables)
        {
            if (assignment is null || string.IsNullOrWhiteSpace(assignment.Name))
                throw new PgInvalidOptionValueException(version, "--set", assignment?.Name);
        }
    }
}
