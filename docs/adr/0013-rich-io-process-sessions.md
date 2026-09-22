# ADR-0013: Separate rich redirected sessions from one-shot process execution

- Status: Accepted
- Date: 2026-09-21
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None
- Complements: ADR-0002, ADR-0003, ADR-0008, ADR-0011

## Context

Phase 6 adds `psql` and `pgbench`. These tools expose I/O behavior that is richer than the backup and maintenance tools implemented earlier.

The existing internal process contract is intentionally one-shot: the caller can provide readable stdin and writable stdout streams, PgCliSharp starts the executable, copies the input until EOF, waits for exit, and returns execution metadata. That is appropriate for finite script/file input, but it cannot represent a caller that needs to keep stdin open and exchange commands with a running psql process.

psql also changes behavior depending on whether stdin/stdout are terminals. A process connected through redirected pipes is not equivalent to a native terminal: Readline, command history, prompt/terminal handling, and terminal-sensitive encoding behavior can differ. PgCliSharp must not describe redirected pipes as PTY/terminal emulation.

pgbench can produce long-running progress, debug, and benchmark output. Forcing all output into strings would make memory usage depend on benchmark duration.

## Decision

### Preserve the existing one-shot runner

The existing `IProcessRunner` and `ProcessRunRequest` remain the default execution mechanism for finite commands. Phase 6 may extend the request with optional stream destinations where backward-compatible, but it does not replace the established runner used by Phases 0-5.

### Add an internal long-lived redirected process session

Phase 6 introduces a separate internal session abstraction for a running child process.

The abstraction must:

- accept tokenized arguments and never invoke a shell;
- expose a writable standard-input stream while the process is running;
- stream standard output and standard error without requiring whole-output buffering;
- support explicit input completion/EOF;
- expose asynchronous completion with exit code and duration;
- support cancellation and timeout/process-tree termination consistent with ADR-0008;
- map target-specific implementation details back into PgCliSharp-owned types.

For modern .NET targets this is implemented with `System.Diagnostics.Process`. The `netstandard2.0` implementation may use CliWrap only if its lifecycle/pipe primitives can satisfy the same contract without leaking CliWrap types. If parity cannot be established, Phase 6 must stop and record a superseding target/backend decision rather than silently weakening behavior.

### Public psql modes are explicit

The psql wrapper separates:

1. finite/batch execution, including ordered `-c` and `-f` actions and optional stdin; and
2. a long-lived redirected duplex session for programmatic command exchange.

The redirected session is explicitly **not** a terminal/PTY. PgCliSharp does not promise Readline, command history, terminal control sequences, or other TTY-only behavior through that API.

Native attached-terminal or PTY support is outside this decision. It can be added later only after cross-platform semantics and the `netstandard2.0` contract are deliberately specified.

### Ordered psql actions are first-class

psql permits `--command` and `--file` to be repeated and interleaved. Their order is observable behavior. The public API therefore uses one ordered action collection rather than separate command and file collections that would lose ordering.

### Stream stderr when requested

Rich-I/O APIs may route PostgreSQL stderr into a caller-owned stream. When stderr is streamed, PgCliSharp must not also require retaining an unbounded in-memory copy. Result types document whether stderr was captured or externally streamed.

### Semantic exit statuses

Where PostgreSQL documents a finite exit-status domain for psql or pgbench, Phase 6 exposes a typed status in the tool-specific result. Known documented statuses are returned to the caller rather than being collapsed into one generic nonzero-process exception. Exit codes outside the documented domain remain execution failures.

This follows the precedent established by `pg_isready`, while keeping each tool's status enum tool-specific.

## Consequences

### Positive

- psql batch actions preserve upstream ordering semantics.
- Programmatic command exchange no longer depends on preconstructing a finite stdin stream.
- Long-running pgbench output can be consumed incrementally with bounded memory.
- The API does not falsely claim redirected pipes behave like a terminal.
- Existing Phase 0-5 wrappers can continue using the simpler one-shot execution path.

### Negative / trade-offs

- Phase 6 adds a second internal process lifecycle abstraction.
- Cancellation and process-tree behavior must be validated on both modern .NET and the netstandard2.0/CliWrap backend.
- Consumers that need genuine PTY behavior do not receive it from the initial Phase 6 API.
- Tool-specific result handling becomes more explicit because documented nonzero statuses are preserved.

## Validation

Phase 6 must include tests for:

- psql `-c`/`-f` interleaving and deterministic argument order;
- stdin EOF handling for finite psql input;
- session writes after process start and explicit input completion;
- stdout/stderr streaming without mandatory whole-payload buffering;
- cancellation and timeout termination;
- psql documented exit statuses 0-3;
- pgbench version-documented exit statuses (0-1 for PostgreSQL 10-11 and 0-2 from PostgreSQL 12);
- PostgreSQL 10-18 option availability and spelling changes;
- Windows .NET Framework 4.8 execution of the netstandard2.0 surface;
- explicit documentation that redirected psql sessions are not TTY/PTY sessions.
