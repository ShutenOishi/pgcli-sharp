# PgCliSharp 0.1.0-alpha.2 — Full wrapper preview / 全ラッパープレビュー

## English

Unpublished candidate; not a Release or authorization to publish.

- 25 strongly typed PostgreSQL client/server CLI wrappers, PostgreSQL 10–18 syntax support.
- Lambda configuration, execution snapshots, offline validation and POSIX/PowerShell command export.
- Awaited psql session completion and shared execution-result metadata.
- MIT licensing; separate caller-supplied PostgreSQL executables, not bundled.
- netstandard2.0, net8.0 and net10.0 assets with English/Japanese XML and diagnostics.

Selected source: `df395760a84643bebec4bab9885c610793e4b222` (MIT main).
Build overrides: Version=`0.1.0-alpha.2` and the exact PackageReleaseNotes in
`.github/release-candidate.json`. Source is otherwise unchanged. SDK, resolved
dependency lock, license/notice data, SHA256s and PDB SourceLink are recorded by
the candidate audit. Consumer dependencies are not bundled or relicensed.
One legacy reference package contains Microsoft .NET Library license terms,
not MIT; its distribution/terms review remains a publication gate. The selected
source's packaged README retains historical alpha.1 instructions; these separate
notes describe alpha.2. No install-from-nuget.org command is currently usable.

Representative real PostgreSQL 10–18 evidence is Linux/net10.0 only; three-OS
deterministic tests do not establish native PostgreSQL coverage on Windows/macOS.
Windows service/TTY/TLS/ICU and listed destructive/migration exclusions remain.
The Windows/net10 completion test timed out once in source main CI and passed
an unchanged rerun. This is an unresolved intermittent risk, not a fixed defect.
No 1.0 stability or release-candidate readiness claim is made.

The historical Phase 3 alpha.1 is retained for provenance but not selected for
publication. Promoting this candidate requires review of the pinned source,
build overrides, dependency lock/notices, release workflow and Trusted Publishing,
plus separate publication approval. Existing publication manifests remain disabled.

## 日本語

未公開の候補です。Release作成や公開承認を意味しません。

全25の型付きCLIラッパー、PostgreSQL 10〜18の構文対応、ラムダ設定、実行前の設定コピー、
オフライン検証とPOSIX／PowerShell形式のコマンド生成、psqlの終了待ち、共通結果を含みます。
MITを採用し、実行ファイルは別途指定します。3対象のアセットと日英文書・診断を提供します。

MIT採用後のmain `df395760a84643bebec4bab9885c610793e4b222` を選び、明示した版と
PackageReleaseNotesだけをビルド時に上書きします。解決した依存のlock・ライセンス表示・
パッケージのSHA256・PDBのSourceLinkを記録し、原ソースや依存物のライセンスは変更しません。
古い参照用パッケージ1件のMicrosoft独自ライセンスは公開前確認を残します。
固定ソースの同梱READMEは旧alpha.1の説明を保持し、この別添がalpha.2を説明します。
現在nuget.orgからのインストールはできません。

実PostgreSQLの10〜18検証はLinux/net10.0の代表シナリオに限られます。Windows／macOSの
実バイナリやサービス、TTY、TLS等の掲載した除外は未検証です。Windows/net10の終了待ち
テストは一度タイムアウトし、無変更の再実行で成功しましたが、原因・安定性は未解決です。
1.0やRCの準備完了とは扱いません。旧alpha.1の履歴は保持し、新候補の正式な公開設定への
移行・lockを使う再ビルド・依存表示・Trusted Publishingの確認と別途の公開承認を残します。
