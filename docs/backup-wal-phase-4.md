# Phase 4 Research - Backup and WAL tools

Status: research/specification baseline complete; implementation follows ADR-0011.

## Scope

Phase 4 covers all Backup/WAL tools named by the roadmap:

- `pg_basebackup` — PostgreSQL 10-18
- `pg_receivewal` — PostgreSQL 10-18
- `pg_recvlogical` — PostgreSQL 10-18
- `pg_verifybackup` — PostgreSQL 13-18; the executable does not exist in 10-12
- `pg_combinebackup` — PostgreSQL 17-18; the executable does not exist in 10-16

The machine-readable sources of truth are:

- `spec/postgresql/pg_basebackup.json`
- `spec/postgresql/pg_receivewal.json`
- `spec/postgresql/pg_recvlogical.json`
- `spec/postgresql/pg_verifybackup.json`
- `spec/postgresql/pg_combinebackup.json`

## Evidence and audit method

Each PostgreSQL major from 10 through 18 was treated independently. Official application documentation was compared with the corresponding `REL_<major>_STABLE` option table/parser source.

Source files:

- `src/bin/pg_basebackup/pg_basebackup.c`
- `src/bin/pg_basebackup/pg_receivewal.c`
- `src/bin/pg_basebackup/pg_recvlogical.c`
- `src/bin/pg_verifybackup/pg_verifybackup.c`
- `src/bin/pg_combinebackup/pg_combinebackup.c`

Resolved long-option table counts are stored in each spec. Missing source files for pg_verifybackup 10-12 and pg_combinebackup 10-16 are intentional tool-availability facts, not inventory gaps.

## pg_basebackup findings

The wrapper must treat the local/client destination and PostgreSQL 15+ backup targets as one output decision. PostgreSQL rejects simultaneous explicit format and backup target selection. PostgreSQL 15 added `--target` and expanded compression into client/server method syntax. PostgreSQL 17 added `--incremental` and `--sync-method`.

Hard validation preserved by the wrapper includes:

- output directory or target is required;
- `--slot` requires WAL streaming;
- `--create-slot` requires a slot name and conflicts with `--no-slot`;
- `--no-slot` conflicts with an explicit slot name;
- WAL directory is plain-format only and must be absolute upstream;
- tar-to-stdout cannot use WAL streaming;
- `--progress` conflicts with `--no-estimate-size`;
- `--no-manifest` conflicts with manifest checksum/force-encode controls;
- PostgreSQL 15+ backup targets conflict with explicit format and with recovery configuration;
- PostgreSQL 17 incremental backup requires server support and a prior backup manifest.

## pg_receivewal findings

The tool has three modes: receive WAL, create slot, and drop slot. Create and drop are mutually exclusive, and receive mode requires a target directory. Slot-management modes require a slot name.

PostgreSQL 11 added `--endpos` and `--no-sync`. Compression began as gzip level syntax and later accepts method syntax; current source still rejects zstd for pg_receivewal. `--synchronous` conflicts with `--no-sync`.

## pg_recvlogical findings

Actions are not a simple enum: create-slot and start may be combined, while drop-slot must be exclusive. A slot is required for every action. Start requires an output file/stdout target. Except for PostgreSQL 18 drop-slot, actions require a database-specific connection.

`--startpos` cannot be combined with create/drop; `--endpos` is start-only. PostgreSQL 15 added two-phase slot creation. PostgreSQL 18 added `--enable-failover` and the canonical `--enable-two-phase` spelling while retaining deprecated `--two-phase`.

## pg_verifybackup findings

pg_verifybackup was introduced in PostgreSQL 13. PostgreSQL 16 added progress reporting; `--progress` conflicts with `--quiet`.

PostgreSQL 18 added direct tar-format backup verification. WAL parsing remains plain-format only, so tar verification requires `--no-parse-wal`. pg_verifybackup also requires a same-version sibling `pg_waldump` when WAL parsing is requested; that sibling-version check remains an upstream executable invariant.

## pg_combinebackup findings

pg_combinebackup was introduced with PostgreSQL 17 incremental backup support. It requires one or more input backup directories and an output directory. The chain must begin with a full backup and subsequent backups must form a consistent incremental chain; these content-level checks remain upstream runtime checks.

Copy modes are mutually exclusive: regular copy (default), clone, copy_file_range, and PostgreSQL 18+ hard-link mode. Tablespace mappings require absolute old/new paths. A manifest cannot be generated when the final input backup has no manifest.

## Public API implications

- Each tool retains its own Options class.
- Identical cross-tool structured semantics may share value objects: LSN values, tablespace mappings, manifest checksum algorithms, sync methods, and password prompt policy.
- pg_basebackup destination/target is modeled explicitly instead of contradictory `PgData` + `Target` strings.
- pg_recvlogical start output is modeled separately so `--file=-` can map to a caller-owned stdout stream without buffering.
- pg_verifybackup and pg_combinebackup enforce executable-level tool availability before normal execution.
- Arbitrary shell command tails are not exposed.

## Completion audit requirements

Before Phase 4 is complete:

1. every spec inventory entry must resolve to a public property or explicit special binding;
2. every major-varying option must have runtime availability metadata;
3. pg_verifybackup/pg_combinebackup tool availability must be tested at the unsupported/supported boundary;
4. argument order and repeatable options must be deterministic;
5. binary/text streams must not be unnecessarily buffered;
6. cancellation, timeout, environment forwarding, version mismatch, and nonzero exit behavior must reuse the established process model;
7. all tests must pass on Linux, macOS, and Windows, including the existing .NET Framework 4.8 consumer tests;
8. external publication manifests remain disabled under ADR-0012.
