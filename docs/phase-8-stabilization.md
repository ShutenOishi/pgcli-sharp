# Phase 8 stabilization checkpoints / Phase 8 安定化チェックポイント

## English

Status: **In progress, not release-ready.** Baseline is the completed Phase 7
main commit `c857161047ec0b7027b6061fa8f19c688e60dc94`.

Stabilization is split into bounded checkpoints. Completing one checkpoint does
not imply API freeze, full real-binary compatibility or publication approval.

| Checkpoint | Scope | Status |
|---|---|---|
| CP-01 | Generated XML EN/JA ordering, resources, diagnostic data and public dependency boundaries | Complete: PR #14; exact-main CI 36997454961 |
| CP-02 | Reproducible PostgreSQL 10-18 evidence matrix, owned migration/rewind scenarios, OS exclusions | Linux checkpoint complete: PR #15; exact-main CI 37006276018; exclusions retained |
| CP-03 | Complete naming/consistency review, checked API baseline, README/examples and breaking-API freeze | Complete including ADR-0018: PR #17; exact-main CI 37077909665; see [API review](phase-8-api-review.md) |
| CP-04 | License, preserved Phase 3 candidate decision, selected-source package re-audit, reviewed release candidate | Final controls under ADR-0021: tested source selected, scoped notices/license review, runtime/PDB audits and shared preflight; exact final PR/main CI gates completion |

### CP-01 audit and changes

- Source-level XML audit found 1,320 bilingual summaries, 55 parameter blocks,
  25 return blocks, five exception blocks and two remarks blocks, with no missing
  or reversed EN/JA markers. Explicit `inheritdoc` blocks are tracked separately.
  This is structural coverage, not proof of translation or option semantics.
- Tests now inspect the actual compiler-generated XML for each consumer target:
  net8.0/net10.0 and netstandard2.0 through the Windows net48 consumer. XML is copied
  to a dedicated test documentation directory; a missing file fails the test.
- The test allows explicit `inheritdoc` and checks bilingual summary/text blocks.
  Strict compiler documentation warnings remain the missing-member gate.
- Reflection tests inspect exported type bases/interfaces, public methods,
  constructors and fields, including nested generic/element types, to reject
  leaked CliWrap/internal implementation types. This is not yet a frozen API
  signature baseline or a full naming review.
- `PgInvalidOptionValueException.Value` previously used culture-sensitive
  `ToString()` despite promising invariant data. It now uses invariant conversion
  while preserving null and literal strings. Tests cover en-US, ja-JP, fr-FR and
  ar-EG formatting/UI cultures. Its summary now correctly covers all tools rather
  than only pg_dump. No public member or exception hierarchy changes.
- Existing resource tests verify neutral/Japanese key and placeholder parity,
  lookup/fallback and version-mismatch diagnostic invariance. This checkpoint
  retains them; it does not claim every exception scenario has been semantically
  audited.
- ADR-0015 is moved into the actual ADR index table (it had been below Template).

The [CP-01 pull request #14](https://github.com/ShutenOishi/pgcli-sharp/pull/14)
records its exact tested head, merge SHA and
exact-main CI. No untested evidence-only follow-up commit is needed.

### Remaining real-executable evidence

The Phase 7 baseline passed [exact-main CI 36977577753](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/36977577753).
It covers the representative PostgreSQL 16/18 Linux/net10.0 scenarios described in
[integration testing](integration-testing.md), including disposable cluster smoke.
There is no new real-executable matrix claim in CP-01.

CP-02 adds [pinned source builds and scoped scenarios](phase-8-real-binary-matrix.md).
It covers the nine-major Linux matrix, checksums from 12, divergent rewind from 13
and one 16-to-18 Copy upgrade. Windows/macOS real execution and listed exclusions
remain untested. [PR #15](https://github.com/ShutenOishi/pgcli-sharp/pull/15) records
the final head and main `3f230bd8bddf59b0bed5aeb8bb7589c77b9f88bf`;
[exact-main CI 37006276018](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/37006276018)
passed all 12 jobs and all nine evidence reports. The scoped Linux checkpoint is complete.

CP-02 must investigate package/container/source reproducibility per major instead
of marking untested majors as unavailable or passing. Record actual numeric CLI
and server versions, OS/TFM, scenario result and explicit exclusion reasons. Real
upgrade/rewind evidence is limited to the configured Linux scenarios; Windows
service lifecycle remains pending. Fake-runner tests do not substitute for real
execution. Older upstream-EOL binaries must stay isolated.

### Publication gate

The historical manifests preserve disabled Phase 3 alpha.1 and its original source.
ADR-0019 resolves MIT licensing; ADR-0021 selects unpublished alpha.2 from
`0b7a2bb2e4dc1c1ed016181b2598645a1e3a8d5d` with source main CI 37104736018 (16 jobs, attempt 1).
The direct completion fixture and shipped notices are in that source; Windows
TRX records 360 successful binary completions. The old timeout cause is unknown.

The [candidate audit](phase-8-candidate-audit.md) checks locked dependencies,
explicit shipped payload, actual PDB/DLL SourceLink and isolated runtime consumers.
The legacy license review closes only for the wrapper's actual distribution scope.
Final PR/main CI must pass all three OS candidate audits and the shared read-only
release preflight before bounded preview engineering is reported complete.
See [final review](phase-8-final-review.md). External publication and 1.0 acceptance
remain separate. ADR-0012/0021 require explicit approval and reviewed enablement;
account-side Trusted Publishing configuration has not been verified.

## 日本語

Phase 8のプレビュー技術準備を最終CIまで進めます。外部公開・1.0判定は別です。
Phase 7 完了 commit を基準に、CP-01〜04 に分けて安定化を進めます。

CP-01 では生成 XML 文書の日英順序、公開シグネチャへの内部型・CliWrap 型の漏出、
カルチャ非依存の診断値を検証します。文書が約束する `Value` のカルチャ非依存性に
実装が追随していなかった点を修正し、4カルチャと null／文字列保持の回帰テストを追加しました。
既存の日英リソースキー・書式プレースホルダ・フォールバック検証も継続します。
日英文書の構造チェックは翻訳の意味や全 API 命名レビューを保証しません。

CP-02 の Linux 10〜18 実バイナリ matrix、13以降の実巻き戻し、16→18コピー移行を追加します。
PR #15 と正確な main CI 37006276018 の全12ジョブが合格し、Linux範囲は完了です。
Windows／macOS実機・サービスや掲載した除外は未検証です。
CP-03とADR-0018の追加はPR #17、main `ddce19c77f9bd2a39eaa71eada9907c97b23aceb`、
CI 37077909665の全12ジョブ成功で完了しました。
CP-04はMIT、権利表示、配布範囲レビュー、直接起動する.NET子プロセスの反復テストを整え、
新しい検証済みmainへ未公開alpha.2を固定します。候補の同梱物・依存・PDB／DLL・
利用側の実行と共通公開前チェックを3OSの最終PR／main CIで検証します。
過去のWindowsタイムアウト原因は断定せず、公開承認・Trusted Publishing確認は残します。
16／18 の既存実証はその範囲を保ち、未検証の版を合格・再現不可とは扱いません。

公開設定は無効のまま、Phase 3 の固定ソースと候補を保存します。NuGet・新規タグ・
GitHub Release・assets は作成せず、候補の明示判断と別途の公開承認を待ちます。
MITの選択は公開承認ではありません。ライセンス本文と日英の説明をパッケージへ含めます。
