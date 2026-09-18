# ADR-0003: Use per-tool typed options and pre-execution validation

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None

## Context

A thin wrapper that accepts arbitrary argument strings provides little value over `Process.Start` and allows invalid combinations, spelling errors, unsupported version-specific options, and contradictory flags to reach the PostgreSQL executable.

PostgreSQL CLI option sets differ by tool and by major version.

## Decision

Each PostgreSQL CLI tool has its own public typed Options class, such as:

- `PgDumpOptions`
- `PgRestoreOptions`
- `PgDumpAllOptions`
- `PgBaseBackupOptions`

Do not create separate public option classes for each PostgreSQL major version. A single per-tool options type models the supported union, while validators enforce availability for the selected CLI version.

Use:

- enums for finite value sets;
- dedicated value objects for structured values;
- collections for repeatable options;
- nullable/absence semantics for unspecified options.

Avoid boolean combinations that can represent impossible states when an enum/value object can model the valid state space.

PgCliSharp validates **before process startup**:

- option availability by PostgreSQL version;
- accepted value/range differences by version;
- mutually exclusive options;
- required option dependencies;
- format-specific restrictions and other documented combinations.

Unsupported or invalid configurations must raise PgCliSharp-specific exceptions rather than relying on PostgreSQL's "unrecognized option" or similar runtime diagnostics.

An arbitrary raw-arguments escape hatch, if ever added, must not be the primary API and must be clearly separated from the typed guarantees.

## Alternatives considered

### One dictionary/string-list of arguments for every tool

Rejected because it eliminates compile-time discoverability and most validation value.

### Separate `PgDumpOptions10` ... `PgDumpOptions18` classes

Rejected because it fragments the public API and makes callers rewrite code when changing executable versions.

### Defer all validation to PostgreSQL executables

Rejected because failures occur later, errors are less structured, and version/combination knowledge cannot be exposed cleanly to .NET callers.

## Consequences

### Positive

- Strong IntelliSense/discoverability.
- Version differences are explicit and testable.
- Invalid combinations fail early with structured exceptions.
- The API can remain mostly stable as newer PostgreSQL versions add options.

### Negative / trade-offs

- More modeling and validation code.
- Complete option coverage requires disciplined compatibility data and tests.
- New PostgreSQL versions may expand existing value-object designs.

## Implementation notes

Machine-readable compatibility specifications may be stored under `spec/postgresql/<major>/` when useful.

Exceptions should expose machine-readable details; callers must not parse localized message strings.

## Validation

- Argument-generation tests for every modeled option.
- Version-matrix tests for introduced/removed/changed options.
- Tests for invalid combinations.
- Completeness checks comparing maintained option inventories with implemented coverage.
