# Phase 3 - First NuGet Preview / 初回 NuGet プレビュー

## English

Phase 3 publishes PgCliSharp's first public NuGet preview, `PgCliSharp 0.1.0-alpha.1`, after completing the typed PostgreSQL backup/restore core in Phases 1 and 2.

### Included

- Public preview package ID/version: `PgCliSharp 0.1.0-alpha.1`.
- NuGet version tag: `v0.1.0-alpha.1`.
- Separate Phase milestone tag: `phase-3`.
- Both tags are required to identify the same source commit for Phase 3 completion.
- NuGet Trusted Publishing/OIDC through the GitHub `release` environment; no long-lived NuGet API key is stored in the repository.
- Exact-commit publication gate: the NuGet release workflow waits for the same `main` commit to pass the full CI matrix before publishing.
- Linux, macOS, and Windows build/test validation, including the Windows .NET Framework 4.8 consumer of the `netstandard2.0` asset.
- Package validation for the `.nupkg` and `.snupkg`, including target-framework assets, XML documentation, symbol PDBs, README files, repository metadata, and the SourceLink source commit.
- Clean local package-consumer builds for `netstandard2.0`, `net8.0`, and `net10.0`.
- Immutable package/tag preflight checks to prevent reusing a published version or mismatching an existing tag.
- A version GitHub Release containing the public NuGet package and symbol package.
- A Phase 3 GitHub Release containing the exact source ZIP, `.nupkg`, and `.snupkg`.

### Published package content

The preview contains the typed `pg_dump`, `pg_restore`, and `pg_dumpall` APIs with PostgreSQL CLI 10-18 compatibility, version/patch-aware validation, binary-safe I/O, executable-version checking, cancellation/timeout handling, bilingual public XML documentation, and English/Japanese localized diagnostics.

### Licensing note

No repository license has been selected in an Accepted project decision as of this preview. Phase 3 does not add or imply a new license grant.

---

## 日本語

Phase 3 では、Phase 1・2 で完成した PostgreSQL バックアップ／リストア中核を、最初の公開 NuGet プレビュー `PgCliSharp 0.1.0-alpha.1` として公開します。

### 主な内容

- 公開プレビュー package ID / version: `PgCliSharp 0.1.0-alpha.1`。
- NuGet version tag: `v0.1.0-alpha.1`。
- Phase マイルストーン用の別 tag: `phase-3`。
- Phase 3 完了時は、両 tag が同じ source commit を示すことを必須化。
- GitHub `release` environment と NuGet Trusted Publishing/OIDC を使用し、長期 NuGet API key はリポジトリへ保存しない。
- 正確な commit 単位の公開ゲート。NuGet release workflow は、同一の `main` commit に対する全 CI matrix 成功を確認してから公開。
- Linux / macOS / Windows の build/test。Windows では `netstandard2.0` asset を利用する .NET Framework 4.8 consumer test も継続。
- `.nupkg` / `.snupkg` の target framework asset、XML documentation、symbol PDB、README、repository metadata、SourceLink source commit を自動検証。
- `netstandard2.0`、`net8.0`、`net10.0` のクリーンな package consumer build。
- 公開済み version の再利用や既存 tag の不一致を拒否する immutable identity preflight。
- 公開 NuGet package と symbol package を添付する version GitHub Release。
- 正確な source ZIP、`.nupkg`、`.snupkg` を保存する Phase 3 GitHub Release。

### 公開パッケージの内容

このプレビューには、PostgreSQL CLI 10〜18 に対応する型付き `pg_dump`、`pg_restore`、`pg_dumpall` API、メジャー／パッチバージョンを考慮した validation、バイナリセーフな I/O、実行ファイル version 検証、cancellation / timeout、英日公開 XML documentation、英語／日本語のローカライズ診断が含まれます。

### ライセンスに関する注記

このプレビュー時点では、Accepted なプロジェクト判断としてリポジトリのライセンス種別は決定されていません。Phase 3 によって新たなライセンス許諾を追加・黙示するものではありません。
