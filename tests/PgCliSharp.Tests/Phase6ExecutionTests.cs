using System.Text;
using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class Phase6ExecutionTests
{
    [Fact]
    public async Task Psql_FiniteExecution_ForwardsAllThreeStreamsAndSemanticStatus()
    {
        byte[] stdoutBytes = Encoding.UTF8.GetBytes("result\n");
        byte[] stderrBytes = Encoding.UTF8.GetBytes("psql diagnostic\n");
        var runner = new FakeRunner("psql", "18.1", stdoutBytes, stderrBytes, 3);
        var sessionRunner = new FakeSessionRunner(0);
        var tool = new Psql(
            "/fake/psql",
            PostgreSqlMajorVersion.V18,
            runner,
            sessionRunner);
        var options = new PsqlOptions();
        options.Actions.Add(PsqlAction.File("-"));
        options.EnvironmentVariables["PGAPPNAME"] = "phase6-psql";

        using var input = new MemoryStream(Encoding.UTF8.GetBytes("select 1;\n"));
        using var output = new MemoryStream();
        using var error = new MemoryStream();

        PsqlResult result = await tool.ExecuteAsync(
            options,
            new PsqlIo(input, output, error));

        Assert.Equal(PsqlExitStatus.ScriptError, result.Status);
        Assert.Equal(stdoutBytes, output.ToArray());
        Assert.Equal(stderrBytes, error.ToArray());
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Same(input, runner.LastRequest!.StandardInput);
        Assert.Same(error, runner.LastRequest.StandardError);
        Assert.Equal("phase6-psql", runner.LastRequest.EnvironmentVariables!["PGAPPNAME"]);
    }

    [Fact]
    public async Task PgBench_RuntimeExitStatus_IsReturned()
    {
        var runner = new FakeRunner(
            "pgbench",
            "18.2",
            Array.Empty<byte>(),
            Array.Empty<byte>(),
            2);
        var tool = new PgBench(
            "/fake/pgbench",
            PostgreSqlMajorVersion.V18,
            runner);

        PgBenchResult result = await tool.ExecuteAsync(new PgBenchOptions());

        Assert.Equal(PgBenchExitStatus.RuntimeError, result.Status);
        Assert.False(runner.LastRequest!.ThrowOnNonZeroExitCode);
    }


    [Fact]
    public async Task PgBench_RuntimeExitStatusTwo_BeginsInPostgreSql12()
    {
        var pg11Runner = new FakeRunner(
            "pgbench",
            "11.22",
            Array.Empty<byte>(),
            Encoding.UTF8.GetBytes("legacy failure"),
            2);
        var pg11 = new PgBench(
            "/fake/pgbench11",
            PostgreSqlMajorVersion.V11,
            pg11Runner);

        await Assert.ThrowsAsync<PgProcessExecutionException>(
            () => pg11.ExecuteAsync(new PgBenchOptions()));

        var pg12Runner = new FakeRunner(
            "pgbench",
            "12.21",
            Array.Empty<byte>(),
            Encoding.UTF8.GetBytes("runtime failure"),
            2);
        var pg12 = new PgBench(
            "/fake/pgbench12",
            PostgreSqlMajorVersion.V12,
            pg12Runner);

        PgBenchResult result = await pg12.ExecuteAsync(new PgBenchOptions());
        Assert.Equal(PgBenchExitStatus.RuntimeError, result.Status);
    }


    [Fact]
    public async Task PgBench_ForwardsOutputAndErrorWithoutExposingStdin()
    {
        byte[] stdoutBytes = Encoding.UTF8.GetBytes("benchmark summary\n");
        byte[] stderrBytes = Encoding.UTF8.GetBytes("progress: 5.0 s\n");
        var runner = new FakeRunner(
            "pgbench",
            "18.2",
            stdoutBytes,
            stderrBytes,
            0);
        var tool = new PgBench(
            "/fake/pgbench",
            PostgreSqlMajorVersion.V18,
            runner);
        using var output = new MemoryStream();
        using var error = new MemoryStream();

        PgBenchResult result = await tool.ExecuteAsync(
            new PgBenchOptions(),
            new PgBenchIo(output, error));

        Assert.Equal(PgBenchExitStatus.Success, result.Status);
        Assert.Equal(stdoutBytes, output.ToArray());
        Assert.Equal(stderrBytes, error.ToArray());
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Null(runner.LastRequest!.StandardInput);
        Assert.Same(output, runner.LastRequest.StandardOutput);
        Assert.Same(error, runner.LastRequest.StandardError);
    }

    [Fact]
    public async Task Psql_Session_ForwardsArgumentsEnvironmentStreamsAndStatus()
    {
        var runner = new FakeRunner(
            "psql",
            "18.3",
            Array.Empty<byte>(),
            Array.Empty<byte>(),
            0);
        var sessionRunner = new FakeSessionRunner(3);
        var tool = new Psql(
            "/fake/psql",
            PostgreSqlMajorVersion.V18,
            runner,
            sessionRunner);
        var options = new PsqlOptions
        {
            Database = "appdb",
            NoPsqlRc = true,
        };
        options.EnvironmentVariables["PGAPPNAME"] = "phase6-session";

        using var output = new MemoryStream();
        using var error = new MemoryStream();
        using PsqlSession session = await tool.StartSessionAsync(
            options,
            new PsqlSessionIo(output, error),
            TimeSpan.FromSeconds(9));

        byte[] inputBytes = Encoding.UTF8.GetBytes("select 42;\n\\q\n");
#if NET8_0_OR_GREATER
        await session.StandardInput.WriteAsync(
            inputBytes.AsMemory(),
            CancellationToken.None);
#else
        await session.StandardInput.WriteAsync(
            inputBytes,
            0,
            inputBytes.Length,
            CancellationToken.None);
#endif
        session.CompleteInput();

        PsqlSessionResult result = await session.Completion;

        Assert.Equal(PsqlExitStatus.ScriptError, result.Status);
        Assert.Equal(new Version(18, 3), result.ExecutableVersion);
        Assert.NotNull(sessionRunner.LastRequest);
        Assert.Same(output, sessionRunner.LastRequest!.StandardOutput);
        Assert.Same(error, sessionRunner.LastRequest.StandardError);
        Assert.Equal(TimeSpan.FromSeconds(9), sessionRunner.LastRequest.Timeout);
        Assert.Equal("phase6-session", sessionRunner.LastRequest.EnvironmentVariables!["PGAPPNAME"]);
        Assert.Equal("appdb", ValueAfter(sessionRunner.LastRequest.Arguments, "--dbname"));
        Assert.Contains("--no-psqlrc", sessionRunner.LastRequest.Arguments);
        Assert.Equal(inputBytes, sessionRunner.Session.Input.ToArray());
        Assert.True(sessionRunner.Session.InputCompleted);
    }

    [Fact]
    public async Task Psql_SessionActions_AreRejectedBeforeVersionProbeOrStart()
    {
        var runner = new FakeRunner(
            "psql",
            "18.0",
            Array.Empty<byte>(),
            Array.Empty<byte>(),
            0);
        var sessionRunner = new FakeSessionRunner(0);
        var tool = new Psql(
            "/fake/psql",
            PostgreSqlMajorVersion.V18,
            runner,
            sessionRunner);
        var options = new PsqlOptions();
        options.Actions.Add(PsqlAction.Command("select 1"));

        await Assert.ThrowsAsync<PgInvalidOptionCombinationException>(
            () => tool.StartSessionAsync(options));

        Assert.Equal(0, runner.InvocationCount);
        Assert.Equal(0, sessionRunner.StartCount);
    }

    [Fact]
    public async Task Psql_SessionVersionMismatch_PreventsSessionStart()
    {
        var runner = new FakeRunner(
            "psql",
            "17.9",
            Array.Empty<byte>(),
            Array.Empty<byte>(),
            0);
        var sessionRunner = new FakeSessionRunner(0);
        var tool = new Psql(
            "/fake/psql",
            PostgreSqlMajorVersion.V18,
            runner,
            sessionRunner);

        await Assert.ThrowsAsync<PgExecutableVersionMismatchException>(
            () => tool.StartSessionAsync(new PsqlOptions()));

        Assert.Equal(1, runner.InvocationCount);
        Assert.Equal(0, sessionRunner.StartCount);
    }

    private static string ValueAfter(
        IReadOnlyList<string> args,
        string option)
    {
        int index = args.ToList().IndexOf(option);
        Assert.True(index >= 0 && index + 1 < args.Count);
        return args[index + 1];
    }

    private sealed class FakeRunner : IProcessRunner
    {
        private readonly string _tool;
        private readonly string _version;
        private readonly byte[] _output;
        private readonly byte[] _error;
        private readonly int _exitCode;

        internal FakeRunner(
            string tool,
            string version,
            byte[] output,
            byte[] error,
            int exitCode)
        {
            _tool = tool;
            _version = version;
            _output = output;
            _error = error;
            _exitCode = exitCode;
        }

        internal int InvocationCount { get; private set; }
        internal ProcessRunRequest? LastRequest { get; private set; }

        public async Task<ProcessRunResult> RunAsync(
            ProcessRunRequest request,
            CancellationToken cancellationToken)
        {
            InvocationCount++;

            if (request.Arguments.Count == 1 &&
                request.Arguments[0] == "--version")
            {
                byte[] version = Encoding.UTF8.GetBytes(
                    _tool + " (PostgreSQL) " + _version + "\n");
#if NET8_0_OR_GREATER
                await request.StandardOutput!.WriteAsync(
                    version.AsMemory(),
                    cancellationToken);
#else
                await request.StandardOutput!.WriteAsync(
                    version,
                    0,
                    version.Length,
                    cancellationToken);
#endif
                return new ProcessRunResult(
                    0,
                    TimeSpan.Zero,
                    string.Empty);
            }

            LastRequest = request;

            if (request.StandardOutput is not null)
            {
#if NET8_0_OR_GREATER
                await request.StandardOutput.WriteAsync(
                    _output.AsMemory(),
                    cancellationToken);
#else
                await request.StandardOutput.WriteAsync(
                    _output,
                    0,
                    _output.Length,
                    cancellationToken);
#endif
            }

            if (request.StandardError is not null)
            {
#if NET8_0_OR_GREATER
                await request.StandardError.WriteAsync(
                    _error.AsMemory(),
                    cancellationToken);
#else
                await request.StandardError.WriteAsync(
                    _error,
                    0,
                    _error.Length,
                    cancellationToken);
#endif
            }

            return new ProcessRunResult(
                _exitCode,
                TimeSpan.FromMilliseconds(25),
                request.StandardError is null
                    ? Encoding.UTF8.GetString(_error)
                    : string.Empty);
        }
    }

    private sealed class FakeSessionRunner : IProcessSessionRunner
    {
        private readonly int _exitCode;

        internal FakeSessionRunner(int exitCode)
        {
            _exitCode = exitCode;
            Session = new FakeSession(exitCode);
        }

        internal int StartCount { get; private set; }
        internal ProcessSessionStartRequest? LastRequest { get; private set; }
        internal FakeSession Session { get; private set; }

        public IProcessSession Start(
            ProcessSessionStartRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartCount++;
            LastRequest = request;
            Session = new FakeSession(_exitCode);
            return Session;
        }
    }

    private sealed class FakeSession : IProcessSession
    {
        internal FakeSession(int exitCode)
        {
            Input = new MemoryStream();
            Completion = Task.FromResult(
                new ProcessSessionResult(
                    exitCode,
                    TimeSpan.FromMilliseconds(30)));
        }

        internal MemoryStream Input { get; }
        internal bool InputCompleted { get; private set; }
        internal bool Cancelled { get; private set; }

        public Stream StandardInput => Input;
        public Task<ProcessSessionResult> Completion { get; }

        public void CompleteInput() => InputCompleted = true;
        public void Cancel() => Cancelled = true;

        public void Dispose()
        {
            InputCompleted = true;
        }
    }
}
