# ADR-0025: Freeze features for an unpublished 1.0 release candidate

- Status: Accepted
- Date: 2026-10-05
- Supersedes: ADR-0021
- Complements: ADR-0012, ADR-0017, ADR-0018, ADR-0022, ADR-0024

## Context

PR #26 replaced CliWrap with a shared Process lifecycle. Exact main
`d66a1e229d230d5f6d7b6fdd8db0c79a85bba89c` passed CI 37194612585:
18 jobs, attempt 1, independently checked Windows 30/3/3 stress, native
Windows/macOS PostgreSQL 18 and Linux 10–18 evidence. The fixed unpublished
alpha.2 predates this replacement. It is not a distribution of current main.
The maintainer chooses bug fixes, validation and documentation only until 1.0.

## Decision

- Prepare unpublished `1.0.0-rc.1`; stop feature additions until the scoped 1.0
  acceptance decision. Fixes must preserve the frozen API where possible;
  any necessary contract change still requires an explicit ADR and review.
- Keep PostgreSQL 10–18, all three TFMs, explicit executable paths, MIT,
  typed/lambda/offline APIs, bilingual documentation and existing exclusions.
- First test RC source preparation on the exact final PR and main. Only then
  select that successful main SHA/CI in a second reviewed control change.
  This avoids self-referential source selection and shipping stale preview guidance.
- Keep SDK 10.0.100, a separate Process-backend dependency lock and known
  audit profile. Require all 18 source CI jobs at attempt 1, including both native
  jobs, nine Linux majors, three deterministic suites, three candidate audits
  and the shared read-only preflight. Reject missing, duplicate or skipped jobs.
- RC selection must pass three-OS locked package/symbol/SourceLink/consumer
  audits on its exact PR/main controls. Only version and release-note metadata
  are overridden in the selected source. No third-party or PostgreSQL binaries
  are embedded. Runtime dependencies must match the two reviewed legacy packages.
- Preserve alpha.2 source, lock, notes and selection history, and disabled Phase 3
  manifests. Do not reuse or silently rewrite an old candidate's provenance.
- Publication remains disabled. RC engineering completion is neither 1.0
  promotion nor approval for a tag, GitHub Release, assets or NuGet push.
  Account-side Trusted Publishing remains unverified.

## Acceptance and limits

RC preparation, RC package selection/audit, 1.0 acceptance and external publication
are separate gates. Existing real-CLI exclusions remain explicit. Historical
stalls and the cancelled replacement run are retained, not erased by green runs;
no complete historical root-cause proof or universal concurrency guarantee is claimed.
No new production implementation or public API change is introduced here.

## 日本語

1.0までは新機能追加を止め、不具合修正・検証・文書改善に絞ります。
未公開`1.0.0-rc.1`を準備し、まず説明・監査準備を含む正確なmainを検証してから、
その成功済みSHA／CIを候補へ固定します。SDK・別lock・既知依存profile・全18ジョブを
必須にし、3OSで実パッケージ・PDB・利用側を監査します。旧alpha.2のソース・lock・
説明・履歴は保存します。API・PG10〜18・3TFM・MIT・未検証範囲は保持します。
RC準備と1.0昇格は別で、公開設定は無効のまま、タグ・Release・NuGet公開は行いません。
