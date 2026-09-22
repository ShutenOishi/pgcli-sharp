using System.Runtime.InteropServices;
using System.Text;

namespace PgCliSharp.Tests;

public sealed class RealPostgreSqlIntegrationTests
{
    [Fact]
    public async Task RepresentativeRealPostgreSql_BackupRestorePsqlSessionAndPgBench()
    {
        string? binaryDirectory = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_BIN");
        string? majorText = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_MAJOR");

        if (string.IsNullOrWhiteSpace(binaryDirectory) ||
            !int.TryParse(majorText, out int major))
        {
            return;
        }

        PostgreSqlMajorVersion version = (PostgreSqlMajorVersion)major;
        string host = RequiredEnvironment("PGCLI_REAL_PG_HOST");
        string user = RequiredEnvironment("PGCLI_REAL_PG_USER");
        string sourceDatabase = RequiredEnvironment("PGCLI_REAL_PG_SOURCE_DB");
        string targetDatabase = RequiredEnvironment("PGCLI_REAL_PG_TARGET_DB");
        string benchDatabase = RequiredEnvironment("PGCLI_REAL_PG_BENCH_DB");

        string psqlPath = Path.Combine(binaryDirectory, ExecutableName("psql"));
        string dumpPath = Path.Combine(binaryDirectory, ExecutableName("pg_dump"));
        string restorePath = Path.Combine(binaryDirectory, ExecutableName("pg_restore"));
        string benchPath = Path.Combine(binaryDirectory, ExecutableName("pgbench"));
        string archivePath = Path.Combine(
            Path.GetTempPath(),
            "pgclisharp-e2e-" + Guid.NewGuid().ToString("N") + ".dump");

        try
        {
            var psql = new Psql(psqlPath, version);
            var seedOptions = ConnectionOptions(sourceDatabase, host, user);
            seedOptions.Actions.Add(
                PsqlAction.Command(
                    "CREATE TABLE IF NOT EXISTS pgclisharp_e2e(value integer NOT NULL);" +
                    "TRUNCATE pgclisharp_e2e;" +
                    "INSERT INTO pgclisharp_e2e(value) VALUES (1),(2);"));

            PsqlResult seed = await psql.ExecuteAsync(
                seedOptions,
                timeout: TimeSpan.FromSeconds(30));
            Assert.Equal(PsqlExitStatus.Success, seed.Status);

            var pgDump = new PgDump(dumpPath, version);
            var dumpOptions = new PgDumpOptions
            {
                Database = sourceDatabase,
                Host = host,
                Username = user,
                Format = PgDumpFormat.Custom,
            };
            PgDumpResult dump = await pgDump.ExecuteAsync(
                dumpOptions,
                PgDumpOutput.ToFile(archivePath),
                TimeSpan.FromSeconds(60));
            Assert.Equal(0, dump.ExitCode);

            var pgRestore = new PgRestore(restorePath, version);
            var restoreOptions = new PgRestoreOptions
            {
                Host = host,
                Username = user,
                NoOwner = true,
            };
            PgRestoreResult restore = await pgRestore.ExecuteAsync(
                restoreOptions,
                PgRestoreInput.FromFile(archivePath),
                PgRestoreOutput.ToDatabase(targetDatabase),
                TimeSpan.FromSeconds(60));
            Assert.Equal(0, restore.ExitCode);

            using var verifyOutput = new MemoryStream();
            var verifyOptions = ConnectionOptions(targetDatabase, host, user);
            verifyOptions.OutputFormat = PsqlOutputFormat.Unaligned;
            verifyOptions.TuplesOnly = true;
            verifyOptions.Actions.Add(
                PsqlAction.Command("SELECT count(*) FROM pgclisharp_e2e;"));

            PsqlResult verify = await psql.ExecuteAsync(
                verifyOptions,
                new PsqlIo(standardOutput: verifyOutput),
                TimeSpan.FromSeconds(30));
            Assert.Equal(PsqlExitStatus.Success, verify.Status);
            Assert.Contains("2", Encoding.UTF8.GetString(verifyOutput.ToArray()));

            using var sessionOutput = new MemoryStream();
            using var sessionError = new MemoryStream();
            var sessionOptions = ConnectionOptions(targetDatabase, host, user);
            sessionOptions.OutputFormat = PsqlOutputFormat.Unaligned;
            sessionOptions.TuplesOnly = true;
            sessionOptions.NoReadline = true;

            using PsqlSession session = await psql.StartSessionAsync(
                sessionOptions,
                new PsqlSessionIo(sessionOutput, sessionError),
                TimeSpan.FromSeconds(30));

            byte[] command = Encoding.UTF8.GetBytes(
                "SELECT 'session-ok';\n\\q\n");
#if NET8_0_OR_GREATER
            await session.StandardInput.WriteAsync(
                command.AsMemory(),
                CancellationToken.None);
#else
            await session.StandardInput.WriteAsync(
                command,
                0,
                command.Length,
                CancellationToken.None);
#endif
            session.CompleteInput();

            PsqlSessionResult sessionResult = await session.Completion;
            Assert.Equal(PsqlExitStatus.Success, sessionResult.Status);
            Assert.Contains(
                "session-ok",
                Encoding.UTF8.GetString(sessionOutput.ToArray()));

            var pgBench = new PgBench(benchPath, version);
            var initialize = new PgBenchOptions
            {
                Database = benchDatabase,
                Host = host,
                Username = user,
                Initialize = true,
                Scale = 1,
                Quiet = true,
            };
            PgBenchResult initializeResult = await pgBench.ExecuteAsync(
                initialize,
                timeout: TimeSpan.FromSeconds(90));
            Assert.Equal(PgBenchExitStatus.Success, initializeResult.Status);

            var benchmark = new PgBenchOptions
            {
                Database = benchDatabase,
                Host = host,
                Username = user,
                Clients = 1,
                TransactionsPerClient = 1,
            };
            PgBenchResult benchmarkResult = await pgBench.ExecuteAsync(
                benchmark,
                timeout: TimeSpan.FromSeconds(30));
            Assert.Equal(PgBenchExitStatus.Success, benchmarkResult.Status);
        }
        finally
        {
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }
        }
    }

    private static PsqlOptions ConnectionOptions(
        string database,
        string host,
        string user) =>
        new PsqlOptions
        {
            Database = database,
            Host = host,
            Username = user,
            NoPsqlRc = true,
        };

    private static string RequiredEnvironment(string name) =>
        Environment.GetEnvironmentVariable(name) ??
        throw new InvalidOperationException(
            "Required real-PostgreSQL integration environment variable is missing: " + name);

    private static string ExecutableName(string name) =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? name + ".exe"
            : name;
}
