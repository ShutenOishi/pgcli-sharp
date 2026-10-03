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
remain asserted. Windows repeats the five completion cases three more times per
TFM, failing on the first failure rather than retrying until success. All TRX
results, including failed cases, are uploaded.

This removes the test's PowerShell dependency and adds regression evidence. It
does not prove the historical cause or change production execution code. Native
psql representative EOF tests remain in the separate Linux PostgreSQL matrix.
Other existing shell-based process fixtures are outside this bounded change.

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
Previous selections are retained as history. The
candidate audit and release preflight must use the same fixed source, version,
SDK, lock, notes and verification controls. The old disabled Phase 3 manifests
remain historical records. Promoting 1.0 or creating public artifacts requires
an explicit later decision. Real-CLI exclusions remain those in ADR-0016.

## 日本語

過去の失敗はPowerShellを使うechoテストが15秒でタイムアウトした記録です。
原因は断定せず、制限時間は変えずに小さな.NET子プロセスへ置き換えます。
空・小・256KiBのバイナリ、EOF後の転送完了、再完了、所有stream保持を繰り返し検証し、
Windowsの3対象ではさらに反復します。失敗を再実行で隠さず、TRXを保存します。
本番ライブラリの実行コードは変更せず、実psql検証も別matrixで継続します。

実行時依存9件のMIT表示と原文の.NET追加表示を同梱します。独自ライセンスの古い
参照パッケージは同梱せず、原文・hashを残し、その配布境界でレビューを閉じます。
アプリや.NETランタイム等を同梱する他の配布まで承認するものではありません。

新しい固定ソースmain CI 37084591633は初回で全13ジョブ成功、WindowsのTRXでは
3対象合計360回のバイナリ終了待ちに成功しました。日英README・SDK・lock・説明を揃え、
最終controlsのPR／main CIと3OS候補監査を準備完了の条件にします。旧履歴と無効の公開設定は保持し、
NuGet・タグ・Releaseの公開や1.0への昇格は別途判断します。
