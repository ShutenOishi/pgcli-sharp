# Phase 5 Completion Evidence

Phase 5 covers the database-management and maintenance roadmap scope and follows ADR-0011 specification-first implementation plus ADR-0012 deferred publication.

## Implemented tool scope

- `createdb`: PostgreSQL 10-18
- `dropdb`: PostgreSQL 10-18
- `createuser`: PostgreSQL 10-18
- `dropuser`: PostgreSQL 10-18
- `vacuumdb`: PostgreSQL 10-18
- `reindexdb`: PostgreSQL 10-18
- `clusterdb`: PostgreSQL 10-18
- `pg_isready`: PostgreSQL 10-18
- `pg_amcheck`: PostgreSQL 14-18

Unsupported whole-tool versions for pg_amcheck are rejected before process startup. Option-level major-version boundaries are represented by centralized runtime availability metadata and validated before normal execution.

## Compatibility evidence

Canonical machine-readable specifications:

- `spec/postgresql/createdb.json`
- `spec/postgresql/dropdb.json`
- `spec/postgresql/createuser.json`
- `spec/postgresql/dropuser.json`
- `spec/postgresql/vacuumdb.json`
- `spec/postgresql/reindexdb.json`
- `spec/postgresql/clusterdb.json`
- `spec/postgresql/pg_isready.json`
- `spec/postgresql/pg_amcheck.json`

Research and upstream-source findings are recorded in `docs/database-maintenance-phase-5.md`.

Coverage tests verify:

- every inventory entry has a public property or explicit special binding;
- all nine tools record explicit per-major availability/inventories;
- every version-varying long option agrees with its runtime availability catalog;
- the PostgreSQL 14 whole-tool boundary for pg_amcheck;
- deterministic argument generation, repeatable-option ordering, aliases, positional arguments, and pg_amcheck optional-argument serialization;
- upstream-determinable hard option/value conflicts and version boundaries;
- executable-version mismatch before normal execution;
- timeout, cancellation, and environment forwarding;
- caller-owned stdin/stdout forwarding for maintenance tools;
- pg_isready exit codes 0-3 as semantic typed statuses and out-of-range exit codes as failures;
- Windows .NET Framework 4.8 compilation/tests in addition to modern Linux/macOS targets.

## Cross-platform implementation gate

Implementation/test head:

`26de678e510335fe09c4ef60f69e63ee0dc21de3`

GitHub Actions CI run:

`35581515460`

Result:

- Linux: success
- macOS: success
- Windows: success
- Windows includes the repository's .NET Framework 4.8 test target, exercising the netstandard2.0 compatibility surface.

A fresh Linux/macOS/Windows CI run is required after this completion-documentation change and before PR #10 is merged. The PR checks are the authoritative final pre-merge evidence.

## I/O and semantic execution decisions

Phase 5 reuses the established direct-process/version-probe execution model.

- `PgMaintenanceIo` carries optional caller-owned readable stdin and writable stdout streams without forcing text buffering into the public result.
- Interactive-capable tools may receive stdin without PgCliSharp inventing prompt semantics.
- `pg_isready` executes with ordinary nonzero throwing disabled so exit codes 1-3 remain observable domain statuses.
- `pg_isready` exit codes outside 0-3 still surface as `PgProcessExecutionException`.
- `pg_amcheck --install-missing[=SCHEMA]` is represented by a typed optional-argument value and the compatibility-spec schema explicitly supports `argumentMode: optional`.

These choices are applications of existing Accepted ADRs and do not require a new repository-wide ADR.

## Publication safeguard

ADR-0012 remains unchanged.

Both publication manifests remain disabled:

- `.github/nuget-release.json`: `publication_enabled: false`
- `.github/phase-release.json`: `publication_enabled: false`

Phase 5 completion does not create:

- a NuGet push;
- a Phase 5 tag;
- a GitHub Release;
- Release assets.

The prepared Phase 3 publication source remains `cccf8d9fbe1f2e1104676ab94a7863209c0220dd` and stays deferred until the final release phase revalidation.

## Completion sequence

1. Final Phase 5 completion documentation is committed to PR #10.
2. That exact PR head must pass Linux/macOS/Windows CI.
3. PR #10 is marked ready and merged to `main`.
4. The exact merge commit on `main` must pass Linux/macOS/Windows CI.
5. Direct GitHub links to the PR, merge commit, and CI evidence are recorded in the PR completion report.

## Final reviewed merge evidence

The later repository evidence index confirms the final Phase 5 completion gate:

- PR: #10
- final PR head: `46b715c7ae0428cb96c650d68898d2d67e898965`
- final PR CI: https://github.com/ShutenOishi/pgcli-sharp/actions/runs/35581853788 — Linux/macOS/Windows success
- `main` merge commit: `0a27f5909e18a9b346a47fde2fc11e0f051cb585`
- exact-main CI: https://github.com/ShutenOishi/pgcli-sharp/actions/runs/35582085056 — Linux/macOS/Windows success

These records supplement the pre-documentation implementation head above; they do not replace or rewrite it.
