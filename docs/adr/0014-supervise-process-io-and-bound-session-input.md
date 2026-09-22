# ADR-0014: Supervise process I/O and bound redirected session input

- Status: Accepted
- Date: 2026-09-23
- Related: ADR-0008, ADR-0013

## Context

The one-shot modern process runner and modern redirected-session runner previously waited primarily for process exit, cancellation, or timeout. Caller-owned stdout/stderr transfer tasks could fault without becoming a lifecycle outcome until the process exited, and abnormal cleanup could wait without a bound after a best-effort process-tree termination attempt.

The `netstandard2.0` redirected-session backend used an unbounded in-memory queue between `PsqlSession.StandardInput` and CliWrap. A producer could therefore enqueue data substantially faster than a child process consumed it.

These behaviors were identified during the Phase 0-6 follow-up review. They do not change the decision to use direct `System.Diagnostics.Process` on modern targets, CliWrap on `netstandard2.0`, or a separate redirected-session abstraction.

## Decision

### I/O lifecycle supervision

Modern one-shot execution and modern redirected sessions supervise stdout/stderr transfer faults alongside process exit, caller cancellation, and configured timeout.

When an I/O transfer fails before process exit:

1. the process tree is terminated on a best-effort basis;
2. standard input/output cleanup is requested;
3. remaining internal tasks are observed for a bounded cleanup interval;
4. caller cancellation has first precedence if it is already requested, configured timeout has precedence when it has elapsed, otherwise the original I/O exception is propagated.

When a process exits before redirected output has drained, the original cancellation/timeout deadline continues to apply to the drain. A configured timeout is therefore not silently abandoned merely because the child process exited first.

Best-effort process-tree termination is not represented as a guarantee that an uncooperative or externally protected descendant was killed. Abnormal cleanup does not wait indefinitely for that guarantee.

Caller-owned streams are never disposed by PgCliSharp. A caller-provided stream implementation that ignores cancellation can outlive the bounded abnormal-cleanup observation; PgCliSharp observes internal faults but does not claim it can forcibly abort arbitrary user code.

### Redirected-session input backpressure

The `netstandard2.0` session input bridge uses 64 KiB segments and at most 16 queued segments (approximately 1 MiB) before applying backpressure.

- `Write` may block while the bounded buffer is full.
- `WriteAsync` waits asynchronously and honors its cancellation token.
- successful write completion means the bytes were accepted into the bounded delivery buffer; it does not mean psql has consumed them.
- `Flush` / `FlushAsync` do not promise child-process consumption.
- `CompleteInput` rejects further writes, allows already accepted queued bytes to drain in order, then supplies EOF.
- cancellation, process exit, or session disposal releases writers waiting for buffer capacity.
- public caller streams remain caller-owned.

The modern backend continues to write directly to the operating-system pipe; the bounded bridge exists only where the CliWrap `PipeSource` adapter needs an asynchronous producer/consumer boundary.

## Consequences

- Caller stream failures surface promptly instead of being hidden behind a child process that continues writing.
- Cancellation and timeout cleanup have a bounded internal observation period after a termination attempt.
- The legacy session backend cannot grow its input queue without bound.
- Very fast producers can now experience intentional backpressure on `netstandard2.0`.
- Large canceled writes can have a prefix already accepted before cancellation, as with ordinary streamed I/O; callers requiring transactional command boundaries must provide them at the application protocol level.
- Tests cover output faults, input faults, bounded-write cancellation, blocked-writer release, normal EOF ordering, and the existing cross-platform cancellation/timeout behavior.
