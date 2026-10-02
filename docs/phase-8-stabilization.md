# Phase 8 stabilization checkpoints / Phase 8 安定化チェックポイント

## English

Status: **In progress, not release-ready.** Baseline is the completed Phase 7
main commit `c857161047ec0b7027b6061fa8f19c688e60dc94`.

Stabilization is split into bounded checkpoints. Completing one checkpoint does
not imply API freeze, full real-binary compatibility or publication approval.

| Checkpoint | Scope | Status |
|---|---|---|
| CP-01 | Generated XML EN/JA ordering, resources, diagnostic data and public dependency boundaries | Implemented; exact-head/main CI required |
| CP-02 | Reproducible PostgreSQL 10-18 evidence matrix, owned migration/rewind scenarios, OS exclusions | Pending |
| CP-03 | Complete naming/consistency review, checked API baseline, README/examples and breaking-API freeze | Pending |
| CP-04 | Preserved Phase 3 candidate decision, selected-source package re-audit, reviewed release candidate | Pending; publication requires separate explicit authorization |

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

The CP-01 pull request titled **Phase 8 CP-01: documentation and diagnostic
stabilization / 文書・診断の安定化** records its exact tested head, merge SHA and
exact-main CI. No untested evidence-only follow-up commit is needed.

### Remaining real-executable evidence

The Phase 7 baseline passed [exact-main CI 36977577753](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/36977577753).
It covers the representative PostgreSQL 16/18 Linux/net10.0 scenarios described in
[integration testing](integration-testing.md), including disposable cluster smoke.
There is no new real-executable matrix claim in CP-01.

CP-02 must investigate package/container/source reproducibility per major instead
of marking untested majors as unavailable or passing. Record actual numeric CLI
and server versions, OS/TFM, scenario result and explicit exclusion reasons. Real
upgrade/rewind and Windows service lifecycle remain pending; fake-runner tests do
not substitute for them. Older upstream-EOL binaries must stay isolated.

### Publication gate

Both manifests remain disabled and preserve Phase 3 source
`cccf8d9fbe1f2e1104676ab94a7863209c0220dd` and package `0.1.0-alpha.1`.
No NuGet push, new tag, GitHub Release or Release asset is authorized by this
checkpoint. Phase 8 completion is not claimed. ADR-0012 and the final-release
workflow require a reviewed candidate decision and explicit authorization before
enabling publication or dispatching release workflows. No API key is requested.

## 日本語

Phase 8 は開始済みですが、まだ公開可能な状態・完了状態ではありません。
Phase 7 完了 commit を基準に、CP-01〜04 に分けて安定化を進めます。

CP-01 では生成 XML 文書の日英順序、公開シグネチャへの内部型・CliWrap 型の漏出、
カルチャ非依存の診断値を検証します。文書が約束する `Value` のカルチャ非依存性に
実装が追随していなかった点を修正し、4カルチャと null／文字列保持の回帰テストを追加しました。
既存の日英リソースキー・書式プレースホルダ・フォールバック検証も継続します。
日英文書の構造チェックは翻訳の意味や全 API 命名レビューを保証しません。

CP-02 の PostgreSQL 10〜18 実バイナリ matrix と移行・巻き戻し・Windows サービス検証、
CP-03 の全 API 命名／互換性レビューと凍結、CP-04 の候補選定／公開前検証は未完了です。
16／18 の既存実証はその範囲を保ち、未検証の版を合格・再現不可とは扱いません。

公開設定は無効のまま、Phase 3 の固定ソースと候補を保存します。NuGet・新規タグ・
GitHub Release・assets は作成せず、候補の明示判断と別途の公開承認を待ちます。
