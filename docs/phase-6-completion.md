# Phase 6 Completion Evidence

Phase 6 covers the roadmap's rich-I/O clients `psql` and `pgbench`. It follows ADR-0011 specification-first implementation, ADR-0013 redirected-session lifecycle separation, and ADR-0012 deferred external publication.

## Implemented tool scope

- `psql`: PostgreSQL 10-18.
- `pgbench`: PostgreSQL 10-18.

Both tools use explicit executable paths and validate the executable's PostgreSQL major version before normal execution. psql supports both finite execution and a long-lived redirected duplex session. pgbench remains finite execution with caller-owned stdout/stderr streaming.

## Compatibility evidence

Canonical machine-readable specifications:

- `spec/postgresql/psql.json`
- `spec/postgresql/pgbench.json`

Research and upstream-source findings are recorded in `docs/rich-io-phase-6.md`.

Coverage and regression tests verify:

- every compatibility inventory entry has a public/API binding or explicit special binding;
- version-varying options agree with centralized runtime availability metadata;
- psql PostgreSQL 12+ CSV availability, ordered/interleaved actions, variable assignment states, separators, and documented exit statuses 0-3;
- pgbench PostgreSQL 11/13/15/17 option changes, PostgreSQL 13+ server-side initialization step `G`, and PostgreSQL 12+ runtime exit status 2;
- pgbench script-weight semantics and script-count limit;
- initialization-versus-benchmark mode separation and logging/sampling/aggregation/progress/partition/retry hard constraints;
- executable-version mismatch before normal execution;
- caller-owned stdout/stderr streaming without mandatory duplicated stderr buffering.

## Redirected psql session evidence

ADR-0013 separates the long-lived process lifecycle from the one-shot runner. `PsqlSession` provides writable stdin after startup, explicit `CompleteInput()` EOF, caller-owned stdout/stderr through `PsqlSessionIo`, asynchronous completion metadata, cancellation, timeout, and process-tree termination. It is explicitly a redirected pipe session, not TTY/PTY emulation.

The internal implementation uses `System.Diagnostics.Process` on modern .NET and CliWrap on `netstandard2.0` without exposing CliWrap types. Real process-session tests verify post-start writes, EOF, cancellation, and timeout. Windows CI includes the .NET Framework 4.8 target and therefore exercises the `netstandard2.0`/CliWrap path.

## Cross-platform implementation gate

Implementation/test head before completion-documentation changes:

`77c6e298827d1cc0a2fce1ea96517cef147fc109`

GitHub Actions CI run:

`35587417468`

Result:

- Linux: success
- macOS: success
- Windows: success
- Windows includes .NET Framework 4.8 tests of the netstandard2.0 compatibility surface.

A fresh Linux/macOS/Windows CI run is required after this completion-documentation commit and before PR #11 is merged. The PR checks are the authoritative final pre-merge evidence.

## Public I/O surface

- `PsqlIo`: optional finite stdin/stdout/stderr.
- `PsqlSessionIo`: stdout/stderr destinations for a running redirected session; session stdin is exposed by `PsqlSession`.
- `PgBenchIo`: stdout/stderr only.

When a caller supplies an stderr destination, the corresponding finite result does not retain an additional unbounded stderr copy.

## Publication safeguard

ADR-0012 remains unchanged. Both `.github/nuget-release.json` and `.github/phase-release.json` retain `publication_enabled: false`.

Phase 6 completion does not create a NuGet push, Phase 6 tag, GitHub Release, or Release assets. The prepared Phase 3 publication source remains `cccf8d9fbe1f2e1104676ab94a7863209c0220dd` and stays deferred until final release-phase revalidation.

## Completion sequence

1. Commit the final Phase 6 documentation to PR #11.
2. Require that exact PR head to pass Linux/macOS/Windows CI.
3. Mark PR #11 ready for review and merge it to `main`.
4. Require the exact `main` merge commit to pass Linux/macOS/Windows CI.
5. Record final PR/merge/CI evidence in the PR completion report.

## Final reviewed merge evidence

The later repository evidence index confirms the final Phase 6 completion gate:

- PR: #11
- final PR head: `8bc8f6f52525294d150cd05470134c353148231b`
- final PR CI: https://github.com/ShutenOishi/pgcli-sharp/actions/runs/35691393213 — Linux/macOS/Windows success
- `main` merge commit: `6c85389946602b200d2822f7667eeef4ada49c48`
- exact-main CI: https://github.com/ShutenOishi/pgcli-sharp/actions/runs/35691571528 — Linux/macOS/Windows success

These records supplement the pre-documentation implementation head above; they do not replace or rewrite it.
