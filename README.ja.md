# PgCliSharp

> **日本語:** このページ  
> **English:** [English README](README.md)

PostgreSQL のコマンドラインツールを、型安全な .NET API から利用するためのラッパーライブラリです。

## 現在の状況

全25ラッパー、レビュー済みAPI、ラムダ設定と実行しないコマンド生成を実装済みです。Phase 8では共有Process実装を含む未公開 `1.0.0-rc.1` の正確なsource／main CIを固定済みで、3OSのRC監査と最終selection CIが残っています。ADR-0025により1.0までは不具合修正・検証・文書改善に限定します。[RC準備](docs/phase-8-rc1.md)に固定・監査の条件と未検証範囲を記録します。1.0昇格や公開承認ではなく、旧alpha.1／alpha.2は履歴として保存します。

初期対応範囲:

- PostgreSQL 10〜18
- `netstandard2.0`
- `net8.0`
- `net10.0`

Phase 0〜2 の既存 GitHub Release は維持します。ADR-0012 により Phase 3 以降は、review 済みの `main` merge と exact-commit CI を完了条件とし、新しい GitHub Release、tag、Release asset、NuGet 公開は最終リリース Phase まで延期します。

## NuGet プレビュー

nuget.orgには未公開です。manifestでは `1.0.0-rc.1` の正確なソースを固定済みです。3OSのRC監査と最終selection CIが成功するまでは候補を使用せず、manifestと監査済みartifactで出所を確認してください。保存したalpha.2は旧実装で、RCではありません。検証済みRC artifactを `./candidate-packages` へ展開後、利用側で次のように指定できます。

利用側プロジェクトの横に次の `NuGet.Config` を作り、候補と別途解決する依存をWindows／Linux／macOSで復元できるようにします:

```xml
<configuration>
  <packageSources>
    <clear />
    <add key="candidate" value="./candidate-packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

```bash
dotnet add package PgCliSharp --version 1.0.0-rc.1
dotnet restore
```

対象は `netstandard2.0`、`net8.0`、`net10.0` です。別の依存パッケージもNuGetまたはオフラインfeedから復元する必要があります。PostgreSQL本体は同梱しません。artifactには候補ソース・SDK・build lock・監査記録を含めます。外部公開は別途の承認とTrusted Publishingの確認後に行い、長期API keyは保存しません。[第三者の権利表示](THIRD-PARTY-NOTICES.md)も参照してください。

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
    @"C:\Program Files\PostgreSQL\18\bin\pg_dump.exe",
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
    @"C:\Program Files\PostgreSQL\18\bin\pg_restore.exe",
    PostgreSqlMajorVersion.V18);

await pgRestore.ExecuteAsync(
    new PgRestoreOptions { Jobs = 4 },
    PgRestoreInput.FromFile("appdb.dump"),
    PgRestoreOutput.ToDatabase("appdb"));

var pgDumpAll = new PgDumpAll(
    @"C:\Program Files\PostgreSQL\18\bin\pg_dumpall.exe",
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
    @"C:\Program Files\PostgreSQL\18\bin\pg_isready.exe",
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
    @"C:\Program Files\PostgreSQL\18\bin\psql.exe",
    PostgreSqlMajorVersion.V18);

using var stdout = new MemoryStream();
using var stderr = new MemoryStream();

using PsqlSession session = await psql.StartSessionAsync(
    new PsqlOptions { Database = "appdb", NoPsqlRc = true },
    new PsqlSessionIo(stdout, stderr),
    timeout: TimeSpan.FromMinutes(2));

byte[] commands = Encoding.UTF8.GetBytes(
    "select current_database();\n\\q\n");
await session.StandardInput.WriteAsync(commands, 0, commands.Length);
PsqlSessionResult sessionResult = await session.CompleteAsync();
```

この session は redirected pipe によるもので、**TTY/PTY terminal ではありません**。Readline、history など terminal 専用動作は保証しません。

`pgbench` は有限実行として扱い、`PgBenchIo` で caller-owned stdout/stderr を指定できます。benchmark summary、progress、debug、diagnostic を全量 memory buffer 化せず stream できます。

```csharp
var pgBench = new PgBench(
    @"C:\Program Files\PostgreSQL\18\bin\pgbench.exe",
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

## サーバー管理ツール

Phase 7 では `PgCliSharp.ServerApplications` に `InitDb`、`PgCtl`、`PgUpgrade`、
`PgRewind`、`PgChecksums`、`PgResetWal` を追加します。PostgreSQL 10〜18 の
専用 Options・実行前検証・実行ファイルの版確認・呼び出し側所有の I/O・`GetHelpAsync` を
用意します。`PgChecksums` は12以降です。

```csharp
using PgCliSharp.ServerApplications;

var ctl = new PgCtl(@"C:\Program Files\PostgreSQL\18\bin\pg_ctl.exe",
    PostgreSqlMajorVersion.V18);
PgCtlResult status = await ctl.ExecuteAsync(new PgCtlOptions
{
    Command = PgCtlCommand.Status,
    DataDirectory = @"C:\pgdata\new-cluster",
});
```

状態確認では稼働・停止・ディレクトリ利用不可を `PgCtlServerStatus` で返します。
起動・再起動は独立したサーバーのパイプを閉じるため、明示的な `LogFile` が必須です。
initdb の `DataChecksums` が null の場合は、17まで無効・18以降有効という upstream の
既定値を維持します。アップグレード・巻き戻し・WAL リセットはデータへ影響し、
キャンセルによる取り消しは保証しません。pg_resetwal は最後の修復手段です。
版固有の制約と実バイナリ検証範囲は [サーバー管理ツールガイド](docs/server-applications-phase-7.md)
を参照してください。

## 開発

上の C# 例は modern target と Windows の .NET Framework 4.8 consumer で
コンパイル検証しますが、CI は例のデータベース操作を実行しません。必要に応じて
`using PgCliSharp;`、`using System;`、`using System.IO;`、`using System.Text;`
を追加し、実行ファイルのパスと接続先を自身の環境へ変更してください。
PostgreSQL 実行ファイルは同梱せず、CLI 版と接続先サーバー版は別です。
ストリーム所有権、状態／例外、互換性の限界は [API レビューと凍結](docs/phase-8-api-review.md)
にまとめています。

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

- `netstandard2.0`: ADR-0024に基づき、Processの終了監視を共有し、内部で引数整形・終了通知を補います。System.ManagementでWindows子孫終了を試み、CliWrap依存は外します。子孫終了APIのない古いUnixランタイムでは直接の子のみ停止できます。固定alpha.2候補は旧実装のままです。
- `net8.0` / `net10.0`: .NET BCL のプロセス API を直接使用します。

## ライセンス

PgCliSharpは[MIT](LICENSE)です。商用利用・改変版の非公開配布を認め、ソース提供を
求めません。著作権表示と許諾文は保持してください。PostgreSQL実行ファイルは同梱せず、
その独自ライセンスは変更しません。依存関係の扱いは[日英ガイド](docs/licensing.md)をご覧ください。

## リリース状況

[GitHub Releases](https://github.com/ShutenOishi/pgcli-sharp/releases) には、Phase 0〜2 の既存 milestone Release を維持します。

Phase 3 以降は ADR-0012 により、実装完了と外部公開を分離します。Phase 完了条件は、review 済みの `main` merge、exact-commit の Linux/macOS/Windows CI、互換性・調査 evidence、repository documentation 更新です。新しい GitHub Release tag、Release、Release asset、NuGet push は最終リリース Phase まで延期します。

## プロジェクト文書

- [AGENTS.md](AGENTS.md) — リポジトリ全体の作業ルール
- [Architecture Decision Records](docs/adr/README.md) — 設計判断の履歴、状態、代替案、影響
- [Architecture and compatibility](docs/architecture.md) — 現在のアーキテクチャと互換性方針
- [Localization and bilingual documentation](docs/localization.md) — 英語・日本語対応方針
- [Implementation and NuGet roadmap](docs/roadmap.md) — 実装・リリース計画

重要な設計方針を変更する場合は、チャット履歴ではなく ADR と統合ドキュメントを同じ変更内で更新します。

## ラムダ設定とコマンド生成

全25ラッパーでラムダ設定・オフライン検証・コマンド生成を利用できます。非同期実行前に設定をコピーします。ストリーム接続・パッチ版検証・秘密情報の扱いは[日英ガイド](docs/configuration-and-commands.md)をご覧ください。

```csharp
var dump = new PgDump(@"C:\Program Files\PostgreSQL\18\bin\pg_dump.exe", PostgreSqlMajorVersion.V18);
PgCommand command = dump.CreateCommand(
    configureOptions: options =>
    {
        options.Database = "appdb";
        options.Format = PgDumpFormat.Custom;
        options.Schemas.Add("public");
    },
    output: PgDumpOutput.ToFile("appdb.dump"));
string actualCommand = command.ToCommandLine(PgCommandLineStyle.PowerShell, includeSensitiveValues: true);
Console.WriteLine(actualCommand);
```

バージョン確認も含めて実行せず、コマンドを生成します。PowerShell 出力は7.5以降の Standard 引数渡しが対象です。生の出力には秘密情報を含み得ます。`command.ToString()` はすべての引数・環境変数値を伏せます。ストリームは別途接続します。`Validate` は最初のオフライン設定エラーを返し、実行ファイルやサーバー状態は確認しません。
