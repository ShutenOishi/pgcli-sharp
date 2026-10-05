# PgCliSharp 1.0.0-rc.1 — Unpublished release candidate / 未公開リリース候補

## English

Planned unpublished release candidate; exact selection and package-audit evidence
are recorded by `.github/release-candidate.json` and the RC validation receipt.
Do not infer availability on nuget.org or GitHub Releases from this file.

- All 25 typed PostgreSQL CLI wrappers; PostgreSQL 10–18, .NET Standard 2.0/.NET 8/.NET 10.
- Reviewed frozen API, configuration lambdas, execution snapshots and offline command generation.
- Shared Process execution lifecycle without CliWrap; narrow legacy argument/exit compatibility,
  bounded input, original cancellation/deadlines and best-effort Windows descendant termination.
- MIT original code, bilingual docs/messages, shipped upstream notices; PostgreSQL binaries are not bundled.
- New features stop until 1.0; only bug fixes, validation and documentation improvements proceed.

Required audit: exact successful source-main CI; fixed SDK/lock; actual package
payload/license/dependency checks; three portable PDB/DLL pairs and negative
SourceLink tests; isolated compile/runtime consumers on all three OS controls.
Real-CLI evidence covers pinned Linux10–18 and native Windows/macOS18/net10.
Existing native-major/TFM, service, TLS/ICU/TTY, optional compression and migration
exclusions remain. Historical stalls are not relabeled as solved root causes.

This is not `1.0.0`, a universal compatibility guarantee or publication approval.
No public artifacts or tags are created; Trusted Publishing account verification
remains pending. Alpha.1/alpha.2 provenance remains history, never silently replaced.

## 日本語

未公開リリース候補の準備です。実際の固定ソースと監査結果はmanifestと検証記録で
確認し、このファイルだけでnuget.org／GitHub Releaseへの公開とは判断しません。
全25型付きCLI、PG10〜18、3TFM、ラムダ設定・実行snapshot・オフラインコマンド、
CliWrapを使わない共有Process実装を含みます。入力上限・EOF・キャンセル・期限・
日英対応・MITと第三者表示を保持し、PostgreSQL本体は同梱しません。
1.0までは不具合修正・検証・文書改善に限定し、新機能を追加しません。
固定SDK／lockと成功済みmainを使い、実パッケージ・PDB・依存・利用側を3OSで監査します。
Linux10〜18とnative Windows／macOS18の実CLI証拠、既存の未検証範囲を分けて保持します。
RCは1.0・全組合せ保証・公開承認ではありません。外部公開とTrusted Publishing確認は
延期し、旧候補のソースと履歴を保存します。
