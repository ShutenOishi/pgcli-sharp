# PgCliSharp

> **日本語:** [日本語版 README](https://github.com/ShutenOishi/pgcli-sharp/blob/main/README.ja.md)  
> **English:** This page

Strongly typed .NET wrapper for PostgreSQL command-line tools.

## Project status

PgCliSharp has completed Phase 1. The first complete typed wrapper, `pg_dump`, supports PostgreSQL 10 through 18 with version-aware validation. Phase 2 will add `pg_restore` and `pg_dumpall`.

Initial PostgreSQL compatibility target:

- PostgreSQL 10 through 18
- `netstandard2.0`
- `net8.0`
- `net10.0`

Each completed roadmap phase is preserved as a GitHub Release with an explicit source ZIP and NuGet package artifacts.

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
