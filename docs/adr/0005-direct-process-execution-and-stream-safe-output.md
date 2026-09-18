# ADR-0005: Execute tools directly and preserve stream-safe output

- Status: Superseded
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: ADR-0008

## Context

PostgreSQL command-line tools accept user-supplied values such as database names, hosts, file paths, filters, and patterns. Building one interpolated shell command creates quoting, portability, and injection problems.

Some tools, notably `pg_dump`, can emit binary archive data to stdout. Treating all stdout as text or buffering entire dumps in memory would be incorrect or inefficient.

## Decision

PgCliSharp executes PostgreSQL binaries directly using `System.Diagnostics.Process`.

The core execution path must not invoke through `cmd.exe`, PowerShell, `bash -c`, or another shell.

Arguments are passed as individual tokens, using `ProcessStartInfo.ArgumentList` on target frameworks that support it and a carefully tested compatibility mechanism where they do not.

Execution behavior must support:

- stdout/stderr redirection as required;
- binary-safe stdout streaming;
- file output where the PostgreSQL CLI itself supports it;
- `CancellationToken`;
- configurable timeout;
- best-effort complete process-tree termination on cancellation/timeout where supported;
- secret-safe diagnostic rendering.

The common result model must not require large/binary stdout to be materialized as a `string` or whole `byte[]`.

Passwords and secrets must not be exposed in arguments/logs when avoidable. PostgreSQL-supported environment or secure mechanisms should be used appropriately.

## Alternatives considered

### Build a single shell command string

Rejected because quoting and escaping differ by platform and can permit argument injection.

### Depend on a third-party CLI wrapper initially

Rejected as the default because the required behavior is implementable with the BCL and the core package aims to minimize dependencies. This can be revisited if a later ADR demonstrates clear benefit.

### Always capture stdout as text

Rejected because PostgreSQL outputs can be binary and/or very large.

## Consequences

### Positive

- More secure and portable argument handling.
- Binary-safe backup support.
- Lower unnecessary memory usage.
- Minimal runtime dependencies.

### Negative / trade-offs

- Multi-target compatibility requires some custom process abstractions.
- Cancellation/process-tree behavior differs across target frameworks/platforms and needs testing.

## Implementation notes

Preserve raw PostgreSQL stderr separately from PgCliSharp localized contextual messages.

Diagnostic command rendering is for humans only and must redact sensitive values.

## Validation

- Unit-test argument tokenization/escaping compatibility layer.
- Integration-test file paths and arguments containing spaces/special characters.
- Test binary stdout without text conversion.
- Test cancellation/timeout behavior on supported platforms.
