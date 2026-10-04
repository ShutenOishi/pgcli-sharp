# Architecture and Compatibility

> This document is the consolidated current-state architecture. Decision rationale and historical changes are recorded in [Architecture Decision Records](adr/README.md). If an Accepted decision is replaced, preserve the old ADR and supersede it with a new ADR.

Key accepted decisions currently include ADR-0001 through ADR-0004, ADR-0007, and ADR-0011 through ADR-0018 plus ADR-0024. ADR-0005 was superseded by ADR-0008; ADR-0008 and ADR-0023 are superseded by ADR-0024; ADR-0006 and ADR-0010 have been superseded by ADR-0012.

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

### Configuration and offline commands (ADR-0018)

Every wrapper also supports lambda configuration through extension methods, offline
`Validate` and `CreateCommand`. Execution captures a private copy of Options,
ordered collections and environment before the first await. Existing instance
Options APIs remain source-compatible, including null-call overload resolution.
The immutable command description exposes exact tokens/environment explicitly;
default string output redacts all values. Offline patch assertions do not bypass
actual executable probing. Stream routing is metadata, not a generated pipeline.
See [Configuration and commands / 設定とコマンド生成](configuration-and-commands.md)
and [ADR-0018](adr/0018-offline-commands-and-configuration-snapshots.md).

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
- `netstandard2.0` shares the Process lifecycle with narrow argument/exit compatibility under ADR-0024;
- only the legacy asset references System.Management 10.0.10 for best-effort Windows descendant termination; CliWrap is removed from new assets.

Requirements:

- no shell mediation;
- pass individual argument values; modern targets use `ProcessStartInfo.ArgumentList`, while the `netstandard2.0` backend uses attributed .NET native argument serialization;
- redirect stdout/stderr where needed;
- support binary stdout without converting the entire stream to text;
- support binary-safe stdin streaming for tools/options that consume standard input, including PostgreSQL 17+ `pg_dump --filter=-`, `pg_restore` archive/filter stdin, and `pg_dumpall --filter=-`;
- support `CancellationToken`;
- support timeout configuration;
- attempt to terminate the complete process tree on cancellation/timeout where the target framework supports it;
- supervise stdout/stderr transfer faults on every target as lifecycle outcomes rather than waiting only for process exit;
- keep the configured cancellation/timeout deadline active while redirected output drains after process exit;
- bound abnormal cleanup after a best-effort termination attempt rather than waiting indefinitely for process exit or a caller-owned stream;
- never dispose caller-owned streams or claim that arbitrary cancellation-noncooperative stream implementations can be forcibly stopped;
- never include passwords or secrets in diagnostic command-line rendering.

The `netstandard2.0` compatibility backend must map execution results, cancellation, timeout, and failures back into PgCliSharp's own result/exception model. Public behavior should remain consistent across target frameworks.

Under ADR-0024, all internal awaits use explicit ConfigureAwait(false), and legacy
exit waiting subscribes to Exited before checking HasExited. There is no command
async iterator or dependency-startup context adapter. Caller context is not
mutated; controlled-context regressions remain required. On legacy Windows,
System.Management discovers descendants for best-effort termination. Modern
runtimes retain the native tree API, including netstandard assets loaded on a
runtime that exposes it. Older Unix runtimes without that API fall back to the
immediate child, as the former dependency did; no tree guarantee is claimed.

Phase 6 additionally separates finite one-shot execution from long-lived redirected process sessions under ADR-0013. A redirected `psql` session exposes programmatic duplex pipe I/O but is not a TTY/PTY and does not promise Readline, command-history, or terminal-emulation behavior. Long-running rich-I/O tools may stream both stdout and stderr to caller-owned destinations so memory usage does not have to grow with process duration. ADR-0014/0024 make session output faults part of lifecycle supervision on all targets and bound the `netstandard2.0` input bridge to approximately 1 MiB. Legacy `WriteAsync` therefore applies cancellation-aware backpressure; successful write completion means acceptance into the bounded delivery buffer, while `CompleteInput` drains already accepted bytes before EOF.

## 9. Output model

Do not assume stdout is text. Some PostgreSQL tools can emit binary archives.

Execution APIs should support streams and file-based output as appropriate. A common result may contain fields such as exit code, duration, parsed executable version, and stderr diagnostics, but should not force large stdout payloads into a string or byte array.

Results implement the minimal read-only `IPgExecutionResult` metadata contract;
tool-specific statuses and stderr ownership stay distinct. `PsqlSession.CompleteAsync`
signals EOF and awaits process completion/drain with cancellation, while preserving
the existing session timeout and caller-owned stream contract.

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

CI performs structural validation for all top-level tool specification JSON files under `spec/postgresql/`. Source-level option-table comparison is a research-time audit whose result is recorded in the specification because CI should not depend on live upstream source availability. Runtime coverage tests also bind the pg_dump specification directly to its availability catalog, including maintenance-release boundaries, and reject unknown explicit special-binding names.

Phase 1 stores the complete pg_dump compatibility inventory in `spec/postgresql/pg_dump.json`. It records per-major resolved option sets, short/long spellings, spelling availability, repeatability, required option arguments, wrapper/upstream defaults, format and compression rules, and patch-level availability. The inventory has also been compared mechanically against the official `REL_10_STABLE` through `REL_18_STABLE` pg_dump source option tables.

Phase 2 applies the same specification-first contract to `spec/postgresql/pg_restore.json` and `spec/postgresql/pg_dumpall.json`. Runtime availability catalogs cover major-version differences and the maintenance-release boundaries for security-backported `--restrict-key`. Spec-to-runtime/API coverage tests keep option inventories, public bindings, and centralized availability metadata synchronized.

Phase 4 extends the same contract to `pg_basebackup`, `pg_receivewal`, `pg_recvlogical`, `pg_verifybackup`, and `pg_combinebackup`. Whole-tool availability is explicit: `pg_verifybackup` is supported from PostgreSQL 13 and `pg_combinebackup` from PostgreSQL 17, with unsupported major versions rejected before process startup. Version-varying options use centralized runtime availability catalogs, while shared structured values such as WAL LSNs, tablespace mappings, manifest checksum algorithms, and filesystem sync methods are reused only where semantics are demonstrably identical.

Phase 4 also preserves explicit I/O models: pg_basebackup tar stdout and pg_recvlogical streaming output can flow directly into caller-owned streams without whole-payload buffering. Windows CI continues to compile and test the Phase 4 surface under the .NET Framework 4.8 test target so the `netstandard2.0` compatibility contract is exercised alongside Linux/macOS modern targets.

Phase 5 extends the same specification-first contract to the database-management and maintenance clients `createdb`, `dropdb`, `createuser`, `dropuser`, `vacuumdb`, `reindexdb`, `clusterdb`, `pg_isready`, and `pg_amcheck`. The first eight are modeled for PostgreSQL 10-18; `pg_amcheck` is a PostgreSQL 14+ whole-tool boundary and earlier majors fail before process startup.

Each Phase 5 tool retains a dedicated public Options type. Connection-token serialization and maintenance execution plumbing are shared internally only where the upstream audit established matching behavior. Version-varying options use centralized availability catalogs, repeatable selectors preserve caller order, and upstream-determinable invalid combinations are rejected before normal process startup.

Interactive-capable Phase 5 clients can receive caller-owned stdin and stdout streams through `PgMaintenanceIo`. `pg_isready` is intentionally exceptional: exit codes 0 through 3 are domain statuses (`AcceptingConnections`, `RejectingConnections`, `NoResponse`, and `NoAttempt`) and are not treated as ordinary nonzero execution failures; values outside that semantic range still fail as process execution errors. `pg_amcheck --install-missing[=SCHEMA]` is modeled as an option with an optional argument rather than an arbitrary command-line escape hatch.

Phase 5 completeness tests bind every machine-readable inventory entry to a public property or explicit special binding and bind every version-varying long option to centralized runtime availability metadata. Argument/validation and execution tests cover major version boundaries, repeatable ordering, optional arguments, version mismatch, timeout/environment forwarding, stdin/stdout forwarding, cancellation, and pg_isready semantic exit codes. Windows continues to execute the test suite under .NET Framework 4.8 in addition to modern Linux/macOS targets.

Phase 6 extends the specification-first contract to `psql` and `pgbench`, both modeled for PostgreSQL 10-18. Their canonical inventories are `spec/postgresql/psql.json` and `spec/postgresql/pgbench.json`.

Finite psql execution uses the tool-specific `PsqlIo` model for optional caller-owned stdin/stdout/stderr. Command and file actions share one ordered collection because upstream permits `--command` and `--file` to repeat and interleave. Variable assignments preserve unset versus empty values, and version-specific output features such as PostgreSQL 12+ CSV are validated before process startup. psql's documented exit statuses 0-3 are returned as `PsqlExitStatus` values.

ADR-0013 adds a separate internal long-lived redirected-process lifecycle rather than weakening the one-shot `IProcessRunner`. `PsqlSession` exposes writable stdin after process start, explicit EOF, streamed stdout/stderr, cancellation, timeout/process-tree termination, and asynchronous completion metadata. All targets share `System.Diagnostics.Process` lifecycle supervision, with narrow legacy compatibility under ADR-0024. Redirected sessions are explicitly pipe based and are not TTY/PTY emulation.

pgbench remains finite execution and uses `PgBenchIo` for caller-owned stdout/stderr; no public stdin contract is exposed because the audited PostgreSQL 10-18 CLI has no stdin workload interface. The wrapper models PostgreSQL 11/13/15/17 option changes, PostgreSQL 13+ server-side initialization step `G`, the report option rename, the PostgreSQL 17 `-d` reassignment while emitting stable `--debug`, script weights and script-count limits, initialization-versus-benchmark mode restrictions, logging/progress/partition/retry constraints, and the historical exit-status boundary where runtime status 2 is defined from PostgreSQL 12.

Phase 6 tests cover spec-to-API/runtime availability, deterministic serialization, semantic exit statuses, caller-owned stderr streaming, executable-version mismatch, and the long-lived session lifecycle. Real process-session tests verify writes after startup, explicit EOF, timeout, and cancellation. Windows executes the suite through .NET Framework 4.8 as well as modern targets, exercising the `netstandard2.0` compatibility session backend.

Phase 7 separates server applications into `PgCliSharp.ServerApplications` under
ADR-0015. Six dedicated Options APIs retain centralized option availability and
reuse the existing finite execution backend internally. Server-specific I/O and
result types keep that category visible without moving earlier public types.
`pg_checksums` is a PostgreSQL 12+ whole-tool boundary. pg_ctl Status preserves
0/3/4 domain statuses; Start/Restart require explicit log redirection for the
independently running server. Upstream server options remain ordered, trusted
fragments for PostgreSQL itself, rather than arbitrary wrapper argument tails.
The wrapper preserves initdb checksum defaults and historical short-option/omission
semantics. Cluster state, migration compatibility and filesystem/build capabilities
remain upstream checks. Cancellation does not promise rollback or server shutdown.
See [Phase 7 research and usage](server-applications-phase-7.md).

## 13. Test layers

Use three logical test layers:

1. Unit tests: typed options -> expected argument tokens, validation, parsing.
2. Compatibility tests: supported option inventory by PostgreSQL major version.
3. Integration tests: invoke real PostgreSQL executables/containers for representative end-to-end behavior.

Phase 8 CP-02 established pinned official-source PostgreSQL 10-18 Linux builds under historical ADR-0016. ADR-0022 retains that evidence and adds required native Windows/macOS PostgreSQL 18/net10.0 representative execution as CP-05. It requires actual source/binary/server versions, hashes, owned data directories and unique Passed TRX evidence. CI and independent artifact inspection must succeed before reporting this checkpoint complete. Native older versions/other TFMs, service lifecycle, TLS/ICU/TTY/compression and additional migration/destructive scenarios remain outside this scope. See `docs/phase-8-native-os.md` and `docs/integration-testing.md`. This checkpoint does not promote 1.0 or authorize publication.

Windows CI should also execute tests through a .NET Framework consumer target so the `netstandard2.0` package asset and compatibility backend run in-process. CI should cover PostgreSQL 10-18 as far as reproducibly possible. Legacy versions may need isolated/containerized test environments.

## 14. Package and target framework policy

ADR-0017 adds a reviewed compiled public/protected API baseline shared by all
library assets. Unreviewed signature drift fails CI; breaking shape changes are
frozen for stabilization and require an ADR, compatibility/migration and versioning
rationale. README C# examples compile on every consumer target without execution.
This is a shape gate, not a substitute for semantic compatibility, native OS
evidence or publication approval. See [CP-03 review](phase-8-api-review.md).

Package ID target: `PgCliSharp`.

Initial target-framework matrix:

```xml
<TargetFrameworks>netstandard2.0;net8.0;net10.0</TargetFrameworks>
```

Phase 0 validation confirmed this matrix across Linux, macOS, and Windows CI. Windows also executes a .NET Framework 4.8 test target against the `netstandard2.0` library asset so the legacy compatibility backend is exercised at runtime. Future changes to this accepted matrix require the ADR superseding workflow.

The core package should avoid unnecessary dependencies such as Npgsql or Microsoft.Extensions packages unless a clear project-wide benefit justifies them. ADR-0024 removes CliWrap from new assets and permits System.Management only for legacy Windows descendant discovery. Modern assets keep no execution runtime dependencies. The preserved alpha.2 candidate retains its original CliWrap dependency lock and source.

## 15. NuGet and release policy

ADR-0025 freezes new features until 1.0: only bug fixes, verification and
documentation improvements proceed. Unpublished `1.0.0-rc.1` preparation uses a
separate Process-backend lock/profile and a successful exact-main source selected
after source preparation CI. All 18 source jobs and three-OS candidate audits are
mandatory. Alpha.2 remains historical evidence, not current-backend distribution.
RC completion does not promote 1.0 or enable publication.

日本語: 1.0までは不具合修正・検証・文書改善に限定します。未公開rc.1は準備mainの
全18CI成功後に固定し、別lockと3OS候補監査を使います。旧alpha.2を保存し、
RC完了・1.0昇格・外部公開を区別します。

ADR-0019 licenses original PgCliSharp code/documentation under MIT. Packages declare
the MIT expression and include the root LICENSE and bilingual licensing guide.
PostgreSQL executables are externally supplied, not bundled or relicensed.
Dependencies retain their own terms; resolved transitive-license auditing remains
a final publication gate. See [Licensing / ライセンス](licensing.md).

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

ADR-0012 defers all new external publication from Phase 3 until the final release phase. The prepared Phase 3 source commit is `cccf8d9fbe1f2e1104676ab94a7863209c0220dd`; its package candidate remains `PgCliSharp 0.1.0-alpha.1`. Release manifests record this provenance with publication disabled.

The NuGet and Phase release workflows are manual-only. They require an explicit dispatch confirmation and `publication_enabled: true` in the reviewed manifest. When eventual publication is authorized, the workflows rebuild and revalidate the recorded source commit rather than silently packaging whichever commit happens to be current. Trusted Publishing/OIDC remains the preferred NuGet credential mechanism.

Ordinary CI packages are development artifacts, not publication candidates. Their artifact name and provenance record include the exact CI source SHA and explicitly state `publication_candidate=false`; package verification still checks SourceLink/repository commit against that source. This does not change the preserved Phase 3 manifest source or package version.

ADR-0021 selects unpublished alpha.2 from tested main
`0b7a2bb2e4dc1c1ed016181b2598645a1e3a8d5d`, retaining the original ADR-0020 selection as history.
CI and manual `release.yml` share the read-only `candidate-preflight.yml` build/audit
controls. The version/notes overrides, SDK and lock are fixed; source main CI and
ancestry, own payload/notices, PDB SourceLink/DLL pairing and isolated consumer
compilation/runtime are checked. All OS cross-compile net48; Windows also runs the netstandard asset on net48.
The publisher uses only preflight packages whose hashes/source/version match.

Direct managed completion stress evidence keeps the 15-second deadline and does
not claim the old PowerShell timeout's cause is known. The legacy Microsoft
reference package is excluded from shipped contents, retains its original terms
and is not relabeled MIT. The wrapper's distribution-scope review does not clear
downstream distributions. Publication remains disabled; explicit approval,
reviewed enablement and account-side Trusted Publishing verification are required.
Preview engineering completion, 1.0 acceptance and external publication are separate.

## 16. Planned implementation order

1. Project/solution and execution infrastructure.
2. Version parsing/validation for PostgreSQL 10-18.
3. Full `pg_dump` typed option coverage.
4. Full typed `pg_restore` and `pg_dumpall` coverage, including archive/script I/O and cross-tool backup/restore contracts.
5. First NuGet preview/package-pipeline validation; external publication remains deferred by ADR-0012.
6. Backup/WAL tools.
7. Database-management and maintenance client tools.
8. `psql`, `pgbench`, and tools with richer stdin/stdout behavior, including ordered psql actions and explicit redirected-session semantics.
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
