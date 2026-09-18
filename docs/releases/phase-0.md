# Phase 0 - Foundation / 基盤

## 日本語

Phase 0 では、PgCliSharp の各 PostgreSQL CLI ラッパーから共通利用する基盤を実装しました。

### 主な内容

- `PgCliSharp.slnx` への移行と .NET 10 SDK の固定
- 対象フレームワーク `netstandard2.0;net8.0;net10.0` の確定
- Nullable、Analyzer、XML ドキュメント、SourceLink、NuGet メタデータの基盤整備
- 英語・日本語のランタイム診断と構造化例外
- PostgreSQL 10〜18 のバージョン／ライフサイクル情報
- シェルを介さないプロセス実行
- キャンセル、タイムアウト、stderr 取得、バイナリ stdout ストリーミング
- 現代 .NET での BCL によるプロセスツリー終了
- `netstandard2.0` 向け CliWrap 互換バックエンド
- 実行ファイルの `--version` 解析、キャッシュ、メジャーバージョン検証
- Linux / Windows / macOS の CI
- .NET Framework 4.8 から `netstandard2.0` アセットを実行する互換性テスト
- 子プロセスツリー終了とバイナリ出力保持の回帰テスト

### 添付ファイル

- `pgcli-sharp-phase-0.zip` — Phase 0 の対象コミットから作成したソース ZIP
- `PgCliSharp.0.1.0-alpha.0.nupkg` — 同じコミットから生成した NuGet パッケージ
- `PgCliSharp.0.1.0-alpha.0.snupkg` — シンボルパッケージ

この Release は開発マイルストーンの記録です。この Release だけで nuget.org に公開されるわけではありません。

---

## English

Phase 0 establishes the reusable foundation for PgCliSharp.

### Included

- `PgCliSharp.slnx` solution format and .NET 10 SDK pinning.
- Accepted target framework matrix: `netstandard2.0;net8.0;net10.0`.
- Strict nullable/analyzer configuration, XML documentation, SourceLink, and NuGet metadata.
- English/Japanese localized runtime diagnostics and structured exception types.
- PostgreSQL 10-18 version/lifecycle metadata.
- Shell-free process execution with cancellation, timeout handling, binary stdout streaming, and stderr capture.
- Modern .NET process-tree termination through the BCL.
- Conditional CliWrap compatibility backend for `netstandard2.0`.
- Executable `--version` parsing, caching, and major-version validation.
- Linux, Windows, and macOS CI.
- Windows .NET Framework 4.8 compatibility execution against the `netstandard2.0` library asset.
- Regression coverage for process-tree termination and binary output preservation.

### Attached artifacts

- `pgcli-sharp-phase-0.zip` — source snapshot for the exact Phase 0 commit.
- `PgCliSharp.0.1.0-alpha.0.nupkg` — NuGet package produced from that commit.
- `PgCliSharp.0.1.0-alpha.0.snupkg` — symbol package produced from that commit.

This Phase Release records a development milestone. It does not by itself publish the package to nuget.org.
