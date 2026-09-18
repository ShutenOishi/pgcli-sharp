# ADR-0012: Defer external publication until the final release phase

- Status: Accepted
- Date: 2026-09-19
- Decision owners: PgCliSharp maintainers
- Supersedes: ADR-0006, ADR-0010
- Superseded by: None
- Preserves: ADR-0004, ADR-0011

## Context

Phase 3 prepared `PgCliSharp 0.1.0-alpha.1` and validated the release pipeline on the exact merge commit
`cccf8d9fbe1f2e1104676ab94a7863209c0220dd`.

The main CI for that commit passed on Linux, macOS, and Windows. The NuGet release workflow then reached the
Trusted Publishing login step and stopped because no matching nuget.org trust policy was configured. No package,
version tag, Phase 3 tag, GitHub Release, or Release asset was published.

The project direction has changed: external publication should not interrupt the remaining implementation phases.
The prepared Phase 3 source and package identity still need to remain reproducible so they can be re-audited at the
final release phase.

## Decision

Starting with Phase 3, implementation-phase completion and external publication are separated.

### Implementation completion

A Phase can be considered implementation-complete when:

1. its implementation, tests, specifications, and documentation are merged to `main`;
2. the exact merge commit passes the required Linux/macOS/Windows CI matrix;
3. the repository records the Phase status and evidence needed to reproduce or audit it.

A GitHub Release is no longer required immediately after each Phase from Phase 3 onward.

Historical Phase 0-2 Releases remain valid and immutable.

### External publication

Until the final release phase (currently Phase 8), do not create new external publication artifacts:

- do not push PgCliSharp packages to nuget.org;
- do not create the deferred `v0.1.0-alpha.1` tag/Release;
- do not create the deferred `phase-3` tag/Release;
- do not create new GitHub Release assets for deferred implementation phases.

NuGet and Phase release workflows are manual-only and require both:

1. an explicit manual dispatch publication confirmation; and
2. `publication_enabled: true` in the corresponding repository manifest.

The checked-in manifests remain disabled during implementation phases.

### Phase 3 preservation

The Phase 3 release source is permanently recorded as:

`cccf8d9fbe1f2e1104676ab94a7863209c0220dd`

The deferred package identity is:

- Package ID: `PgCliSharp`
- Version: `0.1.0-alpha.1`
- Version tag: `v0.1.0-alpha.1`
- Phase tag: `phase-3`

The final release phase must re-check that this commit is in `main` history, re-run package-content and SourceLink
validation against that exact commit, and decide whether the deferred preview is still appropriate to publish.
If it is superseded by a later release decision, the repository must record that decision rather than silently
rewriting Phase 3 history.

### Credentials

Trusted Publishing/OIDC remains the preferred publication mechanism. No long-lived NuGet API key is required by this
decision, and development work must not request or expose NuGet API keys or repository secrets.

### Human-facing content

English/Japanese documentation requirements from ADR-0010 are retained. When a deferred public GitHub Release is
eventually created, its title and notes must remain bilingual.

## Consequences

### Positive

- Remaining implementation phases can proceed without external publication side effects.
- Phase 3 provenance is retained and independently reproducible.
- Accidental push-triggered publishing is removed.
- Trusted Publishing can be configured only when publication is actually scheduled.

### Negative / trade-offs

- GitHub Releases no longer provide a per-Phase public milestone after Phase 2.
- Final release work must re-audit older deferred source commits and package artifacts.
- The release manifests describe deferred publication state rather than the latest implementation Phase.

## Validation

During Phases 4-7:

- `.github/nuget-release.json` must keep `publication_enabled` false;
- `.github/phase-release.json` must keep `publication_enabled` false;
- release workflows must not have a push trigger;
- no deferred Phase 3 tag or Release should exist.

Before final publication:

- verify the recorded source commit is an ancestor of current `main`;
- verify its historical main CI result;
- rebuild/test/package the exact source commit;
- validate `.nupkg`, `.snupkg`, XML docs, SourceLink commit metadata, and clean consumer installation;
- configure and verify Trusted Publishing/OIDC if NuGet publication is still selected;
- enable publication only in an explicit reviewed change and invoke the workflow manually.
