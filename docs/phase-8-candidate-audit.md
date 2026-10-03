# Unpublished candidate audit / 未公開候補の監査

## English

CP-04 is partial, not release-ready. ADR-0020 selects alpha.2 from source
`df395760a84643bebec4bab9885c610793e4b222`. Selected-source main CI
[37079780608](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/37079780608)
passed all 12 jobs on attempt 2. Attempt 1 had the Windows/net10 psql completion
timeout; an unchanged rerun is not a fix or established cause.

### Reproduction and evidence

Use a clean checkout/worktree at the selected SHA, .NET SDK 10.0.100 and Python 3:

```bash
python3 eng/audit_candidate.py /path/to/selected-source /path/to/candidate-output
python3 -m unittest discover -s eng -p test_candidate_audit.py
```

The controls are from this audit change; source is from the selected immutable
commit. The script installs the checked-in build lock into that source checkout,
requires locked restore, and overrides only package version/release notes.
It never pushes packages or starts a PostgreSQL process. Source must have no
tracked changes. Use a disposable checkout, not an active development worktree.

The CI `Unpublished candidate audit (Linux)` artifact contains both packages,
`candidate-audit.json`, `packages.lock.json` and bilingual release notes. Its
record includes package hashes, SDK, selected source, exact notices and hashes,
the library build graph and a separately restored consumer graph. SourceLink
is read from all three actual portable PDBs, not inferred from nuspec metadata.
The isolated consumer cache and exact alpha.2 reference avoid a cached development
package; new lambda/offline APIs compile on netstandard2.0/net8.0/net10.0.
This is compile-only, not runtime or real-CLI coverage. SDK and restore inputs
are pinned/recorded; package bytes are not promised identical across paths/runs.

Local validation with SDK 10.0.100 succeeded for package inspection, three consumer
targets and all three PDB mappings. The reviewed lock SHA256 is
`2c2c59365e63b57a6b9f31e74ad2ef6b10d8a9fe1b54dbce31a72f6b1af7d348`.
CI must independently pass the final PR head and main merge before completion.

### Dependency scope and unresolved license review

The library build graph contains 15 package identities; the clean consumer graph
contains 11 besides PgCliSharp. Nine supply runtime assets to the netstandard2.0
consumer, all with MIT expressions: CliWrap 3.10.5, Microsoft.Bcl.AsyncInterfaces
10.0.8, System.Buffers 4.6.1, System.CodeDom 10.0.10, System.Management 10.0.10,
System.Memory 4.6.3, System.Numerics.Vectors 4.6.1,
System.Runtime.CompilerServices.Unsafe 6.1.2 and System.Threading.Tasks.Extensions
4.6.3. Modern consumers have no third-party package runtime assets in this graph.

NETStandard.Library 2.0.3 has no SPDX expression but includes reviewed MIT
LICENSE.TXT. Microsoft.NETCore.Platforms 1.1.0 instead includes
`dotnet_library_license.txt` with Microsoft .NET Library terms, SHA256
`f1db688d8481c91a452fabcea5060a23da9ea5088329b58c478a040e2e426297`.
Neither contributes runtime/native assets here; they are not embedded in the
PgCliSharp nupkg. Do not classify the latter as MIT from a current repository's
license or silently discard it. The audit records a LicenseRef and sets
`publication_ready: false` / `remaining_license_review`. It fails unknown license
metadata/text and any non-MIT dependency with runtime assets. These gates are a
bounded inventory policy, not a legal opinion or clearance to redistribute.

Review the legacy terms and actual distribution contents before promotion.
Downstream applications must inspect their own resolved graph/notices and any
bundled .NET runtime/PostgreSQL distribution. The build lock does not constrain
their resolution. No source-disclosure requirement is added to PgCliSharp itself.

### Remaining promotion work

- Investigate the intermittent Windows completion-test failure.
- Resolve the legacy license/distribution-notice review; retain upstream notices.
- Review release workflow promotion of alpha.2, pinned source, version/notes
  overrides and locked restore; it currently still targets historical alpha.1.
- Fresh exact-source validation and explicit publication approval, then verify
  Trusted Publishing without requesting an API key.

No NuGet package, tag, Release or release asset is published by this audit.
PostgreSQL native Windows/macOS, services, TLS/ICU/TTY and the previously listed
real-binary exclusions remain untested. Original-source packaged README/guide
still describe older candidate/audit state; the separate alpha.2 notes and audit
record are authoritative for this unpublished selection. Before publication,
review whether that requires a new source selection rather than editing an
immutable source or pretending the packaged documentation was updated.

## 日本語

CP-04は候補選定と技術監査まで進めますが、公開可能・完了とは扱いません。
MIT採用mainを固定して未公開alpha.2を作り、旧alpha.1と無効の公開設定は保存します。
lock付き復元、パッケージの日英文書・MIT・依存境界、実PDB3件のSourceLink、
隔離した利用側3対象のコンパイルを検証します。結果・依存原文・hashはCI artifactへ残します。

利用側はPgCliSharp以外に11件を解決し、netstandard2.0の実行時アセット9件はMIT表示です。
modern対象では第三者パッケージの実行時アセットはありません。古い2件は実行時アセットを
持たず、そのうちMicrosoft.NETCore.Platforms 1.1.0には独自ライセンスが含まれます。
MITと読み替えず原文とhashを記録し、配布範囲・条件の判断を公開前に残します。
これは法的な保証やセキュリティ監査ではありません。アプリ側の解決版や同梱物は別途確認が必要です。

Windows/net10終了待ちテストの一度の失敗は再実行で成功しましたが原因未確定です。
その調査、独自ライセンス・配布表示、公開workflowのalpha.2対応、固定ソース再検証、
公開承認とTrusted Publishing確認を残します。NuGet・タグ・Releaseは作りません。
固定ソース内のREADMEは古い候補の説明を保持しており、今回の別添候補説明が選定状態を示します。
公開前に文書更新を含む新ソース選定が必要かをレビューします。実CLIの既存除外も変更しません。
