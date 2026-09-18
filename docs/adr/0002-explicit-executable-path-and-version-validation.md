# ADR-0002: Require explicit executable path and validate executable version

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None

## Context

A machine can have multiple PostgreSQL installations and multiple versions of tools such as `pg_dump`. Automatic discovery through PATH, the Windows registry, package-manager conventions, or installation directories can select a different executable from the one intended by the caller.

Because PgCliSharp performs version-specific option validation, the library must know which CLI major version it is targeting.

## Decision

The caller explicitly supplies the executable path for each PostgreSQL tool wrapper.

The caller also selects the expected PostgreSQL CLI major version.

PgCliSharp should invoke the executable's `--version` output, parse the actual version, and detect a mismatch between the selected version and actual executable before normal command execution. Version results may be cached by executable identity/path when safe.

The selected CLI executable version is distinct from the connected PostgreSQL server version.

Executable auto-discovery may be considered later as an optional helper, but it must not replace explicit executable selection as the core API contract.

## Alternatives considered

### Resolve executable names from PATH

Rejected as the primary API because PATH ordering is environment-dependent and ambiguous when multiple PostgreSQL versions are installed.

### Discover PostgreSQL installation directories automatically

Rejected as the primary API because discovery differs across Windows, Linux, macOS, package managers, containers, and custom installations.

### Trust the caller-provided version without checking the binary

Rejected because it can cause the library to generate arguments that the actual executable does not support.

## Consequences

### Positive

- Deterministic executable selection.
- Safer multi-version installations.
- Reliable version-specific validation.
- Easier test setup because the binary under test is explicit.

### Negative / trade-offs

- Callers must know the executable path.
- A version probe introduces a small startup cost, mitigated by caching.

## Implementation notes

A mismatch should raise a PgCliSharp-specific exception with structured expected/actual version properties.

Do not infer server compatibility solely from the executable major version. Server-version compatibility rules can be modeled separately.

## Validation

- Unit-test version parsing.
- Integration-test `--version` against PostgreSQL 10-18 binaries.
- Test mismatch behavior before command execution.
