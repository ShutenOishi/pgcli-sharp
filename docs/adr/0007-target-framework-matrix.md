# ADR-0007: Initial target framework matrix

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None

## Context

PgCliSharp should work in modern .NET applications while retaining broad compatibility where practical. Process APIs such as `ArgumentList`, async process waiting, and process-tree termination differ across framework targets.

A broad target matrix increases consumer compatibility but also increases implementation and test complexity.

## Decision

Initial proposal:

```xml
<TargetFrameworks>netstandard2.0;net8.0;net10.0</TargetFrameworks>
```

Goals:

- `netstandard2.0`: broad compatibility, including older .NET/.NET Framework consumers where feasible;
- `net8.0`: direct support for the widely deployed .NET 8 LTS ecosystem;
- `net10.0`: current LTS optimization and modern APIs.

Public API behavior should remain consistent across targets even where internal compatibility implementations differ.

Phase 0 implementation validated that this matrix is maintainable while preserving the required execution semantics. The matrix is therefore accepted as the initial PgCliSharp target-framework policy.

## Alternatives considered

### net10.0 only

Simpler, but excludes many existing applications.

### net8.0;net10.0 only

Simpler than including `netstandard2.0`, but loses older framework compatibility.

### netstandard2.0 only

Broad reach but prevents target-specific use of modern APIs and optimizations without additional work.

## Consequences

### Positive if accepted

- Broad consumer reach.
- Modern target-specific implementations can coexist with compatibility support.

### Negative / trade-offs

- More CI combinations.
- Process/cancellation APIs require compatibility code.
- Package behavior must be kept consistent across targets.

## Validation

Phase 0 validation completed on 2026-09-18.

- The library builds for `netstandard2.0`, `net8.0`, and `net10.0` under the strict warnings-as-errors configuration.
- Linux, macOS, and Windows CI builds and tests pass.
- Windows CI runs a `.NET Framework 4.8` test target, which consumes the `netstandard2.0` PgCliSharp asset and exercises the CliWrap compatibility backend defined by ADR-0008.
- Cancellation and timeout behavior are covered by execution tests.
- Argument values containing spaces are verified as a single argument token.
- Binary stdout is verified byte-for-byte without text conversion.
- A descendant-process integration test verifies process-tree termination after timeout on Windows for the `net48`, `net8.0`, and `net10.0` test targets.
- The `netstandard2.0` dependency trade-off is explicitly documented and accepted by ADR-0008; CliWrap is conditional to that target and is not exposed in the public API.
- NuGet package creation succeeds in CI with XML documentation, symbols, and SourceLink enabled.

The validation did not reveal a disproportionate maintenance or behavioral compromise requiring removal of `netstandard2.0`.
