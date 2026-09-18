# ADR-0011: Use a specification-first workflow for PostgreSQL CLI tools

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None
- Complements: ADR-0001, ADR-0002, ADR-0003, ADR-0004, ADR-0008, ADR-0010

## Context

Phase 1 implemented complete typed `pg_dump` coverage for PostgreSQL 10 through 18. It also exposed recurring risks that will affect later PostgreSQL CLI wrappers:

- current documentation for an old PostgreSQL major can include options that were security-backported after that major's initial release;
- one feature can have different canonical/compatibility spellings by major;
- documentation tables alone do not always expose parser ranges, repeatability semantics, or hard option-combination checks implemented by the executable;
- writing public value objects before inspecting upstream parser behavior can lock in an incorrect range or shape;
- scattering version checks through validators makes later completeness auditing harder;
- compatibility research kept only in chat cannot be reused reliably by another coding agent;
- opening/updating a PR for many small intermediate commits creates avoidable CI churn;
- Phase completion needs a separate release gate after implementation is already green.

The `pg_dump --restrict-key` security backport and PostgreSQL 16 large-object spelling changes are concrete Phase 1 examples.

## Decision

### 1. Research before public API implementation

For each PostgreSQL CLI tool, research PostgreSQL 10 through 18 before designing the complete public options surface.

The research must compare each supported major independently. The newest option list must never be projected backward as a historical compatibility model.

Use the following evidence hierarchy:

1. official per-major PostgreSQL application documentation;
2. official PostgreSQL source for option tables, argument parsing, defaults, and hard validation where documentation is ambiguous or incomplete;
3. official security advisories and release history for patch-level backports;
4. executable integration tests where reproducible and useful.

### 2. Machine-readable compatibility specification

Each implemented CLI tool must have a machine-readable specification under `spec/postgresql/` before its public API is considered complete.

At minimum, the specification records:

- supported PostgreSQL majors;
- official per-major documentation sources;
- option IDs;
- short and long spellings/aliases;
- spelling availability when it differs from feature availability;
- argument mode and value kind;
- repeatability;
- major availability and patch-level minimums where applicable;
- wrapper default/unset behavior and upstream omission/default behavior;
- constraints and incompatible combinations;
- format/I/O/environment interactions where relevant;
- intended typed API binding;
- per-major resolved option inventory;
- verification/audit metadata.

The specification is a durable cross-agent reference, not generated prose from the current chat.

### 3. Inventory verification gate

Before implementation completeness is claimed:

- every resolved per-major option ID must exist in the tool inventory;
- option IDs must be unique;
- every inventory entry must have an API binding or an explicit special binding;
- aliases/spellings whose availability differs by version must be represented separately from feature availability;
- the resolved long-option inventory should be compared with the corresponding upstream PostgreSQL source option table when the tool uses one.

The repository CI performs structural checks that can be validated offline. Source-level semantic verification remains a research-time requirement recorded in the specification.

### 4. Runtime availability metadata

Version-varying options should use centralized, machine-testable runtime availability metadata rather than independent ad-hoc version constants scattered through validators.

The metadata must support exact executable versions when maintenance releases introduce behavior, not only major versions.

Validators consume the centralized metadata and add tool-specific value/combination rules.

### 5. Typed design order

After the compatibility inventory is stable enough to design safely, implementation proceeds in this order:

1. enums/value objects/public option shape;
2. localization resources required by those types;
3. runtime availability metadata;
4. value and combination validation;
5. deterministic argument generation;
6. stdin/stdout/output execution integration;
7. result API;
8. regression and compatibility tests.

Finite values use enums. Structured values use value objects. Repeatable options use ordered collections. Mutually exclusive CLI switches should not be exposed as contradictory public booleans when one enum/value type can model the state.

### 6. Shared abstractions are evidence-driven

Public options remain tool-specific.

Do not introduce speculative public "common connection options" or other shared public option bases simply because two PostgreSQL tools have similarly named switches.

Internal serializers/validators may be extracted after at least two tools demonstrate genuinely identical semantics, argument spelling, validation, and lifecycle behavior. The extraction must not erase tool-specific differences.

### 7. Test gates

A tool implementation must cover, as applicable:

- PostgreSQL 10-18 availability boundaries;
- patch-level boundaries;
- deterministic argument token generation;
- spelling/alias changes;
- repeatable option ordering and multiplicity;
- typed value ranges and structured serialization;
- dependencies and incompatible combinations;
- format/output/input constraints;
- binary stdin/stdout where relevant;
- cancellation and timeout;
- executable version mismatch;
- bilingual resource consistency and structured exception data.

Unit/compatibility tests must not depend on every historical PostgreSQL binary being installed. Real executable integration tests supplement, rather than replace, the deterministic compatibility matrix.

### 8. CI and PR efficiency

Logically related repository changes should be batched when practical.

PR CI uses concurrency cancellation so a newer commit supersedes an older in-progress run for the same PR/ref.

The Phase release workflow is serialized and is not cancelled mid-release.

### 9. Phase-completion gate

Do not update the Phase release manifest merely because the tool implementation exists.

The order is:

1. implementation/spec/docs complete;
2. all-platform CI green;
3. add final bilingual Phase release notes and update `.github/phase-release.json`;
4. run fresh all-platform CI for that exact head;
5. merge;
6. verify main CI;
7. verify successful Phase Release workflow;
8. verify `phase-N` tag and Release target the exact merge commit;
9. verify explicit source ZIP, `.nupkg`, and `.snupkg` assets.

Only then is the Phase complete.

## Alternatives considered

### Implement first, document compatibility afterward

Rejected. Phase 1 showed that parser ranges, security backports, and spelling availability can change the correct public type design.

### Use only the newest PostgreSQL documentation

Rejected. This cannot represent removals, renamed spellings, changed semantics, or maintenance-branch backports accurately.

### Store research only as prose

Rejected. Prose is useful for explanation but insufficient for systematic completeness checks and reuse by later agents.

### Build a universal public common-options hierarchy now

Rejected. Similar switch names do not prove identical semantics across tools. Premature public abstraction would make later corrections breaking changes.

### Run every historical PostgreSQL executable in all CI jobs

Rejected as the baseline requirement because legacy binaries are not equally reproducible on every current runner. Deterministic spec/runtime tests remain mandatory; executable integration coverage is added where practical.

## Consequences

### Positive

- Public types are designed from verified semantics rather than assumptions.
- New tool work can reuse a consistent research and implementation sequence.
- Patch-level compatibility becomes a first-class concern.
- Spec/API/runtime drift is easier to detect.
- Coding agents can resume work from repository artifacts rather than chat history.
- CI churn is reduced for rapidly updated PRs.
- Shared abstractions are introduced only when there is real evidence of reuse.
- Phase completion has a repeatable, auditable definition.

### Negative / trade-offs

- A new tool has more up-front research before visible API code appears.
- Machine-readable specifications require maintenance alongside code.
- Some source-level audits still require deliberate upstream research and cannot be fully replaced by local CI.
- Tool-specific public models may temporarily duplicate concepts until a second implementation proves that internal reuse is safe.

## Implementation notes

The detailed operational checklist is maintained in `docs/tool-implementation-workflow.md`.

Structural compatibility-spec checks are implemented by `eng/validate-compatibility-specs.sh`.

Phase metadata validation is shared by CI and Phase Release through `eng/validate-phase-metadata.sh`.

## Validation

This decision is enforced by:

- AGENTS.md guidance;
- compatibility-spec structural validation in CI;
- strict build/analyzer settings;
- cross-platform tests;
- per-tool runtime metadata tests;
- the Phase release workflow and post-merge release verification.

## References

- `docs/pg-dump-phase-1.md`
- `spec/postgresql/pg_dump.json`
- `docs/architecture.md`
- `docs/roadmap.md`
