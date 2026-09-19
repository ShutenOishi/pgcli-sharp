# PostgreSQL CLI compatibility specifications

This directory contains machine-readable compatibility research used by PgCliSharp.

New tool implementations follow [ADR-0011](../../docs/adr/0011-specification-first-tool-implementation.md) and [the tool implementation workflow](../../docs/tool-implementation-workflow.md).

## pg_dump

[`pg_dump.json`](pg_dump.json) is the Phase 1 canonical compatibility inventory for PostgreSQL 10 through 18.

It records:

- official per-major documentation URLs;
- the resolved option inventory for each major;
- short/long spellings and aliases;
- option argument/value kinds;
- repeatability;
- major and patch-level availability;
- defaults and constraints;
- archive/output/compression behavior;
- connection/environment relationships;
- intended typed .NET binding;
- validation rules and version-specific semantic changes.

Do not infer historical support by applying the newest PostgreSQL option list to old versions. Update the per-major source comparison first.

A notable example is `--restrict-key`: current historical-major documentation contains it because it was security-backported. Its minimum executable versions are therefore recorded per major instead of as a simple major-only `since` value.

Human-readable research and API planning are in [`docs/pg-dump-phase-1.md`](../../docs/pg-dump-phase-1.md).

## Specification conventions for new tools

Create one top-level JSON file per PostgreSQL CLI tool, for example:

```text
spec/postgresql/pg_restore.json
spec/postgresql/pg_dumpall.json
```

Every tool specification should retain the same minimum machine-checkable concepts established by Phase 1:

- `schemaVersion` and `tool`;
- `scope.postgresqlMajors` containing PostgreSQL 10 through 18;
- `sources.perMajorDocumentation` for every supported major;
- a unique `options[].id`;
- option spellings/aliases;
- explicit `argumentMode`;
- boolean `repeatable`;
- availability metadata;
- explicit wrapper/upstream defaults;
- an `api.property` or explicit `api.binding`;
- `versions.<major>.optionIdsInCurrentMajorDocumentation` for each supported major.

Tool-specific sections for formats, compression, filter files, environment variables, or I/O should be added when relevant rather than forced into tools that do not have those concepts.

Run:

```bash
bash eng/validate-compatibility-specs.sh
```

to validate the repository-local structural invariants.

The structural validator intentionally does not fetch PostgreSQL source from the network. Upstream option-table/parser comparison is performed during research and its result is recorded in the specification so ordinary CI remains deterministic.


## Phase 2 tools

[`pg_restore.json`](pg_restore.json) and [`pg_dumpall.json`](pg_dumpall.json) are the Phase 2 compatibility inventories for PostgreSQL 10 through 18.

Both specifications record per-major source-table audits, exact executable-version boundaries for the security-backported `--restrict-key`, input/output semantics, ignored-versus-error behavior, and intended API bindings. Human-readable research is stored in [`docs/pg-restore-phase-2.md`](../../docs/pg-restore-phase-2.md) and [`docs/pg-dumpall-phase-2.md`](../../docs/pg-dumpall-phase-2.md).

## Phase 4 tools

Phase 4 adds machine-readable inventories for `pg_basebackup`, `pg_receivewal`, `pg_recvlogical`, `pg_verifybackup`, and `pg_combinebackup`. The specs explicitly record tool-level availability (pg_verifybackup 13+, pg_combinebackup 17+), per-major option tables, source-audit counts, and intended typed bindings. Human-readable research is in [`docs/backup-wal-phase-4.md`](../../docs/backup-wal-phase-4.md).
