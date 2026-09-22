# PgCliSharp

> **日本語:** このページ  
> **English:** [English README](README.md)

PostgreSQL のコマンドラインツールを、型安全な .NET API から利用するためのラッパーライブラリです。

## 現在の状況

Phase 2 のバックアップ／リストア中核、Phase 3 のリリースパイプライン、Phase 4 の Backup/WAL ツール、Phase 5 のデータベース管理・maintenance ツールに加え、Phase 6 の `psql` / `pgbench` rich-I/O 実装まで完了しています。準備済みの `PgCliSharp 0.1.0-alpha.1` は ADR-0012 により最終リリース Phase まで外部公開を延期したまま、Phase 7 の開発へ進みます。

初期対応範囲:

- PostgreSQL 10〜18
- `netstandard2.0`
- `net8.0`
- `net10.0`

Phase 0〜2 の既存 GitHub Release は維持します。ADR-0012 により Phase 3 以降は、review 済みの `main` merge と exact-commit CI を完了条件とし、新しい GitHub Release、tag、Release asset、NuGet 公開は最終リリース Phase まで延期します。

## NuGet プレビュー

準備済みのプレビュー候補は `0.1.0-alpha.1` ですが、現時点では nuget.org に公開していません。下記は最終的に公開する場合の利用形式です。

```bash
dotnet add package PgCliSharp --version 0.1.0-alpha.1
```

package の target framework は `netstandard2.0`、`net8.0`、`net10.0` です。公開は最終リリース Phase まで延期します。公開を承認する場合は、記録済みの Phase 3 source commit を GitHub Actions で再検証し、NuGet Trusted Publishing/OIDC を使用します。長期 NuGet API key はリポジトリへ保存しません。

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

## pg_restore / pg_dumpall

Phase 2 では、アーカイブ入力、データベースへの直接復元、生成 SQL・一覧出力、クラスタ全体の SQL スクリプト出力を、任意文字列のコマンドライン末尾ではなく専用の型で表現します。

```csharp
var pgRestore = new PgRestore(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pg_restore.exe",
    PostgreSqlMajorVersion.V18);

await pgRestore.ExecuteAsync(
    new PgRestoreOptions { Jobs = 4 },
    PgRestoreInput.FromFile("appdb.dump"),
    PgRestoreOutput.ToDatabase("appdb"));

var pgDumpAll = new PgDumpAll(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pg_dumpall.exe",
    PostgreSqlMajorVersion.V18);

await pgDumpAll.ExecuteAsync(
    new PgDumpAllOptions { InitialDatabase = "postgres" },
    PgDumpAllOutput.ToFile("cluster.sql"));
```

`PgRestoreInput` はアーカイブのファイル・ディレクトリ・stdin を区別します。`PgRestoreOutput` はデータベースへの直接復元と生成 SQL・一覧出力を区別し、ストリーム出力では `--file=-` を使用して PostgreSQL 10〜18 で一貫したラッパー動作にします。`PgDumpAllScope` は globals/roles/tablespaces-only の排他的な状態を矛盾する bool の組み合わせなしで表現します。

機械可読な互換性仕様は [`spec/postgresql/pg_restore.json`](spec/postgresql/pg_restore.json) と [`spec/postgresql/pg_dumpall.json`](spec/postgresql/pg_dumpall.json)、調査記録は [`docs/pg-restore-phase-2.md`](docs/pg-restore-phase-2.md) と [`docs/pg-dumpall-phase-2.md`](docs/pg-dumpall-phase-2.md) に保存しています。

## Backup / WAL ツール

Phase 4 では、`pg_basebackup`、`pg_receivewal`、`pg_recvlogical`、`pg_verifybackup`、`pg_combinebackup` の型付き wrapper を追加しました。

`pg_basebackup`、`pg_receivewal`、`pg_recvlogical` は PostgreSQL 10〜18、`pg_verifybackup` は PostgreSQL 13 以降、`pg_combinebackup` は PostgreSQL 17 以降を対象にします。ツール自体が存在しない古い major version を選択した場合は、process 起動前に `PgUnsupportedToolException` で拒否します。

Phase 4 の API は出力先と streaming を明示的に表現します。pg_basebackup の tar stdout や pg_recvlogical の stdout は全量を buffer 化せず呼び出し側所有の stream へ直接渡し、version 固有 option や不正な組み合わせは実行前に検証します。

機械可読な互換性仕様は `spec/postgresql/pg_basebackup.json`、`pg_receivewal.json`、`pg_recvlogical.json`、`pg_verifybackup.json`、`pg_combinebackup.json` に保存しています。調査・実装記録は [`docs/backup-wal-phase-4.md`](docs/backup-wal-phase-4.md) を参照してください。

## データベース管理・maintenance ツール

Phase 5 では `createdb`、`dropdb`、`createuser`、`dropuser`、`vacuumdb`、`reindexdb`、`clusterdb`、`pg_isready`、`pg_amcheck` の型付き wrapper を追加しました。

最初の8ツールは PostgreSQL 10〜18 を対象とします。`pg_amcheck` は PostgreSQL 14 以降で利用でき、PostgreSQL 10〜13 を指定した場合は process 起動前に拒否します。`dropdb --force`、`reindexdb --concurrently`、PostgreSQL 18 の `vacuumdb --missing-stats-only` など、version 固有 option も選択した CLI version に対して実行前検証します。

`PgMaintenanceIo` により、interactive 動作や text streaming に必要な呼び出し側所有の stdin/stdout を転送できます。`pg_isready` の終了コード 0〜3 は通常の非0終了エラーではなく、`PgIsReadyStatus` の意味を持つ状態として返します。

```csharp
var ready = new PgIsReady(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pg_isready.exe",
    PostgreSqlMajorVersion.V18);

PgIsReadyResult status = await ready.ExecuteAsync(new PgIsReadyOptions
{
    Host = "localhost",
    Port = 5432,
    ConnectTimeoutSeconds = 2,
});
```

9ツールの機械可読な互換性仕様は `spec/postgresql/` に保存しています。調査・実装記録は [`docs/database-maintenance-phase-5.md`](docs/database-maintenance-phase-5.md)、完了 evidence は [`docs/phase-5-completion.md`](docs/phase-5-completion.md) を参照してください。

## psql / pgbench の rich I/O

Phase 6 では、`psql` と `pgbench` を PostgreSQL 10〜18 向けの型付き API として追加し、dump 系の one-shot API に rich I/O を無理に押し込みません。

有限な psql 実行では `PsqlIo` に任意の caller-owned stdin/stdout/stderr を渡せます。`PsqlAction` は command/file の混在順序をそのまま保持します。process 起動後も caller から command を送りたい場合は、`StartSessionAsync` で `PsqlSession` を開始します。

```csharp
var psql = new Psql(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\psql.exe",
    PostgreSqlMajorVersion.V18);

using var stdout = new MemoryStream();
using var stderr = new MemoryStream();

using PsqlSession session = await psql.StartSessionAsync(
    new PsqlOptions { Database = "appdb", NoPsqlRc = true },
    new PsqlSessionIo(stdout, stderr),
    timeout: TimeSpan.FromMinutes(2));

byte[] commands = Encoding.UTF8.GetBytes(
    "select current_database();\n\\q\n");
await session.StandardInput.WriteAsync(commands);
session.CompleteInput();
PsqlSessionResult sessionResult = await session.Completion;
```

この session は redirected pipe によるもので、**TTY/PTY terminal ではありません**。Readline、history など terminal 専用動作は保証しません。

`pgbench` は有限実行として扱い、`PgBenchIo` で caller-owned stdout/stderr を指定できます。benchmark summary、progress、debug、diagnostic を全量 memory buffer 化せず stream できます。

```csharp
var pgBench = new PgBench(
    @"C:\\Program Files\\PostgreSQL\\18\\bin\\pgbench.exe",
    PostgreSqlMajorVersion.V18);

await pgBench.ExecuteAsync(
    new PgBenchOptions
    {
        Database = "appdb",
        Clients = 10,
        DurationSeconds = 30,
        ProgressSeconds = 5,
    },
    new PgBenchIo(
        standardOutput: Console.OpenStandardOutput(),
        standardError: Console.OpenStandardError()));
```

PostgreSQL 11/13/15/17 の option 境界、PostgreSQL 13 以降の server-side initialization step `G`、logging/progress/partition/retry 制約、script weight、PostgreSQL 12 以降の runtime-error exit status などは process 起動前または typed result として扱います。

互換性仕様は [`spec/postgresql/psql.json`](spec/postgresql/psql.json) と [`spec/postgresql/pgbench.json`](spec/postgresql/pgbench.json)、調査記録は [`docs/rich-io-phase-6.md`](docs/rich-io-phase-6.md)、session lifecycle の判断は ADR-0013、完了 evidence は [`docs/phase-6-completion.md`](docs/phase-6-completion.md) に保存しています。

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

[GitHub Releases](https://github.com/ShutenOishi/pgcli-sharp/releases) には、Phase 0〜2 の既存 milestone Release を維持します。

Phase 3 以降は ADR-0012 により、実装完了と外部公開を分離します。Phase 完了条件は、review 済みの `main` merge、exact-commit の Linux/macOS/Windows CI、互換性・調査 evidence、repository documentation 更新です。新しい GitHub Release tag、Release、Release asset、NuGet push は最終リリース Phase まで延期します。

## プロジェクト文書

- [AGENTS.md](AGENTS.md) — リポジトリ全体の作業ルール
- [Architecture Decision Records](docs/adr/README.md) — 設計判断の履歴、状態、代替案、影響
- [Architecture and compatibility](docs/architecture.md) — 現在のアーキテクチャと互換性方針
- [Localization and bilingual documentation](docs/localization.md) — 英語・日本語対応方針
- [Implementation and NuGet roadmap](docs/roadmap.md) — 実装・リリース計画

重要な設計方針を変更する場合は、チャット履歴ではなく ADR と統合ドキュメントを同じ変更内で更新します。
