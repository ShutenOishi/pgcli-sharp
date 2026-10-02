# Phase 7 completion evidence

## English

Baseline main: `5827964490435f9e5c1f329a2abe32d9b05b90a0`.

Six server applications (`InitDb`, `PgCtl`, `PgUpgrade`, `PgRewind`, `PgChecksums`,
`PgResetWal`) are implemented in `PgCliSharp.ServerApplications` with dedicated
Options, bilingual XML documentation, per-major compatibility inventories,
centralized availability, validation, deterministic arguments, finite I/O,
help commands, numeric value objects and semantic pg_ctl status results.
Research and consumer usage: [server applications](server-applications-phase-7.md).
Lifecycle/category rationale: [ADR-0015](adr/0015-server-application-boundaries.md).

### Checkpoint and final evidence

- Implementation/test checkpoint head: `bfa4bd7fdd349665f1f1d9916b378be28eae0b18`.
- [Checkpoint CI run 36976084795](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/36976084795): Linux/macOS/Windows success, including Windows net48 and Linux development-package verification.
- The same run passed real PostgreSQL 16 and 18 Linux/net10.0 smoke integration.
- Final change adds 114 individual option-binding tests plus complete usage/status documentation and richer specification defaults/semantic metadata.
- [PR #13](https://github.com/ShutenOishi/pgcli-sharp/pull/13) indexes the exact final PR head, its CI, main merge commit and exact-main CI after those gates complete. The immutable evidence is maintained in that linked report so the document does not require a self-referential commit hash or an extra untested follow-up commit.

### Audits

- Official per-major manuals and stable parser inventories: 10-18 independently; pg_checksums absent before 12. Source and SGML SHA256 fingerprints indexed in specs.
- Every inventory entry has a public Options/property or explicit help/version binding; all version-varying long options match runtime metadata.
- 114 individual switch bindings verify canonical tokens, values containing spaces, enum/numeric serialization and ordered repeated options.
- Boundary/regression tests cover checksum default/disable behavior, historical short spellings, file-copy omission, WAL size, XID/commit timestamp/multixact changes, ICU fallback, authentication mapping, filenode ranges, rewind source dependencies and pg_ctl semantic statuses.
- Execution tests cover version mismatch, validation before probe, finite streams, environment, timeout/cancellation forwarding and help.
- Real integration creates an owned disposable cluster, checks/disables/enables checksums, runs pg_resetwal DryRun, starts a server with an owned log/socket directory, checks status, stops it, and removes only owned files.
- Windows service lifecycle, real rewind/upgrade migration and the full historical PostgreSQL/OS evidence matrix remain Phase 8 work; no such real-binary coverage is implied by unit/fake tests.
- `.github/nuget-release.json` and `.github/phase-release.json` remain disabled and pin the preserved Phase 3 source. No NuGet push, tag, GitHub Release or Release asset is part of Phase 7.

Final completion requires successful exact-final-head CI, merge with that head fixed,
and successful exact-merge-commit main CI, all recorded in PR #13.

## 日本語

サーバー管理6ツールの専用 Options・型付き値・版固有検証・実行処理・日英公開 API 文書を
実装しました。PostgreSQL 10〜18 の公式資料とソースを版ごとに比較し、互換性仕様に保存しています。
114項目の引数対応確認、数値・ロケール・認証・ソースの境界テスト、I/O と終了コードの検証を追加しました。

中間 head `bfa4bd7` の CI は Linux／macOS／Windows（net48 を含む）で成功し、
PostgreSQL 16／18 の実バイナリ検証も成功しています。最終 head・マージ commit・対応 CI の
固定証跡は [PR #13](https://github.com/ShutenOishi/pgcli-sharp/pull/13) に記録し、最終差分と
main の CI 成功を完了条件にします。

実バイナリ検証は専用の破棄可能なクラスタで初期化・チェックサム操作・WAL リセット dry-run・
起動／状態確認／停止を実施します。実際の移行・巻き戻し・Windows サービス操作および全履歴版の
実行 matrix は Phase 8 へ引き継ぎます。外部公開は ADR-0012 により延期したままです。
