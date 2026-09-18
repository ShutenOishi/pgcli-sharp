# Phase 1: pg_dump research and implementation plan

This document records the Phase 1 research conclusions and implementation plan. The machine-readable source of truth for option compatibility is [`spec/postgresql/pg_dump.json`](../spec/postgresql/pg_dump.json).

## Scope and source policy

Phase 1 targets `pg_dump` from PostgreSQL 10 through PostgreSQL 18 inclusive.

The inventory was built by comparing the official `pg_dump` page for every major version independently:

- PostgreSQL 10: https://www.postgresql.org/docs/10/app-pgdump.html
- PostgreSQL 11: https://www.postgresql.org/docs/11/app-pgdump.html
- PostgreSQL 12: https://www.postgresql.org/docs/12/app-pgdump.html
- PostgreSQL 13: https://www.postgresql.org/docs/13/app-pgdump.html
- PostgreSQL 14: https://www.postgresql.org/docs/14/app-pgdump.html
- PostgreSQL 15: https://www.postgresql.org/docs/15/app-pgdump.html
- PostgreSQL 16: https://www.postgresql.org/docs/16/app-pgdump.html
- PostgreSQL 17: https://www.postgresql.org/docs/17/app-pgdump.html
- PostgreSQL 18: https://www.postgresql.org/docs/18/app-pgdump.html

The PostgreSQL security notice for CVE-2025-8714 is additionally authoritative for the patch-level introduction of `--restrict-key`:
https://www.postgresql.org/support/security/CVE-2025-8714/

The later CVE-2026-18408 notice is retained as a behavioral/security-history note for the generated restricted-mode dump scripts:
https://www.postgresql.org/support/security/CVE-2026-18408/

The versioned PostgreSQL documentation is a living representation of the current maintenance state of each branch. Therefore, an option visible on a historical major's current documentation page is not automatically assumed to have existed in `.0`. The `--restrict-key` backport is the concrete Phase 1 case that requires patch-level metadata.

## Inventory verification

After the documentation comparison, the resolved long-option sets were mechanically compared with the official PostgreSQL source branches (`REL_10_STABLE` through `REL_18_STABLE`), specifically the `long_options[]` table in `src/bin/pg_dump/pg_dump.c`.

After applying spelling-level availability for the large-object aliases, the specification exactly matches the upstream long-option set for every supported major:

| PostgreSQL | Distinct long options |
|---|---:|
| 10 | 54 |
| 11 | 56 |
| 12 | 58 |
| 13 | 60 |
| 14 | 62 |
| 15 | 62 |
| 16 | 67 |
| 17 | 70 |
| 18 | 77 |

An important distinction is that the large-object *feature* exists throughout the supported range, while the canonical `--large-objects` / `--no-large-objects` spellings appear from PostgreSQL 16. PostgreSQL 10-15 use `--blobs` / `--no-blobs`. PostgreSQL 16+ retain those older names as compatibility aliases.

## Compatibility findings

The PgCliSharp-supported union currently contains 75 inventory entries, including normal dump options, connection options, and the `--help`/`--version` utility commands.

The major-version boundaries that materially affect the typed model are:

| PostgreSQL | pg_dump compatibility change relevant to PgCliSharp |
|---|---|
| 10 | Baseline. `--compress` is an integer level `0..9`; `--oids` and `--no-synchronized-snapshots` exist. |
| 11 | Adds `--load-via-partition-root` and `--no-comments`. |
| 12 | Removes `--oids`; adds `--extra-float-digits`, `--on-conflict-do-nothing`, `--rows-per-insert`; documents `PG_COLOR`. |
| 13 | Adds `--include-foreign-data`. Current branch documentation also exposes `--restrict-key`, but only binaries 13.22+ have it. |
| 14 | Adds `-e/--extension` and `--no-toast-compression`. `--restrict-key` requires 14.19+. |
| 15 | Adds `--no-table-access-method`; removes `--no-synchronized-snapshots`; documented minimum dumpable server becomes 9.2; `--no-unlogged-table-data` now covers unlogged sequences as well as tables. `--restrict-key` requires 15.14+. |
| 16 | Compression becomes `level` or `method[:detail]` with `gzip/lz4/zstd/none`; adds table-and-children selectors; `--large-objects` names become canonical and `--blobs` aliases become deprecated. `--restrict-key` requires 16.10+. |
| 17 | Adds `--exclude-extension`, repeatable `--filter` (including `-` for stdin), and `--sync-method`. `--restrict-key` requires 17.6+. |
| 18 | Adds data/schema/statistics inclusion and exclusion controls including `--statistics`, `--statistics-only`, `--sequence-data`, `--no-data`, `--no-schema`, `--no-statistics`, and `--no-policies`. Statistics also change the meaning of the data/post-data sections. |

### Repeated verbose option semantics

`-v/--verbose` is accepted repeatedly throughout PostgreSQL 10-18, but its effect changed:

- PostgreSQL 10-11: each occurrence only sets the same boolean verbose flag; occurrences after the first are redundant.
- PostgreSQL 12-13: each occurrence selects INFO logging; occurrences after the first are redundant.
- PostgreSQL 14-18: pg_dump calls `pg_logging_increase_verbosity()`, so repeated occurrences increase logging verbosity.

`PgDumpOptions.Verbosity` therefore remains a non-negative occurrence count. This preserves exact CLI expressiveness while documenting that values greater than one only have additional semantic effect from PostgreSQL 14 onward.

### Patch-level availability

`--restrict-key` was introduced by the CVE-2025-8714 fix, not by one ordinary major-version boundary:

- 13.22+
- 14.19+
- 15.14+
- 16.10+
- 17.6+
- 18.0+

For this reason, Phase 1 availability metadata must be able to express a minimum numeric executable version per major. Execution already probes the real executable's `--version`; Phase 1 validation will use that parsed numeric version for options with patch-level requirements.

### Formats and output

The public API must preserve these distinctions:

- `plain` is the default SQL-script format.
- `custom` is an archive format and is compressed by default.
- `directory` is an archive directory, is compressed by default, is the only format supporting parallel dump, and requires an output directory supplied via `--file`.
- `tar` is an archive format and does not support compression.
- For plain/custom/tar, omitting `--file` sends the dump to stdout.
- stdout is not inherently text. Custom/tar output is binary, and a compressed plain dump is compressed bytes.
- Core execution must therefore stream stdout as bytes and must not buffer/decode the complete payload.
- PostgreSQL 17+ allows `--filter=-`; this consumes stdin. Full typed coverage therefore requires stdin streaming in the process backend as well as stdout streaming.

### Compression model

A single integer property is not sufficient.

For PostgreSQL 10-15:

- syntax is `-Z 0..9`;
- zero means no compression;
- custom/directory formats compress by default;
- plain output defaults to no compression;
- tar does not support compression.

For PostgreSQL 16-18:

- syntax is `level` or `method[:detail]`;
- methods are `gzip`, `lz4`, `zstd`, and `none`;
- details may be an integer level or keyword items; documented keywords are `level` and `long`;
- level-only syntax retains the gzip/non-compressed shorthand;
- `long` is meaningful for zstd;
- tar still does not support compression.

The public model will use `PgDumpCompression`, not `int?` or an arbitrary command-line string.

The common PostgreSQL compression parser also recognizes a `workers` detail, but current pg_dump source explicitly warns that compression workers are not supported by pg_dump. PgCliSharp therefore does not expose a typed pg_dump worker-count setting. For supported method details, gzip accepts its default level or 1-9, LZ4 accepts 0-12, and zstd's exact numeric bounds are linked-library-dependent; the wrapper validates the stable method constraints and leaves the build-dependent zstd bound to the executable.

### Connection and environment behavior

Typed connection properties cover:

- database / connection string (`-d/--dbname`);
- host;
- port;
- user;
- password-prompt policy (`-w` versus `-W`);
- role.

A database connection string can itself override conflicting command-line connection settings; PgCliSharp does not attempt to rewrite libpq's resolution rules.

Direct pg_dump defaults come from `PGDATABASE`, `PGHOST`, `PGOPTIONS`, `PGPORT`, and `PGUSER`. `PGCLIENTENCODING` is an alternative to `--encoding`. `PG_COLOR` is documented from PostgreSQL 12 and accepts `always`, `auto`, or `never`. pg_dump also consumes the environment variables supported by libpq.

PgCliSharp will not add a password command-line value because pg_dump itself has no such option. Authentication secrets should remain in libpq-supported mechanisms such as password files or caller-controlled environment/configuration.

## Public API design

The intended public surface is:

- `PgDump`: executable-bound runner. Constructor receives explicit executable path and expected PostgreSQL major version.
- `PgDumpOptions`: one union options type for PostgreSQL 10-18.
- `PgDumpFormat`: Plain, Custom, Directory, Tar.
- `PgDumpSection`: PreData, Data, PostData.
- `PgDumpCompressionMethod`: Gzip, Lz4, Zstd, None.
- `PgDumpCompression`: structured compression value object.
- `PgDumpSyncMethod`: Fsync, Syncfs.
- `PgDumpLargeObjectMode`: Default, Include, Exclude, avoiding contradictory `-b/-B` booleans.
- `PgPasswordPromptMode`: Default, NeverPrompt, ForcePrompt, avoiding contradictory `-w/-W` booleans.
- `PgDumpRestrictKey`: validated non-empty alphanumeric value.
- `PgDumpFilterSource`: file versus stdin source, instead of using the magic filename `-` as an untyped escape.
- `PgDumpOutput`: stdout stream, file path, or directory path.
- `PgDumpResult`: exit code, duration, actual executable version, and original PostgreSQL stderr; it never forces the dump payload into a string/byte array.

All public types and members receive English-first/Japanese-second XML documentation.

## Internal design

### Compatibility metadata

Runtime availability is represented by `PgDumpOptionAvailabilityCatalog`. Each version-varying option records:

- first supported PostgreSQL major;
- last supported PostgreSQL major;
- optional minimum exact executable versions by major.

The validator consumes this catalog rather than maintaining a second independent set of version constants. The machine-readable JSON specification remains the durable cross-agent reference and is separately tested/reviewed against the runtime catalog.

### Argument generation

Argument generation is deterministic and shell-free.

Rules:

1. emit long options internally for stable readable tests/diagnostics unless a short form is specifically needed;
2. preserve insertion order within repeatable collections;
3. emit one option occurrence per collection element;
4. never concatenate an untyped raw argument tail;
5. serialize `TimeSpan` lock timeouts as integer milliseconds because all target PostgreSQL server versions accept that form;
6. use the canonical large-object names appropriate for the executable version;
7. use version-specific compression serialization;
8. redact secret-bearing environment information from diagnostics.

### Validation order

Before the dump process starts:

1. validate the executable path and actual executable version;
2. validate option availability using the actual numeric executable version where required;
3. validate value-object invariants;
4. validate incompatible/dependent options;
5. validate output-format/output-target constraints;
6. generate arguments;
7. execute with binary-safe streams.

Important hard errors include:

- `--jobs` outside directory format;
- compression with tar;
- directory format without a directory output target;
- stdout output with directory format;
- `--if-exists` without `--clean`;
- `--on-conflict-do-nothing` without an INSERT-producing option;
- non-positive `--rows-per-insert`;
- `--restrict-key` outside plain format;
- unsupported option/version combinations;
- PostgreSQL 18 mutually-exclusive only/no modes.

Options that PostgreSQL documents as merely ignored for archive output are retained as accepted/no-op combinations rather than incorrectly rejected.

## Test plan

Phase 1 tests will include:

- a PostgreSQL 10-18 availability matrix;
- patch-level `--restrict-key` boundaries;
- deterministic argument generation for every modeled option;
- repeatable option ordering/count;
- compression serialization before and after PostgreSQL 16;
- format/output constraints;
- incompatible/dependent combinations;
- filter stdin behavior on PostgreSQL 17+;
- byte-for-byte binary stdout;
- cancellation and timeout through pg_dump execution;
- executable major-version mismatch before dump execution;
- localized validation errors with culture-independent structured properties;
- specification/runtime completeness checks.

Executable integration tests should use real pg_dump binaries where the CI environment makes that reproducible. Unit compatibility tests must not depend on having every historical binary installed.

## Phase-completion boundary

Phase 1 is **not complete** merely because this research/specification lands.

Until all implementation and required tests are complete:

- do not change `.github/phase-release.json` to Phase 1;
- do not create the `phase-1` GitHub Release;
- do not claim Phase 1 complete.

At actual completion, the existing Phase Release workflow will be used with release notes ordered `## English` then `## 日本語`, and the source ZIP, `.nupkg`, and `.snupkg` artifacts will be recorded.
