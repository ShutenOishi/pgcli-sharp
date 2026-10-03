# ADR-0020: Select an unpublished full-wrapper preview

- Status: Superseded
- Superseded by: ADR-0021
- Date: 2026-10-03
- Supersedes: None
- Complements: ADR-0012, ADR-0019

## Context

The preserved Phase 3 alpha.1 contains only the backup/restore trio and predates
the full wrapper API and MIT adoption. ADR-0012 requires an explicit candidate
decision without rewriting that historical source or enabling publication.

## Decision

- Select `0.1.0-alpha.2` from MIT main
  `df395760a84643bebec4bab9885c610793e4b222` for unpublished preparation only.
  It supersedes alpha.1 as the intended future preview, not as a published release.
- Preserve both old disabled publication manifests, old source, notes and artifacts.
  Record the separate selection in `.github/release-candidate.json` with
  `publication_enabled: false`. Existing release workflows do not consume it.
- Build selected source without code changes, explicitly overriding Version and
  PackageReleaseNotes. Pin the reviewed library dependency lock separately in
  `eng/candidate.packages.lock.json` and require locked restore. Record the SDK,
  nupkg/snupkg hashes, packaged MIT/docs, dependency/notice evidence and actual
  portable-PDB SourceLink mappings. Compile clean package consumers on all three
  advertised TFMs without executing PostgreSQL.
- Treat the legacy Microsoft.NETCore.Platforms 1.1.0 packaged Microsoft .NET
  Library license honestly, not as MIT. It supplies no runtime/native assets in
  the inspected graph. Preserve its text/hash and flag remaining review before
  publication; inventory success is not legal clearance or a distribution audit.
- This is alpha preparation, not 1.0 or RC readiness. Retain the scoped real-CLI
  exclusions and unresolved Windows/net10 psql completion timeout risk.
- Future promotion requires explicit publication approval, reviewed release
  workflow changes for selected source/version/notes/lock, fresh exact-source
  validation, resolved distribution notices and Trusted Publishing verification.
  No tags, Releases, NuGet push or credential changes occur in this checkpoint.

## Consequences

Current CI builds a separately named unpublished candidate artifact. Ordinary
development alpha.1 artifacts remain explicitly non-candidates. Package consumers
may resolve different transitive versions unless they lock their own graph; the
library build lock does not impose a lock on downstream applications. This audit
does not claim byte-identical packages across SDKs/paths or a security audit.

## 日本語

全ラッパー・新API・MITを含む固定mainを未公開alpha.2候補に選びます。旧alpha.1と
無効の公開設定は履歴として保持します。版とリリース説明だけを上書きし、依存lock・
パッケージ内容・実PDBのSourceLink・3対象のクリーンな利用側コンパイルを検証します。
古いMicrosoftパッケージの独自ライセンスをMITとは表示せず、原文とhashを保存し、
公開前の判断を残します。Windowsの断続的タイムアウトと実CLIの除外も未解決です。
公開設定への移行・再検証・権利表示・Trusted Publishing・別途の公開承認は後続作業です。
