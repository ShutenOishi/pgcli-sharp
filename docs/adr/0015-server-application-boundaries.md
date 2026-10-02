# ADR-0015: Separate server applications and make detached-server I/O explicit

- Status: Accepted
- Date: 2026-10-02
- Supersedes: None
- Complements: ADR-0002, ADR-0003, ADR-0008, ADR-0011, ADR-0014

## Context

Phase 7 adds executables that initialize, start, migrate, rewind or repair clusters.
They need a visible category distinct from SQL clients. pg_ctl can leave postgres
running after pg_ctl exits; an inherited redirected stdout/stderr pipe can prevent
the one-shot runner from completing its drain.

## Decision

- Place all six wrappers, their per-tool Options types and server-specific values
  in `PgCliSharp.ServerApplications`. Existing root-namespace APIs do not move.
- Use the established internal finite process runner, version provider and
  availability primitives. Add server-specific public I/O/result types rather
  than exposing internal or backup/maintenance category types.
- Require an explicit `PgCtlOptions.LogFile` for Start and Restart. Emit `--log`
  so the detached postgres output is redirected by pg_ctl. Do not invent a file,
  silently weaken draining, or claim the server has exited when pg_ctl has exited.
- Treat pg_ctl Status exit codes 0/3/4 as domain statuses. Only the Status command
  accepts those codes; other operations retain ordinary failure semantics.
- pg_ctl `--options` and pg_upgrade `--old-options/--new-options` are upstream
  option fragments, preserved as ordered collections. They do not provide a raw
  argument tail for the wrapper. PostgreSQL itself interprets these fragments in
  subprocess invocations, which may use a shell. Callers supply trusted fragments;
  PgCliSharp directly launches the selected tool without shell mediation.
- Model finite values as enums, numeric pairs/WAL names/initdb settings as value
  objects, and mutually exclusive transfer/checksum/wait policies as single values.
- Preserve upstream omitted defaults, including initdb checksums off through 17
  and on from 18. Explicit Copy on pg_upgrade before 16 is expressed by omission;
  explicit false initdb checksums before 18 is also expressed by omission.
- The executable version is the CLI version, not proof of data-directory version,
  an upgrade source version, filesystem capabilities, ICU build support or cluster
  consistency. Those stateful checks remain PostgreSQL's responsibility.
- Cancellation and timeout remain best effort; no mutation rollback is promised.
  Cancelling pg_ctl is not a request to stop an independently running server.
  A real test must issue a separate Stop and clean up only its owned directory.

## Validation

Version-boundary, complete spec/API/runtime mapping, command-token, numeric-pair,
locale/authentication, semantic-status and execution tests are required. Windows
net48 continues to exercise the netstandard2.0 asset. Disposable Linux PostgreSQL
16/18 integration covers initialization, checksum changes, dry-run reset and
start/status/stop. Rewind/upgrade data migration and Windows service lifecycle
remain explicitly outside this representative real-binary smoke scope.

## 日本語

サーバー管理 API を `PgCliSharp.ServerApplications` に分離します。pg_ctl の起動・再起動は
独立したサーバーの出力パイプが終了待ちを妨げないよう、明示ログファイルを必須にします。
省略時は PostgreSQL の既定値を維持し、キャンセルで変更が元に戻るとは保証しません。
upstream が受け取るサーバー起動オプション断片は信頼できる入力に限定します。
