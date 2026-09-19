# Phase 5 Research - Database management and maintenance tools

Status: research/specification baseline complete; implementation follows ADR-0011.

## Scope

Phase 5 covers the nine client-side database management/maintenance tools named by the current roadmap:

- `createdb`
- `dropdb`
- `createuser`
- `dropuser`
- `vacuumdb`
- `reindexdb`
- `clusterdb`
- `pg_isready`
- `pg_amcheck`

The first eight exist throughout PostgreSQL 10-18. `pg_amcheck` was introduced in PostgreSQL 14 and is rejected before process startup for PostgreSQL 10-13.

Phase 6 retains `psql` and `pgbench`; Phase 7 retains server/data-directory applications. This phase does not pull either category forward.

## Evidence

Every supported major was inspected independently using official PostgreSQL application documentation and the corresponding stable source branch.

Upstream source paths:

- `src/bin/scripts/createdb.c`
- `src/bin/scripts/dropdb.c`
- `src/bin/scripts/createuser.c`
- `src/bin/scripts/dropuser.c`
- `src/bin/scripts/vacuumdb.c`
- `src/bin/scripts/reindexdb.c`
- `src/bin/scripts/clusterdb.c`
- `src/bin/scripts/pg_isready.c`
- `src/bin/pg_amcheck/pg_amcheck.c`

Machine-readable inventories are maintained in `spec/postgresql/<tool>.json`.

## Version boundaries

### createdb

- PostgreSQL 15 adds `--strategy`, `--locale-provider`, and `--icu-locale`.
- PostgreSQL 16 adds `--icu-rules`.
- PostgreSQL 17 adds `--builtin-locale`.
- Positional syntax remains optional database name plus optional description.

### dropdb

- PostgreSQL 13 adds `--force`.
- Database name is required.
- `--if-exists` is implemented through a long-option flag entry upstream and must remain part of the inventory even though it does not use a normal switch case.

### createuser

- PostgreSQL 10-12 retain deprecated `--adduser` / `--no-adduser` aliases for superuser state.
- PostgreSQL 13 removes those aliases.
- PostgreSQL 16 adds `--with-admin`, `--member-of`, `--with-member`, `--valid-until`, and BYPASSRLS controls.
- `--role` remains an alias in newer versions; the wrapper emits the version-appropriate canonical membership spelling.
- Mutually opposed flags such as createdb/no-createdb are modeled as nullable booleans instead of contradictory public properties.

### dropuser

- Option inventory is stable from PostgreSQL 10-18.
- Role name is required unless upstream interactive prompting is explicitly requested.

### vacuumdb

- PostgreSQL 12 adds page-skipping, skip-locked, and transaction-age controls.
- PostgreSQL 13 adds `--parallel`.
- PostgreSQL 14 adds index-cleanup, truncate, and toast-processing controls.
- PostgreSQL 16 adds schema filters, `--no-process-main`, and `--buffer-usage-limit`.
- PostgreSQL 18 adds `--missing-stats-only`.
- Analyze-only mode has hard conflicts with full/freeze/page skipping/index cleanup/truncate/process-main/process-toast and parallel vacuum.
- Specific tables and schema-wide selection are mutually exclusive in the upstream parser.

### reindexdb

- PostgreSQL 12 adds `--concurrently`.
- PostgreSQL 13 adds `--jobs`.
- PostgreSQL 14 adds `--tablespace`.
- Reindex-all conflicts with a specific database; multiple jobs cannot target system catalogs.

### clusterdb

The option inventory is stable from PostgreSQL 10-18. All-databases mode conflicts with a specific database.

### pg_isready

The option inventory is stable from PostgreSQL 10-18. Exit status is semantic and must not be treated as an ordinary nonzero process failure:

- 0: accepting connections
- 1: rejecting connections
- 2: no response
- 3: no attempt

`--timeout=0` explicitly disables the connection timeout and is therefore valid.

### pg_amcheck

- Tool availability starts at PostgreSQL 14.
- PostgreSQL 17 adds `--checkunique`.
- `--skip` accepts `none`, `all-visible`, or `all-frozen`.
- start/end block values are unsigned block numbers and end cannot precede start.
- `--all` conflicts with a positional database name; a positional database name conflicts with database include/exclude pattern selection.
- `--install-missing[=SCHEMA]` is the first PgCliSharp compatibility-spec entry with an optional option argument. The structural spec validator is extended to represent that upstream shape.

## API and execution implications

- Every tool has its own public Options class.
- Identical connection argument serialization is shared internally only after the source audit confirmed matching semantics.
- Finite states use enums/value objects, including createdb strategy/provider, vacuum index-cleanup mode, pg_amcheck skip mode, and optional install-missing state.
- Ordered collections preserve repeatable pattern/table/schema/index option order.
- Interactive-capable scripts can receive a caller-owned standard-input stream; command output can be streamed to a caller-owned output stream without whole-output buffering.
- Executable version probing remains mandatory before normal execution.
- pg_isready uses a typed status result and runs with nonzero-exit throwing disabled so status 1-3 remain observable outcomes.
- Server-version checks performed after connection remain upstream runtime checks because selected CLI executable version and connected server version are deliberately separate concepts under ADR-0002.

## Completion audit

Phase 5 completion requires:

1. all nine specs to pass structural validation;
2. every inventory entry to resolve to a public property or explicit special binding;
3. every version-varying option and whole-tool boundary to be covered by centralized runtime availability metadata;
4. deterministic argument tests for repeatable options, aliases, positional arguments, and optional-argument serialization;
5. hard upstream combination/value checks to fail before normal process startup where they can be determined from caller input;
6. execution tests for version mismatch, timeout/cancellation, environment forwarding, text output streaming, interactive stdin forwarding, and pg_isready semantic exit codes;
7. Windows net48 compilation/tests plus Linux/macOS modern-target tests;
8. final documentation and completion evidence followed by a fresh all-platform CI run;
9. publication manifests remaining disabled under ADR-0012.
