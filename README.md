# PgCliSharp

Strongly typed .NET wrapper for PostgreSQL command-line tools.

## Project status

PgCliSharp has completed its Phase 0 foundation. Phase 1 will implement the first complete typed wrapper, `pg_dump`. Initial PostgreSQL compatibility target is PostgreSQL 10 through 18.

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

The initial accepted library target matrix is `netstandard2.0;net8.0;net10.0`. The `netstandard2.0` process backend uses CliWrap internally according to ADR-0008; modern targets use the .NET BCL process APIs directly.

## Canonical project documents

Project-wide decisions are stored in the repository so they can be reused across chats, contributors, and coding agents.

- [AGENTS.md](AGENTS.md) — repository entry point and mandatory working rules
- [Architecture Decision Records](docs/adr/README.md) — decision history, status, alternatives, and consequences
- [Architecture and compatibility](docs/architecture.md) — consolidated current state
- [Localization and bilingual XML documentation](docs/localization.md) — consolidated current state
- [Implementation and NuGet roadmap](docs/roadmap.md)

When implementation work changes a material project-wide decision, add or supersede an ADR and update the relevant consolidated document in the same change. Chat history is not the source of truth.
