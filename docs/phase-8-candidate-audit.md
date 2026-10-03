# Unpublished candidate audit / 未公開候補の監査

## English

ADR-0021 selects `0.1.0-alpha.2` from source `f0eb822c62def95e3cc8420b386b283437bf46cd`.
[Source main CI 37084591633](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/37084591633)
passed all 13 jobs on its first attempt. PR #20 contains the current packaged
README/notices and direct managed completion fixture. The previous unpublished
selection remains in manifest history; historical Phase 3 manifests stay disabled.
Final controls must pass the exact PR head and main CI before preview engineering
is reported complete. That completion does not authorize external publication.

### Reproduction and evidence

Use Python 3, SDK 10.0.100 and installed .NET 8/10 runtimes, with a disposable
clean source checkout at the selected SHA:

```bash
python3 eng/audit_candidate.py /path/to/selected-source /path/to/candidate-output
python3 -m unittest discover -s eng -p 'test_candidate_*.py'
```

Audit controls and immutable source are separate. Only Version and
PackageReleaseNotes are overridden. The checked-in lock requires locked restore;
its SHA256 is `2c2c59365e63b57a6b9f31e74ad2ef6b10d8a9fe1b54dbce31a72f6b1af7d348`.
No PostgreSQL process is launched and no publication is performed.

CI audits on Linux, macOS and Windows. Each artifact contains nupkg/snupkg,
`candidate-audit.json`, lock, bilingual notes and `source-ci-verification.json`.
Schema 2 records selected source, audit control commit, actual SDK, OS, package
hashes, exact dependency license/notice evidence, payload and runtime consumers.
SDK/inputs are fixed; byte-identical packages across paths/runs are not promised.

All three assets compile in a clean isolated consumer cache. net8.0/net10.0
actually execute offline wrapper code and check the loaded assembly target;
Windows additionally compiles/runs net48 using the netstandard2.0 asset.
netstandard2.0 itself is compile-only. Consumer reference-assembly build packages
are recorded separately as independently licensed build tools, not MIT runtime
dependencies or redistributed contents. No real-CLI coverage follows from this.

Three portable PDBs must map SourceLink to the selected SHA and match their
associated DLL CodeView GUIDs. Wrong-SHA and deliberately mismatched-DLL tests
must fail. Package inspection compares six documents byte-for-byte with source,
including upstream .NET notices, and permits only the explicit own payload.
Unexpected shipped files, duplicate entries and non-MIT runtime dependencies fail.

### Distribution scope

The library build graph has 15 package identities; the non-net48 consumer graph
has 11 besides PgCliSharp. The nine netstandard2.0 runtime dependencies declare
MIT: CliWrap 3.10.5, Microsoft.Bcl.AsyncInterfaces 10.0.8, System.Buffers 4.6.1,
System.CodeDom 10.0.10, System.Management 10.0.10, System.Memory 4.6.3,
System.Numerics.Vectors 4.6.1, System.Runtime.CompilerServices.Unsafe 6.1.2 and
System.Threading.Tasks.Extensions 4.6.3. Their version inventory must exactly
match THIRD-PARTY-NOTICES.md. Modern assets have no package runtime dependencies.

NETStandard.Library 2.0.3 includes reviewed MIT LICENSE.TXT.
Microsoft.NETCore.Platforms 1.1.0 includes Microsoft .NET Library terms, not MIT:
`dotnet_library_license.txt` SHA256
`f1db688d8481c91a452fabcea5060a23da9ea5088329b58c478a040e2e426297`.
Neither supplies compile/runtime/native binaries in this graph. No reference
package, RID runtime.json, dependency assembly, PostgreSQL or .NET runtime is
embedded. The actual wrapper payload distribution review is complete; original
terms remain recorded. This does not clear downstream applications' separately
resolved or bundled distributions. The lock does not pin their dependency choices.

### Shared preflight and publication boundary

CI calls `candidate-preflight.yml`; manual `release.yml` uses that same workflow.
It validates manifest identity, successful exact-source main CI and main ancestry,
then produces audited packages. CI and default manual preflight have read-only
permissions and no publishing step. Publication additionally requires a reviewed
`publication_enabled: true` change and explicit manual `publish: true`, followed
by the release environment and NuGet OIDC policy. No credentials are requested.

Expected Trusted Publishing policy: NuGet owner ShutenOishi, GitHub owner
ShutenOishi, repository pgcli-sharp, workflow filename release.yml, environment
release. Account-side configuration remains unverified until separately authorized.
The publisher consumes preflight packages by hash, rejects existing version/tag,
and checks the resulting release target/assets. Partial publication requires
inspection and recovery, not blind retries or version/tag reuse.

`publication_ready: false` and `publication_performed: false` remain explicit.
Remaining gates are publication approval/reviewed enablement and account policy
verification. The historical Windows timeout cause remains unknown; the direct
fixture regression evidence is described in the final review. Native OS and
real-CLI exclusions in ADR-0016 remain. No 1.0/RC acceptance is claimed.

## 日本語

ADR-0021に従い、日英README・権利表示・直接起動する.NETテスト子プロセスを含む
main `f0eb822c62def95e3cc8420b386b283437bf46cd` を未公開alpha.2のソースに固定します。
そのmain CI 37084591633は初回で全13ジョブに成功しました。以前の選定と旧Phase 3の
無効な公開設定は履歴として保持します。最終controlsのPR・main CI成功が準備完了の条件です。

固定SDK・lock付き復元でパッケージを生成し、全ファイルの同梱範囲、日英文書と原文権利表示、
PDB3件のSourceLinkとDLL対応を監査します。誤ったSHA・DLLの組合せは失敗させます。
隔離した利用側で3アセットをコンパイルし、net8/net10、Windowsではnet48も実行して
実際に読み込むアセットを確認します。PostgreSQLは起動しません。成果物に原文・hash・
ソースCI・監査controlsのSHAを残します。OS間・実行間の完全同一バイトは保証しません。

実行時依存9件のMIT表示と追加表示を含めます。独自ライセンスの古い参照パッケージは
MITと読み替えず、同梱しない配布範囲でレビューを閉じます。下流アプリや.NET／PostgreSQL
等を同梱する別の配布まで承認するものではありません。

CIと手動公開前チェックは同じ監査workflowを使用します。公開設定は無効です。
公開承認とレビュー済み設定変更、nuget.org側のTrusted Publishing確認は別途必要です。
過去のWindows失敗原因は断定せず、実CLIの未検証範囲を維持し、1.0・RC完了とは扱いません。
