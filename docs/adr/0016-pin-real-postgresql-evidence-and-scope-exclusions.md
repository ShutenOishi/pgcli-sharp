# ADR-0016: Pin real PostgreSQL evidence and scope exclusions

- Status: Accepted
- Date: 2026-10-02
- Supersedes: None
- Complements: ADR-0001, ADR-0008, ADR-0011, ADR-0015

## Context

Representative PostgreSQL 16/18 package jobs cannot establish historical real
coverage. Legacy packages vary by runner, and environment-gated tests returning
early can look like passes. Phase 8 needs numeric versions/results/exclusions.

## Decision

- Build fixed official release archives for majors 10-18 on ubuntu-24.04, with
  archive SHA256 checked before extraction. Cache keys include manifest/script
  hashes; cached versions and source provenance must match.
- Omit ICU/Readline/SSL; retain Zlib. These are narrow test builds, not production
  recommendations. Source pinning does not guarantee bit-identical output across
  mutable compiler/system packages. Record build inputs and actual binary hashes.
- Invoke binaries directly through public wrappers; setup shell scripts are only
  fixture provisioning, not a shell-based backend.
- Use job-owned non-root clusters, unique Unix socket directories and no TCP
  listeners. EOL binaries never serve external traffic. Stop before deleting only
  owned files; retain failed migration logs/data on the disposable runner.
- Required CI configuration fails closed. `PGCLI_REAL_PG_REQUIRED=true` forbids
  missing/invalid configuration from silently returning success.
- Record source/run SHA, actual CLI/server versions, binary/source hashes,
  OS/image/architecture, TFM, expected TRX scenario results and exclusions.
  Missing/failed tests and version mismatches fail the collector/job.
- Scope: representative nine-major scenarios, checksums from 12, divergent
  live-source recovery-conf rewind from 13, one actual 16-to-18 Copy upgrade.
- Exclude explicitly: Windows/macOS real binaries, Windows service lifecycle,
  other patches/upgrade pairs, historical manual recovery-conf rewind fixtures,
  TLS/ICU/TTY and unlisted/destructive tool scenarios. Exclusion is neither a pass
  nor a claim that reproduction is impossible.
- Keep three-OS deterministic coverage separate. Typed syntax compatibility,
  package/backends and deferred publication decisions do not change.

## Validation

Require all nine real jobs and three platform jobs on the exact final PR head and
main merge. Collector tests reject absent/failed TRX and wrong versions. No
publication is authorized by this checkpoint; ADR-0012 remains binding.

## 日本語

公式ソースを版・SHA256固定でビルドし、専用 Unix socket の破棄可能な環境で公開 API を
実行します。設定欠落・未実行を合格にせず、実 CLI／server 版・OS／TFM・結果・除外理由を
保存します。除外や unit test を実バイナリ成功・再現不可能の証明とは扱いません。
