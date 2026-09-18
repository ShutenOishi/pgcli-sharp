# PgCliSharp

> **日本語:** このページ  
> **English:** [English README](README.md)

PostgreSQL のコマンドラインツールを、型安全な .NET API から利用するためのラッパーライブラリです。

## 現在の状況

Phase 1 まで完了しています。最初の完全な型付きラッパーとして `pg_dump` を実装し、PostgreSQL 10〜18 のバージョン差を考慮した検証に対応しました。Phase 2 では `pg_restore` と `pg_dumpall` を実装します。

初期対応範囲:

- PostgreSQL 10〜18
- `netstandard2.0`
- `net8.0`
- `net10.0`

ロードマップ上の各 Phase が完了するたびに GitHub Release を作成し、その時点のソース ZIP と NuGet パッケージ成果物を保存します。

## 主な設計方針

- PostgreSQL 実行ファイルのパスは呼び出し側が明示的に指定します。
- PostgreSQL の各 CLI ごとに専用の型付き Options クラスを用意します。
- 列挙可能な値には enum、構造を持つ値には専用 value object を使用します。
- PostgreSQL のバージョンごとのオプション差異を、プロセス起動前に検証します。
- シェルを介さず PostgreSQL CLI を直接起動します。
- stdout のバイナリ出力を文字列化せず扱える構成にします。
- 公開 API の XML コメントと PgCliSharp 自身のユーザー向け診断は英語・日本語に対応します。

## pg_dump クイックスタート

実行ファイルのパスと、期待する PostgreSQL CLI メジャーバージョンを明示します。

```csharp
var pgDump = new PgDump(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pg_dump.exe",
    PostgreSqlMajorVersion.V18);

var options = new PgDumpOptions
{
    Database = "appdb",
    Format = PgDumpFormat.Custom,
};

options.Schemas.Add("public");

PgDumpResult result = await pgDump.ExecuteAsync(
    options,
    PgDumpOutput.ToFile("appdb.dump"),
    timeout: TimeSpan.FromMinutes(10));
```

`PgDumpOptions` は PostgreSQL 10〜18 のオプション union を enum、value object、順序を保持する collection で表現します。実行前に実際の実行ファイルバージョン、オプションのバージョン別 availability、不正な組み合わせを検証します。stdout はバイナリセーフに扱い、PostgreSQL 17 以降の `--filter=-` ではフィルタールールを stdin からストリーミングできます。

機械可読な互換性仕様は [`spec/postgresql/pg_dump.json`](spec/postgresql/pg_dump.json)、Phase 1 の調査内容は [`docs/pg-dump-phase-1.md`](docs/pg-dump-phase-1.md) に保存しています。
## 開発

ソリューションは XML 形式の `.slnx` を使用します。

```text
PgCliSharp.slnx
```

`global.json` で .NET 10 SDK を基準にし、feature band のロールフォワードを許可しています。

代表的な検証コマンド:

```bash
dotnet restore PgCliSharp.slnx
dotnet build PgCliSharp.slnx --configuration Release --no-restore
dotnet test PgCliSharp.slnx --configuration Release --no-build --no-restore
```

ターゲットフレームワークは `netstandard2.0;net8.0;net10.0` です。

- `netstandard2.0`: ADR-0008 に基づき、内部のプロセス実行互換層として CliWrap を使用します。
- `net8.0` / `net10.0`: .NET BCL のプロセス API を直接使用します。

## Releases

[GitHub Releases](https://github.com/ShutenOishi/pgcli-sharp/releases) に、完了した Phase ごとの成果物を保存します。

各 Phase Release には次を添付します。

- その Phase の正確なコミットから作成したソース ZIP
- `.nupkg`
- `.snupkg`
- 英語・日本語を併記した Release Notes

Phase Release は開発上のマイルストーンです。nuget.org への公開は別途ロードマップと ADR-0006 に従って行います。

## プロジェクト文書

- [AGENTS.md](AGENTS.md) — リポジトリ全体の作業ルール
- [Architecture Decision Records](docs/adr/README.md) — 設計判断の履歴、状態、代替案、影響
- [Architecture and compatibility](docs/architecture.md) — 現在のアーキテクチャと互換性方針
- [Localization and bilingual documentation](docs/localization.md) — 英語・日本語対応方針
- [Implementation and NuGet roadmap](docs/roadmap.md) — 実装・リリース計画

重要な設計方針を変更する場合は、チャット履歴ではなく ADR と統合ドキュメントを同じ変更内で更新します。
