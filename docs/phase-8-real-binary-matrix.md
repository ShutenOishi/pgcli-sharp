# Phase 8 CP-02 real-binary matrix / 実バイナリ検証 matrix

## English

Baseline: CP-01 main `6f3f07e81186f2156cc7c5632e7b5c9c96db4f9c`.
Status: **Linux evidence checkpoint complete**: PR #15 and exact-main CI
37006276018 passed on `3f230bd8bddf59b0bed5aeb8bb7589c77b9f88bf`.
Exclusions remain unchanged. Phase 8 is not release-ready.

Official [source index](https://ftp.postgresql.org/pub/source/) and archive SHA256
files were checked on 2026-10-02. Versions/hashes are fixed in
`eng/postgresql-source-versions.json`; no floating latest version is selected.

| Major | Version | Representative scenarios | Additional mutation |
|---|---|---|---|
| 10 | 10.23 | backup/restore, finite/session psql, pgbench, initdb/resetwal dry-run/pg_ctl | none |
| 11 | 11.22 | same | none |
| 12 | 12.22 | same plus checksum check/disable/enable | none |
| 13 | 13.23 | same plus checksums | divergent live-source rewind |
| 14 | 14.24 | same plus checksums | divergent live-source rewind |
| 15 | 15.19 | same plus checksums | divergent live-source rewind |
| 16 | 16.15 | same plus checksums | divergent live-source rewind |
| 17 | 17.11 | same plus checksums | divergent live-source rewind |
| 18 | 18.6 | same plus checksums | divergent rewind; real 16.15-to-18.6 Copy upgrade |

Jobs use ubuntu-24.04/net10.0 with explicit source prefixes, verified archive
hashes and `--without-readline --without-icu` (SSL off by default; Zlib retained).
Caches validate version/provenance. Compiler inputs and executable hashes are
recorded; identical-source selection is not a bit-identical rebuild guarantee.
[ADR-0016](adr/0016-pin-real-postgresql-evidence-and-scope-exclusions.md) records
the evidence contract; no public library API or process backend changes.

### Owned fixtures and evidence

- Setup creates a new non-root cluster with no TCP and a unique owned Unix socket.
  It never uses an OS-installed/user cluster; ownership is registered before start.
- Representative wrapper scenarios run on all nine majors; checksum operations
  are excluded before 12 because pg_checksums does not exist.
- Rewind initializes with checksums, takes a streaming base backup, starts a
  standby, stops the source, promotes the standby and writes a divergent row.
  It writes a distinct source row and runs real PgRewind against the live source.
  Restarted target must be in recovery with the source row, not the fork row.
  This is not a no-op same-timeline rewind; recovery-conf is typed from 13.
- Upgrade initializes separate 16/18 clusters, creates real rows, stops the
  source and runs PgUpgrade Copy (not CheckOnly). It verifies rows/server major
  on 18 and restarts the old copy to verify the source remains usable. No
  link/swap mode or generated deletion script is run.
- Stop owned servers before deleting successful fixture roots. Failed migration
  fixtures retain their logs/data on the disposable runner for diagnosis.
- Required environment rejects missing/partial/unsupported settings. The
  collector validates actual CLI/server numeric versions and requires exactly
  one Passed TRX entry per enabled scenario. Missing/failed tests fail the job.
- Per-major `real-postgresql-<major>-<source SHA>` development CI artifacts contain
  JSON/TRX/failure logs. JSON records source/run SHA, CLI numeric/raw versions,
  executable hashes, server version/number, OS/image/TFM, build inputs, results
  and exclusions. They are not GitHub Release assets.
- Six collector unit tests cover manifest completeness, successful collection,
  exclusions, absent/failed TRX and wrong versions. These synthetic tests are not
  real PostgreSQL evidence.

### Explicit limitations / remaining Phase 8 work

| Scope | Reason/status |
|---|---|
| Windows/macOS real binaries | Not configured in this Linux matrix; 3-OS unit tests are separate. No irreproducibility claim. |
| Windows service lifecycle | Separate privileged service fixture still required. |
| Rewind on 10-12 | Manual historical recovery configuration fixture not implemented; this fixture uses 13+ recovery-conf. |
| Other patches/upgrade pairs | Only fixed patch points and one 16-to-18 pair are tested. |
| TLS/ICU/TTY | Not built/exercised by the scoped redirected-pipe environment. |
| Unlisted tools/options, forced reset, link/swap, full workload recovery | Not exercised; unit/spec tests do not substitute. |

The [CP-02 PR report](https://github.com/ShutenOishi/pgcli-sharp/pull/15)
indexes final head, merge SHA, exact CI and all nine outcomes. No API freeze or
publication authorization is inferred from this matrix.

Official references: [10 source build](https://www.postgresql.org/docs/10/install-procedure.html),
[18 source build](https://www.postgresql.org/docs/18/install-make.html),
[13 rewind](https://www.postgresql.org/docs/13/app-pgrewind.html),
[18 rewind](https://www.postgresql.org/docs/18/app-pgrewind.html),
[18 upgrade](https://www.postgresql.org/docs/18/pgupgrade.html).

## 日本語

Linux/net10.0 の実バイナリ証拠を10〜18の9メジャーへ拡張します。公式アーカイブと
SHA256 を固定し、使い捨てクラスタと専用 Unix socket で公開 API を実行します。
TCP は無効で、利用者所有環境や OS 既存クラスタには接続しません。上表は検証対象であり、
PR #15 と正確な main CI 37006276018 が合格し、Linux のチェックポイントは完了です。

全9版でバックアップ／リストア、有限・セッション psql、pgbench、初期化・WAL reset
dry-run・起動／停止を実行します。チェックサム操作は12以降、分岐クラスタの実巻き戻しは
復旧設定オプションを使う13以降です。16.15→18.6 はコピーで実移行し、移行後の行と
旧コピーの再起動を検証します。成功時のみ停止後に専用データを削除し、移行失敗時は保持します。

設定欠落・版不一致・TRX欠落・失敗を合格にせず、実 CLI／server 版・実行ファイルhash・
OS／TFM・結果・除外理由をJSONに保存します。synthetic test は実 PostgreSQL 証拠ではありません。
Windows／macOS実機・サービス、10〜12の旧復旧設定rewind、別patch／移行ペア、
TLS／ICU／TTY・未掲載シナリオは未検証です。除外は合格・再現不可能の意味ではありません。
API凍結・Phase 8全体の完了・公開承認も主張せず、公開設定は無効のままです。
