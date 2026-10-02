using System.Text;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Tests;

public sealed class RealPostgreSqlMigrationTests
{
    [Fact]
    public async Task DivergentOwnedCluster_RewindsAndPreservesSourceRows()
    {
        if (!RealPostgreSqlTestEnvironment.TryGet(out string binary, out PostgreSqlMajorVersion version)) return;
        // This fixture deliberately uses pg_rewind's typed recovery-conf option (13+).
        if (version < PostgreSqlMajorVersion.V13) return;
        string root = CreateOwnedRoot();
        var source = new OwnedCluster(root, "source", binary, version);
        var target = new OwnedCluster(root, "target", binary, version);
        bool succeeded = false;
        try
        {
            await source.InitializeAsync();
            await source.StartAsync();
            await source.SqlAsync("CREATE TABLE rewind_probe(value text); INSERT INTO rewind_probe VALUES ('seed'); CHECKPOINT;");
            var backup = new PgBaseBackup(Path.Combine(binary, "pg_basebackup"), version);
            await backup.ExecuteAsync(new PgBaseBackupOptions
            {
                Host = source.SocketDirectory, Port = 5432, Username = source.Username,
                WalMethod = PgBaseBackupWalMethod.Stream, WriteRecoveryConf = true,
                Checkpoint = PgBaseBackupCheckpointMode.Fast,
            }, PgBaseBackupDestination.ToDirectory(target.DataDirectory), TimeSpan.FromMinutes(2));
            await target.StartAsync();
            Assert.Equal("seed", await target.SqlAsync("SELECT value FROM rewind_probe;"));
            await source.StopAsync();
            await target.ControlAsync(PgCtlCommand.Promote);
            await target.SqlAsync("INSERT INTO rewind_probe VALUES ('fork'); CHECKPOINT;");
            Assert.Equal("fork,seed", await target.SqlAsync("SELECT string_agg(value, ',' ORDER BY value) FROM rewind_probe;"));
            await target.StopAsync();
            await source.StartAsync();
            await source.SqlAsync("INSERT INTO rewind_probe VALUES ('source'); CHECKPOINT;");

            var rewind = new PgRewind(Path.Combine(binary, "pg_rewind"), version);
            PgServerResult result = await rewind.ExecuteAsync(new PgRewindOptions
            {
                TargetDataDirectory = target.DataDirectory,
                SourceConnectionString = "host=" + source.SocketDirectory + " port=5432 dbname=postgres user=" + source.Username,
                WriteRecoveryConfiguration = true,
            }, timeout: TimeSpan.FromMinutes(2));
            Assert.Equal(0, result.ExitCode);
            await target.StartAsync();
            Assert.Equal("seed,source", await target.SqlAsync("SELECT string_agg(value, ',' ORDER BY value) FROM rewind_probe;"));
            Assert.Equal("t", await target.SqlAsync("SELECT pg_is_in_recovery();"));
            succeeded = true;
        }
        finally
        {
            await target.StopAsync();
            await source.StopAsync();
            if (succeeded) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task OwnedClusters_CopyUpgrade16To18PreservesRows()
    {
        if (!RealPostgreSqlTestEnvironment.TryGet(out string binary, out PostgreSqlMajorVersion version)) return;
        if (version != PostgreSqlMajorVersion.V18) return;
        string? oldBinary = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_UPGRADE_FROM_BIN");
        if (string.IsNullOrWhiteSpace(oldBinary))
        {
            if (Environment.GetEnvironmentVariable("PGCLI_REAL_PG_REQUIRED") == "true")
                throw new InvalidOperationException("The required PostgreSQL 16-to-18 fixture has no source binaries.");
            return;
        }
        string root = CreateOwnedRoot();
        var oldCluster = new OwnedCluster(root, "old", oldBinary!, PostgreSqlMajorVersion.V16);
        var newCluster = new OwnedCluster(root, "new", binary, version);
        bool succeeded = false;
        try
        {
            await oldCluster.InitializeAsync();
            await newCluster.InitializeAsync();
            await oldCluster.StartAsync();
            await oldCluster.SqlAsync("CREATE TABLE upgrade_probe(value integer); INSERT INTO upgrade_probe VALUES (16),(18); CHECKPOINT;");
            await oldCluster.StopAsync();
            var upgrade = new PgUpgrade(Path.Combine(binary, "pg_upgrade"), version);
            var options = new PgUpgradeOptions
            {
                OldDataDirectory = oldCluster.DataDirectory, NewDataDirectory = newCluster.DataDirectory,
                OldBinaryDirectory = oldBinary, NewBinaryDirectory = binary,
                Username = oldCluster.Username, SocketDirectory = root,
                OldPort = 50432, NewPort = 50433, TransferMode = PgUpgradeTransferMode.Copy,
                NoStatistics = true,
            };
            PgServerResult result = await upgrade.ExecuteAsync(options, timeout: TimeSpan.FromMinutes(3));
            Assert.Equal(0, result.ExitCode);
            await newCluster.StartAsync();
            Assert.Equal("34", await newCluster.SqlAsync("SELECT sum(value) FROM upgrade_probe;"));
            Assert.Equal("18", await newCluster.SqlAsync("SELECT current_setting('server_version_num')::integer / 10000;"));
            // Copy mode, unlike link/swap, must leave this owned source usable.
            await oldCluster.StartAsync();
            Assert.Equal("34", await oldCluster.SqlAsync("SELECT sum(value) FROM upgrade_probe;"));
            succeeded = true;
        }
        finally
        {
            await newCluster.StopAsync();
            await oldCluster.StopAsync();
            if (succeeded) Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateOwnedRoot()
    {
        string root = Path.Combine(Path.GetTempPath(), "pgcli-phase8-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed class OwnedCluster
    {
        private readonly string _binary;
        private readonly string _log;
        private readonly PostgreSqlMajorVersion _version;

        internal OwnedCluster(string root, string name, string binary, PostgreSqlMajorVersion version)
        {
            _binary = binary;
            _version = version;
            Username = Environment.UserName;
            _log = Path.Combine(root, name + ".log");
            DataDirectory = Path.Combine(root, name + "-data");
            SocketDirectory = Path.Combine(root, name + "-socket");
            Directory.CreateDirectory(SocketDirectory);
        }

        internal string DataDirectory { get; }
        internal string SocketDirectory { get; }
        internal string Username { get; }

        internal async Task InitializeAsync() =>
            _ = await new InitDb(Path.Combine(_binary, "initdb"), _version).ExecuteAsync(new InitDbOptions
            {
                DataDirectory = DataDirectory, NoLocale = true, Encoding = "UTF8",
                DataChecksums = true, Username = Username,
            }, timeout: TimeSpan.FromMinutes(2));

        internal async Task StartAsync()
        {
            var options = new PgCtlOptions
            {
                Command = PgCtlCommand.Start, DataDirectory = DataDirectory,
                LogFile = _log, Wait = true, WaitTimeoutSeconds = 30,
            };
            options.ForwardedOptions.Add("-c listen_addresses='' -k '" + SocketDirectory + "' -p 5432");
            await new PgCtl(Path.Combine(_binary, "pg_ctl"), _version).ExecuteAsync(options, timeout: TimeSpan.FromMinutes(1));
        }

        internal async Task ControlAsync(PgCtlCommand command) =>
            _ = await new PgCtl(Path.Combine(_binary, "pg_ctl"), _version).ExecuteAsync(new PgCtlOptions
            {
                Command = command, DataDirectory = DataDirectory, Wait = true, WaitTimeoutSeconds = 30,
            }, timeout: TimeSpan.FromMinutes(1));

        internal async Task StopAsync()
        {
            if (!File.Exists(Path.Combine(DataDirectory, "PG_VERSION"))) return;
            var ctl = new PgCtl(Path.Combine(_binary, "pg_ctl"), _version);
            PgCtlResult status = await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Status, DataDirectory = DataDirectory }, timeout: TimeSpan.FromSeconds(30));
            if (status.ServerStatus == PgCtlServerStatus.Running)
                await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Stop, DataDirectory = DataDirectory, ShutdownMode = PgCtlShutdownMode.Fast, Wait = true }, timeout: TimeSpan.FromMinutes(1));
        }

        internal async Task<string> SqlAsync(string sql)
        {
            using var output = new MemoryStream();
            var options = new PsqlOptions
            {
                Database = "postgres", Host = SocketDirectory, Port = 5432, Username = Username,
                NoPsqlRc = true, TuplesOnly = true, OutputFormat = PsqlOutputFormat.Unaligned,
            };
            options.Actions.Add(PsqlAction.Command(sql));
            PsqlResult result = await new Psql(Path.Combine(_binary, "psql"), _version).ExecuteAsync(options, new PsqlIo(standardOutput: output), TimeSpan.FromSeconds(30));
            Assert.True(result.Status == PsqlExitStatus.Success, result.StandardError);
            return Encoding.UTF8.GetString(output.ToArray()).Trim();
        }
    }
}
