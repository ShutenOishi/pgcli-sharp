# Phase 8 final engineering review / Phase 8 最終技術レビュー

## English

Status: final controls prepared; completion requires the exact final PR/main CI
and three-OS selected-source audits. CI artifacts and PR evidence record those results. No external publication is enabled or authorized here.

### Windows completion investigation

Original evidence: main CI 37079780608 attempt 1, job 111077517167, net10.0
failed after the unchanged 15-second deadline while launching Windows PowerShell
as a binary echo fixture. The log establishes a timeout, not which component
caused it. Two subsequent unchanged main runs passed; that was not a fix.

The completion test now launches a minimal directly executed .NET fixture on
each consumer target (Windows net48/net8/net10; Linux/macOS net8/net10).
It reports ready/EOF/drain stages, preserves binary bytes and closes after EOF.
The original 15-second deadline remains. Empty, five-byte and 256-KiB inputs each
run ten times; the latter exceeds an OS pipe's usual buffer and exercises
concurrent output draining. Completion idempotence and caller stream ownership
remain asserted. Windows repeats the completion and process-session cases three more times per
TFM, failing on the first failure rather than retrying until success. All TRX
results, including failed cases, are uploaded. The CI harness additionally terminates
a test host after two minutes without test-case progress and caps each test step
at ten minutes. Sequence/VSTest diagnostics are retained without memory dumps.
This bounds a stalled runner; it does not relax any test deadline or fix a stall.
A later controls run (37086422295) stalled in Windows Test before candidate audit;
its cancelled logs remain separate evidence, not a successful run.

This removes the test's PowerShell dependency and adds regression evidence. It
does not prove the historical cause or change production execution code. Native
psql representative EOF tests remain in the separate Linux PostgreSQL matrix.
Other existing shell-based process fixtures are outside this bounded change.

Controls CI 37088094167 additionally failed net48's blocked-writer assertion:
the test used ping/sleep that naturally exited after about five seconds, while
its delayed assertion expected the writer to remain blocked until cancellation.
The five-second failure duration is consistent with a fixture-lifetime race,
not proof of a production buffer defect or of the earlier stalled run's cause.
Cancellation/backpressure tests now use the same directly launched managed
helper in a non-reading mode that only the test terminates. The queue size,
250-ms observation/cancellation and ten-second writer-release deadlines remain.
The CI watchdog/step caps bound a broken test; no failure is silently retried.

### Distribution/license boundary

Nine reviewed netstandard2.0 runtime dependencies declare MIT. Their copyright
and permission notices and byte-identical upstream .NET additional notices are
packaged. Dependencies remain separately resolved; no dependency assemblies,
PostgreSQL, libpq or .NET runtime are embedded.

Microsoft.NETCore.Platforms 1.1.0 is a legacy reference/RID package under Microsoft
.NET Library terms. It supplies no compile/runtime/native binaries in the audited
graph, and its package/runtime.json are not redistributed here. The original
terms and package hashes remain recorded. Distribution-scope review therefore
closes for PgCliSharp's explicit nupkg/snupkg payload, without relabeling it MIT
or clearing distributions that actually bundle that package or a .NET runtime.
Package audit must fail unexpected shipped files and any non-MIT runtime assets.

### Candidate and final gates

README EN/JA now describe alpha.2 and local feed usage, not an available NuGet
release. Selected source is `f0eb822c62def95e3cc8420b386b283437bf46cd`, containing current docs/notices/fixture.
Source main CI 37084591633 passed all 13 jobs at attempt 1; independently inspected
Windows TRX has 360 successful binary completions across net48/net8/net10.
Previous selections are retained as history. The consumer-feed correction also
updates both README instructions to a project-local NuGet.Config; after this
preparation merges, final candidate selection must include that documentation
correction rather than modify an immutable source or ship the old instructions. The
candidate audit and release preflight must use the same fixed source, version,
SDK, lock, notes and verification controls. The old disabled Phase 3 manifests
remain historical records. Promoting 1.0 or creating public artifacts requires
an explicit later decision. Real-CLI exclusions remain those in ADR-0016.

## 日本語

過去の失敗はPowerShellを使うechoテストが15秒でタイムアウトした記録です。
原因は断定せず、制限時間は変えずに小さな.NET子プロセスへ置き換えます。
空・小・256KiBのバイナリ、EOF後の転送完了、再完了、所有stream保持を繰り返し検証し、
Windowsの3対象ではさらに反復します。失敗を再実行で隠さず、TRXと実行診断を保存します。
CI側もテストの進行が2分止まった場合と各stepが10分を超えた場合に終了させます。
停止の検出を有限にする仕組みであり、15秒の制限緩和や原因修正ではありません。
本番ライブラリの実行コードは変更せず、実psql検証も別matrixで継続します。
CI 37088094167ではnet48の書き込みブロック確認も失敗しました。約5秒で自然終了する
pingを使うテストの前提を見直し、テストが終了を管理する・stdinを読まない.NET子プロセスへ
置き換えます。バッファ容量、250msの観測／キャンセル、10秒の解放期限は変えません。
本番の欠陥や以前の停止の原因と断定せず、終了待ち・sessionの両方を反復検証します。

実行時依存9件のMIT表示と原文の.NET追加表示を同梱します。独自ライセンスの古い
参照パッケージは同梱せず、原文・hashを残し、その配布境界でレビューを閉じます。
アプリや.NETランタイム等を同梱する他の配布まで承認するものではありません。

新しい固定ソースmain CI 37084591633は初回で全13ジョブ成功、WindowsのTRXでは
3対象合計360回のバイナリ終了待ちに成功しました。日英README・SDK・lock・説明を揃え、
最終controlsのPR／main CIと3OS候補監査を準備完了の条件にします。旧履歴と無効の公開設定は保持し、
NuGet・タグ・Releaseの公開や1.0への昇格は別途判断します。
