# Phase 4 Completion Evidence

Phase 4 covers the Backup/WAL roadmap scope and follows ADR-0011 specification-first implementation plus ADR-0012 deferred publication.

## Implemented tool scope

- `pg_basebackup`: PostgreSQL 10-18
- `pg_receivewal`: PostgreSQL 10-18
- `pg_recvlogical`: PostgreSQL 10-18
- `pg_verifybackup`: PostgreSQL 13-18
- `pg_combinebackup`: PostgreSQL 17-18

Unsupported whole-tool versions for pg_verifybackup and pg_combinebackup are rejected before process startup.

## Compatibility evidence

Canonical machine-readable specifications:

- `spec/postgresql/pg_basebackup.json`
- `spec/postgresql/pg_receivewal.json`
- `spec/postgresql/pg_recvlogical.json`
- `spec/postgresql/pg_verifybackup.json`
- `spec/postgresql/pg_combinebackup.json`

Research and upstream-source findings are recorded in `docs/backup-wal-phase-4.md`.

Coverage tests verify:

- every inventory entry has a public property or explicit special binding;
- version-varying inventory entries agree with runtime availability catalogs;
- whole-tool availability boundaries for pg_verifybackup and pg_combinebackup;
- deterministic argument generation and key upstream hard-error combinations;
- binary-safe pg_basebackup and pg_recvlogical stdout;
- executable-version mismatch, timeout, cancellation, and environment forwarding.

## Cross-platform implementation gate

Implementation head:

`16118b9990a4d6bd4971ec0e80357ba8bce64dfc`

GitHub Actions CI run:

`35410482613`

Result:

- Linux: success
- macOS: success
- Windows: success
- Windows includes the repository's .NET Framework 4.8 test target, exercising the netstandard2.0 compatibility surface.

A fresh Linux/macOS/Windows CI run is required after this completion-documentation change and before PR #9 is merged. The PR checks are the authoritative final pre-merge evidence.

## Publication safeguard

ADR-0012 remains unchanged.

Both publication manifests remain disabled:

- `.github/nuget-release.json`: `publication_enabled: false`
- `.github/phase-release.json`: `publication_enabled: false`

Phase 4 completion does not create or recreate:

- a NuGet push;
- `v0.1.0-alpha.1` or Phase 4 tags;
- a GitHub Release;
- Release assets;
- PR #8.

The prepared Phase 3 publication source remains `cccf8d9fbe1f2e1104676ab94a7863209c0220dd` and stays deferred until Phase 8/final release revalidation.

## Completion sequence

1. Final completion documentation is committed to PR #9.
2. That exact PR head must pass Linux/macOS/Windows CI.
3. PR #9 is merged to `main`.
4. The exact merge commit on `main` must pass Linux/macOS/Windows CI.
5. Direct GitHub links to the PR, merge commit, and CI runs are recorded in the PR completion comment and completion report.

## Final reviewed merge evidence

The later repository evidence index confirms the final Phase 4 completion gate:

- PR: #9
- final PR head: `23862f96a70d820ac048523f487cd4d0e0a87803`
- final PR CI: https://github.com/ShutenOishi/pgcli-sharp/actions/runs/35410748526 — Linux/macOS/Windows success
- `main` merge commit: `2357ead742e5703a357058d18e8e583e96bb425d`
- exact-main CI: https://github.com/ShutenOishi/pgcli-sharp/actions/runs/35410918344 — Linux/macOS/Windows success

These records supplement the pre-documentation implementation head above; they do not replace or rewrite it.
