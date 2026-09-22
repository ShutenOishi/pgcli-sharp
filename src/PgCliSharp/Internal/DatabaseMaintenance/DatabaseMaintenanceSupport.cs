using System.Collections.ObjectModel;
using System.Globalization;
using PgCliSharp.Internal.Execution;
using PgCliSharp.Internal.Localization;
using PgCliSharp.Internal.Versioning;

namespace PgCliSharp.Internal.DatabaseMaintenance;

internal sealed class MaintenanceOptionAvailabilityInfo
{
    internal MaintenanceOptionAvailabilityInfo(string optionName, PostgreSqlMajorVersion since, PostgreSqlMajorVersion until)
    {
        OptionName = optionName;
        Since = since;
        Until = until;
    }

    internal string OptionName { get; }
    internal PostgreSqlMajorVersion Since { get; }
    internal PostgreSqlMajorVersion Until { get; }
}

internal static class MaintenanceAvailability
{
    internal static MaintenanceOptionAvailabilityInfo Since(string optionName, PostgreSqlMajorVersion since) =>
        new MaintenanceOptionAvailabilityInfo(optionName, since, PostgreSqlMajorVersion.V18);

    internal static MaintenanceOptionAvailabilityInfo Range(string optionName, PostgreSqlMajorVersion since, PostgreSqlMajorVersion until) =>
        new MaintenanceOptionAvailabilityInfo(optionName, since, until);

    internal static void Ensure(MaintenanceOptionAvailabilityInfo info, PostgreSqlMajorVersion version)
    {
        if ((int)version < (int)info.Since || (int)version > (int)info.Until)
            throw new PgUnsupportedOptionException(version, info.OptionName, info.Since, info.Until);
    }

    internal static void EnsureTool(string toolName, PostgreSqlMajorVersion version, PostgreSqlMajorVersion since)
    {
        if ((int)version < (int)since)
            throw new PgUnsupportedToolException(toolName, version, since);
    }
}

internal static class MaintenanceArgument
{
    internal static void AddValue(ICollection<string> args, string name, object? value)
    {
        if (value is null) return;
        string text = value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value.ToString()!;
        args.Add(name);
        args.Add(text);
    }

    internal static void AddFlag(ICollection<string> args, string name, bool enabled)
    {
        if (enabled) args.Add(name);
    }

    internal static void AddNullableBoolean(ICollection<string> args, bool? value, string positive, string negative)
    {
        if (!value.HasValue) return;
        args.Add(value.Value ? positive : negative);
    }

    internal static void AddPassword(ICollection<string> args, PgPasswordPromptMode mode)
    {
        switch (mode)
        {
            case PgPasswordPromptMode.Default:
                break;
            case PgPasswordPromptMode.NeverPrompt:
                args.Add("--no-password");
                break;
            case PgPasswordPromptMode.ForcePrompt:
                args.Add("--password");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    internal static void AddConnection(
        ICollection<string> args,
        string? host,
        int? port,
        string? username,
        PgPasswordPromptMode passwordPrompt)
    {
        AddValue(args, "--host", host);
        AddValue(args, "--port", port);
        AddValue(args, "--username", username);
        AddPassword(args, passwordPrompt);
    }

    internal static void AddRepeatable(ICollection<string> args, string name, IEnumerable<string> values)
    {
        foreach (string value in values) AddValue(args, name, value);
    }

    internal static IReadOnlyDictionary<string, string>? Environment(IDictionary<string, string> source) =>
        source.Count == 0 ? null : new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(source, StringComparer.Ordinal));

    internal static void ValidatePort(int? port, PostgreSqlMajorVersion version)
    {
        if (port.HasValue && (port.Value < 1 || port.Value > 65535))
            throw new PgInvalidOptionValueException(version, "--port", port.Value);
    }

    internal static void ValidatePositive(int? value, string option, PostgreSqlMajorVersion version, bool allowZero = false)
    {
        if (value.HasValue && (allowZero ? value.Value < 0 : value.Value <= 0))
            throw new PgInvalidOptionValueException(version, option, value.Value);
    }

    internal static void ValidateNonEmpty(IEnumerable<string> values, string option, PostgreSqlMajorVersion version)
    {
        foreach (string value in values)
            if (string.IsNullOrWhiteSpace(value))
                throw new PgInvalidOptionValueException(version, option, value);
    }
}

internal sealed class MaintenanceExecutionInfo
{
    internal MaintenanceExecutionInfo(ProcessRunResult process, PostgreSqlExecutableVersion executableVersion)
    {
        Process = process;
        ExecutableVersion = executableVersion;
    }

    internal ProcessRunResult Process { get; }
    internal PostgreSqlExecutableVersion ExecutableVersion { get; }
}

internal sealed class MaintenanceExecutor
{
    private readonly IProcessRunner _runner;
    private readonly PostgreSqlExecutableVersionProvider _versionProvider;

    internal MaintenanceExecutor(
        string executablePath,
        PostgreSqlMajorVersion version,
        IProcessRunner runner,
        string? toolName = null,
        PostgreSqlMajorVersion supportedSince = PostgreSqlMajorVersion.V10)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.ExecutablePathRequired), nameof(executablePath));

        _ = PostgreSqlVersionCatalog.Get(version);
        if (toolName is not null)
            MaintenanceAvailability.EnsureTool(toolName, version, supportedSince);

        ExecutablePath = executablePath;
        Version = version;
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _versionProvider = new PostgreSqlExecutableVersionProvider(_runner);
    }

    internal string ExecutablePath { get; }
    internal PostgreSqlMajorVersion Version { get; }

    internal async Task<MaintenanceExecutionInfo> RunAsync(
        IReadOnlyList<string> arguments,
        PgMaintenanceIo? io,
        IDictionary<string, string> environmentVariables,
        TimeSpan? timeout,
        CancellationToken cancellationToken,
        bool throwOnNonZeroExitCode = true)
    {
        if (timeout.HasValue && timeout.Value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        PostgreSqlExecutableVersion executableVersion =
            await _versionProvider.ValidateVersionAsync(ExecutablePath, Version, cancellationToken).ConfigureAwait(false);

        var request = new ProcessRunRequest(
            ExecutablePath,
            arguments,
            io?.StandardOutput,
            timeout,
            throwOnNonZeroExitCode,
            MaintenanceArgument.Environment(environmentVariables),
            io?.StandardInput,
            io?.StandardError);

        ProcessRunResult process = await _runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
        return new MaintenanceExecutionInfo(process, executableVersion);
    }

    internal static PgMaintenanceResult ToResult(MaintenanceExecutionInfo info) =>
        new PgMaintenanceResult(
            info.Process.ExitCode,
            info.Process.Duration,
            info.ExecutableVersion.NumericVersion,
            info.ExecutableVersion.RawVersion,
            info.Process.StandardError);
}
