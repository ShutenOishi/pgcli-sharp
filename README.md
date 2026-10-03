# PgCliSharp

> **日本語:** [日本語版 README](https://github.com/ShutenOishi/pgcli-sharp/blob/main/README.ja.md)  
> **English:** This page

Strongly typed .NET wrapper for PostgreSQL command-line tools.

## Project status

All 25 wrappers and the reviewed API are implemented. Phase 8 prepares the unpublished `0.1.0-alpha.2` preview, including lambda configuration and offline command generation. See the [final review](docs/phase-8-final-review.md) for tested scope and remaining publication gates. This is not a 1.0 stability claim. Historical alpha.1 remains preserved under ADR-0012.

Initial PostgreSQL compatibility target:

- PostgreSQL 10 through 18
- `netstandard2.0`
- `net8.0`
- `net10.0`

Phase 0-2 retain their existing GitHub Releases. Under ADR-0012, Phase 3 onward is completed by a reviewed `main` merge plus exact-commit CI; new GitHub Releases, tags, Release assets, and NuGet publication are deferred until the final release phase.

## NuGet preview

The selected preview version is `0.1.0-alpha.2`; it is not published to nuget.org. After downloading an unpublished candidate CI artifact and extracting it to `./candidate-packages`, a local consumer can use:

Create a project-local `NuGet.Config` beside the consumer project so both the candidate and its separately resolved dependencies can restore on Windows/Linux/macOS:

```xml
<configuration>
  <packageSources>
    <clear />
    <add key="candidate" value="./candidate-packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

```bash
dotnet add package PgCliSharp --version 0.1.0-alpha.2
dotnet restore
```

The package targets `netstandard2.0`, `net8.0`, and `net10.0`. Local restore also needs the separate dependencies available from NuGet or an offline feed; no PostgreSQL binary is included. Candidate source, SDK, build lock and audit accompany the CI artifact. External publication requires explicit approval and Trusted Publishing verification. Long-lived NuGet API keys are not stored in this repository. See [third-party notices](THIRD-PARTY-NOTICES.md).

## pg_dump quick start

The executable path and expected PostgreSQL CLI major version are explicit:

```csharp
var pgDump = new PgDump(
    @"C:\Program Files\PostgreSQL\18\bin\pg_dump.exe",
    PostgreSqlMajorVersion.V18);

var options = new PgDumpOptions
{
    Database = "appdb",
    Format = PgDumpFormat.Custom,
};

options.Schemas.Add("public");

PgDumpResult result = await pgDump.ExecuteAsync(
    options,
    PgDumpOutput.ToFile("appdb.dump"),
    timeout: TimeSpan.FromMinutes(10));
```

`PgDumpOptions` models the PostgreSQL 10-18 option union with enums, value objects, and ordered collections. PgCliSharp validates the selected executable version, version-specific option availability, and incompatible combinations before starting the dump. Stdout destinations remain binary-safe, and PostgreSQL 17+ `--filter=-` can stream filter rules through stdin.

The maintained compatibility inventory is in [`spec/postgresql/pg_dump.json`](spec/postgresql/pg_dump.json), with human-readable Phase 1 research in [`docs/pg-dump-phase-1.md`](docs/pg-dump-phase-1.md).

## pg_restore and pg_dumpall

Phase 2 keeps archive input, direct-database restore, generated SQL/list output, and cluster-wide SQL-script output explicit rather than reducing them to arbitrary command-line tails.

```csharp
var pgRestore = new PgRestore(
    @"C:\Program Files\PostgreSQL\18\bin\pg_restore.exe",
    PostgreSqlMajorVersion.V18);

await pgRestore.ExecuteAsync(
    new PgRestoreOptions { Jobs = 4 },
    PgRestoreInput.FromFile("appdb.dump"),
    PgRestoreOutput.ToDatabase("appdb"));

var pgDumpAll = new PgDumpAll(
    @"C:\Program Files\PostgreSQL\18\bin\pg_dumpall.exe",
    PostgreSqlMajorVersion.V18);

await pgDumpAll.ExecuteAsync(
    new PgDumpAllOptions { InitialDatabase = "postgres" },
    PgDumpAllOutput.ToFile("cluster.sql"));
```

`PgRestoreInput` distinguishes archive file, directory, and stdin consumers. `PgRestoreOutput` distinguishes direct database restore from generated SQL/list output, and stream output is normalized with `--file=-` so PostgreSQL 10-18 have one stable wrapper behavior. `PgDumpAllScope` represents the mutually exclusive global/roles/tablespaces-only modes without contradictory boolean pairs.

Machine-readable inventories are maintained in [`spec/postgresql/pg_restore.json`](spec/postgresql/pg_restore.json) and [`spec/postgresql/pg_dumpall.json`](spec/postgresql/pg_dumpall.json). Their research notes are [`docs/pg-restore-phase-2.md`](docs/pg-restore-phase-2.md) and [`docs/pg-dumpall-phase-2.md`](docs/pg-dumpall-phase-2.md).

## Backup and WAL tools

Phase 4 adds typed wrappers for `pg_basebackup`, `pg_receivewal`, `pg_recvlogical`, `pg_verifybackup`, and `pg_combinebackup`.

`pg_basebackup`, `pg_receivewal`, and `pg_recvlogical` are modeled for PostgreSQL 10-18. `pg_verifybackup` is available from PostgreSQL 13 and `pg_combinebackup` from PostgreSQL 17; selecting an earlier major fails before process startup with `PgUnsupportedToolException`.

The Phase 4 APIs keep destinations and streaming explicit. For example, pg_basebackup tar output and pg_recvlogical stdout can be sent directly to caller-owned streams without whole-payload buffering, while version-specific options and incompatible combinations are validated before execution.

Machine-readable inventories are maintained in `spec/postgresql/pg_basebackup.json`, `pg_receivewal.json`, `pg_recvlogical.json`, `pg_verifybackup.json`, and `pg_combinebackup.json`. Research and implementation notes are in [`docs/backup-wal-phase-4.md`](docs/backup-wal-phase-4.md).

## Database management and maintenance tools

Phase 5 adds typed wrappers for `createdb`, `dropdb`, `createuser`, `dropuser`, `vacuumdb`, `reindexdb`, `clusterdb`, `pg_isready`, and `pg_amcheck`.

The first eight tools are modeled for PostgreSQL 10-18. `pg_amcheck` is available from PostgreSQL 14 and is rejected before process startup for PostgreSQL 10-13. Version-specific options such as `dropdb --force`, `reindexdb --concurrently`, and PostgreSQL 18 `vacuumdb --missing-stats-only` are validated against the selected CLI version.

`PgMaintenanceIo` forwards caller-owned stdin/stdout streams for interactive or streamed text behavior. `pg_isready` preserves its semantic exit codes as `PgIsReadyStatus` values instead of treating codes 1-3 as ordinary command failures.

```csharp
var ready = new PgIsReady(
    @"C:\Program Files\PostgreSQL\18\bin\pg_isready.exe",
    PostgreSqlMajorVersion.V18);

PgIsReadyResult status = await ready.ExecuteAsync(new PgIsReadyOptions
{
    Host = "localhost",
    Port = 5432,
    ConnectTimeoutSeconds = 2,
});
```

The nine compatibility inventories are maintained under `spec/postgresql/`. Research and implementation notes are in [`docs/database-maintenance-phase-5.md`](docs/database-maintenance-phase-5.md), with completion evidence in [`docs/phase-5-completion.md`](docs/phase-5-completion.md).

## psql and pgbench rich I/O

Phase 6 adds typed PostgreSQL 10-18 wrappers for `psql` and `pgbench` without forcing their richer I/O behavior through a dump-style one-shot API.

Finite psql execution uses `PsqlIo` for optional caller-owned stdin/stdout/stderr. Ordered `PsqlAction` values preserve interleaved command/file execution exactly. For a process that must stay alive while the caller writes commands, `StartSessionAsync` returns a `PsqlSession`.

```csharp
var psql = new Psql(
    @"C:\Program Files\PostgreSQL\18\bin\psql.exe",
    PostgreSqlMajorVersion.V18);

using var stdout = new MemoryStream();
using var stderr = new MemoryStream();

using PsqlSession session = await psql.StartSessionAsync(
    new PsqlOptions { Database = "appdb", NoPsqlRc = true },
    new PsqlSessionIo(stdout, stderr),
    timeout: TimeSpan.FromMinutes(2));

byte[] commands = Encoding.UTF8.GetBytes(
    "select current_database();\n\\q\n");
await session.StandardInput.WriteAsync(commands, 0, commands.Length);
PsqlSessionResult sessionResult = await session.CompleteAsync();
```

The session is a redirected pipe session, **not** a TTY/PTY terminal. It deliberately does not promise Readline, history, or other terminal-only behavior.

`pgbench` remains finite execution. `PgBenchIo` exposes caller-owned stdout/stderr destinations so benchmark summaries, progress, debug output, and diagnostics can stream without mandatory whole-output buffering.

```csharp
var pgBench = new PgBench(
    @"C:\Program Files\PostgreSQL\18\bin\pgbench.exe",
    PostgreSqlMajorVersion.V18);

await pgBench.ExecuteAsync(
    new PgBenchOptions
    {
        Database = "appdb",
        Clients = 10,
        DurationSeconds = 30,
        ProgressSeconds = 5,
    },
    new PgBenchIo(
        standardOutput: Console.OpenStandardOutput(),
        standardError: Console.OpenStandardError()));
```

PgCliSharp validates version-specific pgbench behavior before startup, including PostgreSQL 11/13/15/17 option boundaries, PostgreSQL 13+ server-side initialization step `G`, logging/progress/partition/retry constraints, script-weight rules, and the PostgreSQL 12+ runtime-error exit status.

The compatibility inventories are [`spec/postgresql/psql.json`](spec/postgresql/psql.json) and [`spec/postgresql/pgbench.json`](spec/postgresql/pgbench.json). Research is recorded in [`docs/rich-io-phase-6.md`](docs/rich-io-phase-6.md), ADR-0013 records the session lifecycle decision, and completion evidence is in [`docs/phase-6-completion.md`](docs/phase-6-completion.md).

## Server applications

Phase 7 adds `InitDb`, `PgCtl`, `PgUpgrade`, `PgRewind`, `PgChecksums`, and
`PgResetWal` in `PgCliSharp.ServerApplications`. They model PostgreSQL 10-18;
`PgChecksums` begins at 12. All six have typed Options, pre-execution validation,
version probing, caller-owned I/O and `GetHelpAsync`.

```csharp
using PgCliSharp.ServerApplications;

var init = new InitDb(@"C:\Program Files\PostgreSQL\18\bin\initdb.exe",
    PostgreSqlMajorVersion.V18);
await init.ExecuteAsync(new InitDbOptions
{
    DataDirectory = @"C:\pgdata\new-cluster",
    NoLocale = true,
    Encoding = "UTF8",
    DataChecksums = true,
});

var ctl = new PgCtl(@"C:\Program Files\PostgreSQL\18\bin\pg_ctl.exe",
    PostgreSqlMajorVersion.V18);
PgCtlResult status = await ctl.ExecuteAsync(new PgCtlOptions
{
    Command = PgCtlCommand.Status,
    DataDirectory = @"C:\pgdata\new-cluster",
});
```

`PgCtlServerStatus` distinguishes running, stopped and unavailable directories.
Start/Restart require an explicit `LogFile` to release detached-server pipes.
Null initdb checksum policy preserves the upstream default (off through 17, on
from 18). Upgrade/rewind/reset operations can change data directories; interruption
does not promise rollback, and pg_resetwal is last-resort repair. See the
[server application guide](docs/server-applications-phase-7.md) for complete
version boundaries, trusted upstream option fragments and real-test limits.

## Development

All C# examples above are compile-checked on modern targets and the Windows
.NET Framework 4.8 consumer; CI does not execute their database operations.
Add `using PgCliSharp;`, `using System;`, `using System.IO;` and
`using System.Text;` as needed. Supply executable paths and connection settings
for your own environment; no PostgreSQL binary is bundled. The CLI version is
not automatically the server version. See [API review and freeze](docs/phase-8-api-review.md)
for stream ownership, status/exception conventions and compatibility limits.

The repository uses the XML solution format:

```text
PgCliSharp.slnx
```

The repository SDK is pinned through `global.json` to .NET 10 with feature-band roll-forward enabled.

Typical validation commands:

```bash
dotnet restore PgCliSharp.slnx
dotnet build PgCliSharp.slnx --configuration Release --no-restore
dotnet test PgCliSharp.slnx --configuration Release --no-build --no-restore
```

The `netstandard2.0` process backend uses CliWrap internally according to ADR-0008. Modern targets use the .NET BCL process APIs directly.

## License

PgCliSharp is [MIT licensed](LICENSE): commercial use and proprietary distribution
of modifications are allowed without source disclosure. Retain the copyright
and permission notice. PostgreSQL executables are not bundled and keep their own
license. See [Licensing / ライセンス](docs/licensing.md) for dependency boundaries.

## Release status

[GitHub Releases](https://github.com/ShutenOishi/pgcli-sharp/releases) preserves the existing Phase 0-2 milestone releases.

Starting with Phase 3, ADR-0012 separates implementation completion from external publication. A phase is completed by a reviewed `main` merge, exact-commit Linux/macOS/Windows CI, compatibility/research evidence, and updated repository documentation. New GitHub Release tags, Releases, Release assets, and NuGet pushes remain deferred until the final release phase.

## Project documents

- [AGENTS.md](AGENTS.md) — repository entry point and mandatory working rules
- [Architecture Decision Records](docs/adr/README.md) — decision history, status, alternatives, and consequences
- [Architecture and compatibility](docs/architecture.md) — consolidated current state
- [Localization and bilingual documentation](docs/localization.md) — English/Japanese policy
- [Implementation and NuGet roadmap](docs/roadmap.md)

When implementation work changes a material project-wide decision, add or supersede an ADR and update the relevant consolidated document in the same change. Chat history is not the source of truth.

## Configuration and command generation

All 25 wrappers support lambda configuration, offline validation and command generation. Options are copied before asynchronous execution. See [the English/Japanese guide](docs/configuration-and-commands.md) for stream routing, patch-version checks and secret handling.

```csharp
var dump = new PgDump(@"C:\Program Files\PostgreSQL\18\bin\pg_dump.exe", PostgreSqlMajorVersion.V18);
PgCommand command = dump.CreateCommand(
    configureOptions: options =>
    {
        options.Database = "appdb";
        options.Format = PgDumpFormat.Custom;
        options.Schemas.Add("public");
    },
    output: PgDumpOutput.ToFile("appdb.dump"));
string actualCommand = command.ToCommandLine(PgCommandLineStyle.PowerShell, includeSensitiveValues: true);
Console.WriteLine(actualCommand);
```

This creates the command without executing even a version probe. PowerShell export targets 7.5+ with Standard native argument passing. Raw export may contain secrets; `command.ToString()` redacts every argument/environment value. Streams require separate routing. `Validate` returns the first offline option error; it does not verify installed executables or server state.
