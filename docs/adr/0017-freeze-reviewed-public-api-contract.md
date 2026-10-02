# ADR-0017: Freeze the reviewed public API contract

- Status: Accepted
- Date: 2026-10-02
- Complements: ADR-0003, ADR-0004, ADR-0007, ADR-0008, ADR-0011, ADR-0012

## Context

Phase 8 CP-01 checks bilingual documentation and dependency boundaries. Existing
per-tool specification tests check inventory bindings. Neither detects an
unreviewed public member removal, rename, signature/default/enum change or
target-framework drift. CP-03 needs a reviewable contract without another
runtime/package dependency or an automatic API acceptance switch.

## Decision

Keep the existing per-tool public models and names, including intentional result,
status, help and I/O differences recorded in `docs/phase-8-api-review.md`. Add a
checked-in, ordinal-sorted compiled reflection baseline under `tests/api/`.
Modern consumers and the Windows net48 consumer of the netstandard2.0 asset must
match the same baseline. It covers public/exported types and declared public or
protected constructors, methods/accessors, properties, fields/events, bases and
interfaces, generic constraints, named parameters/defaults, enum constants,
required/optional modifiers and compiler nullable metadata. BCL assembly identity
and inherited BCL members are deliberately excluded to avoid framework noise.

Mismatch fails CI; diagnostics can show the actual inventory but never rewrite
the baseline. Baseline changes require an explicit reviewed diff and rationale.
Breaking shape changes are frozen for the stabilization candidate: they require
a new ADR with compatibility/migration and versioning consequences before any
baseline update. Additive changes also require review; API drift is not accepted
by regenerating the file. Accepted historical decisions are not rewritten.

Compile every English/Japanese README C# block on modern targets and the Windows
legacy consumer without executing database operations. Keep documentation,
structured exception data and behavior tests as independent gates.

## Consequences and limits

There is no public rename/removal, TFM or runtime-dependency change here. The
snapshot is a conservative shape gate, not a complete semantic compatibility
analyzer: it does not prove behavior, translation meaning, reflection-only
private details, every attribute contract, binary compatibility against every
historical package, native execution on every OS or release readiness. Protected
nested types are not currently present; introducing them requires extending the
inventory collector as part of that review. Full Phase 8 and publication remain
separate; ADR-0012 continues to disable publication. License/candidate decisions
are not made by this API freeze.

## 日本語

公開 API の名前と既存の専用モデルを維持し、コンパイル済み公開／protected
シグネチャを ordinal 順のベースラインへ固定します。modern target と Windows の
net48 consumer（netstandard2.0 asset）が同じ一覧に一致することを CI で検証します。
差分は自動承認せず、追加もレビュー、破壊的変更は ADR と移行・version 判断が必要です。
README の日英 C# 例はコンパイルのみで検証し、DB 操作は実行しません。
意味上の互換性・全 OS 実機・翻訳品質・公開可否をこの検証だけで保証しません。
