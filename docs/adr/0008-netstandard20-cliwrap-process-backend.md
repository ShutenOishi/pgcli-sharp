# ADR-0008: Use a conditional CliWrap backend for .NET Standard 2.0 process execution

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: ADR-0005
- Superseded by: None

## Context

ADR-0005 established direct, shell-free PostgreSQL process execution, binary-safe streaming, cancellation, timeout handling, and best-effort process-tree termination.

Phase 0 implementation validated that the modern .NET targets can satisfy these requirements directly with `System.Diagnostics.Process`, including `ProcessStartInfo.ArgumentList`, `WaitForExitAsync`, and `Process.Kill(entireProcessTree: true)`.

The proposed `netstandard2.0` target does not expose the same process APIs. A hand-written compatibility path would require custom argument escaping and would otherwise fall back to terminating only the immediate child process, weakening the process-tree behavior required by the architecture.

CliWrap provides a maintained .NET Standard 2.0-compatible process abstraction with tokenized argument formatting, binary stream piping, cancellation, and forceful process-tree termination. Version 3.10.5 is MIT-licensed and supports `netstandard2.0`.

## Decision

PgCliSharp keeps a shell-free, target-specific internal execution backend.

For `net8.0` and `net10.0`:

- use `System.Diagnostics.Process` directly;
- pass arguments through `ProcessStartInfo.ArgumentList`;
- use `WaitForExitAsync`;
- use `Kill(entireProcessTree: true)` for cancellation and timeout termination.

For `netstandard2.0`:

- use CliWrap 3.10.5 as an implementation dependency;
- reference CliWrap only for the `netstandard2.0` target;
- keep CliWrap types entirely internal and out of the PgCliSharp public API;
- pass arguments as individual values rather than preformatted command strings;
- stream stdout to a `Stream` without text conversion;
- capture PostgreSQL stderr separately;
- use CliWrap forceful cancellation so the process tree is terminated rather than only the immediate child process.

The library must not invoke PostgreSQL executables through `cmd.exe`, PowerShell, `bash -c`, or another shell on any target framework.

PgCliSharp continues to expose its own localized exceptions and structured diagnostic data. CliWrap exceptions and result types are implementation details and must not leak through the public API contract where PgCliSharp defines equivalent behavior.

## Alternatives considered

### Keep a custom .NET Standard 2.0 process compatibility layer

Rejected because it requires maintaining custom argument escaping and cannot provide equivalent process-tree termination through the .NET Standard 2.0 BCL alone.

### Implement platform-specific process-tree discovery and termination ourselves

Rejected for Phase 0 because doing so correctly across Windows, Linux, macOS, .NET Framework consumers, and legacy runtimes would add substantial security-sensitive native/process-management code that is not core PostgreSQL functionality.

### Use CliWrap for every target framework

Rejected because modern .NET already provides the required primitives. Restricting the dependency to `netstandard2.0` keeps the modern-target dependency graph minimal.

### Remove `netstandard2.0`

Not decided here. ADR-0007 remains Proposed until the full Phase 0 compatibility and dependency trade-offs are validated.

## Consequences

### Positive

- `netstandard2.0` can match modern targets more closely for argument safety, cancellation, streaming, and process-tree termination.
- Custom command-line escaping code can be removed.
- The modern `net8.0` and `net10.0` package assets remain free of the CliWrap runtime dependency.
- CliWrap remains an internal implementation choice rather than part of the public API.

### Negative / trade-offs

- The `netstandard2.0` package asset gains a CliWrap dependency and its transitive compatibility dependencies.
- Process behavior must be tested through both execution backends.
- Updating CliWrap becomes a compatibility/security maintenance task.
- The dependency graph is heavier for legacy consumers than for modern .NET consumers.

## Implementation notes

Pin the initial compatibility backend to CliWrap 3.10.5 and update deliberately.

A Windows `.NET Framework 4.8` test target should reference PgCliSharp to exercise the `netstandard2.0` asset at runtime. Linux/macOS CI continues to build the `netstandard2.0` asset and tests the modern runtime backends.

No public PgCliSharp API should accept or return CliWrap types.

## Validation

- Build `netstandard2.0`, `net8.0`, and `net10.0`.
- Run a .NET Framework consumer test on Windows so the `netstandard2.0` PgCliSharp asset is executed.
- Test cancellation and timeout behavior.
- Test arguments containing spaces and special characters.
- Test binary stdout streaming without text conversion.
- Add an explicit descendant-process termination integration test before ADR-0007 is accepted.
- Inspect the NuGet dependency graph before the first public preview.
