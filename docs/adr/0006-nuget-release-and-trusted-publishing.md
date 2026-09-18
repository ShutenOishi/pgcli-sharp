# ADR-0006: Publish NuGet packages through automated trusted releases

- Status: Superseded
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: ADR-0012

## Context

PgCliSharp is intended to be consumed as a NuGet library. Release artifacts must be reproducible, versioned consistently, debuggable through SourceLink/symbol packages, and publishable without storing a long-lived NuGet API key in the repository.

## Decision

PgCliSharp will be packaged and released through GitHub Actions.

Target package identity is `PgCliSharp`, subject to final nuget.org package-ID availability at publication time.

Use Semantic Versioning.

Before 1.0, use preview versions such as `0.x.y-alpha.n` as appropriate. `1.0.0` represents a reviewed/stabilized public API.

Release automation should:

1. restore/build in Release;
2. run required unit/compatibility/integration tests;
3. validate tag/package version consistency;
4. create `.nupkg`;
5. create symbol `.snupkg`;
6. include XML documentation;
7. enable SourceLink/repository metadata;
8. publish to nuget.org using Trusted Publishing/OIDC where available;
9. avoid long-lived NuGet API keys as the preferred mechanism;
10. create/attach GitHub Release artifacts where useful.

A published package version is immutable. Fixes require a new package version rather than replacing an existing nuget.org version.

## Alternatives considered

### Manual local `dotnet nuget push` for normal releases

Rejected as the primary process because it is harder to audit and reproduce.

### Store a long-lived NuGet API key in GitHub Secrets

Rejected as the preferred model because Trusted Publishing/OIDC reduces credential lifetime and secret-management risk.

### Publish every main-branch commit

Rejected because NuGet releases should be deliberate, versioned release events.

## Consequences

### Positive

- Reproducible release pipeline.
- Reduced long-lived credential exposure.
- Better debugging experience through symbols/SourceLink.
- Clear correspondence between Git tags/releases and packages.

### Negative / trade-offs

- Initial Trusted Publishing and GitHub environment setup is required.
- Release workflows become security-sensitive code and need review.

## Implementation notes

Use a protected GitHub release environment if practical.

The exact package ID must be confirmed before the first public push. If `PgCliSharp` is unavailable, selecting a fallback ID requires documentation and, if it affects public identity materially, a new ADR.

## Validation

Before the first public preview:

- inspect package contents;
- verify symbols and SourceLink;
- verify XML documentation is present;
- test install into a clean sample project;
- verify OIDC Trusted Publishing from the intended repository/workflow/environment;
- confirm tag, assembly/package version, and GitHub Release alignment.
