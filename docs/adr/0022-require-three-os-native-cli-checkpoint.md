# ADR-0022: Require a three-OS native CLI checkpoint for 1.0

- Status: Accepted
- Date: 2026-10-03
- Supersedes: ADR-0016
- Complements: ADR-0012, ADR-0017, ADR-0021

## Context

The unpublished preview has completed its package/API audit. The user requires
representative real PostgreSQL execution on Windows, Linux and macOS before 1.0.
Three-OS deterministic/package tests alone do not establish this evidence.
External publication is postponed again; this decision authorizes no Release,
tag, NuGet push or publication-manifest enablement.

## Decision

- Retain the entire pinned Linux 10–18 source-build matrix and its migration,
  checksum, version/hash/TRX and owned socket-only environment contract.
- Add Windows/macOS at the same pinned PostgreSQL 18 archive/hash. Build native
  binaries: Visual Studio/Meson on Windows, native compiler/Meson on macOS.
  Meson/Ninja versions and the Windows Flex/Bison ZIP/hash are fixed. Record
  actual compiler, architecture, build options and executable hashes. Compiler
  and runner images can change; byte-identical builds are not guaranteed.
- Execute the established public-wrapper backup/custom-archive restore,
  finite/redirected psql and one-transaction pgbench scenario on net10.0.
  Create dedicated job-owned clusters/databases, verify the actual server version
  and data directory, and stop only a fixture whose ownership receipt matches.
  macOS uses a unique short Unix-socket directory with TCP disabled. Windows
  uses an ephemeral 127.0.0.1 port with no service registration or external bind.
  No OS/user cluster is reused. Provisioning scripts do not change the library's
  direct execution backend.
- Required configuration, absent/failed/duplicate TRX scenario results, wrong
  source/build/CLI/server version and wrong data directory fail closed. Preserve
  diagnostics and failed evidence; do not relabel skipped tests as real passes.
- Native fixture builds disable optional compression/TLS/ICU/Readline features.
  This exercises archive transport, not compression algorithms or TTY behavior.
  Native net8/net48, older majors on Windows/macOS, privileged service lifecycle,
  native migration/rewind, other patch/upgrade pairs and unlisted/destructive
  scenarios remain explicitly outside this checkpoint.
- CP-05 requires all nine Linux real jobs plus both native jobs, along with all
  deterministic/candidate/preflight jobs, on the exact final PR and main commits.
  Download and independently inspect both new evidence artifacts. This is a
  required 1.0 checkpoint, not automatic 1.0 acceptance/version promotion.
- Preserve frozen APIs, original Linux history and the selected alpha.2 source.
  Publication remains deferred and disabled under ADR-0012/0021.

## References

- [PostgreSQL 18 native platform notes](https://www.postgresql.org/docs/18/installation-platform-notes.html)
- [PostgreSQL 18 Meson installation](https://www.postgresql.org/docs/18/install-meson.html)
- Pinned official archive and SHA256: `eng/postgresql-source-versions.json`.
- Windows Flex/Bison v2.5.25 upstream archive: `eng/native_postgresql.py`.

## 日本語

1.0の必須チェックとして、既存Linux 10〜18にWindows／macOSの固定PostgreSQL 18を
追加します。各OSのネイティブ実行ファイルと公開APIでバックアップ／リストア、有限・
セッションpsql、pgbenchを実行し、版・hash・実結果・所有クラスタを確認します。
Windowsは専用ループバック、macOSは専用Unix socketを使い、利用者DBやサービスを
操作しません。未実行・失敗・重複・版／所有先不一致を合格にせず、診断を保存します。
旧版・別TFM・サービス・TLS／TTY・圧縮・移行等の除外は残します。
PR／main全ジョブと成果物確認でこのチェックポイントを完了しますが、1.0昇格や
公開は別判断です。公開延期・無効の設定・旧候補／ADR履歴を維持します。
