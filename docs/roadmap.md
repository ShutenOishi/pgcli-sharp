# PgCliSharp Roadmap

> This roadmap describes planned work. Accepted architecture/product decisions are recorded separately in [Architecture Decision Records](adr/README.md). Proposed ADRs are not commitments until accepted.

## Product direction

Build a strongly typed .NET API for PostgreSQL command-line tools with explicit executable selection, PostgreSQL 10-18 compatibility, proactive validation, bilingual public documentation, localized runtime diagnostics, and automated NuGet publishing.

## Phase 0 - Foundation

**Status: Complete (2026-09-18).** The accepted foundation is implemented and validated on Linux, Windows, and macOS. Windows additionally exercises the `netstandard2.0` asset through a .NET Framework 4.8 consumer test.

- Create solution/project structure.
- Establish package ID and NuGet metadata.
- Implement target-framework matrix.
- Enable nullable reference types and strict build/analyzer settings where practical.
- Enable XML documentation and SourceLink.
- Add localization resources (English neutral + Japanese).
- Implement common exceptions/diagnostics.
- Implement process runner abstraction.
- Use the BCL process backend on modern .NET and a conditional CliWrap backend on `netstandard2.0`.
- Implement cancellation and timeout behavior.
- Implement executable version parser/cache.
- Add PostgreSQL 10-18 version metadata.
- Establish CI.

## Phase 1 - pg_dump

**Status: Complete (2026-09-18).** PostgreSQL 10-18 option coverage, version/patch availability metadata, typed argument generation, validation, binary-safe stdout, PostgreSQL 17+ filter stdin, bilingual public API documentation, and cross-platform tests are implemented. The maintained compatibility specification is `spec/postgresql/pg_dump.json`.
- Inventory official pg_dump options for PostgreSQL 10-18.
- Implement `PgDumpOptions`.
- Use enums for finite option values.
- Use value objects for structured option values such as compression.
- Support repeatable options with collections.
- Implement version availability validation.
- Implement option-combination validation.
- Support binary/stdout streaming safely, including stdin for PostgreSQL 17+ `--filter=-`.
- Add bilingual XML docs to all public API.
- Add unit/compatibility/integration tests.

This phase defines the design template for later tools.

## Phase 2 - pg_restore and pg_dumpall

**Status: Complete (2026-09-18).** The typed `pg_restore` and `pg_dumpall` APIs, PostgreSQL 10-18 compatibility specifications, centralized availability checks, execution/I/O support, backup/restore trio tests, bilingual documentation, and cross-platform completion gate are complete.

Phase 2 follows ADR-0011 and `docs/tool-implementation-workflow.md`.

- Research `pg_restore` and `pg_dumpall` independently across PostgreSQL 10-18 before completing either public API.
- Create machine-readable `spec/postgresql/pg_restore.json` and `spec/postgresql/pg_dumpall.json`.
- Verify each per-major option inventory against upstream option tables/parser source and security/release history where necessary.
- Complete typed option coverage for PostgreSQL 10-18.
- Build centralized runtime availability metadata for each tool before scattering version checks through validators.
- Compare `pg_dump`, `pg_restore`, and `pg_dumpall` connection semantics; extract shared internal argument/validation helpers only for behavior proven identical.
- Keep public Options classes tool-specific.
- Complete backup/restore trio tests, including cross-tool archive/I/O scenarios where meaningful. **Implemented.**
- Keep spec-to-API and spec-to-runtime availability coverage tests in CI so inventory drift fails the build. **Implemented.**
- Do not update Phase 2 release metadata until both tools pass the full implementation/completeness gate.

## Phase 3 - First NuGet preview

**Status: Implementation complete; external publication deferred (2026-09-19).** The release pipeline and `PgCliSharp 0.1.0-alpha.1` package candidate were validated at source commit `cccf8d9fbe1f2e1104676ab94a7863209c0220dd`. Its exact main CI passed on Linux/macOS/Windows. No NuGet package, `v0.1.0-alpha.1` tag/Release, `phase-3` tag/Release, or Release asset was published. ADR-0012 defers that decision and revalidation to the final release phase.

- Finalize package metadata. **Prepared for `0.1.0-alpha.1`.**
- Produce `.nupkg` and `.snupkg`. **Validated in CI/release workflows.**
- Verify SourceLink, repository commit metadata, XML docs, and clean local package consumption. **Automated in `eng/verify-nuget-package.sh`.**
- Configure GitHub Actions release workflow. **Implemented in `.github/workflows/release.yml`.**
- Configure NuGet Trusted Publishing/OIDC. **Workflow uses the `release` environment and `NuGet/login@v1`; nuget.org policy must match the repository/workflow/environment.**
- Validate package version/tag/source-commit consistency. **Automated by `.github/nuget-release.json` and the release workflow.**
- Publish first preview. **Deferred to the final release phase under ADR-0012. The prepared source commit and package identity are preserved for re-audit.**

## Phase 4 - Backup and WAL tools

**Status: Complete (2026-09-19).** Specification-first research, typed APIs, runtime availability/validation, deterministic argument and I/O handling, execution tests, bilingual documentation, and the Linux/macOS/Windows completion gate are implemented. The maintained compatibility specifications are the five Phase 4 JSON files under `spec/postgresql/`.

- `pg_basebackup` — PostgreSQL 10-18.
- `pg_receivewal` — PostgreSQL 10-18.
- `pg_recvlogical` — PostgreSQL 10-18.
- `pg_verifybackup` — PostgreSQL 13-18; earlier majors are rejected before process startup.
- `pg_combinebackup` — PostgreSQL 17-18; earlier majors are rejected before process startup.
- Model option and whole-tool availability explicitly and test the supported/unsupported boundaries.
- Preserve binary-safe stdout for pg_basebackup tar output and pg_recvlogical streaming.
- Keep Windows .NET Framework 4.8 coverage so the `netstandard2.0` compatibility asset remains validated.

External NuGet/GitHub Release publication remains deferred under ADR-0012. Phase 4 completion is recorded by the reviewed main merge and exact-commit CI rather than a new tag or Release.

## Phase 5 - Database management and maintenance tools

**Status: Complete (2026-09-21).** Specification-first research, typed APIs, centralized availability/validation, deterministic argument generation, execution/I/O behavior, completeness tests, bilingual public API documentation, and the Linux/macOS/Windows completion gate are implemented.

- `createdb` — PostgreSQL 10-18.
- `dropdb` — PostgreSQL 10-18; `--force` is modeled from PostgreSQL 13.
- `createuser` — PostgreSQL 10-18, including version-aware membership aliases and PostgreSQL 16+ role attributes.
- `dropuser` — PostgreSQL 10-18, including interactive role-name input.
- `vacuumdb` — PostgreSQL 10-18, including versioned maintenance controls through PostgreSQL 18 `--missing-stats-only`.
- `reindexdb` — PostgreSQL 10-18, including concurrent/jobs/tablespace version boundaries.
- `clusterdb` — PostgreSQL 10-18.
- `pg_isready` — PostgreSQL 10-18; exit codes 0-3 are returned as typed readiness statuses rather than ordinary process failures.
- `pg_amcheck` — PostgreSQL 14-18; PostgreSQL 10-13 are rejected before process startup.

The nine maintained inventories are under `spec/postgresql/`, with research in `docs/database-maintenance-phase-5.md` and completion evidence in `docs/phase-5-completion.md`. Spec-to-API and spec-to-runtime availability coverage tests, argument/validation tests, and execution tests keep the implementation synchronized with those inventories.

External NuGet/GitHub Release publication remains deferred under ADR-0012. Phase 5 completion is recorded by the reviewed main merge and exact-commit CI rather than a new tag or Release.

## Phase 6 - Rich I/O tools

**Status: Complete (2026-09-22).** Specification-first PostgreSQL 10-18 research, typed APIs, tool-specific rich I/O models, redirected-session infrastructure, upstream-aligned validation, semantic exit statuses, bilingual public documentation, and the Linux/macOS/Windows completion gate are implemented.

- `psql` — PostgreSQL 10-18, with ordered/interleaved command/file actions, typed variable/separator values, PostgreSQL 12+ CSV output, finite `PsqlIo`, and a long-lived redirected `PsqlSession`.
- `pgbench` — PostgreSQL 10-18, including PostgreSQL 11/13/15/17 option changes, PostgreSQL 13+ server-side initialization step `G`, PostgreSQL 12+ runtime exit status 2, script-weight semantics, and upstream-determinable mode/logging/progress/partition/retry validation.
- `PsqlSession` exposes writable stdin after process startup, explicit EOF, cancellation, timeout/process-tree termination, caller-owned stdout/stderr streaming, and completion metadata.
- Redirected psql sessions are pipe based and explicitly are not TTY/PTY emulation.
- The internal session lifecycle is separate from the established one-shot process runner. Modern .NET uses `System.Diagnostics.Process`; the `netstandard2.0` path uses CliWrap without leaking dependency types.
- Windows CI exercises the `netstandard2.0` surface through .NET Framework 4.8 in addition to Linux/macOS modern targets.

The maintained inventories are `spec/postgresql/psql.json` and `spec/postgresql/pgbench.json`. Research and implementation notes are in `docs/rich-io-phase-6.md`, the lifecycle decision is ADR-0013, and completion evidence is recorded in `docs/phase-6-completion.md`.

External NuGet/GitHub Release publication remains deferred under ADR-0012. Phase 6 completion is recorded by the reviewed main merge and exact-commit CI rather than a new tag or Release.

## Phase 7 - Server applications

Add server-side administrative executables in a clearly separated namespace/category, for example:

- `initdb`
- `pg_ctl`
- `pg_upgrade`
- `pg_rewind`
- `pg_checksums`
- `pg_resetwal`

Tools that can alter or recover data directories require especially explicit documentation and validation.

## Phase 8 - 1.0 stabilization

- Review all public APIs for naming and consistency.
- Review exception hierarchy and structured diagnostic properties.
- Verify English/Japanese XML documentation coverage.
- Verify English/Japanese resource coverage.
- Expand the representative real-PostgreSQL CI introduced by the Phase 0-6 follow-up review into a PostgreSQL 10-18 evidence matrix where reproducible maintained runners are available.
- Track CLI numeric version, server version, OS/TFM, scenario result, and any exclusion reason; do not count fake-runner tests as real PostgreSQL evidence.
- Keep backup/restore, finite/redirected psql, and pgbench scenarios as the initial real-executable baseline; isolate destructive/privileged Phase 4/5 scenarios in disposable environments before adding them.
- Verify Windows/Linux/macOS behavior where applicable and record where legacy PostgreSQL packages prevent a reproducible combination.
- Freeze breaking API changes.
- Complete README/examples.
- Explicitly decide whether the preserved Phase 3 `0.1.0-alpha.1` candidate is published or superseded, then revalidate the selected source/package/notes provenance before enabling any publication manifest.
- Release candidates followed by `1.0.0`.

See [Real PostgreSQL integration testing](integration-testing.md) for the current representative CI scope and expansion rules.

## Phase completion and deferred publication

Phase 0-2 retain their existing GitHub Releases. Starting with Phase 3, ADR-0012 separates implementation completion from external publication. A Phase is completed by a reviewed `main` merge, exact-commit Linux/macOS/Windows CI, compatibility/research evidence, and updated repository documentation. New NuGet pushes, GitHub Release tags, Releases, and Release assets are deferred until the final release phase.

The deferred Phase 3 publication manifests remain checked in for provenance, with `publication_enabled: false` and release source commit `cccf8d9fbe1f2e1104676ab94a7863209c0220dd`. The final release phase must revalidate that exact source before deciding whether to publish or explicitly supersede the prepared preview.

## Continuous work

For every phase:

- follow `docs/tool-implementation-workflow.md` for PostgreSQL CLI implementation work;
- complete compatibility research/specification and inventory/API audits before treating the public API as complete;
- create or supersede ADRs for material repository-wide decisions;
- update consolidated canonical docs in the same change;
- extend compatibility specifications;
- add regression tests for discovered PostgreSQL/version differences;
- keep release notes/changelog;
- keep publication manifests disabled during implementation phases; record Phase completion in roadmap/research/completion documentation after a green all-platform CI run;
- avoid relying on chat history as project specification.

## Future PostgreSQL releases

New PostgreSQL major versions should be added by extending version metadata, compatibility specifications, typed options, and tests without rewriting older-version support.

Pre-release PostgreSQL versions should not automatically be advertised as stable supported versions.
