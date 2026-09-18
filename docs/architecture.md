# Architecture and Compatibility

> This document is the consolidated current-state architecture. Decision rationale and historical changes are recorded in [Architecture Decision Records](adr/README.md). If an Accepted decision is replaced, preserve the old ADR and supersede it with a new ADR.

Key accepted decisions currently include ADR-0001 through ADR-0004 and ADR-0006 through ADR-0008. ADR-0005 has been superseded by ADR-0008.

## 1. Project purpose

PgCliSharp is a strongly typed .NET wrapper for PostgreSQL command-line tools such as `pg_dump`, `pg_restore`, `pg_dumpall`, and later the wider PostgreSQL client/server tool set.

The project should provide more value than a thin `Process.Start` helper. It should model PostgreSQL CLI options as typed .NET APIs, validate version support and invalid combinations before process startup, and provide predictable execution/cancellation/diagnostics behavior.

## 2. Initial PostgreSQL version scope

Initial compatibility target:

- PostgreSQL 10
- PostgreSQL 11
- PostgreSQL 12
- PostgreSQL 13
- PostgreSQL 14
- PostgreSQL 15
- PostgreSQL 16
- PostgreSQL 17
- PostgreSQL 18

PostgreSQL 10-13 are legacy/EOL upstream releases, but PgCliSharp may continue to support their command-line syntax. Upstream support status must not be confused with PgCliSharp compatibility.

Once a PostgreSQL major version is supported by a released PgCliSharp version, its enum member should not be removed merely because PostgreSQL upstream reaches EOL. Removal would be a breaking change and requires an explicit major-version policy decision.

## 3. Executable selection

The caller explicitly provides the executable path. PgCliSharp does not primarily discover executables through PATH, the Windows registry, package managers, or installation directories.

Example intent:

```csharp
var pgDump = new PgDump(
    executablePath: @"C:\Program Files\PostgreSQL\18\bin\pg_dump.exe",
    version: PostgreSqlMajorVersion.V18);
```

The selected PostgreSQL version describes the CLI executable version, not automatically the connected server version.

The library should be able to invoke `--version`, parse the actual executable version, cache the result by executable identity/path where safe, and detect a caller-specified/executable version mismatch before normal execution.

## 4. Version model

Initial enum:

```csharp
public enum PostgreSqlMajorVersion
{
    V10 = 10,
    V11 = 11,
    V12 = 12,
    V13 = 13,
    V14 = 14,
    V15 = 15,
    V16 = 16,
    V17 = 17,
    V18 = 18
}
```

Upstream lifecycle information is metadata, not execution permission. A separate model should expose whether a PostgreSQL major version is currently upstream-supported or EOL.

## 5. Typed options

Each CLI tool has its own options model, for example:

- `PgDumpOptions`
- `PgRestoreOptions`
- `PgDumpAllOptions`
- `PgBaseBackupOptions`
- `VacuumDbOptions`
- `ReindexDbOptions`

Do not create version-specific public option classes such as `PgDumpOptions10` and `PgDumpOptions18`. One public options type should model the union of supported functionality, while validation determines whether a property is available for the selected PostgreSQL version.

Use:

- enums for finite sets of CLI values;
- dedicated value objects for structured values such as compression specifications;
- collections for repeatable options;
- nullable/reference absence to represent an option that was not requested.

Avoid pairs of booleans that can represent impossible states when a single enum can model the state safely.

## 6. Version-specific behavior

Every option that differs by PostgreSQL version must have machine-testable compatibility metadata or equivalent validation logic.

Availability is not assumed to be major-version-only. Security backports or other maintenance-branch changes can introduce an option in a later patch release. Phase 1's `pg_dump --restrict-key` support is the reference case: validation uses the actual numeric executable version from the `--version` probe as well as the selected major version.

Examples include:

- introduction/removal of options;
- changed accepted values;
- changed semantics;
- CLI tools introduced after PostgreSQL 10;
- combinations that only become legal in later releases.

Unsupported options must fail before process startup with a PgCliSharp-specific validation exception rather than relying on PostgreSQL to emit "unrecognized option".

## 7. Option combination validation

The library validates relationships such as:

- options that require another option;
- mutually exclusive options;
- options restricted to particular output formats;
- values whose legal range or syntax changed by PostgreSQL version.

Validation is part of the product, not merely test code.

## 8. Process execution

Execution uses target-specific internal backends while preserving one PgCliSharp behavior model:

- `net8.0` and `net10.0` use `System.Diagnostics.Process` directly;
- `netstandard2.0` uses CliWrap 3.10.5 internally as a compatibility backend;
- CliWrap is referenced only by the `netstandard2.0` package asset and its types do not appear in the public PgCliSharp API.

Requirements:

- no shell mediation;
- pass individual argument values; modern targets use `ProcessStartInfo.ArgumentList`, while the `netstandard2.0` backend delegates token formatting to CliWrap;
- redirect stdout/stderr where needed;
- support binary stdout without converting the entire stream to text;
- support binary-safe stdin streaming for tools/options that consume standard input, such as PostgreSQL 17+ `pg_dump --filter=-`;
- support `CancellationToken`;
- support timeout configuration;
- attempt to terminate the complete process tree on cancellation/timeout where the target framework supports it;
- never include passwords or secrets in diagnostic command-line rendering.

The `netstandard2.0` compatibility backend must map execution results, cancellation, timeout, and failures back into PgCliSharp's own result/exception model. Public behavior should remain consistent across target frameworks.

## 9. Output model

Do not assume stdout is text. Some PostgreSQL tools can emit binary archives.

Execution APIs should support streams and file-based output as appropriate. A common result may contain fields such as exit code, duration, parsed executable version, and stderr diagnostics, but should not force large stdout payloads into a string or byte array.

## 10. Exceptions and diagnostics

Use library-specific exception types for:

- executable not found/start failure;
- executable version mismatch;
- unsupported option for selected PostgreSQL version;
- invalid option combination;
- timeout/cancellation-related execution failure where appropriate;
- non-zero process exit.

Runtime user-facing messages are localized according to `docs/localization.md`.

Exceptions should expose machine-readable structured properties in addition to localized message text whenever practical. Callers must not need to parse localized strings to understand the failure.

## 11. Specification-first tool implementation

New PostgreSQL CLI wrappers follow [the tool implementation workflow](tool-implementation-workflow.md) defined by ADR-0011.

Before the complete public API for a tool is considered stable enough to implement:

- compare PostgreSQL 10-18 documentation per major rather than projecting the newest option list backward;
- resolve ambiguous parser/default/constraint behavior with the corresponding upstream stable source;
- inspect official security/release history for patch-level backports;
- distinguish feature availability from spelling/alias availability;
- record the result in a machine-readable tool specification;
- perform inventory and API-binding completeness audits.

Version-varying runtime checks should be centralized in availability metadata. Exact executable versions must be supported when availability begins in a maintenance release.

Public option types remain tool-specific. Reusable internal serializers/validators should be extracted only after at least two tools demonstrate identical semantics; similar switch names alone are not sufficient evidence for a shared public abstraction.

## 12. Specification data and compatibility testing

The repository should maintain machine-readable compatibility data when it provides a net benefit, for example under:

```text
spec/postgresql/
  10/
  11/
  12/
  13/
  14/
  15/
  16/
  17/
  18/
```

This data can drive:

- implementation completeness checks;
- documentation generation;
- version support tests;
- future PostgreSQL major-version upgrades.

The authoritative source for CLI semantics is PostgreSQL official documentation and executable behavior.

CI performs structural validation for all top-level tool specification JSON files under `spec/postgresql/`. Source-level option-table comparison is a research-time audit whose result is recorded in the specification because CI should not depend on live upstream source availability.

Phase 1 stores the complete pg_dump compatibility inventory in `spec/postgresql/pg_dump.json`. It records per-major resolved option sets, short/long spellings, spelling availability, repeatability, required option arguments, wrapper/upstream defaults, format and compression rules, and patch-level availability. The inventory has also been compared mechanically against the official `REL_10_STABLE` through `REL_18_STABLE` pg_dump source option tables.

## 13. Test layers

Use three logical test layers:

1. Unit tests: typed options -> expected argument tokens, validation, parsing.
2. Compatibility tests: supported option inventory by PostgreSQL major version.
3. Integration tests: invoke real PostgreSQL executables/containers for representative end-to-end behavior.

Windows CI should also execute tests through a .NET Framework consumer target so the `netstandard2.0` package asset and CliWrap compatibility backend run in-process. CI should cover PostgreSQL 10-18 as far as reproducibly possible. Legacy versions may need isolated/containerized test environments.

## 14. Package and target framework policy

Package ID target: `PgCliSharp`.

Initial target-framework matrix:

```xml
<TargetFrameworks>netstandard2.0;net8.0;net10.0</TargetFrameworks>
```

Phase 0 validation confirmed this matrix across Linux, macOS, and Windows CI. Windows also executes a .NET Framework 4.8 test target against the `netstandard2.0` library asset so the legacy compatibility backend is exercised at runtime. Future changes to this accepted matrix require the ADR superseding workflow.

The core package should avoid unnecessary dependencies such as Npgsql or Microsoft.Extensions packages unless a clear project-wide benefit justifies them. ADR-0008 allows CliWrap specifically and only for the `netstandard2.0` execution backend because the .NET Standard 2.0 BCL does not provide equivalent argument/process-tree APIs.

## 15. NuGet and release policy

Use Semantic Versioning.

Planned progression:

- early previews: `0.x.y-alpha.n`
- feature-complete preview/release candidates before 1.0
- `1.0.0` only after public API review and compatibility validation

Release automation should:

- build in Release;
- run tests;
- create `.nupkg` and symbol `.snupkg`;
- include SourceLink and XML documentation;
- validate package metadata/version against the release tag;
- publish through NuGet Trusted Publishing/OIDC when available;
- attach release artifacts to the GitHub Release where useful.

Long-lived NuGet API keys should not be the preferred design.

## 16. Planned implementation order

1. Project/solution and execution infrastructure.
2. Version parsing/validation for PostgreSQL 10-18.
3. Full `pg_dump` typed option coverage.
4. `pg_restore` and `pg_dumpall`.
5. First NuGet preview and publication pipeline validation.
6. Backup/WAL tools.
7. Database-management and maintenance client tools.
8. `psql`, `pgbench`, and tools with richer stdin/stdout behavior.
9. Server applications in a clearly separated namespace/category.
10. Public API review and 1.0 stabilization.

## 17. Living specification and ADR policy

This document is intentionally a living, consolidated specification.

When a better design is discovered:

- follow the ADR workflow in `docs/adr/README.md` for material repository-wide decisions;
- do not rewrite an old Accepted ADR to hide the previous decision;
- create a superseding ADR when an Accepted decision changes;
- update this consolidated document in the same PR/commit;
- update tests and examples;
- keep chat discussions non-authoritative.

The repository, including its ADR history, not conversation history, is the durable project context.
