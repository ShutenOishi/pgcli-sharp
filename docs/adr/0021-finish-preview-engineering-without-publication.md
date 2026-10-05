# ADR-0021: Finish preview engineering without publication

- Status: Superseded
- Superseded by: ADR-0025
- Date: 2026-10-03
- Supersedes: ADR-0020
- Complements: ADR-0012, ADR-0016, ADR-0019

## Context

The first unpublished alpha.2 selection predates accurate package README wording,
included dependency notices and a bounded regression fixture for the observed
Windows timeout. Source selections must not be silently rewritten, but an
unpublished preview can be reselected with its old provenance retained.

## Decision

- Keep alpha.2 as an unpublished preview, not a 1.0 or RC guarantee. Final selection
  uses a fresh tested main containing current docs/notices and test preparation.
  Record the previous selected SHA/CI/version in manifest history and preserve
  historical ADR-0020 and Phase 3 manifests/artifacts. No published version is reused.
- Replace the completion echo fixture with a directly launched minimal managed
  process, keeping the 15-second timeout, binary drain assertions and repeated
  Windows coverage. Preserve original failure evidence without asserting its root
  cause or introducing an unproven production fix.
- Complete license review within the actual PgCliSharp distribution boundary:
  own assemblies/docs only, dependencies separately resolved, no legacy reference
  package/RID files, .NET runtime or PostgreSQL binaries. Retain the Microsoft
  legacy terms honestly and include runtime dependency attribution/permission and
  upstream notices. Fail closed if that boundary or reviewed dependency graph changes.
- Share one fixed-source candidate build/audit path between CI, manual preflight
  and eventual publication. Pin SDK/lock/notes, require exact-source main CI, and
  verify package/symbol metadata, payload and portable-PDB SourceLink.
- Keep publication disabled. Prepare the final manual workflow and reviewable
  preflight artifacts now; require explicit approval and a reviewed enablement
  change before NuGet login/push or public tags/Releases. Verify the nuget.org
  Trusted Publishing policy when publication is requested; do not claim account
  configuration is verified merely because YAML is correct.
- Preview engineering completion is distinct from external publication and 1.0
  stabilization. Scoped ADR-0016 exclusions remain explicit, not passed or rejected.

## Consequences

An exact green final PR/main and new-source audit can complete the bounded preview
preparation. It cannot establish unknown historical timeout causes, untested
native OS/tool combinations, consumer dependency choices or Trusted Publishing
account setup. Further 1.0 promotion needs a separate scoped acceptance decision.

## 日本語

未公開alpha.2を、文書・権利表示・終了待ち反復テストを含む新しい検証済みmainへ
選び直します。以前の選定ソース・CI・版と旧Phase 3履歴は残します。未公開候補の
準備完了と1.0・外部公開は区別し、実CLIの未検証範囲も維持します。
依存の条件を変更せず、実際に同梱するファイルの範囲で配布レビューを閉じます。
CIと手動preflight・将来公開が同じ固定ソース監査を使い、公開設定は無効のままにします。
公開承認と設定変更前にNuGet pushやタグ・Release作成は行いません。
