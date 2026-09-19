# PgCliSharp

> **日本語:** [日本語版 README](https://github.com/ShutenOishi/pgcli-sharp/blob/main/README.ja.md)  
> **English:** This page

Strongly typed .NET wrapper for PostgreSQL command-line tools.

## Project status

PgCliSharp has completed the Phase 4 Backup/WAL implementation on top of the Phase 2 backup/restore core and Phase 3 release-pipeline work. External publication of the prepared `PgCliSharp 0.1.0-alpha.1` candidate remains deferred until the final release phase under ADR-0012; development continues with Phase 5.

Initial PostgreSQL compatibility target:

- PostgreSQL 10 through 18
- `netstandard2.0`
- `net8.0`
- `net10.0`

Each completed roadmap phase is preserved as a GitHub Release with an explicit source ZIP and NuGet package artifacts.

## NuGet preview

The prepared preview candidate is `0.1.0-alpha.1`, but it is not currently published to nuget.org. The following command is retained as the intended consumer form for eventual publication:

```bash
dotnet add package PgCliSharp --version 0.1.0-alpha.1
```

The package targets `netstandard2.0`, `net8.0`, and `net10.0`. Publication is deferred until the final release phase. When authorized, GitHub Actions will revalidate the recorded Phase 3 source commit and use NuGet Trusted Publishing/OIDC; no long-lived NuGet API key is stored in the repository.

## pg_dump quick start

The executable path and expected PostgreSQL CLI major version are explicit:

```csharp
var pgDump = new PgDump(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pg_dump.exe",
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
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pg_restore.exe",
    PostgreSqlMajorVersion.V18);

await pgRestore.ExecuteAsync(
    new PgRestoreOptions { Jobs = 4 },
    PgRestoreInput.FromFile("appdb.dump"),
    PgRestoreOutput.ToDatabase("appdb"));

var pgDumpAll = new PgDumpAll(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pg_dumpall.exe",
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

## Development

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

## Releases

Completed implementation phases are recorded under [GitHub Releases](https://github.com/ShutenOishi/pgcli-sharp/releases).

Each Phase Release contains:

- an explicit source ZIP;
- the `.nupkg`;
- the `.snupkg`;
- release notes in English and Japanese.

Phase Releases are development milestones. Publication to nuget.org is handled separately according to the release roadmap.

## Project documents

- [AGENTS.md](AGENTS.md) — repository entry point and mandatory working rules
- [Architecture Decision Records](docs/adr/README.md) — decision history, status, alternatives, and consequences
- [Architecture and compatibility](docs/architecture.md) — consolidated current state
- [Localization and bilingual documentation](docs/localization.md) — English/Japanese policy
- [Implementation and NuGet roadmap](docs/roadmap.md)

When implementation work changes a material project-wide decision, add or supersede an ADR and update the relevant consolidated document in the same change. Chat history is not the source of truth.
