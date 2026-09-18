# ADR-0007: Initial target framework matrix

- Status: Proposed
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None

## Context

PgCliSharp should work in modern .NET applications while retaining broad compatibility where practical. Process APIs such as `ArgumentList`, async process waiting, and process-tree termination differ across framework targets.

A broad target matrix increases consumer compatibility but also increases implementation and test complexity.

## Proposed decision

Initial proposal:

```xml
<TargetFrameworks>netstandard2.0;net8.0;net10.0</TargetFrameworks>
```

Goals:

- `netstandard2.0`: broad compatibility, including older .NET/.NET Framework consumers where feasible;
- `net8.0`: direct support for the widely deployed .NET 8 LTS ecosystem;
- `net10.0`: current LTS optimization and modern APIs.

Public API behavior should remain consistent across targets even where internal compatibility implementations differ.

This remains **Proposed** until a foundation implementation proves that the matrix is maintainable and does not force unacceptable compromises.

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

## Validation required before acceptance

- Implement the Phase 0 process abstraction for all proposed targets.
- Run representative unit tests on all targets.
- Confirm package dependency graph remains acceptable.
- Confirm cancellation, argument passing, and output streaming can meet ADR-0005.
- Reassess whether `netstandard2.0` introduces disproportionate complexity.

After validation, either change this ADR to Accepted or create a superseding ADR with the final matrix.
