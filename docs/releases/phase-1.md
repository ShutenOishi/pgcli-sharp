# Phase 1 - pg_dump / pg_dump 型付きラッパー

## English

Phase 1 adds the first complete typed PostgreSQL CLI wrapper: `pg_dump`.

### Included

- PostgreSQL 10-18 `pg_dump` option research based on each major version's official documentation.
- Machine-readable compatibility specification at `spec/postgresql/pg_dump.json`.
- Source-level inventory verification against `REL_10_STABLE` through `REL_18_STABLE`.
- Complete typed `PgDumpOptions` coverage for normal dump execution, with explicit bindings for output, version probing, and utility commands.
- Enums for finite values and value objects for structured values such as compression and restrict keys.
- Ordered collections for repeatable schema, table, section, extension, foreign-data, child-table, and filter options.
- Version availability metadata, including exact patch-level requirements for the security-backported `--restrict-key` option.
- Version-aware compression handling for PostgreSQL 10-15 and PostgreSQL 16-18.
- Pre-execution validation for option availability, values, output formats, dependencies, and incompatible combinations.
- Explicit output modeling for stdout, files, and directory-format archives.
- Binary-safe stdout streaming and PostgreSQL 17+ `--filter=-` stdin streaming.
- Explicit executable version validation before normal pg_dump execution.
- Structured `PgDumpResult` metadata without buffering the dump payload.
- English/Japanese public XML documentation and localized PgCliSharp validation diagnostics.
- Linux, macOS, and Windows CI coverage, including .NET Framework 4.8 execution of the `netstandard2.0`/CliWrap backend.
- Regression coverage for argument generation, PostgreSQL 10-18 availability differences, patch-level availability, repeatable options, incompatible combinations, binary I/O, cancellation, timeout propagation, and executable version mismatch.

### Compatibility research highlights

- PostgreSQL 12 removes `--oids`.
- PostgreSQL 15 removes `--no-synchronized-snapshots`.
- PostgreSQL 16 introduces the structured `method[:detail]` compression model and canonical large-object spellings.
- PostgreSQL 17 adds filter files/stdin and sync-method support.
- PostgreSQL 18 adds data/schema/statistics selection controls.
- `--restrict-key` is patch-level availability rather than a simple major boundary: 13.22+, 14.19+, 15.14+, 16.10+, 17.6+, and 18.0+.

### Attached artifacts

- `pgcli-sharp-phase-1.zip` — source snapshot for the exact Phase 1 commit.
- `PgCliSharp.0.1.0-alpha.0.nupkg` — NuGet package produced from that commit.
- `PgCliSharp.0.1.0-alpha.0.snupkg` — symbol package produced from that commit.

This Phase Release records a development milestone. It does not by itself publish the package to nuget.org.

---

## 日本語

Phase 1 では、最初の完全な型付き PostgreSQL CLI ラッパーとして `pg_dump` を実装しました。

### 主な内容

- PostgreSQL 10〜18 の各メジャーバージョン公式資料を個別に比較した `pg_dump` オプション調査。
- `spec/postgresql/pg_dump.json` に保存した機械可読な互換性仕様。
- `REL_10_STABLE`〜`REL_18_STABLE` の公式ソースとの option inventory 照合。
- 通常のダンプ実行を網羅する型付き `PgDumpOptions` と、output・version probe・utility command の明示的な binding。
- 有限値の enum と、compression / restrict key など構造値の value object。
- schema、table、section、extension、foreign data、children、filter など複数回指定可能なオプションの順序保持 collection。
- バージョン別 availability metadata と、セキュリティ修正でバックポートされた `--restrict-key` の正確なパッチバージョン検証。
- PostgreSQL 10〜15 と 16〜18 の差を考慮した compression。
- option availability、値、出力形式、依存関係、相互排他を pg_dump 起動前に検証。
- stdout / file / directory archive を区別する出力モデル。
- バイナリセーフな stdout と、PostgreSQL 17 以降の `--filter=-` 用 stdin ストリーミング。
- 通常実行前の pg_dump 実行ファイルバージョン検証。
- ダンプ本体をメモリへ強制格納しない `PgDumpResult`。
- 公開 API の英語・日本語 XML コメントと、PgCliSharp 側 validation diagnostic の英日ローカライズ。
- Linux / macOS / Windows CI。Windows では .NET Framework 4.8 から `netstandard2.0` / CliWrap backend も実行。
- argument generation、PostgreSQL 10〜18 の差異、パッチ版 availability、repeatable option、非互換な組み合わせ、バイナリ I/O、cancellation、timeout、実行ファイル version mismatch の回帰テスト。

### 互換性調査の主な差分

- PostgreSQL 12 で `--oids` が廃止。
- PostgreSQL 15 で `--no-synchronized-snapshots` が廃止。
- PostgreSQL 16 で `method[:detail]` compression と canonical な large-object 名称を導入。
- PostgreSQL 17 で filter file/stdin と sync-method を追加。
- PostgreSQL 18 で data/schema/statistics の選択制御を追加。
- `--restrict-key` は単純なメジャーバージョン境界ではなく、13.22+ / 14.19+ / 15.14+ / 16.10+ / 17.6+ / 18.0+ で利用可能。

### 添付ファイル

- `pgcli-sharp-phase-1.zip` — Phase 1 の対象コミットから作成したソース ZIP。
- `PgCliSharp.0.1.0-alpha.0.nupkg` — 同じコミットから生成した NuGet パッケージ。
- `PgCliSharp.0.1.0-alpha.0.snupkg` — シンボルパッケージ。

この Release は開発マイルストーンの記録です。この Release だけで nuget.org に公開されるわけではありません。
