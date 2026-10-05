# Unpublished 1.0.0-rc.1 preparation / 未公開RC準備

## English

Status: source preparation; candidate selection and three-OS audit pending.
ADR-0025 limits remaining 1.0 work to bug fixes, validation and documentation.
All 25 wrappers, lambda configuration, offline commands and the reviewed API
remain unchanged. PR #26 / main `d66a1e229d230d5f6d7b6fdd8db0c79a85bba89c`
completed the Process-backend replacement through all 18 CI jobs at attempt 1;
[the exact artifact receipt](https://github.com/ShutenOishi/pgcli-sharp/pull/26#issuecomment-5978994921)
records Windows 30/3/3 stress and actual three-OS PostgreSQL evidence.

### Preparation CI history / 準備CI履歴

The first preparation run, CI `37242487375`, is intentionally retained as a
failed receipt rather than retried into acceptance. Linux/macOS deterministic
jobs and both native PostgreSQL 18 jobs completed successfully. Windows normal
tests completed successfully, but the bounded Windows stress step remained
reported in progress until the job itself ended in failure; GitHub did not
produce a Windows artifact and the job-log endpoint returned `BlobNotFound`.
The nine Linux PostgreSQL jobs, three candidate audits and shared preflight were
therefore skipped by dependency gating. This evidence does not establish a
library regression or runner root cause and does not satisfy ADR-0025's
attempt-1 all-18 gate. A new source commit and fresh CI run are required.

最初の準備CI `37242487375` は、再実行で合格扱いにせず失敗記録として保持します。
Linux／macOS通常ジョブとWindows／macOSのnative PostgreSQL 18は成功し、Windowsの
通常テストも成功しました。一方、Windows反復stepは実行中表示のままジョブが失敗終了し、
Windows成果物は生成されず、ログ取得も `BlobNotFound` でした。このためLinux 10〜18、
3OS候補監査、preflightは依存関係でskipされています。ライブラリ回帰やrunner原因とは
断定せず、ADR-0025の「attempt 1・全18ジョブ」条件も満たしません。新しいソースcommitで
新規CIを実行し直します。

1. Prepare current source documentation, RC notes and fail-closed audit controls.
2. Require fresh final PR and exact-main all-18 CI and actual artifact inspection.
3. Fix that successful source SHA/main CI as `1.0.0-rc.1`, retaining alpha.2
   source/lock/notes/history. Use SDK 10.0.100 and the new reviewed dependency lock.
4. Audit the actual nupkg/snupkg, all three PDB/DLL pairs, byte-exact source docs,
   exact MIT runtime dependencies and isolated consumers on Windows/Linux/macOS.
   Windows runs net48; netstandard itself is compile-only. Negative tests must fail.
5. Require exact final selection PR/main CI; retain links, hashes and exclusions.

No RC artifact exists merely because these instructions or notes exist. The
machine-readable `.github/release-candidate.json` and its verified CI artifacts
identify the currently fixed candidate. Historical alpha.2 includes CliWrap and
does not contain the new backend. Ordinary development artifacts are not RCs.

Real evidence is Linux PostgreSQL 10–18/net10 and native Windows/macOS 18/net10.
Other native majors/TFMs, service lifecycle, TLS/ICU/TTY, optional compression,
other upgrade pairs and unlisted scenarios remain excluded. A finite successful
stress run does not prove every possible scheduling behavior or the past root cause.

Publication remains disabled: no tag, Release, assets or NuGet push. Successful
RC engineering is not 1.0 promotion. Trusted Publishing account verification and
publication approval are still separate. RC feedback/fixes and final scoped
acceptance determine a future 1.0 selection.

## 日本語

現在はソース準備段階で、候補の固定と3OS監査は未完です。新機能を止め、
不具合修正・検証・文書改善に限定します。Process置換はPR #26と正確なmainで
全18CI・Windows30/3/3反復・3OS実PostgreSQL成果物まで確認済みです。
準備PR／mainを検証後、その成功済みソースをrc.1へ固定し、旧alpha.2の履歴を保持します。
別lock・固定SDKで実nupkg／snupkg・PDB／DLL・原文・依存・利用側と異常系を監査します。
説明ファイルがあるだけでは候補完成とせず、manifestと実CI成果物で判断します。
旧alpha.2は旧CliWrap実装で、通常開発artifactもRCとは別です。
未検証範囲と過去の失敗は残し、1.0昇格・公開承認・Trusted Publishing確認は別途必要です。
