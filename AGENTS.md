# PgCliSharp Repository Guidance

This file is the entry point for humans and AI coding agents working on this repository.

## Canonical project documents

Before making design or implementation changes, read these documents:

1. `docs/adr/README.md` - ADR index, statuses, and decision-change workflow
2. `docs/architecture.md` - consolidated current architecture and compatibility policy
3. `docs/localization.md` - consolidated English/Japanese documentation and runtime localization policy
4. `docs/roadmap.md` - current implementation and NuGet release roadmap
5. `docs/tool-implementation-workflow.md` - required specification-first workflow for adding PostgreSQL CLI tools

Accepted ADRs preserve the authoritative decision history. The consolidated documents describe the current intended state. Proposed ADRs are not binding until accepted.

If an Accepted decision changes, create a new superseding ADR instead of rewriting the old decision to erase history, then update the consolidated documents in the same change.

## Core rules

- PgCliSharp is a strongly typed .NET wrapper around PostgreSQL command-line tools.
- PostgreSQL executable paths are supplied explicitly by the caller.
- PostgreSQL CLI versions 10 through 18 are supported initially.
- PostgreSQL upstream EOL status and PgCliSharp compatibility are separate concepts.
- Each CLI gets its own typed Options class.
- Prefer enums for finite option sets and dedicated value objects for structured values.
- Do not expose arbitrary command-line strings as the primary API when a typed representation is practical.
- Validate option availability and incompatible combinations before starting the process.
- Use `ProcessStartInfo.ArgumentList` on modern targets. The `netstandard2.0` compatibility backend uses CliWrap according to ADR-0008. Do not execute PostgreSQL tools through `cmd.exe`, PowerShell, `bash -c`, or another shell.
- Keep the core package free of unnecessary runtime dependencies.
- Public APIs must have bilingual English/Japanese XML documentation according to `docs/localization.md`.
- Human-facing project surfaces must provide Japanese as well as English. This includes the repository README, GitHub Release titles/notes, user-oriented guides, and other prominent public documentation. The root README may link prominently to a maintained `README.ja.md` instead of duplicating the full text inline.
- User-facing diagnostics and exception messages must be localizable in English and Japanese using resources; do not hard-code localized strings throughout the implementation.
- Tests must cover version-specific argument generation and validation.
- Changes to PostgreSQL compatibility claims must be backed by PostgreSQL official documentation and/or executable integration tests.
- New PostgreSQL CLI tool work follows `docs/tool-implementation-workflow.md`: research and machine-readable compatibility specification come before public API implementation.
- Do not infer old-version behavior from the newest PostgreSQL documentation. Compare every supported major independently, then use upstream source/security history to resolve parser behavior, aliases, constraints, and patch-level backports.
- Distinguish feature availability from spelling/alias availability. A feature can exist in older majors under different option names.
- Before implementation is considered complete, the compatibility specification must map every inventory entry to a public/API binding or an explicit special binding, and runtime availability metadata must be covered by tests.
- Prefer tool-specific public option types. Extract shared internal serializers/validators only after at least two tools demonstrate genuinely identical semantics; do not create speculative public "common options" abstractions.
- NuGet releases use Semantic Versioning and GitHub Actions. Trusted Publishing/OIDC is preferred over long-lived NuGet API keys. Under ADR-0012, external publication is deferred until the final release phase and release workflows remain manual-only/manifest-disabled during implementation phases.
- Phase 0-2 keep their existing immutable GitHub Releases. Under ADR-0012, Phase 3 onward is completed by main merge + exact-commit CI + repository evidence; external GitHub Releases and NuGet publication are deferred until the final release phase.
- Starting with Phase 1, Phase Release Notes must present the `## English` section before the `## 日本語` section. Phase 0 is grandfathered and keeps its existing Japanese-first ordering.

## Updating project decisions

For a material repository-wide design change, follow `docs/adr/README.md`.

If a rule becomes obsolete or implementation uncovers an exception:

1. Determine whether the change requires a new ADR.
2. If it replaces an Accepted ADR, create a new ADR with `Supersedes: ADR-NNNN` and mark the old ADR as superseded.
3. Update the consolidated current-state document in the same change.
4. Record the reason and consequences.
5. Update tests and examples to match.
6. Avoid silently changing a project-wide convention only in source code.

Do not rewrite Accepted ADR history merely to make it match new code.

Chat conversations are not the canonical record. The repository documents and ADRs are.

## Efficient repository workflow

- Batch logically related edits into a coherent commit before opening or updating a PR when practical. This reduces redundant CI runs and review noise.
- Open a Draft PR once the research/spec baseline is coherent enough to review; do not use a Draft PR as a substitute for recording research in the repository.
- CI cancels superseded runs for the same PR/ref. Do not interpret a cancelled older run as a failure when a newer commit replaced it.
- Keep deferred publication manifests disabled during implementation phases. Phase implementation status belongs in roadmap/research/completion documentation; do not enable publication as part of ordinary Phase completion.
- Before a Phase completion merge, require a fresh all-platform CI pass after final implementation/completion documentation changes.
- After merge, verify the exact main commit and its CI result. For Phase 3 onward, do not require or create a GitHub Release until the final release phase; ADR-0012 governs deferred publication.
