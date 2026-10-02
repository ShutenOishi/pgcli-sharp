# Phase 7 — Server applications

## English

Phase 7 adds six independently typed server-administration wrappers in
`PgCliSharp.ServerApplications`. Executable paths are explicit, the CLI major is
selected from PostgreSQL 10-18, and the actual executable version is checked.
`pg_checksums` exists from PostgreSQL 12; PostgreSQL 10-11 are rejected in its
constructor. PostgreSQL 11's separate `pg_verify_checksums` is outside this phase.

### Research and audit

On 2026-10-02 all applicable per-major official reference manuals and parser sources
in `REL_10_STABLE` through `REL_18_STABLE` were read independently. The six JSON
inventories under `spec/postgresql/` record official URLs, resolved options,
API mappings, short/long spelling availability, source table inventories and
SHA256 fingerprints for both the exact C and SGML contents reviewed.

Source locations: `src/bin/initdb/initdb.c`, `src/bin/pg_ctl/pg_ctl.c`,
`src/bin/pg_upgrade/option.c`, `src/bin/pg_rewind/pg_rewind.c`,
`src/bin/pg_checksums/pg_checksums.c`, `src/bin/pg_resetwal/pg_resetwal.c`.
Official rendered reference URLs are indexed for every major in each spec.
The matching reference SGML files are `initdb.sgml`, `pg_ctl-ref.sgml`,
`pgupgrade.sgml`, `pg_rewind.sgml`, `pg_checksums.sgml`, `pg_resetwal.sgml`.

Inventory differences were accounted for: pre-parsed help/version utilities,
pg_ctl short-only/service options and command operands, initdb `-L`, historical
`--noclean/--nosync` aliases, and source-supported options absent from some manuals.
`initdb --show` exists in source throughout 10-18 although documented only from 17.
`pg_resetwal --pgdata` is source-supported from 11 although documented only from 17.
The JSON resolved inventory therefore includes documented **and source-supported**
entries; it is not a claim that every entry appeared in the rendered manual.

| Tool | Audited changes |
| --- | --- |
| initdb | 11: group access/WAL size; 14: discard-caches/no-instructions; 15: ICU/provider; 16: repeated settings/ICU rules; 17: builtin locale/sync method; 18: checksums default on, disable flag, selective data-file syncing, PG_UNICODE_FAST |
| pg_ctl | Options stable across 10-18; logrotate operation begins at 12; Windows service options are separately modeled |
| pg_upgrade | 12: clone/socket directory; 15: no-sync; 16: explicit copy; 17: copy-file-range/sync method; 18: swap/statistics/char signedness |
| pg_rewind | 12: no-sync; 13: recovery-conf/restore-target-wal/no-ensure-shutdown; 15: config-file; 17: sync method |
| pg_checksums | Whole tool begins at 12; 15: zero filenode accepted; 17: sync method |
| pg_resetwal | 10: short options (plus first-position help/version); 11: long aliases/WAL size; 12: next XID minimum 3; 15: next multixact zero and maximum offset become accepted; 17: commit-timestamp minimum 3 instead of 2; 18: char signedness |

### Validation and I/O contract

- Optional environment keys inherit the calling process. Required directory inputs
  are resolved from the specific upstream environment keys where supported.
  pg_resetwal and pg_rewind require their explicit directory properties.
- WAL size is a power of two from 1-1024 MiB. ID pairs have named components and
  invariant-culture serialization; XID/offset boundaries follow the audited parser.
- initdb authentication rejects host-only/local-only mismatches. Password prompt
  and file are exclusive. ICU/builtin parameters require their provider. ICU needs explicit icu-locale in 15; locale fallback starts at 16. Combined auth=ident/peer maps the other connection type to peer/ident. Builtin
  values and PostgreSQL 18's UnicodeFast boundary are explicit. ICU build/locale
  availability, encodings and server GUC names remain upstream checks.
- `PgChecksumsMode` and `PgUpgradeTransferMode` prevent contradictory flags.
  Filenode filtering is restricted to checksum-check mode. Rewind requires exactly
  one source, and recovery configuration requires a server connection string.
- All normal invocations are individual argument tokens. `PgServerIo` streams
  caller-owned stdin/stdout/stderr; null stderr retains captured original diagnostics.
  Help is available through `GetHelpAsync`. Existing finite process infrastructure
  supplies environment, cancellation, timeout and version checking.
- pg_ctl Start/Restart require a LogFile (ADR-0015). Status preserves exit codes
  0, 3 and 4 as `PgCtlServerStatus`; code 1 and unexpected codes still throw.
  Service operations are Windows-only. syncfs is Linux-only; filesystem transfer
  capabilities remain upstream checks.
- Upstream server-option fragments are ordered and trusted, not wrapper argument
  tails. PostgreSQL may interpret them through its own subprocess shell paths.

### Operations affecting data

A successful process result does not prove cluster consistency. Link upgrades can
make the old cluster unusable after new-server writes; Swap upgrades modify the old
cluster during transfer. pg_rewind may modify its target; even dry-run can initiate
shutdown recovery unless NoEnsureShutdown is supplied. pg_resetwal is last-resort
repair and can leave logically inconsistent data. Use isolated cluster copies for
validation, keep verified backups, and follow the corresponding PostgreSQL manual.
The library does not infer filesystem state or guarantee rollback on interruption.

### Validation evidence scope

Deterministic compatibility/argument/execution tests cover the six APIs, including
version boundaries and fake-runner I/O. Representative real PostgreSQL integration
uses disposable directories on Linux with PostgreSQL 16 and 18. It runs initdb,
checksum check/disable/enable, pg_resetwal dry-run, and pg_ctl start/status/stop.
This is separate from the full Phase 8 historical/OS matrix. Fake-runner tests are
not real-executable evidence; rewind, upgrade migration and Windows service
lifecycle are not claimed as real-binary scenarios in Phase 7.

## 日本語

Phase 7 は `PgCliSharp.ServerApplications` に `InitDb`、`PgCtl`、`PgUpgrade`、
`PgRewind`、`PgChecksums`、`PgResetWal` を追加します。全ツールは専用 Options を持ち、
実行ファイルの明示パス・CLI 版の指定・実際の版確認を維持します。
`PgChecksums` は PostgreSQL 12 以降で、10〜11 はプロセス起動前に拒否します。

PostgreSQL 10〜18 の各版について公式マニュアルと stable ソースを独立比較し、
オプション一覧・短縮名／長い名前・版の境界・API 対応・取得内容の SHA256 を JSON に記録しました。
PostgreSQL 18 の initdb チェックサム既定値変更も、省略と明示指定を分けて扱います。

認証・ロケール・WAL サイズ・ID 範囲・ソースの排他指定などを実行前に検証し、
クラスタ状態・ファイルシステム・ICU ビルド固有の確認は PostgreSQL が実施します。
pg_ctl の起動・再起動はログファイル必須です。状態確認の終了コード 0／3／4 は
稼働・停止・ディレクトリ利用不可として返します。呼び出し側所有のストリームは破棄しません。

アップグレード・巻き戻し・WAL リセットはデータへ影響する操作です。キャンセルで元に戻る
保証はありません。upstream の起動オプション断片には信頼できる入力のみ使用してください。
実バイナリ検証は Linux の PostgreSQL 16／18 に作成した破棄可能なクラスタを使用し、
初期化・チェックサム操作・WAL リセットの dry-run・起動／状態確認／停止を検証します。
実際の巻き戻し・アップグレード移行・Windows サービス操作と全履歴版の実行検証は Phase 8 の課題です。
NuGet・タグ・Release の外部公開延期は ADR-0012 に従って維持します。
