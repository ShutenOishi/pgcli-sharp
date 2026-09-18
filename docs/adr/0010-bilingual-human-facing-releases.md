# ADR-0010: Provide bilingual human-facing documentation and Phase Releases

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: ADR-0009
- Superseded by: None
- Complements: ADR-0004, ADR-0006

## Context

ADR-0004 established bilingual English/Japanese public API documentation and localized runtime messages. ADR-0009 established durable per-Phase GitHub Releases with immutable build artifacts.

The repository itself is also a product surface. README files, GitHub Releases, release notes, and user-oriented guides are read directly by users before they interact with the API or runtime diagnostics.

English-only project surfaces make the Japanese support promise difficult to discover. Duplicating every document inline, however, can make long pages harder to scan.

Phase Release artifacts must remain reproducible and immutable, while human-readable text sometimes needs later translation or typo correction.

## Decision

### Root README

The root `README.md` may remain English-first for broad .NET/NuGet ecosystem readability, but it must display a prominent Japanese link near the top.

A maintained `README.ja.md` provides the Japanese version of the user-facing repository overview.

The two README files must describe the same current support scope and major project status. Exact sentence-by-sentence translation is not required, but they must not make conflicting guarantees.

### Human-facing public surfaces

Prominent project content intended primarily for human users must provide Japanese as well as English.

At minimum, this includes:

- the root README experience;
- GitHub Release titles and release notes;
- Phase release notes under `docs/releases/`;
- user-oriented setup and usage guides;
- prominent consumer notices.

Internal engineering records such as ADR rationale may remain English-first unless translation materially improves usability.

### Phase Releases

Every completed implementation Phase continues to create one GitHub Release with:

- an explicit source ZIP;
- the PgCliSharp `.nupkg`;
- the PgCliSharp `.snupkg`;
- bilingual English/Japanese Release Notes.

Phase tags use `phase-N`.

The Release title should be understandable in both English and Japanese, for example:

```text
Phase 0 - Foundation / 基盤
```

### Immutability boundary

For a completed Phase Release, these are immutable milestone data:

- the `phase-N` tag;
- the target commit;
- the attached source ZIP;
- the attached `.nupkg`;
- the attached `.snupkg`.

These human-readable metadata fields may be synchronized after publication:

- Release title;
- Release notes/body.

Metadata updates are allowed only for localization, typo correction, clearer wording, or similarly non-semantic documentation improvements. They must not claim a different implementation than the immutable artifacts contain.

### Automation

The Phase Release workflow is driven by `.github/phase-release.json` and the selected `docs/releases/phase-N.md`.

When a new Phase tag does not exist, the workflow builds/tests/packages and creates the Release with immutable artifacts.

When the Phase Release already exists, the workflow must not replace its tag, target commit, or assets. It may synchronize the Release title and notes from the repository so translation/documentation improvements reach the existing human-facing Release.

Changes to the active Phase release notes should trigger the synchronization workflow on `main`.

## Alternatives considered

### Put full English and Japanese text in one root README

Rejected as the mandatory format because it makes the landing page unnecessarily long. A prominent language switch to a maintained Japanese README is easier to scan.

### Keep Releases English-only and link elsewhere

Rejected because Release pages are commonly visited directly from tags, dependency tools, and GitHub notifications. The essential milestone summary should be bilingual on the Release page itself.

### Treat all Release fields as permanently immutable

Rejected for title/body text because it would preserve avoidable translation omissions and typos forever. Artifact identity and commit provenance remain immutable.

## Consequences

### Positive

- Japanese users can discover and understand the project from the repository landing page.
- Release pages are usable without navigating to separate documentation.
- Phase artifacts retain exact provenance.
- Translation and typo fixes can improve already-published Release pages without changing code artifacts.
- Future coding agents have an explicit rule for human-facing localization.

### Negative / trade-offs

- README and release-note changes require translation maintenance.
- Release automation needs write permission to synchronize title/body metadata.
- Review must check that English and Japanese descriptions do not diverge semantically.

## Relationship to earlier ADRs

ADR-0004 remains Accepted and continues to govern bilingual public XML documentation and localized runtime diagnostics.

ADR-0006 remains Accepted and continues to govern NuGet publication.

ADR-0009 is superseded by this ADR. Its durable Phase Release and artifact requirements are retained, with the immutability boundary clarified and bilingual human-facing Release metadata added.
