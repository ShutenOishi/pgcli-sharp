# ADR-0009: Record every completed phase as a GitHub Release with immutable artifacts

- Status: Superseded
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: ADR-0010

## Context

PgCliSharp is developed in explicit implementation phases. CI artifacts are useful during development but are retention-limited and are not a durable milestone record.

ADR-0006 defines the NuGet publication model, including deliberate Semantic Versioning releases and Trusted Publishing. Phase milestones need a related but separate record even when a phase is not yet suitable for publication to nuget.org.

The repository should make it easy to retrieve the exact source and package artifacts that correspond to each completed phase.

## Decision

Every completed implementation phase must create one GitHub Release.

Phase releases use tags of the form:

```text
phase-0
phase-1
phase-2
...
```

The phase release is driven by `.github/phase-release.json`. A phase-completion change updates that manifest with the completed phase number, tag, title, release-notes file, and prerelease state.

When that manifest change reaches `main`, GitHub Actions must:

1. restore, build, and test the repository in Release configuration;
2. produce the PgCliSharp `.nupkg`;
3. produce the PgCliSharp `.snupkg`;
4. create an explicit source ZIP from the exact `main` commit being released;
5. create the `phase-N` GitHub Release targeting that exact commit;
6. attach the source ZIP, `.nupkg`, and `.snupkg`;
7. verify that all required assets are present on the resulting Release.

A phase tag/release is immutable as a project milestone. If a completed phase requires a correction after release, do not silently replace its assets or retarget its tag. Record the correction in a subsequent release/tag or a documented corrective milestone.

Phase releases are separate from publication to nuget.org:

- a Phase release records development progress and always contains package artifacts;
- nuget.org publication remains governed by ADR-0006 and only occurs when the roadmap calls for a deliberate package release;
- the package version embedded in the attached `.nupkg` remains the project package version and does not need to equal the phase number.

## Automation model

The phase-release workflow runs on `main` when either the phase manifest or the phase-release workflow changes. It may also be run manually for recovery.

If the manifest's release tag already exists, the workflow treats the release as immutable and verifies the existing assets rather than replacing them.

The release-notes file for each phase is kept under `docs/releases/` so the milestone description is reviewable in the same pull request as the implementation.

## Alternatives considered

### Keep only GitHub Actions artifacts

Rejected because Actions artifacts are operational build outputs with retention limits, not durable versioned project milestones.

### Publish every phase package to nuget.org

Rejected because implementation phases and public NuGet versions serve different purposes. Some phases may not yet provide a useful public API.

### Rely only on GitHub's automatically generated source ZIP

Rejected because an explicitly attached ZIP gives the release workflow a verifiable named asset alongside the exact `.nupkg` and `.snupkg` produced by that phase.

### Create Phase releases manually

Rejected as the normal process because manual asset selection and tagging are easier to perform inconsistently.

## Consequences

### Positive

- Every completed phase has a durable and discoverable release record.
- Source and NuGet artifacts are tied to the exact released commit.
- Phase history remains useful even before nuget.org publication begins.
- Release notes and release configuration are code-reviewed.
- Future phases use the same repeatable mechanism.

### Negative / trade-offs

- Every completed phase creates an additional Git tag and GitHub Release.
- The release workflow requires `contents: write` permission.
- Phase completion PRs must update the manifest and add phase release notes.
- Correcting an already released phase requires a new milestone instead of overwriting history.

## Relationship to ADR-0006

ADR-0006 remains Accepted.

ADR-0009 adds durable per-phase GitHub milestone releases. ADR-0006 continues to govern Semantic Versioning and actual nuget.org publication.
