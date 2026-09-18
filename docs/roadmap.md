# PgCliSharp Roadmap

> This roadmap describes planned work. Accepted architecture/product decisions are recorded separately in [Architecture Decision Records](adr/README.md). Proposed ADRs are not commitments until accepted.

## Product direction

Build a strongly typed .NET API for PostgreSQL command-line tools with explicit executable selection, PostgreSQL 10-18 compatibility, proactive validation, bilingual public documentation, localized runtime diagnostics, and automated NuGet publishing.

## Phase 0 - Foundation

- Create solution/project structure.
- Establish package ID and NuGet metadata.
- Implement target-framework matrix.
- Enable nullable reference types and strict build/analyzer settings where practical.
- Enable XML documentation and SourceLink.
- Add localization resources (English neutral + Japanese).
- Implement common exceptions/diagnostics.
- Implement process runner abstraction.
- Use the BCL process backend on modern .NET and a conditional CliWrap backend on `netstandard2.0`.
- Implement cancellation and timeout behavior.
- Implement executable version parser/cache.
- Add PostgreSQL 10-18 version metadata.
- Establish CI.

## Phase 1 - pg_dump

- Inventory official pg_dump options for PostgreSQL 10-18.
- Implement `PgDumpOptions`.
- Use enums for finite option values.
- Use value objects for structured option values such as compression.
- Support repeatable options with collections.
- Implement version availability validation.
- Implement option-combination validation.
- Support binary/stdout streaming safely.
- Add bilingual XML docs to all public API.
- Add unit/compatibility/integration tests.

This phase defines the design template for later tools.

## Phase 2 - pg_restore and pg_dumpall

- Implement complete typed option coverage for PostgreSQL 10-18.
- Reuse internal execution/connection argument infrastructure without leaking inappropriate options between public option classes.
- Complete backup/restore trio tests.

## Phase 3 - First NuGet preview

Target an initial preview such as `0.1.0-alpha.1` once the foundation and backup/restore core are usable.

- Finalize package metadata.
- Produce `.nupkg` and `.snupkg`.
- Verify SourceLink and XML docs.
- Configure GitHub Actions release workflow.
- Configure NuGet Trusted Publishing/OIDC.
- Validate version/tag consistency.
- Publish first preview.

## Phase 4 - Backup and WAL tools

Examples:

- `pg_basebackup`
- `pg_verifybackup`
- `pg_receivewal`
- `pg_recvlogical`
- `pg_combinebackup` where available

Model version/tool availability explicitly.

## Phase 5 - Database management and maintenance tools

Examples:

- `createdb`
- `dropdb`
- `createuser`
- `dropuser`
- `vacuumdb`
- `reindexdb`
- `clusterdb`
- `pg_isready`
- `pg_amcheck` where available

## Phase 6 - Rich I/O tools

Examples:

- `psql`
- `pgbench`

Design stdin/stdout/interactive behavior deliberately rather than forcing it through an API designed for pg_dump.

## Phase 7 - Server applications

Add server-side administrative executables in a clearly separated namespace/category, for example:

- `initdb`
- `pg_ctl`
- `pg_upgrade`
- `pg_rewind`
- `pg_checksums`
- `pg_resetwal`

Tools that can alter or recover data directories require especially explicit documentation and validation.

## Phase 8 - 1.0 stabilization

- Review all public APIs for naming and consistency.
- Review exception hierarchy and structured diagnostic properties.
- Verify English/Japanese XML documentation coverage.
- Verify English/Japanese resource coverage.
- Run PostgreSQL 10-18 compatibility matrix.
- Verify Windows/Linux/macOS behavior where applicable.
- Freeze breaking API changes.
- Complete README/examples.
- Release candidates followed by `1.0.0`.

## Continuous work

For every phase:

- create or supersede ADRs for material repository-wide decisions;
- update consolidated canonical docs in the same change;
- extend compatibility specifications;
- add regression tests for discovered PostgreSQL/version differences;
- keep release notes/changelog;
- avoid relying on chat history as project specification.

## Future PostgreSQL releases

New PostgreSQL major versions should be added by extending version metadata, compatibility specifications, typed options, and tests without rewriting older-version support.

Pre-release PostgreSQL versions should not automatically be advertised as stable supported versions.
