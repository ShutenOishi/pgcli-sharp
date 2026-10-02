# Phase 8 CP-03 public API review / 公開 API レビュー

## English

Baseline source: completed CP-02 main `3f230bd8bddf59b0bed5aeb8bb7589c77b9f88bf`.
Status: reviewed compiled baseline and gates implemented; final-head/exact-main
CI are required before CP-03 completion. Phase 8 is not release-ready.

### Naming and consistency decisions

The review retains the current names and dedicated models rather than introducing
breaking cosmetic normalization. All 25 executable wrappers have an explicit
`(string executablePath, PostgreSqlMajorVersion version)` constructor,
`ExecutablePath` and `Version`, a dedicated sealed `<Wrapper>Options`, and
Task-returning `*Async` operations with optional `timeout` and a final optional
`cancellationToken`. A test enumerates the complete wrapper set so new wrappers
cannot escape that review. Units remain visible in names such as `DurationSeconds`,
`WaitTimeoutSeconds` and `Port`; structured values and ordered collections remain
tool-specific. Existing inventory-to-API and version-catalog tests cover all
25 specifications; the compiled baseline reviews every exported type/member,
not only Options properties.

| Category / wrappers | Reviewed convention or intentional difference |
|---|---|
| PgDump, PgRestore, PgDumpAll | Explicit input/output/destination types; binary payloads are not result strings. Existing names remain unchanged. |
| PgBaseBackup, PgReceiveWal, PgRecvLogical, PgVerifyBackup, PgCombineBackup | Tool-specific destination/input/output and result types; streaming and whole-tool boundaries are not merged into speculative common public options. |
| CreateDb, DropDb, CreateUser, DropUser, VacuumDb, ReindexDb, ClusterDb, PgIsReady, PgAmcheck | Established `Db` casing retained. Shared `PgMaintenanceIo`/result only where semantics match; readiness 0–3 is a domain status. |
| Psql, PgBench | Finite execution is separate from `PsqlSession`; ordered actions and explicit EOF remain. pgbench exposes stdout/stderr, not a fictitious stdin workload contract. |
| InitDb, PgCtl, PgUpgrade, PgRewind, PgChecksums, PgResetWal | `ServerApplications` namespace and `PgServerIo`/result category retained. `GetHelpAsync` is explicitly modeled here; earlier utility/help bindings are not renamed merely for uniformity. |
| Version/lifecycle and value objects | Expected CLI major is separate from actual executable version and upstream lifecycle. Enum numeric values, factories, conversions, equality members and nullable/default contracts are captured unchanged. |

### Exceptions, statuses and ownership

All library-specific exceptions remain rooted at `PgCliSharpException`.
`PgOptionValidationException` holds `SelectedVersion`/`OptionName`; unsupported
options add version/patch bounds and invalid values add invariant `Value`.
`PgInvalidOptionCombinationException` deliberately remains a sibling carrying an
ordered `OptionNames` list rather than inheriting a single-option abstraction.
Unsupported whole tools have `ToolName`, `SelectedVersion`, `SupportedSince`.
Start/parse/mismatch failures expose executable path and raw/expected/actual
version data; nonzero execution exposes code and original stderr; timeout exposes
the configured duration. Existing exception classes are retained, not rebased.
Localized messages are for humans; inspect properties rather than parsing them.
BCL argument/cancellation and caller-stream errors are not promised to become
PgCliSharp-specific exceptions.

Nonzero status is not uniformly failure: psql 0–3, readiness 0–3, pg_ctl Status
0/3/4 and pgbench's version-aware statuses use their established typed results.
Caller-owned streams are never disposed by PgCliSharp. Redirected sessions are
not TTY/PTYs; cancellation is best-effort termination, not cluster rollback or
detached-server shutdown. Password/connection strings and raw PostgreSQL output
must be handled as sensitive by consumers; this freeze does not add logging or
claim that arbitrary upstream stderr is scrubbed.

### Checked contract and examples

`tests/api/PublicApi.txt` records the compiled contract. The collector normalizes
BCL type names without assembly versions and sorts ordinally; inherited BCL
interfaces and zero-valued reflection flag aliases are normalized. It includes type
shape, bases/interfaces, public/protected methods and accessors, fields/events,
named parameters, default values, generic constraints, enum values, modifiers and
nullable/obsolete/flags metadata. Public/protected nested types are included when
their containing type is externally accessible. All three library assets match one baseline
through net8/net10 and Windows net48 tests. Missing/changed baselines fail; no
automatic regeneration occurs. A deliberate fixture checks ordering, optional
defaults, protected methods, enum values and nullable metadata.

The review corrects doubled separators in README verbatim Windows paths and uses
the portable `WriteAsync(byte[], int, int)` session-input overload. CI extracts
every EN/JA C# block and compiles it on all consumer targets without executing it.
Required imports, explicit executable selection, unpublished-package status,
stream ownership, CLI-versus-server versions and dangerous-operation boundaries
are documented. This is compile evidence, not real-database execution evidence.

ADR-0017 freezes breaking shape changes for stabilization. Even additions require
a reviewed baseline diff; breaking changes require an ADR and compatibility,
migration and versioning rationale. Baseline acceptance is not a semantic audit
of every option/default, every custom attribute, every prior binary package or
every native OS scenario. CP-01/CP-02 and independent behavior/spec tests remain
necessary. CP-04's license/candidate/provenance decisions and explicit publication
authorization remain outstanding. Both manifests stay false with Phase 3 source.

Initial compiled capture on Linux/macOS/net8/net10 and Windows/net48/net8/net10
found **177 exported types and 2,675 contract lines**. After excluding inherited
BCL interfaces/zero flag display aliases, all seven captures agree. The collector
and baseline are now checked against each compiled target, rather than inferred
from a source-text parser. Source API names/shapes are unchanged from CP-02.

Completion evidence is indexed by [PR #16](https://github.com/ShutenOishi/pgcli-sharp/pull/16), including exact
tested head, merge SHA and both CI runs; no untested follow-up commit is needed.

## 日本語

CP-02 完了 main を基準に、25ラッパーと公開型／メンバーの名前・引数・結果・例外・
所有権をレビューします。既存の `Db` 表記、専用 Options、server 名前空間、
結果／I/O 型、意味を持つ終了状態など、意図した差異を保持し、見た目の統一のための
破壊的 rename・例外の継承変更は行いません。引数名と既定値、enum 数値、
nullable 情報、public／protected メソッド等をコンパイル済みベースラインへ記録し、
net8／net10 と Windows net48 consumer で3つの asset の一致を検証します。

README の Windows verbatim path の重複区切りと、legacy で使えない stream overload
を修正し、日英の全 C# ブロックを compile-only 検証します。実行ファイルは同梱せず、
CLI とサーバーの版は別です。ストリームは呼び出し側所有、session は TTY ではなく、
キャンセルはデータ変更の取り消しや独立サーバー停止を保証しません。
状態コードは各ツールの typed result、診断は構造化プロパティで判定してください。

ADR-0017 により安定化中の破壊的 shape 変更を凍結し、ベースライン差分は自動承認
しません。追加にもレビュー、破壊的変更には ADR・移行・version 判断が必要です。
これは意味上の全互換性、翻訳の意味、全 custom attribute、過去の全バイナリ、
全 OS 実機の保証ではありません。最終 head と正確な main CI の成功で CP-03 を完了とし、
CP-04 のライセンス・候補・公開前検証と公開承認は残します。
