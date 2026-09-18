# Phase 2 Research - pg_dumpall

Status: research/specification and implementation complete; completion-gate validation is in progress under ADR-0011.

## Scope and evidence

PostgreSQL 10 through 18 were checked independently against the official `pg_dumpall` application documentation and `REL_10_STABLE` through `REL_18_STABLE` `src/bin/pg_dump/pg_dumpall.c`. Security-backported `--restrict-key` availability was checked against CVE-2025-8714 and current maintenance documentation.

The machine-readable source of truth is [`spec/postgresql/pg_dumpall.json`](../spec/postgresql/pg_dumpall.json).

## Findings that affect API shape

- pg_dumpall always creates one SQL script. Upstream writes to stdout unless `--file` is supplied.
- pg_dumpall invokes a sibling `pg_dump` for each database and checks that it is the same PostgreSQL version. This is an upstream invariant distinct from PgCliSharp's pg_dumpall executable probe.
- `-d/--dbname` is a libpq connection string. Its database-name component is ignored because pg_dumpall connects to many databases.
- `-l/--database` separately chooses the initial database used for global objects and database discovery; when absent, upstream tries `postgres` and then `template1`.
- These semantics differ materially from pg_dump/pg_restore `--dbname`, so no public common connection-options base is appropriate.
- `--globals-only`, `--roles-only`, and `--tablespaces-only` are mutually exclusive. The public API should use one `PgDumpAllScope` enum.
- PostgreSQL 12 removes `--oids` and adds database exclusion plus several nested-pg_dump output options.
- PostgreSQL 17 adds repeatable `--filter`; its only supported filter grammar is `exclude database PATTERN`, and `-` consumes stdin.
- PostgreSQL 18 adds the statistics/data/schema controls also present in pg_dump, plus `--sequence-data`.
- `--no-acl` aliases `--no-privileges`; `--attribute-inserts` aliases `--column-inserts`. PgCliSharp should emit canonical spellings.

## Security backport

CVE-2025-8714 explicitly affects pg_dumpall. `--restrict-key` first exists in 13.22, 14.19, 15.14, 16.10, 17.6, and 18.0. Major-only availability would therefore be incorrect.

CVE-2026-18408 is recorded as a follow-up security notice affecting plain dump restoration. It does not move the option's first-availability boundary.

## Inventory audit

The current stable-branch `long_options[]` entry counts (including aliases) are:

| Major | Long-option entries |
| ---: | ---: |
| 10 | 38 |
| 11 | 41 |
| 12 | 44 |
| 13 | 45 |
| 14 | 46 |
| 15 | 47 |
| 16 | 47 |
| 17 | 48 |
| 18 | 55 |

All entries are represented in the spec, with `--help` and `--version` modeled separately.

## Implementation audit

- `PgDumpAllOptions` remains tool-specific; `PgDumpAllScope` represents globals/roles/tablespaces-only as one mutually exclusive state.
- `ConnectionString` and `InitialDatabase` preserve the distinct upstream meanings of `--dbname` and `--database`.
- Runtime availability is centralized for PostgreSQL 10-18, including removal of `--oids` and exact security-backport boundaries for `--restrict-key`.
- Validation covers hard scope/dependency/value conflicts while avoiding errors for options that upstream ignores outside database-dump scope.
- Argument generation emits canonical alias spellings and preserves repeatable database-exclusion/filter order.
- SQL-script stdout and filter stdin remain byte streams; file output is delegated to pg_dumpall through `--file`.
- Tests cover version/patch boundaries, argument generation, scope and PostgreSQL 18 content semantics, execution/I/O, spec/API synchronization, and backup/restore trio relationships. The sibling `pg_dump` version check remains an upstream pg_dumpall invariant and surfaces through its normal non-zero process result.
