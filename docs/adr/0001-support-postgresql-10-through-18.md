# ADR-0001: Support PostgreSQL CLI versions 10 through 18

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None

## Context

PgCliSharp is intended to wrap PostgreSQL command-line tools with a strongly typed .NET API. Real deployments may continue to use PostgreSQL versions that are already upstream end-of-life, especially for maintenance, migration, and backup tooling.

Limiting the library only to currently upstream-supported releases would exclude PostgreSQL 10 through 13 and would make the supported range change whenever PostgreSQL upstream changes lifecycle status.

## Decision

PgCliSharp's initial PostgreSQL CLI compatibility range is **PostgreSQL 10 through PostgreSQL 18 inclusive**.

PostgreSQL upstream lifecycle status and PgCliSharp compatibility are separate concepts.

An upstream-EOL PostgreSQL version:

- may remain supported by PgCliSharp;
- must be clearly identified as upstream EOL in metadata/documentation;
- must not be blocked from execution solely because of upstream EOL status.

Once an enum value for a released PostgreSQL major version has shipped in a stable PgCliSharp API, it must not be removed merely because upstream support ends. Removing support requires an explicit later breaking-change decision.

Pre-release PostgreSQL major versions are not automatically advertised as stable supported versions.

## Alternatives considered

### Support only upstream-supported PostgreSQL versions

Rejected because the support window would exclude common legacy installations and would shift over time independently of PgCliSharp releases.

### Treat EOL versions as unsupported but allow raw escape-hatch arguments

Rejected because it undermines the goal of complete typed, version-aware coverage.

## Consequences

### Positive

- Legacy migration and maintenance scenarios remain first-class.
- Compatibility behavior is stable across PostgreSQL upstream lifecycle transitions.
- Tests can explicitly document syntax differences across nine major versions.

### Negative / trade-offs

- Compatibility data and integration testing are substantially larger.
- Old executable/container environments may require special CI handling.
- Security fixes for upstream-EOL PostgreSQL binaries are outside PgCliSharp's control.

## Implementation notes

Use a major-version enum covering V10 through V18 initially.

Maintain separate lifecycle metadata such as Supported/EOL. Lifecycle metadata is informational and must not itself determine whether the wrapper executes.

Version-specific option availability must be machine-testable.

## Validation

- Unit/compatibility tests must include PostgreSQL 10-18.
- Documentation must distinguish upstream lifecycle from PgCliSharp support.
- Integration testing should exercise real binaries/containers for each version where reproducible.
