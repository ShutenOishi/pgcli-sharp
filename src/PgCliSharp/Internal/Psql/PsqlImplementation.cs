using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Versioning;

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

    private static void AddSeparator(List<string> args, PsqlSeparator? separator, string textOption, string zeroOption)
    {
        if (separator is null) return;
        if (separator.IsZeroByte)
            args.Add(zeroOption);
        else
            MaintenanceArgument.AddValue(args, textOption, separator.Value);
    }
}

internal sealed class PsqlSessionStartInfo
{
    internal PsqlSessionStartInfo(
        IProcessSession session,
        PostgreSqlExecutableVersion executableVersion)
    {
        Session = session;
        ExecutableVersion = executableVersion;
    }

    internal IProcessSession Session { get; }
    internal PostgreSqlExecutableVersion ExecutableVersion { get; }
}

internal sealed class PsqlSessionExecutor
{
    private readonly IProcessSessionRunner _sessionRunner;
    private readonly PostgreSqlExecutableVersionProvider _versionProvider;

    internal PsqlSessionExecutor(
        string executablePath,
        PostgreSqlMajorVersion version,
        IProcessRunner processRunner,
        IProcessSessionRunner sessionRunner)
    {
        ExecutablePath = executablePath;
        Version = version;
        _versionProvider = new PostgreSqlExecutableVersionProvider(processRunner);
        _sessionRunner = sessionRunner;
    }

    internal string ExecutablePath { get; }
    internal PostgreSqlMajorVersion Version { get; }

    internal async Task<PsqlSessionStartInfo> StartAsync(
        IReadOnlyList<string> arguments,
        PsqlSessionIo? io,
        IDictionary<string, string> environmentVariables,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        PostgreSqlExecutableVersion executableVersion =
            await _versionProvider.ValidateVersionAsync(
                    ExecutablePath,
                    Version,
                    cancellationToken)
                .ConfigureAwait(false);

        var request = new ProcessSessionStartRequest(
            ExecutablePath,
            arguments,
            io?.StandardOutput ?? Stream.Null,
            io?.StandardError ?? Stream.Null,
            timeout,
            MaintenanceArgument.Environment(environmentVariables));

        IProcessSession session = _sessionRunner.Start(request, cancellationToken);
        return new PsqlSessionStartInfo(session, executableVersion);
    }
}

internal static class PsqlValidator
{
    internal static void ValidateForSession(
        PsqlOptions options,
        PostgreSqlMajorVersion version)
    {
        Validate(options, version);

        if (options.Actions.Count != 0)
            throw new PgInvalidOptionCombinationException(
                version,
                "redirected-session",
                "--command/--file");

        if (options.ListDatabases)
            throw new PgInvalidOptionCombinationException(
                version,
                "redirected-session",
                "--list");
    }

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
