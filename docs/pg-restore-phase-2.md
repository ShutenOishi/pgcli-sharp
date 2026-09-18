# Phase 2 Research - pg_restore

Status: research/specification and implementation complete; the Phase 2 cross-platform completion gate has passed under ADR-0011.

## Scope and evidence

PostgreSQL 10 through 18 were checked independently against the official `pg_restore` application documentation and `REL_10_STABLE` through `REL_18_STABLE` `src/bin/pg_dump/pg_restore.c`. Security-backported `--restrict-key` availability was checked against CVE-2025-8714 and current maintenance documentation.

The machine-readable source of truth is [`spec/postgresql/pg_restore.json`](../spec/postgresql/pg_restore.json).

## Findings that affect API shape

- Archive input is a positional archive file/directory or stdin, not an ordinary option. PgCliSharp should model it with a dedicated `PgRestoreInput`.
- Restore has two materially different output/execution modes: direct database restore with `--dbname`, and generated SQL/list output with `--file`. They should be mutually exclusive in a dedicated output abstraction.
- PostgreSQL 10-11 permit generated script stdout when both `-d` and `-f` are omitted. PostgreSQL 12+ hard-requires one of them for normal restore (except `--list`). PgCliSharp can provide stable cross-version behavior by emitting `--file=-` when a caller selects stream/stdout output.
- `--no-reconnect` is deliberately accepted as a no-op. The wrapper must not reject it merely because it has no effect.
- Parallel restore is effective only for custom/directory archives and requires a regular file/directory input. It is documented as ignored when generating a script, so the wrapper must not over-reject that accepted ignored case.
- `--single-transaction` conflicts with multiple jobs across the supported range. The explicit `--create` + `--single-transaction` parser error appears from PostgreSQL 12 onward and must not be projected backward.
- PostgreSQL 17 adds repeatable `--filter` and `--transaction-size`; filter `-` consumes stdin. Archive stdin and filter stdin therefore cannot both be represented simultaneously in one process invocation.
- PostgreSQL 18 adds data/schema/statistics include/exclude modes with explicit hard conflicts.
- Format parsing becomes stricter in PostgreSQL 18, but a typed enum emitting canonical values avoids exposing historical parser quirks.

## Security backport

CVE-2025-8714 affected `pg_restore` when it generates a plain SQL script. Current historical-major documentation therefore contains `--restrict-key` even though older maintenance executables did not. Minimum executable versions are 13.22, 14.19, 15.14, 16.10, 17.6, and 18.0.

CVE-2026-18408 is a follow-up psql restriction issue. It does not change the first availability boundary of the `--restrict-key` option, but it is recorded in the spec because safe plain-script restoration depends on the patched toolchain.

## Connection semantics

`--host`, `--port`, `--username`, password prompting, and `--role` are similar to pg_dump, but `PGDATABASE` does not switch pg_restore into direct database mode when `--dbname` is omitted. Public options remain tool-specific. Any internal sharing must preserve that distinction.

## Inventory audit

The current stable-branch option-table counts (including long aliases but excluding pre-getopt utility handling) are:

| Major | Long-option entries |
| ---: | ---: |
| 10 | 41 |
| 11 | 42 |
| 12 | 42 |
| 13 | 43 |
| 14 | 43 |
| 15 | 44 |
| 16 | 44 |
| 17 | 46 |
| 18 | 52 |

All entries are represented in the spec, with `--help` and `--version` modeled as explicit utility/version bindings.

## Implementation audit

- `PgRestoreOptions` uses typed content/mode/transaction models instead of contradictory switch pairs.
- `PgRestoreInput` separates file, directory, and archive-stdin input; `PgRestoreOutput` separates direct database restore from generated SQL/list output.
- Runtime availability is centralized, including exact `--restrict-key` maintenance-release boundaries.
- Validation covers upstream hard errors and preserves documented ignored cases such as parallel options during script generation.
- Argument generation is deterministic, repeatable selectors preserve caller order, and stream output emits `--file=-`.
- Execution forwards archive stdin or filter stdin as a stream, preserves stdout as bytes, and reuses the Phase 0/1 cancellation, timeout, process-tree, stderr, and version-probe infrastructure.
- Tests cover major/patch availability, argument generation, invalid combinations, ignored-vs-error behavior, binary-safe I/O, execution control, spec/API synchronization, and pg_dump -> pg_restore archive contracts.
