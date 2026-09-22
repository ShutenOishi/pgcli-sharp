# PostgreSQL CLI Tool Implementation Workflow

This is the required working sequence for adding or substantially extending a PostgreSQL command-line tool in PgCliSharp.

It operationalizes [ADR-0011](adr/0011-specification-first-tool-implementation.md). The goal is to make later phases faster by moving compatibility mistakes to the research/specification stage, where they are cheaper to fix.

## 1. Start from the current repository state

Before tool-specific work:

1. synchronize with the latest `main`;
2. read `AGENTS.md`, the ADR index and Accepted ADRs, `docs/architecture.md`, `docs/localization.md`, and `docs/roadmap.md`;
3. review existing tool specifications and implementations for reusable patterns;
4. create a phase/tool branch;
5. leave `.github/phase-release.json` and the next Phase Release untouched until the completion gate.

Do not treat a previous chat as authoritative project state.

## 2. Build the compatibility inventory first

For every supported PostgreSQL major (currently 10 through 18), inspect that major's official application documentation independently.

Capture the complete tool option inventory, including:

- short forms;
- long forms;
- aliases;
- required versus absent option arguments;
- value syntax and ranges;
- finite values suitable for enums;
- structured values suitable for value objects;
- repeatability;
- defaults and behavior when omitted;
- dependencies and mutual exclusions;
- format/mode-specific restrictions;
- stdin/stdout/file/directory behavior;
- connection options;
- relevant environment variables;
- changed semantics across majors;
- additions and removals.

Do not begin with PostgreSQL 18 and assume older versions are subsets.

## 3. Resolve historical ambiguity with upstream source

Documentation is the primary user-facing source, but source code is required when behavior affects API correctness and the documentation is not precise enough.

Check the relevant stable branches (`REL_10_STABLE` through `REL_18_STABLE`) for:

- the tool's `long_options[]` or equivalent option table;
- switch/argument parser cases;
- value-range validation;
- hard incompatible-combination checks;
- repeatability behavior;
- default initialization;
- aliases and deprecated spellings;
- shared parser helpers used by structured options.

For maintenance-branch additions, also inspect official PostgreSQL security advisories/release history.

Record the evidence in the tool spec. A source audit performed only in chat is not sufficient.

## 4. Separate feature availability from spelling availability

An option concept and one of its spellings are not the same compatibility fact.

Example from Phase 1:

- large-object inclusion exists across the supported range;
- PostgreSQL 10-15 use `--blobs`/`--no-blobs`;
- PostgreSQL 16+ make `--large-objects`/`--no-large-objects` canonical while retaining compatibility aliases.

The specification and argument generator must preserve this distinction.

## 5. Create the machine-readable specification

Create or update:

```text
spec/postgresql/<tool>.json
```

Follow the conventions in `spec/postgresql/README.md`.

Before the public API is considered complete, the specification must contain enough data to answer mechanically:

- What options exist?
- In which major/patch versions?
- Under which spellings?
- Does the option take a value?
- Is it repeatable?
- What happens when omitted?
- What values/structures are legal?
- What conflicts or dependencies exist?
- What API member represents it?
- What output/input/environment behavior changes its meaning?

Run:

```bash
bash eng/validate-compatibility-specs.sh
```

before moving to the completion stage.

## 6. Perform the completeness audits before locking public API

Before implementation is treated as feature-complete:

### Inventory audit

Compare the per-major resolved long-option sets with the upstream source option tables where applicable.

Every unexplained difference is a research defect until resolved.

### API coverage audit

Every inventory entry must map to one of:

- a typed `Options` property;
- a dedicated output/input/execution abstraction;
- executable-version probing;
- another explicit special binding.

Do not silently omit utility or connection entries simply because they do not fit the main options class.

### Availability audit

For every version-varying option, decide whether the runtime check needs:

- major-only `since`;
- major `since/until`;
- exact minimum patch versions per major;
- spelling selection by version;
- semantic/value changes by version.

## 7. Design typed public API

Use one tool-specific public Options type for the supported-version union.

Prefer:

- enum for finite values;
- dedicated value object for structured values;
- ordered collections for repeatable options;
- one state enum instead of contradictory boolean pairs;
- explicit stream/file/directory abstractions when I/O semantics differ.

Do not expose a raw arbitrary command-line tail as the normal escape hatch for options that can be typed reasonably.

Public XML documentation is English first, Japanese second.

Any user-facing validation message introduced with the API must be added to both neutral English and Japanese resources in the same implementation batch.

## 8. Add runtime metadata before scattered validator checks

Create a centralized availability catalog for options whose availability changes.

The validator should consume that catalog rather than repeating version numbers independently.

Tool-specific combination/value checks stay in the tool validator.

This separation makes these questions auditable:

- Is the option available?
- Is its requested value legal?
- Is its combination legal?

## 9. Implement in dependency order

Recommended order:

1. public enums/value objects/options;
2. localization resources;
3. availability metadata;
4. validator;
5. deterministic argument builder;
6. I/O and process integration;
7. execution/result API;
8. tests;
9. consumer documentation.

This avoids writing argument or execution code around an unstable type model.

## 10. Reuse internals carefully

Before the second tool, do not extract a broad "common PostgreSQL options" public abstraction.

When implementing the second and later tools:

1. compare the actual CLI semantics;
2. identify repeated internal serialization/validation;
3. extract only the parts that are genuinely identical;
4. keep public Options classes tool-specific;
5. retain tool-specific validation around the shared primitive.

Connection switches are a likely internal reuse candidate for Phase 2, but their exact accepted forms and tool semantics must be confirmed for both `pg_restore` and `pg_dumpall` before extraction.

## 11. Test in layers

### Unit

- typed value serialization;
- deterministic argument tokens;
- option ordering;
- localized validation behavior.

### Compatibility

- PostgreSQL 10-18 availability matrix;
- additions/removals;
- alias/spelling boundaries;
- exact patch boundaries;
- changed semantics/ranges.

### Execution

- binary stdout;
- binary stdin where applicable;
- file/directory outputs;
- cancellation;
- timeout;
- process failure;
- executable version mismatch;
- environment propagation.

Use real executables where reproducible, but do not make deterministic historical compatibility testing depend on all PostgreSQL versions being installed.

## 12. Keep PR/CI iterations efficient

Before opening a PR, batch enough work to make the research/spec baseline coherent.

After a PR exists:

- batch logically related changes;
- prefer one coherent commit/tree update over many tiny remote writes when practical;
- expect older CI runs for the same PR/ref to be cancelled automatically;
- investigate the newest run, not a superseded cancelled run;
- use strict analyzer feedback early rather than deferring it to Phase completion.

A Draft PR is appropriate when review/visibility is useful before completion.

## 13. Phase completion and deferred-publication gates

ADR-0012 changes the operational completion gate from Phase 3 onward without rewriting the historical ADR-0011 sequence.

### Phase 0-2 historical release gate

Phase 0-2 retain their already-published immutable Phase Releases and artifacts. Their historical completion evidence is not regenerated.

### Phase 3-7 implementation-completion gate

During implementation phases after ADR-0012:

1. finish specification, implementation, tests, README/architecture/roadmap/completion-evidence updates;
2. keep `.github/phase-release.json` and `.github/nuget-release.json` disabled and do not move the preserved Phase 3 publication source;
3. require a Linux/macOS/Windows green CI run for the exact final PR head;
4. mark the PR ready if it was Draft;
5. merge with that tested head fixed;
6. require Linux/macOS/Windows green CI for the exact `main` merge commit;
7. record the final PR head, merge SHA, and CI run links in repository completion evidence.

A Phase 3-7 implementation can be reported complete after step 7. No new tag, GitHub Release, NuGet push, or Release asset is required or permitted as part of ordinary implementation completion.

### Final release phase

External publication remains a separate, explicitly authorized final-phase action:

1. complete the Phase 8 compatibility/public-API stabilization evidence;
2. decide explicitly whether the preserved Phase 3 preview candidate is still publishable or is superseded by a later reviewed candidate;
3. review the selected source SHA, package version, release notes, SourceLink/repository commit metadata, and publication manifests as one provenance unit;
4. enable publication only through a reviewed manifest change and explicit manual workflow dispatch;
5. rebuild and revalidate the selected source commit;
6. verify the resulting NuGet/GitHub Release tags, targets, and package/source artifacts.

Do not infer publication approval from implementation completion.

## 14. Phase 2 application

For Phase 2:

- research `pg_restore` and `pg_dumpall` independently;
- create `spec/postgresql/pg_restore.json` and `spec/postgresql/pg_dumpall.json`;
- complete each tool's inventory/source/API audits separately;
- compare their connection-option semantics with `pg_dump`;
- then extract internal shared connection argument/validation helpers only where behavior is proven identical;
- do not update the Phase 2 release manifest until both tools and backup/restore-trio tests are complete.
