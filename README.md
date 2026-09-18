# PgCliSharp

> **日本語:** [日本語版 README](https://github.com/ShutenOishi/pgcli-sharp/blob/main/README.ja.md)  
> **English:** This page

Strongly typed .NET wrapper for PostgreSQL command-line tools.

## Project status

PgCliSharp has completed its Phase 0 foundation. Phase 1 will implement the first complete typed wrapper, `pg_dump`.

Initial PostgreSQL compatibility target:

- PostgreSQL 10 through 18
- `netstandard2.0`
- `net8.0`
- `net10.0`

Each completed roadmap phase is preserved as a GitHub Release with an explicit source ZIP and NuGet package artifacts.

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
