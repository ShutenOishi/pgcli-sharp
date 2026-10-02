using PgCliSharp.ServerApplications;

namespace PgCliSharp.Tests;

public sealed class RealPostgreSqlServerApplicationsTests
{
    [Fact]
    public async Task OwnedDisposableCluster_InitializesChecksumsDryRunAndServerLifecycle()
    {
        string? binaryDirectory = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_BIN");
        string? majorText = Environment.GetEnvironmentVariable("PGCLI_REAL_PG_MAJOR");
        if (string.IsNullOrWhiteSpace(binaryDirectory) || string.IsNullOrWhiteSpace(majorText)) return;
        var version = (PostgreSqlMajorVersion)int.Parse(majorText, System.Globalization.CultureInfo.InvariantCulture);
        string ownedRoot = Path.Combine(Path.GetTempPath(), "pgclisharp-phase7-" + Guid.NewGuid().ToString("N"));
        string data = Path.Combine(ownedRoot, "data");
        Directory.CreateDirectory(ownedRoot);
        var init = new InitDb(Path.Combine(binaryDirectory, "initdb"), version);
        var checksums = new PgChecksums(Path.Combine(binaryDirectory, "pg_checksums"), version);
        var reset = new PgResetWal(Path.Combine(binaryDirectory, "pg_resetwal"), version);
        var ctl = new PgCtl(Path.Combine(binaryDirectory, "pg_ctl"), version);
        bool serverMayBeRunning = false;
        try
        {
            await init.ExecuteAsync(new InitDbOptions { DataDirectory = data, NoLocale = true, Encoding = "UTF8", DataChecksums = true }, timeout: TimeSpan.FromMinutes(2));
            await checksums.ExecuteAsync(new PgChecksumsOptions { DataDirectory = data }, timeout: TimeSpan.FromMinutes(1));
            await checksums.ExecuteAsync(new PgChecksumsOptions { DataDirectory = data, Mode = PgChecksumsMode.Disable }, timeout: TimeSpan.FromMinutes(1));
            await checksums.ExecuteAsync(new PgChecksumsOptions { DataDirectory = data, Mode = PgChecksumsMode.Enable }, timeout: TimeSpan.FromMinutes(1));
            await reset.ExecuteAsync(new PgResetWalOptions { DataDirectory = data, DryRun = true }, timeout: TimeSpan.FromMinutes(1));
            var start = new PgCtlOptions { Command = PgCtlCommand.Start, DataDirectory = data, LogFile = Path.Combine(ownedRoot, "postgres.log"), Wait = true, WaitTimeoutSeconds = 30 };
            // No TCP listener. The unique socket directory isolates the default port.
            start.ForwardedOptions.Add("-c listen_addresses='' -k '" + ownedRoot + "'");
            serverMayBeRunning = true;
            await ctl.ExecuteAsync(start, timeout: TimeSpan.FromMinutes(1));
            PgCtlResult running = await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Status, DataDirectory = data });
            Assert.Equal(PgCtlServerStatus.Running, running.ServerStatus);
            await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Stop, DataDirectory = data, ShutdownMode = PgCtlShutdownMode.Fast, Wait = true }, timeout: TimeSpan.FromMinutes(1));
            serverMayBeRunning = false;
            PgCtlResult stopped = await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Status, DataDirectory = data });
            Assert.Equal(PgCtlServerStatus.NotRunning, stopped.ServerStatus);
        }
        finally
        {
            if (serverMayBeRunning)
            {
                PgCtlResult status = await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Status, DataDirectory = data });
                if (status.ServerStatus == PgCtlServerStatus.Running)
                    await ctl.ExecuteAsync(new PgCtlOptions { Command = PgCtlCommand.Stop, DataDirectory = data, ShutdownMode = PgCtlShutdownMode.Immediate, Wait = true }, timeout: TimeSpan.FromMinutes(1));
            }
            Directory.Delete(ownedRoot, recursive: true);
        }
    }
}
