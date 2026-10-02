using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using PgCliSharp.Internal.Execution;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Tests;

public sealed class ConfigurationAndCommandTests
{
    private static readonly string[] QuotingTokens = { "", "a'b", "x\"y", "$()`; & | >", "日本語\nnext", "‘curly’", "trailing\\", "two\\slashes\\\\" };
    private static readonly string[] PosixHelperArguments = { "-c", "printf '%s\\0' \"$@\"; printf '%s\\0' \"$PGCLI_EXPORT_TEST\"", "command-helper" };

    public static IEnumerable<object[]> Wrappers => typeof(PgDump).Assembly.GetExportedTypes()
        .Where(type => type.GetProperty("ExecutablePath") is not null && type.GetMethod("ExecuteAsync") is not null)
        .OrderBy(type => type.FullName).Select(type => new object[] { type });

    [Theory]
    [MemberData(nameof(Wrappers))]
    public async Task AllWrappers_CommandMatchesExecution_DespiteMutationDuringVersionProbe(Type wrapper)
    {
        var runner = new SuspendedProbeRunner();
        object tool = Activator.CreateInstance(wrapper, BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { "/not-installed/" + wrapper.Name, PostgreSqlMajorVersion.V18, runner }, CultureInfo.InvariantCulture)!;
        Type optionsType = wrapper.Assembly.GetType(wrapper.FullName + "Options")!;
        object options = ValidOptions(optionsType);
        var environment = (IDictionary<string, string>)optionsType.GetProperty("EnvironmentVariables")!.GetValue(options)!;
        environment["PGAPPNAME"] = "original application";
        MethodInfo create = wrapper.GetMethod("CreateCommand")!;
        var command = (PgCommand)create.Invoke(tool, Parameters(create, options))!;
        Assert.Equal(0, runner.InvocationCount);
        Assert.True(((PgValidationResult)wrapper.GetMethod("Validate")!.Invoke(tool, Parameters(wrapper.GetMethod("Validate")!, options))!).IsValid);
        Assert.Equal(0, runner.InvocationCount);

        // The lambda counterpart has the same token contract and creates a fresh options object once.
        Type extensions = wrapper.Namespace == "PgCliSharp" ? typeof(PgCliConfigurationExtensions) : typeof(PgServerConfigurationExtensions);
        MethodInfo extension = Assert.Single(extensions.GetMethods(), method => method.Name == "CreateCommand" && method.GetParameters()[0].ParameterType == wrapper);
        int calls = 0;
        object? configured = null;
        Delegate configure = MakeConfigure(optionsType, value => { calls++; configured = value; CopyFixture(options, value); });
        object?[] extensionArgs = new object?[] { tool, configure }.Concat(Parameters(create, options).Skip(1)).ToArray();
        PgCommand lambdaCommand = (PgCommand)extension.Invoke(null, extensionArgs)!;
        Assert.Equal(1, calls);
        Assert.NotSame(options, configured);
        Assert.Equal(command.Arguments, lambdaCommand.Arguments);
        Assert.Equal(command.EnvironmentVariables, lambdaCommand.EnvironmentVariables);

        MethodInfo execute = wrapper.GetMethod("ExecuteAsync")!;
        Task execution = (Task)execute.Invoke(tool, Parameters(execute, options))!;
        await runner.ProbeStarted.Task;
        foreach (PropertyInfo property in optionsType.GetProperties())
        {
            if (property.CanWrite && property.PropertyType == typeof(string)) property.SetValue(options, "changed after invocation");
            if (property.GetValue(options) is System.Collections.IList list) list.Clear();
        }
        environment["PGAPPNAME"] = "changed environment";
        runner.ReleaseProbe.SetResult(true);
        await execution;
        Assert.NotNull(runner.Execution);
        Assert.Equal(command.Arguments, runner.Execution!.Arguments);
        Assert.Equal("original application", runner.Execution.EnvironmentVariables!["PGAPPNAME"]);
        Assert.Equal("original application", command.EnvironmentVariables["PGAPPNAME"]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)command.Arguments).Add("mutation"));
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, string>)command.EnvironmentVariables).Clear());
        Assert.IsAssignableFrom<IPgExecutionResult>(execution.GetType().GetProperty("Result")!.GetValue(execution));
    }

    [Fact]
    public async Task LambdaExecution_UsesFreshOptionsOnce_PreservesNullInstanceResolution()
    {
        var runner = new SuspendedProbeRunner();
        var tool = new PgDump("/fake/pg_dump", PostgreSqlMajorVersion.V18, runner);
        PgDumpOptions? captured = null;
        int count = 0;
        Task<PgDumpResult> operation = tool.ExecuteAsync(configureOptions: options =>
        {
            captured = options;
            count++;
            options.Database = "original";
            options.Schemas.Add("public");
        }, output: PgDumpOutput.ToFile("archive.dump"));
        await runner.ProbeStarted.Task;
        captured!.Database = "changed";
        captured.Schemas.Add("other");
        runner.ReleaseProbe.SetResult(true);
        await operation;
        Assert.Equal(1, count);
        Assert.Contains("original", runner.Execution!.Arguments);
        Assert.DoesNotContain("changed", runner.Execution.Arguments);
        Assert.DoesNotContain("other", runner.Execution.Arguments);
        await Assert.ThrowsAsync<ArgumentNullException>(() => tool.ExecuteAsync(null!, PgDumpOutput.ToFile("unused")));
        Assert.Throws<ArgumentNullException>(() => tool.CreateCommand(configureOptions: null!, output: PgDumpOutput.ToFile("unused")));
        Assert.Throws<InvalidOperationException>(() => tool.CreateCommand(options => throw new InvalidOperationException(), PgDumpOutput.ToFile("unused")));
        Assert.Empty(tool.CreateCommand(options => { }, PgDumpOutput.ToFile("unused")).EnvironmentVariables);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("ja-JP")]
    public void OfflineValidation_ReturnsLocalizedFailureAndStableBindings(string culture)
    {
        CultureInfo saved = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var tool = new PgDump("/not-installed", PostgreSqlMajorVersion.V17);
            PgValidationResult invalid = tool.Validate(options => options.Port = 0, PgDumpOutput.ToFile("uncreated.dump"));
            Assert.False(invalid.IsValid);
            PgValidationError error = Assert.Single(invalid.Errors);
            Assert.Equal(PgValidationErrorCode.InvalidValue, error.Code);
            Assert.Equal("--port", Assert.Single(error.OptionNames));
            Assert.Equal("Port", Assert.Single(error.PropertyNames));
            Assert.Equal(new PgInvalidOptionValueException(PostgreSqlMajorVersion.V17, "--port", 0).Message, error.Message);
            PgValidationResult combination = tool.Validate(options => { options.DataOnly = true; options.SchemaOnly = true; }, PgDumpOutput.ToFile("unused"));
            Assert.Equal(PgValidationErrorCode.InvalidCombination, Assert.Single(combination.Errors).Code);
            Assert.Contains("DataOnly", combination.Errors[0].PropertyNames);
            Assert.Contains("SchemaOnly", combination.Errors[0].PropertyNames);
        }
        finally { CultureInfo.CurrentUICulture = saved; }
    }

    [Fact]
    public void OfflinePatchValidation_DistinguishesUnknownAssertedAndActualVersions()
    {
        var tool = new PgDump("/not-installed", PostgreSqlMajorVersion.V17);
        var options = new PgDumpOptions { RestrictKey = new PgDumpRestrictKey("Abc123") };
        PgDumpOutput output = PgDumpOutput.ToFile("unused");
        Assert.True(tool.Validate(options, output).RequiresExecutableVersionCheck);
        Assert.True(tool.CreateCommand(options, output).RequiresExecutableVersionCheck);
        Assert.True(tool.Validate(options, output).IsValid);
        PgValidationResult old = tool.Validate(options, output, new Version(17, 5));
        Assert.False(old.IsValid);
        Assert.Equal(PgValidationErrorCode.UnsupportedOption, Assert.Single(old.Errors).Code);
        Assert.True(tool.Validate(options, output, new Version(17, 6)).IsValid);
        Assert.False(tool.CreateCommand(options, output, new Version(17, 6)).RequiresExecutableVersionCheck);
        Assert.Throws<PgExecutableVersionMismatchException>(() => tool.CreateCommand(options, output, new Version(18, 1)));
    }

    [Theory]
    [InlineData(10, "--blobs")]
    [InlineData(15, "--blobs")]
    [InlineData(16, "--large-objects")]
    [InlineData(18, "--large-objects")]
    public void CommandGeneration_PreservesVersionSpecificSpellings(int major, string expected)
    {
        var tool = new PgDump("/not-installed", (PostgreSqlMajorVersion)major);
        PgCommand command = tool.CreateCommand(options => options.LargeObjects = PgDumpLargeObjectMode.Include, PgDumpOutput.ToFile("unused"));
        Assert.Contains(expected, command.Arguments);
    }

    [Fact]
    public void Rendering_RedactsByDefault_AndQuotesShellMetacharacters()
    {
        var command = new PgCommand("/a path/tool", QuotingTokens,
            new Dictionary<string, string> { ["PGPASSWORD"] = "secret'password" });
        Assert.DoesNotContain("secret", command.ToString());
        Assert.DoesNotContain("日本語", command.ToString());
        string posix = command.ToCommandLine(PgCommandLineStyle.PosixShell, true);
        Assert.Contains("'a'\"'\"'b'", posix);
        Assert.Contains("'$()`; & | >'", posix);
        Assert.Contains("'PGPASSWORD=secret'\"'\"'password'", posix);
        string powershell = command.ToCommandLine(PgCommandLineStyle.PowerShell, true);
        Assert.Contains("'a''b'", powershell);
        Assert.Contains("finally", powershell);
        Assert.Contains("& '/a path/tool'", powershell);
        Assert.Throws<ArgumentOutOfRangeException>(() => command.ToCommandLine((PgCommandLineStyle)99));
    }

    [Fact]
    public async Task ExportedCommand_RoundTripsTokensAndEnvironment_ThroughPlatformShell()
    {
        const string Variable = "PGCLI_EXPORT_TEST";
        const string Value = "password '‘’ $(); & 日本語\nnext";
        string scriptPath = Path.Combine(Path.GetTempPath(), "pgcli command ‘' " + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + ".ps1");
        bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        string? originalEnvironment = Environment.GetEnvironmentVariable(Variable);
        try
        {
            string helper;
            IEnumerable<string> arguments;
            if (windows)
            {
                File.WriteAllText(scriptPath, "foreach ($item in $args) { $bytes = [Text.Encoding]::UTF8.GetBytes([string]$item + [char]0); [Console]::OpenStandardOutput().Write($bytes, 0, $bytes.Length) }; $bytes = [Text.Encoding]::UTF8.GetBytes($env:PGCLI_EXPORT_TEST + [char]0); [Console]::OpenStandardOutput().Write($bytes, 0, $bytes.Length)", new UTF8Encoding(false));
                helper = "pwsh";
                arguments = new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-File", scriptPath }.Concat(QuotingTokens);
            }
            else
            {
                helper = "/bin/sh";
                arguments = PosixHelperArguments.Concat(QuotingTokens);
            }
            var command = new PgCommand(helper, arguments, new Dictionary<string, string> { [Variable] = Value });
            string exported = command.ToCommandLine(windows ? PgCommandLineStyle.PowerShell : PgCommandLineStyle.PosixShell, true);
            string[] shellArguments = windows
                ? new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-Command", "$PSNativeCommandArgumentPassing = 'Standard'; [Environment]::SetEnvironmentVariable('PGCLI_EXPORT_TEST', 'before', 'Process'); " + exported + "; $bytes = [Text.Encoding]::UTF8.GetBytes($env:PGCLI_EXPORT_TEST + [char]0); [Console]::OpenStandardOutput().Write($bytes, 0, $bytes.Length)" }
                : new[] { "-c", exported };
            using var output = new MemoryStream();
            var request = new ProcessRunRequest(windows ? "pwsh" : "/bin/sh", shellArguments, output, TimeSpan.FromSeconds(30));
            await new ProcessRunner().RunAsync(request, CancellationToken.None);
            IEnumerable<string> expected = QuotingTokens.Append(Value);
            if (windows) expected = expected.Append("before");
            Assert.Equal(Encoding.UTF8.GetBytes(string.Join("\0", expected) + "\0"), output.ToArray());
            Assert.Equal(originalEnvironment, Environment.GetEnvironmentVariable(Variable));
        }
        finally { if (File.Exists(scriptPath)) File.Delete(scriptPath); }
    }

    private static object ValidOptions(Type type)
    {
        object options = Activator.CreateInstance(type)!;
        void Set(string property, object value) => type.GetProperty(property)!.SetValue(options, value);
        switch (type.Name)
        {
            case nameof(DropDbOptions): Set("DatabaseName", "database to drop"); break;
            case nameof(DropUserOptions): Set("RoleName", "role to drop"); break;
            case nameof(PgReceiveWalOptions): Set("Directory", "wal directory"); break;
            case nameof(PgRecvLogicalOptions): Set("Action", PgRecvLogicalAction.CreateSlot); Set("Slot", "slot1"); Set("Database", "db"); break;
            case nameof(PgCombineBackupOptions): Set("OutputDirectory", "combined backup"); ((IList<string>)type.GetProperty("InputDirectories")!.GetValue(options)!).Add("base backup"); break;
            case nameof(InitDbOptions): case nameof(PgChecksumsOptions): case nameof(PgResetWalOptions): Set("DataDirectory", "cluster directory"); break;
            case nameof(PgCtlOptions): Set("Command", PgCtlCommand.Status); Set("DataDirectory", "cluster directory"); break;
            case nameof(PgUpgradeOptions): Set("OldDataDirectory", "old data"); Set("NewDataDirectory", "new data"); Set("OldBinaryDirectory", "old bin"); Set("NewBinaryDirectory", "new bin"); break;
            case nameof(PgRewindOptions): Set("TargetDataDirectory", "target data"); Set("SourceDataDirectory", "source data"); break;
        }
        return options;
    }

    private static object?[] Parameters(MethodInfo method, object options) => method.GetParameters().Select((parameter, index) => index == 0 ? options :
        parameter.ParameterType == typeof(PgDumpOutput) ? PgDumpOutput.ToFile("archive path.dump") :
        parameter.ParameterType == typeof(PgDumpAllOutput) ? PgDumpAllOutput.ToFile("script path.sql") :
        parameter.ParameterType == typeof(PgRestoreInput) ? PgRestoreInput.FromFile("archive path.dump") :
        parameter.ParameterType == typeof(PgRestoreOutput) ? PgRestoreOutput.ToFile("restore path.sql") :
        parameter.ParameterType == typeof(PgBaseBackupDestination) ? PgBaseBackupDestination.ToDirectory("backup directory") :
        parameter.ParameterType == typeof(PgVerifyBackupInput) ? new PgVerifyBackupInput("backup directory") :
        parameter.ParameterType == typeof(CancellationToken) ? CancellationToken.None : parameter.DefaultValue).ToArray();

    private static Delegate MakeConfigure(Type options, Action<object> callback) => (Delegate)typeof(ConfigurationAndCommandTests)
        .GetMethod(nameof(Configure), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(options).Invoke(null, new object[] { callback })!;
    private static Action<T> Configure<T>(Action<object> callback) => value => callback(value!);
    private static void CopyFixture(object source, object target)
    {
        foreach (PropertyInfo property in source.GetType().GetProperties())
        {
            if (property.CanWrite) property.SetValue(target, property.GetValue(source));
            else if (property.GetValue(source) is System.Collections.IDictionary dictionary)
            {
                var destination = (System.Collections.IDictionary)property.GetValue(target)!;
                foreach (object key in dictionary.Keys) destination.Add(key, dictionary[key]);
            }
            else if (property.GetValue(source) is System.Collections.IList list)
            {
                var destination = (System.Collections.IList)property.GetValue(target)!;
                foreach (object item in list) destination.Add(item);
            }
        }
    }

    private sealed class SuspendedProbeRunner : IProcessRunner
    {
        internal TaskCompletionSource<bool> ProbeStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<bool> ReleaseProbe { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int InvocationCount { get; private set; }
        internal ProcessRunRequest? Execution { get; private set; }
        public async Task<ProcessRunResult> RunAsync(ProcessRunRequest request, CancellationToken cancellationToken)
        {
            InvocationCount++;
            if (request.Arguments.Contains("--version"))
            {
                ProbeStarted.TrySetResult(true);
                await ReleaseProbe.Task;
                byte[] version = Encoding.UTF8.GetBytes("tool (PostgreSQL) 18.6\n");
#if NET8_0_OR_GREATER
                await request.StandardOutput!.WriteAsync(version.AsMemory(), cancellationToken);
#else
                await request.StandardOutput!.WriteAsync(version, 0, version.Length, cancellationToken);
#endif
            }
            else Execution = request;
            return new ProcessRunResult(0, TimeSpan.FromMilliseconds(5), string.Empty);
        }
    }
}
