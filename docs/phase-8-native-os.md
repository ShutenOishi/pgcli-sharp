# CP-05 native CLI checkpoint / 3OS実CLIチェックポイント

## English

Status: implementation prepared; completion requires exact final PR/main CI and
independent artifact inspection. Baseline: preview-controls main
`846943fb739efdc30b4d5a6da469eef0d8855a98`. ADR-0022 supersedes the historical
Windows/macOS exclusion for the bounded representative scope below. No 1.0
promotion or external publication is performed.

Initial controls CI 37111052935 failed Windows/net10's
Session_OutputWriteFault_TerminatesProducerAndPropagatesOriginalFailure at its
unchanged ten-second completion deadline. Native jobs were skipped, not passed.
That continuous producer used PowerShell. The same managed fixture now has a
direct binary producer mode used by the write-fault/noncooperative-output tests
on each OS. Ten/six-second deadlines, assertions and production code are unchanged.
This removes a shell dependency; it does not prove the observed timeout's cause.

Follow-up CI 37111416924 passed the Windows net8/net10 full suites but aborted
net48 after the two-minute inactivity watchdog. Its preserved Blame sequence
identifies the zero-byte PsqlCompletion binary-drain case as incomplete; it does
not identify the blocked operation or root cause. Native jobs again were skipped.
The next controls revision adds separate parent/child timestamped stage logs to
the existing artifact, covering start, write, CompleteAsync, child EOF and drain.
Assertions and the fifteen-second process deadline remain unchanged; diagnostic
instrumentation is not reported as a production fix or a successful native run.

CI 37112048520 passed all three Windows full suites and the first net48 repeat,
then stalled in the second net48 repeat's five-byte completion case. Artifact
11270078805 records parent start/write/CompleteAsync returning and child EOF/drain,
with no parent completion. This narrows the symptom to completion observation,
without proving its cause. A test-only Windows hang stack reader (ClrMD 3.1.512801,
isolated from the solution/package dependencies) now analyzes watchdog dumps on
their originating runner. Only stack/async-state text is retained; raw dumps are
removed before upload. The watchdog, process deadlines and assertions remain.
Native jobs run independently so their build/runtime diagnostics are available
while deterministic Windows failures are investigated. All 18 jobs must still
pass on the same final PR/main commits; this does not waive the deterministic gate.

| OS | Required real execution | Scope |
|---|---|---|
| Linux | Existing pinned 10–18/net10.0 matrix | Existing representative scenarios plus scoped checksum/rewind/16→18 Copy migration |
| Windows | Pinned 18.6/native Visual Studio/Meson/net10.0 | PgDump custom archive, PgRestore row verification, finite and redirected Psql, PgBench initialization/one transaction |
| macOS | Pinned 18.6/native compiler/Meson/net10.0 | Same representative public-wrapper scenario |

Official archive SHA256 stays `555610c24d53e4316da5b7d3fc25c279d96856d5e0e23ee308c328c5fa881d9f`.
Meson 1.8.3/Ninja 1.11.1.4 and upstream Windows Flex/Bison 2.5.25 are fixed.
Build scripts record actual compiler/architecture/options and binary hashes;
runner images can change, so bit-identical recompilation is not promised.
Optional compression/SSL/ICU/Readline components are disabled in native fixtures.
Custom archive transport is exercised without claiming compression coverage.

The native fixture creates unique owned data/databases. macOS uses a short Unix
socket directory and no TCP listener. Windows binds an ephemeral port exclusively
to 127.0.0.1; it never registers a service or reuses an installed/user cluster.
Evidence validates server version and exact data directory before counting the
scenario. Missing, duplicate, failed or NotExecuted TRX results fail collection.
Cleanup checks the job/root/binary ownership receipt before stopping a process.
Logs/failure reports are uploaded even on failure. Test watchdog and job/step caps
bound a stalled runner; failures are investigated rather than blindly retried.

Artifacts `native-postgresql-18-<OS>-<Actions SHA>` include native evidence JSON,
TRX and build/server failure logs. JSON records selected archive/version/compiler,
actual executable hashes/version, OS/architecture/TFM, source and tested-head SHA,
run ID, transport, actual server version and exclusions. The single Passed xUnit
scenario checks backup/restore row preservation, psql output/session EOF and
pgbench success; it is not all-option/all-tool coverage. Provisioning initdb,
pg_ctl and createdb binaries are recorded, but not counted as public-wrapper
coverage in this native scenario.

Require all 18 CI jobs on the final PR and exact main: three deterministic,
three candidate, shared preflight, nine Linux real and two native. Independently
inspect native evidence against those exact SHAs/run IDs and the pinned archive.
The existing immutable alpha.2 selection is not changed by this CI extension.

Remaining exclusions: native net8/net48 CLI runtime, Windows/macOS older majors,
other patches/upgrade pairs, service lifecycle, native migration/rewind, compression,
TLS/ICU/TTY and unlisted/destructive scenarios. Existing three-OS package consumers
remain distinct evidence. A final 1.0 version/acceptance decision and publication
approval/account setup are separate gates; all publication flags remain false.

Sources: [official native platform notes](https://www.postgresql.org/docs/18/installation-platform-notes.html)
and [Meson build procedure](https://www.postgresql.org/docs/18/install-meson.html), checked 2026-10-03.

## 日本語

初回CI 37111052935はWindows/net10の出力失敗テストが10秒で完了せず失敗しました。
実CLI jobは未実行で、成功には数えません。PowerShellを使う連続出力fixtureを
直接起動する.NET子プロセスへ置き換え、10／6秒の期限・検証・本番コードは維持します。
シェル依存を除く変更であり、観測された停止原因を断定するものではありません。

続くCI 37111416924はWindowsのnet8／net10全件に成功しましたが、net48は2分の
進行停止で打ち切られました。保存した実行順序では入力0バイトのEOF／drainテストが
未完了であり、停止した操作や原因はまだ不明です。親子プロセスの開始・書き込み・
完了要求・EOF・drainを時刻付きファイルに記録して絞り込みます。15秒の期限と検証を
維持し、診断追加を本番修正や実CLI成功として数えません。

CI 37112048520ではWindowsの全3対象とnet48反復1回目が成功し、2回目の5バイト
ケースで停止しました。成果物11270078805には親の開始・write・CompleteAsync復帰と
子のEOF・drainを記録できましたが、親の完了待ちは戻っていません。次は所有CI内で
停止時のスタックと非同期状態を読み、テキストのみ保存します。解析依存は本番・候補に
追加せず、メモリダンプはアップロード前に削除します。期限・検証は維持します。
実CLIジョブは独立に起動し、Windowsの通常テスト調査中にもビルド・実行診断を得ます。
最終PR／mainの同一commitで全18ジョブ成功という条件は変えません。

Linuxの既存10〜18検証を維持し、Windows／macOSに同じ版・hashの18.6をネイティブ
ビルドして追加します。公開APIでバックアップ／行のリストア確認、有限・セッションpsql、
pgbenchを実行します。ライブラリ／パッケージのテストとは別の実CLI証拠です。

Windowsは専用ループバック・macOSは専用Unix socketの所有クラスタを使います。
利用者DBやサービスには触れず、版・実行ファイルhash・実データディレクトリ・TRX実結果を
検証します。未実行・失敗・重複を合格にせず、失敗時も診断を保存し、所有先のみ停止します。
ビルドのコンパイラ／設定も記録し、再ビルドの完全同一バイトは保証しません。

最終PR／mainの全18ジョブ成功とWindows／macOS成果物の独立確認を完了条件とします。
旧版・別TFM・サービス・TLS／TTY・圧縮・native移行等は未検証として残します。
1.0の版・受入判断、GitHub／NuGet公開は別です。alpha.2の固定ソースと無効の公開設定は
維持し、今回の作業では公開しません。
