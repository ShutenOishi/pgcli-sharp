# PgCliSharp 0.1.0-alpha.1 - First NuGet Preview / 初回 NuGet プレビュー

## English

PgCliSharp 0.1.0-alpha.1 is the first public NuGet preview of the typed PostgreSQL backup/restore core.

### Included

- Typed `pg_dump`, `pg_restore`, and `pg_dumpall` APIs.
- PostgreSQL CLI compatibility for versions 10 through 18, including patch-level availability where PostgreSQL security backports require it.
- `netstandard2.0`, `net8.0`, and `net10.0` package assets.
- Binary-safe standard I/O, cancellation, timeout handling, process-tree termination, executable version validation, and localized English/Japanese diagnostics.
- Generated XML documentation with bilingual public API comments.
- SourceLink repository metadata and symbol `.snupkg`.
- Package README files in English and Japanese.

### Release identity

- NuGet package: `PgCliSharp`
- Package version: `0.1.0-alpha.1`
- Git tag: `v0.1.0-alpha.1`
- Source repository: `ShutenOishi/pgcli-sharp`

This is a pre-1.0 preview. Public APIs may still change before 1.0.

### Licensing note

No repository license has been selected in the accepted project decisions as of this preview. This release does not add or imply a new license grant.

---

## 日本語

PgCliSharp 0.1.0-alpha.1 は、PostgreSQL のバックアップ／リストア中核を型付き API として提供する最初の公開 NuGet プレビューです。

### 主な内容

- 型付き `pg_dump`、`pg_restore`、`pg_dumpall` API。
- PostgreSQL CLI 10〜18 の互換性。セキュリティ修正のバックポートで必要となるパッチバージョン単位の availability も検証。
- `netstandard2.0`、`net8.0`、`net10.0` 向け package asset。
- バイナリセーフな標準入出力、キャンセル、タイムアウト、プロセスツリー終了、実行ファイルバージョン検証、英語／日本語のローカライズ診断。
- 公開 API の英日 XML ドキュメント。
- SourceLink のリポジトリメタデータとシンボル `.snupkg`。
- 英語／日本語の package README。

### リリース識別子

- NuGet package: `PgCliSharp`
- Package version: `0.1.0-alpha.1`
- Git tag: `v0.1.0-alpha.1`
- Source repository: `ShutenOishi/pgcli-sharp`

1.0 より前のプレビューであり、1.0 までに公開 API が変更される可能性があります。

### ライセンスに関する注記

このプレビュー時点では、Accepted なプロジェクト判断としてリポジトリのライセンス種別は決定されていません。このリリースによって新たなライセンス許諾を追加・黙示するものではありません。
