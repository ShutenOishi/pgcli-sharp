# Architecture Decision Records (ADR)

This directory records significant architectural and product decisions for PgCliSharp.

## Purpose

ADR files preserve **why** a decision was made, including alternatives and consequences. They complement, rather than replace, the consolidated current-state documents:

- `docs/architecture.md` — current architecture and compatibility model
- `docs/localization.md` — current localization/documentation policy
- `docs/roadmap.md` — current implementation/release plan

When a decision changes, do not rewrite history. Create a new ADR that supersedes the old one, then update the consolidated documents.

## Status values

- **Proposed** — under consideration; not binding yet
- **Accepted** — current project decision
- **Deprecated** — still documented but discouraged
- **Superseded** — replaced by a newer ADR
- **Rejected** — considered and intentionally not adopted

## Numbering and file names

Use monotonically increasing four-digit numbers:

```text
0001-short-kebab-case-title.md
0002-next-decision.md
```

Never reuse an ADR number.

## Required workflow

Create or update an ADR when a change materially affects one or more of:

- public API shape or compatibility;
- supported PostgreSQL versions;
- executable/process execution behavior;
- localization/documentation policy;
- target frameworks or runtime dependencies;
- package/release/versioning policy;
- security-sensitive behavior;
- testing/compatibility guarantees;
- repository-wide development rules.

For an already accepted decision that changes:

1. Add a new ADR with status `Accepted`.
2. Add `Supersedes: ADR-NNNN` to the new ADR.
3. Change the old ADR status to `Superseded` and add `Superseded by: ADR-NNNN`.
4. Update the consolidated current-state documents and tests in the same change.

Do not silently edit an old Accepted ADR to make history appear different.

## ADR index

| ADR | Title | Status |
|---|---|---|
| [0001](0001-support-postgresql-10-through-18.md) | Support PostgreSQL CLI versions 10 through 18 | Accepted |
| [0002](0002-explicit-executable-path-and-version-validation.md) | Require explicit executable path and validate executable version | Accepted |
| [0003](0003-typed-options-and-pre-execution-validation.md) | Use per-tool typed options and pre-execution validation | Accepted |
| [0004](0004-bilingual-documentation-and-localized-messages.md) | Provide bilingual XML documentation and localized runtime messages | Accepted |
| [0005](0005-direct-process-execution-and-stream-safe-output.md) | Execute tools directly and preserve stream-safe output | Superseded |
| [0006](0006-nuget-release-and-trusted-publishing.md) | Publish NuGet packages through automated trusted releases | Accepted |
| [0007](0007-target-framework-matrix.md) | Initial target framework matrix | Accepted |
| [0008](0008-netstandard20-cliwrap-process-backend.md) | Use a conditional CliWrap backend for .NET Standard 2.0 process execution | Accepted |
| [0009](0009-phase-release-artifacts.md) | Record every completed phase as a GitHub Release with immutable artifacts | Superseded |
| [0010](0010-bilingual-human-facing-releases.md) | Provide bilingual human-facing documentation and Phase Releases | Accepted |
| [0011](0011-specification-first-tool-implementation.md) | Use a specification-first workflow for PostgreSQL CLI tools | Accepted |

## Template

Copy [0000-template.md](0000-template.md) for new decisions.
