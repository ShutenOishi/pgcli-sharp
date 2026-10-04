# ADR-0023: Isolate the legacy backend's startup synchronization context

- Status: Superseded
- Date: 2026-10-04
- Superseded by: ADR-0024
- Related: ADR-0008, ADR-0013, ADR-0014, ADR-0017

## Context

Exact-main CI 37188258263 reproduced a net48 completion stall. The actual CLR4
dump showed all four CliWrap process/input/output/error tasks normally completed
while the outer async enumeration and wrapper completion remained pending.
This narrows the captured incident but does not by itself identify which caller
context or continuation mechanism caused the historical stall.

Pinned CliWrap 3.10.5 uses an unconfigured `await foreach` over its internalized
PolyShim Task.WhenEach. Awaiting the returned command with ConfigureAwait(false)
does not prevent the dependency's own awaits from capturing the startup context.
Draft PR #25 baseline CI 37188939390 confirms this separate, deterministic defect:
session and one-shot execution both fail to complete under an occupied queued
context on net48 (two posts each); modern targets pass. The one-shot fixture is
gated until startup returns, preventing a synchronous early-exit false positive.

## Decision

Keep the target matrix, CliWrap 3.10.5 dependency and shell-free execution policy.
Initialize CliWrap's command task synchronously under a null SynchronizationContext
for both finite and redirected execution. Restore the caller's context in finally,
including synchronous startup failures. Continue observing the returned task with
ConfigureAwait(false), mapping results/failures through existing PgCliSharp types.

Do not wrap startup in Task.Run, block on asynchronous startup, suppress the wider
ExecutionContext, or change culture/cancellation/timeout/stream ownership. This
scope only prevents capture of the caller's SynchronizationContext by dependency
initialization; it does not promise arbitrary custom TaskScheduler independence.
No public API or runtime dependency changes are needed.

## Consequences and validation

The compatibility backend's internal completion no longer requires the caller's
startup synchronization context to pump callbacks. The caller context is restored
before Start returns and after start errors. Existing synchronous startup error
translation remains intact.

The deterministic regressions must turn green on net48 and remain green on modern
targets; run the existing full suites, bounded Windows 30/3/3 stress, real PostgreSQL
and candidate audits at the final PR head and exact main commit. Preserve failed
baseline/main runs, and inspect actual diagnostic artifacts. Diagnostic heap filters
also include the dependency's internalized MemberPolyfills types, whose names do
not contain the PolyShim namespace.

The reproduced context defect is confirmed; attributing every historical stall to
it would exceed the evidence. A passing bounded run is regression evidence, not
proof of absence of all intermittent process defects. 1.0 acceptance and postponed
publication remain separate.

CI 37189457631 confirms the limitation: the context regressions pass, but net48
stress round 27 still stalls with all four lower tasks complete and a yielded
WhenEach value awaiting consumer progress. Context isolation is a confirmed
partial fix; final merge gates remain blocked. Nested promise/awaiter diagnostics
are expanded before choosing any additional runtime/dependency change.

## Primary-source evidence

- [Pinned CliWrap execution](https://github.com/Tyrrrz/CliWrap/blob/3.10.5/CliWrap/Command.Execution.cs)
- [Pinned PolyShim WhenEach](https://github.com/Tyrrrz/PolyShim/blob/2.12.2/PolyShim/Net90/Task.cs)
- [Framework Task state flags](https://github.com/microsoft/referencesource/blob/main/mscorlib/system/threading/Tasks/Task.cs)
- [Investigation and preserved receipts](../phase-8-net48-investigation.md)
