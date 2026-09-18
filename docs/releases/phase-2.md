# Phase 2 - pg_restore + pg_dumpall / pg_restore・pg_dumpall 型付きラッパー

## English

Phase 2 completes PgCliSharp's initial PostgreSQL backup/restore trio by adding fully typed `pg_restore` and `pg_dumpall` wrappers alongside Phase 1's `pg_dump`.

### Included

- Independent PostgreSQL 10-18 research for `pg_restore` and `pg_dumpall` based on official application documentation and `REL_10_STABLE` through `REL_18_STABLE` upstream source.
- Machine-readable compatibility specifications at `spec/postgresql/pg_restore.json` and `spec/postgresql/pg_dumpall.json`.
- Source-level option inventory audits, alias/spelling tracking, per-major resolved inventories, constraints, defaults, and explicit API bindings.
- Tool-specific `PgRestoreOptions` and `PgDumpAllOptions` APIs rather than a public common-options base.
- Typed mutually exclusive models including `PgRestoreMode`, `PgRestoreContentMode`, `PgRestoreTransactionMode`, `PgDumpAllScope`, and `PgDumpAllContentMode`.
- Dedicated `PgRestoreInput` modeling for archive file, directory, and stdin sources.
- Dedicated restore/script/list and cluster-script output abstractions that preserve stdout as bytes.
- PostgreSQL 17+ filter stdin support for both tools and pg_restore archive-stdin/filter-stdin conflict validation.
- Centralized version availability catalogs, including exact `--restrict-key` security-backport boundaries: 13.22+, 14.19+, 15.14+, 16.10+, 17.6+, and 18.0+.
- Version-aware hard-error validation without turning upstream accepted/ignored combinations into wrapper errors.
- Deterministic shell-free argument generation, canonical alias spellings, invariant numeric formatting, and caller-order preservation for repeatable options.
- Reuse of the established version probe, cancellation, timeout, process-tree termination, stderr, environment, and non-zero-exit execution infrastructure.
- Spec-to-public-API and spec-to-runtime-availability coverage checks.
- Backup/restore trio tests covering `pg_dump` custom/directory archive relationships with `pg_restore`, while preserving `pg_dumpall` as SQL-script output.
- English/Japanese XML documentation, localized diagnostics, and updated README/architecture/roadmap documentation.
- Linux, macOS, and Windows CI coverage, including .NET Framework 4.8 execution against the `netstandard2.0`/CliWrap asset.

### Compatibility research highlights

- `pg_restore` script-output behavior differs before/after PostgreSQL 12; PgCliSharp normalizes stream output with `--file=-`.
- `pg_restore --no-reconnect` remains accepted as an upstream compatibility no-op.
- `pg_restore --create` + `--single-transaction` becomes an explicit hard error from PostgreSQL 12 onward and is not projected backward.
- PostgreSQL 17 adds `pg_restore --filter` and `--transaction-size`; PostgreSQL 18 adds data/schema/statistics controls.
- `pg_dumpall -d/--dbname` is a libpq connection string, while `-l/--database` chooses the initial catalog database; these semantics are intentionally modeled separately.
- PostgreSQL 12 removes `pg_dumpall --oids`; PostgreSQL 17 adds filters; PostgreSQL 18 adds data/schema/statistics controls and `--sequence-data`.
- pg_dumpall's sibling-`pg_dump` same-version requirement remains an upstream invariant and surfaces through the ordinary process failure path.

### Attached artifacts

- `pgcli-sharp-phase-2.zip` — source snapshot for the exact Phase 2 merge commit.
- `PgCliSharp.0.1.0-alpha.0.nupkg` — NuGet package produced from that commit.
- `PgCliSharp.0.1.0-alpha.0.snupkg` — symbol package produced from that commit.

This Phase Release records a development milestone. It does not by itself publish the package to nuget.org.

---

## 日本語

Phase 2 では、Phase 1 の `pg_dump` に `pg_restore` と `pg_dumpall` を加え、PgCliSharp の初期バックアップ／リストア3ツールを型付き API として完成させました。

### 主な内容

- PostgreSQL 10〜18 の `pg_restore` / `pg_dumpall` を、公式アプリケーション文書と `REL_10_STABLE`〜`REL_18_STABLE` の上流ソースで個別に調査。
- `spec/postgresql/pg_restore.json` と `spec/postgresql/pg_dumpall.json` に保存した機械可読な互換性仕様。
- source option inventory、alias/spelling、メジャー別 resolved inventory、制約、既定値、API binding の監査。
- 公開共通 Options 基底を作らず、ツール固有の `PgRestoreOptions` / `PgDumpAllOptions` を実装。
- `PgRestoreMode`、`PgRestoreContentMode`、`PgRestoreTransactionMode`、`PgDumpAllScope`、`PgDumpAllContentMode` など、相互排他状態を型で表現。
- archive file / directory / stdin を区別する `PgRestoreInput`。
- restore/script/list と cluster SQL script の出力を専用型で表現し、stdout を文字列化せずバイト列のまま扱う I/O。
- PostgreSQL 17 以降の両ツールの filter stdin と、pg_restore の archive stdin / filter stdin 競合検証。
- `--restrict-key` のセキュリティバックポート境界 13.22+ / 14.19+ / 15.14+ / 16.10+ / 17.6+ / 18.0+ を含む、集中管理された availability catalog。
- 上流の hard error を事前検証しつつ、上流が受理・無視する組み合わせを過剰に拒否しない version-aware validation。
- shell を介さない決定的な引数生成、canonical alias、invariant な数値書式、repeatable option の順序保持。
- 既存の executable version probe、cancellation、timeout、process tree 終了、stderr、environment、non-zero exit の実行基盤を再利用。
- spec と公開 API、spec と runtime availability の同期を検証するテスト。
- `pg_dump` custom/directory archive と `pg_restore` の関係、および `pg_dumpall` が SQL script 出力であることを固定する trio テスト。
- 公開 API の英語・日本語 XML コメント、ローカライズ診断、README / architecture / roadmap の更新。
- Linux / macOS / Windows CI。Windows では .NET Framework 4.8 から `netstandard2.0` / CliWrap asset も検証。

### 互換性調査の主な差分

- `pg_restore` の script 出力条件は PostgreSQL 12 前後で異なるため、PgCliSharp の stream 出力は `--file=-` で統一。
- `pg_restore --no-reconnect` は上流互換性のための no-op として受理。
- `pg_restore --create` + `--single-transaction` の明示的 hard error は PostgreSQL 12 以降に限定。
- PostgreSQL 17 で `pg_restore --filter` / `--transaction-size`、PostgreSQL 18 で data/schema/statistics 制御を追加。
- `pg_dumpall -d/--dbname` は libpq 接続文字列、`-l/--database` は初期 catalog DB であり、意味を分離してモデル化。
- PostgreSQL 12 で `pg_dumpall --oids` を廃止、17 で filter、18 で data/schema/statistics 制御と `--sequence-data` を追加。
- pg_dumpall が sibling `pg_dump` と同一バージョンを要求する点は上流 invariant とし、通常のプロセス失敗経路で保持。

### 添付ファイル

- `pgcli-sharp-phase-2.zip` — Phase 2 の正確な merge commit から作成したソース ZIP。
- `PgCliSharp.0.1.0-alpha.0.nupkg` — 同じコミットから生成した NuGet パッケージ。
- `PgCliSharp.0.1.0-alpha.0.snupkg` — シンボルパッケージ。

この Release は開発マイルストーンの記録です。この Release だけで nuget.org に公開されるわけではありません。
