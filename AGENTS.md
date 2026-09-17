# PgCliSharp Repository Guidance

This file is the entry point for humans and AI coding agents working on this repository.

## Canonical project documents

Before making design or implementation changes, read these documents:

1. `docs/architecture.md` - architecture and compatibility policy
2. `docs/localization.md` - English/Japanese XML documentation and runtime message localization
3. `docs/roadmap.md` - implementation and NuGet release roadmap

These documents are the source of truth for project-wide decisions. When implementation work reveals a better design, update the relevant document in the same change so that future work does not rely on chat history.

## Core rules

- PgCliSharp is a strongly typed .NET wrapper around PostgreSQL command-line tools.
- PostgreSQL executable paths are supplied explicitly by the caller.
- PostgreSQL CLI versions 10 through 18 are supported initially.
- PostgreSQL upstream EOL status and PgCliSharp compatibility are separate concepts.
- Each CLI gets its own typed Options class.
- Prefer enums for finite option sets and dedicated value objects for structured values.
- Do not expose arbitrary command-line strings as the primary API when a typed representation is practical.
- Validate option availability and incompatible combinations before starting the process.
- Use `ProcessStartInfo.ArgumentList` where supported; do not execute through `cmd.exe`, PowerShell, `bash -c`, or another shell.
- Keep the core package free of unnecessary runtime dependencies.
- Public APIs must have bilingual English/Japanese XML documentation according to `docs/localization.md`.
- User-facing diagnostics and exception messages must be localizable in English and Japanese using resources; do not hard-code localized strings throughout the implementation.
- Tests must cover version-specific argument generation and validation.
- Changes to PostgreSQL compatibility claims must be backed by PostgreSQL official documentation and/or executable integration tests.
- NuGet releases use Semantic Versioning and GitHub Actions. Trusted Publishing/OIDC is preferred over long-lived NuGet API keys.

## Updating project decisions

If a rule becomes obsolete or implementation uncovers an exception:

1. Update the canonical document first or in the same commit.
2. Record the reason for the change.
3. Update tests and examples to match.
4. Avoid silently changing a project-wide convention only in source code.

Chat conversations are not the canonical record. The repository documents are.
