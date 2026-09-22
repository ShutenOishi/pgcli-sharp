# Phase 6 Research - psql and pgbench rich I/O

Phase 6 implements the roadmap's rich-I/O clients `psql` and `pgbench` under ADR-0011 and the new ADR-0013.

## Scope

- `psql`: PostgreSQL 10-18
- `pgbench`: PostgreSQL 10-18
- explicit executable path and executable-major verification remain mandatory
- publication stays disabled under ADR-0012

Canonical inventories:

- `spec/postgresql/psql.json`
- `spec/postgresql/pgbench.json`

## psql findings

The official psql synopsis remains `psql [option...] [dbname [username]]` across the supported family.

The upstream option-table audit found one long-option addition across PostgreSQL 10-18:

- PostgreSQL 12 adds `--csv`.

Important execution semantics:

- `--command` and `--file` are repeatable and can be interleaved; psql processes them in command-line order.
- Once either action form is used, normal stdin command reading is disabled, except that `--file=-` explicitly reads stdin.
- `--single-transaction` is meaningful only with the non-interactive action path.
- `--set` / `--variable` distinguish an unset variable (`name`) from an empty value (`name=`), so a plain dictionary is insufficient for exact modeling.
- psql decides whether it is interactive using terminal detection. Redirected pipes are therefore not equivalent to a real TTY and must not be marketed as PTY emulation.
- documented exit statuses are 0 (normal), 1 (fatal psql error), 2 (bad connection in non-interactive mode), and 3 (script error with ON_ERROR_STOP).

API implications:

- one ordered `PsqlAction` collection represents command/file sequencing;
- `PsqlVariableAssignment` preserves unset versus empty values;
- literal and zero-byte field/record separators use a typed separator value;
- output formatting uses a finite typed mode, with CSV rejected before PostgreSQL 12;
- finite batch execution and a long-lived redirected duplex session are separate APIs.

## pgbench findings

The upstream option table changes materially across the support range.

### PostgreSQL 11

Adds:

- `--init-steps`
- `--random-seed`

### PostgreSQL 13

Adds:

- `--show-script`
- `--partitions`
- `--partition-method`

The `--init-steps` option itself exists from PostgreSQL 11, but its accepted step alphabet changes: PostgreSQL 11-12 support `d/t/g/v/p/f`, while server-side generation `G` is available from PostgreSQL 13.

### PostgreSQL 15

Changes:

- `--report-latencies` becomes `--report-per-command` (the `-r` short form remains);
- adds `--failures-detailed`;
- adds `--max-tries`;
- adds `--verbose-errors`.

### PostgreSQL 17

Changes:

- adds `--dbname` / `-d`;
- `-d` therefore stops being the short form for debug, while `--debug` remains;
- adds `--exit-on-abort`.

The wrapper can use the positional database form across PostgreSQL 10-18, avoiding needless spelling variation for the ordinary typed Database property. It uses the stable long form `--debug` so the PostgreSQL 17 short-option reassignment cannot create ambiguity.

pgbench supports weighted builtin/file scripts and repeatable variable definitions. Individual script weight 0 is legal and means that script is ignored; negative weights and an explicitly configured script set whose total selectable weight is zero are rejected. The upstream script-set limit of 128 is also validated. It also has structured values worth typing, especially protocol, partition method, initialization steps, and random seed (`time`, `rand`, or an unsigned integer). PostgreSQL 11-17 parse a numeric seed through C `unsigned long`, making the practical upper bound ABI/platform-dependent; PostgreSQL 18 changes to explicit 64-bit unsigned parsing. PgCliSharp keeps the documented unsigned-integer model and records that historical implementation detail rather than imposing an unnecessarily narrow cap on every platform.

The source-level parser audit also establishes hard pre-execution constraints: initialization-only and benchmarking-only option groups cannot cross modes; sampling/aggregation/log-prefix depend on transaction logging; sampling and aggregation conflict; timestamped progress requires progress; partition method requires a positive partition count; aggregation must divide a configured duration without exceeding it; and unlimited retries require either a latency limit or duration. `--select-only` and `--skip-some-updates` are not mutually exclusive: upstream adds both as builtin scripts when both are specified.

pgbench exit-status behavior also has a historical boundary. PostgreSQL 10-11 use success/status-1 failure behavior, while PostgreSQL 12 introduces the explicit runtime-error exit path and documents status 2 for errors during the benchmark or script execution. Phase 6 therefore accepts status 2 as a typed `RuntimeError` only for PostgreSQL 12+; status 2 from a PostgreSQL 10-11 executable is treated as an unexpected process failure.

## I/O design

The existing one-shot runner remains suitable for finite psql scripts and pgbench. Phase 6 exposes tool-specific public I/O models rather than reusing the Phase 5 maintenance type: `PsqlIo` carries optional stdin/stdout/stderr for finite psql runs, while `PgBenchIo` deliberately exposes stdout/stderr only because the audited pgbench CLI has no stdin workload contract. Internally both still use the established process runner.

It is not sufficient for a caller that needs to write commands after psql has started. ADR-0013 therefore adds a separate long-lived redirected-process abstraction. The session must allow writes, explicit EOF, streamed stdout/stderr, cancellation, timeout, and completion metadata.

A redirected psql session is intentionally documented as non-TTY. Native PTY/terminal emulation is not included in the initial Phase 6 contract. `PsqlSessionIo` carries caller-owned stdout/stderr destinations while the running `PsqlSession` exposes its writable stdin stream directly.

## Audit status

Completed across research, implementation, and final compatibility review:

- official documentation URLs recorded for every PostgreSQL major 10-18;
- stable-branch source option tables compared for both tools;
- psql PostgreSQL 12 CSV boundary recorded;
- pgbench PostgreSQL 11/13/15/17 deltas recorded;
- psql ordered action and variable-assignment semantics recorded;
- tool-specific exit-status domains recorded;
- rich I/O design separated from the one-shot process API;
- redirected psql session lifecycle validated on modern .NET and Windows .NET Framework 4.8 through the netstandard2.0/CliWrap backend;
- caller-owned stderr streaming validated for finite rich-I/O execution;
- pgbench mode/logging/progress/partition/retry constraints, script weights, initialization-step value boundaries, and historical exit-status behavior incorporated into the maintained specification.

The implementation mirrors the audited exact value ranges and upstream-determinable hard conflicts described above. Boundaries discovered during implementation are recorded in the specifications and regression tests.
