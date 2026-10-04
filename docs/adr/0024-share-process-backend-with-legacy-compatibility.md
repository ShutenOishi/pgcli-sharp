# ADR-0024: Share the Process backend with narrow legacy compatibility

- Status: Accepted
- Date: 2026-10-04
- Related: ADR-0008, ADR-0013, ADR-0014, ADR-0023
- Supersedes: ADR-0008, ADR-0023

## Context

The exact-main CLR4 capture and the context-isolated PR #25 capture both show
CliWrap's four lower tasks normally completed while its async enumeration and
outer command remain pending. The deterministic startup-context defect is real,
but isolating startup does not resolve the intermittent stall. Failed runs and
artifact digests are preserved in the [investigation](../phase-8-net48-investigation.md).
Nested-diagnostic CI 37191060046 again aborts net48 in the initial full suite.
Its actual CLR4 promise version and consumer token both equal 3, with iterator
state -4 and all four lower tasks complete. Pinned PolyShim's promise uses lazy
unsynchronized TCS initialization, which introduces a potential race; the capture
does not prove the continuation graph or establish this as the root cause.
The backend replacement removes that iterator/promise transition entirely.

The maintainer authorizes consideration of replacing CliWrap if necessary. The
target matrix, public API, binary I/O, caller stream ownership, cancellation and
timeout contracts remain requirements; removing netstandard2.0 is not proposed.

## Alternatives

| Approach | Benefit | Cost / acceptance condition |
|---|---|---|
| Fix the pinned dependency upstream | Retains upstream process maintenance | A demonstrated fix must cover both the deterministic context defect and the captured completion stall; a newer fixed release is currently unavailable |
| Vendor/fork CliWrap and change its async loop | Can remove the observed iterator transition | Carries an entire process library and upstream patch/license maintenance; changing that loop alone is not proof of a root fix |
| Share the existing Process backend, add narrow compatibility | Removes the suspect command/iterator completion path and several legacy dependencies | Own argument serialization, race-safe exit waiting and legacy tree termination must receive explicit review and runtime tests |
| Keep the current adapter and repeat until green | Small code change | Rejected: it already reproduced the stall and a later pass would not explain it |

## Decision

- Reuse the existing finite/session Process lifecycle and its explicit awaits,
  fault supervision, original deadline through output draining, and bounded
  abnormal task observation.
- Keep modern ArgumentList, WaitForExitAsync and Kill(entireProcessTree: true).
- For netstandard2.0, serialize argument tokens using the .NET Foundation's MIT
  PasteArguments rules with attribution and execution tests for empty values,
  quotes, Unicode, whitespace and trailing backslashes. Never add a shell.
- Wait for legacy exit with an asynchronously continuing TaskCompletionSource,
  subscribe before checking HasExited, and detach the event handler after exit.
- Retain Windows best-effort descendant discovery using System.Management
  10.0.10, already present transitively, instead of adding job-object/native
  launch machinery. Document unsupported legacy platform limits honestly.
- Retain the 64 KiB / 16-segment legacy input bridge, feed it directly to stdin,
  and deliver EOF only after accepted bytes drain. Exit/cancel/disposal must
  release blocked producers without disposing caller streams.
- Remove CliWrap and its now-unneeded async-iterator dependencies from new
  package assets. Do not retarget frozen alpha.2 or alter its dependency lock.

## Acceptance gates

Build all three TFMs; exercise netstandard2.0 on Windows net48. Controlled-context
regressions, binary equality/EOF, early exit, nonzero status, stderr/environment,
caller-stream faults, cancellation/timeout and descendant termination must pass.
Run bounded Windows 30/3/3 stress without retrying failures, review actual TRX and
stage artifacts, and require fresh full PR CI plus exact-main CI. Native PostgreSQL
3-OS and Linux 10–18 evidence remain required. Inspect the new dependency graph
and notices separately from the preserved preview candidate.

This decision replaces the legacy completion path; it is not proof that every
historical stall shares one cause. A replacement changes the owned lifecycle path; its
evidence must establish maintained behavior rather than merely a passing retry.
External publication stays postponed.

## Primary sources

- [Pinned CliWrap execution](https://github.com/Tyrrrz/CliWrap/blob/45819e3bc44d9be4d778e91a0ec5cf6a5176de56/CliWrap/Command.Execution.cs)
- [Pinned PolyShim legacy tree termination](https://github.com/Tyrrrz/PolyShim/blob/2.12.2/PolyShim/NetCore30/Process.cs)
- [.NET argument serialization](https://github.com/dotnet/runtime/blob/9a50493f9f1125fda5e2212b9d6718bc7cdbc5c0/src/libraries/System.Private.CoreLib/src/System/PasteArguments.cs)

## 日本語

CliWrapの下位4Taskが完了した後も上位が保留される停止が、同期コンテキスト修正後も
再現しました。必要なら依存を置き換える方針を検討します。既存のProcess実装を共有し、
古いAPIに不足する引数整形・終了通知・Windows子孫終了だけを補う案を優先します。
対応TFM・公開API・バイナリI/O・入力上限・呼出元ストリームの所有権・期限を維持します。
互換処理の保守責任は本プロジェクトへ移るため、実行テスト・依存と権利表示の確認・
最終PRとmainのCIを移行条件とします。移行の最終PR／main検証が済むまで完了扱いにはせず、alpha.2と公開延期は保持します。
