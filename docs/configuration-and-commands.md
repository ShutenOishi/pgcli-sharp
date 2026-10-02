# Configuration and commands / 設定とコマンド生成

## English

All 25 wrappers accept existing Options objects and lambda configuration. Lambdas
are extension methods in the wrapper's namespace, so ordinary call syntax stays
`tool.ExecuteAsync(options => ...)`. Import `PgCliSharp`, and also
`PgCliSharp.ServerApplications` for server tools. Each call creates fresh defaults,
invokes the callback synchronously exactly once, and captures its settings. Reuse
an `Action<PgDumpOptions>` helper for common settings without sharing mutable Options.
Callback exceptions propagate synchronously. A null callback is a programmer error.
Existing instance calls, including an untyped null Options argument, keep their
previous overload resolution.

Execution copies Options before the first await, including ordered lists and
process environment overrides. Mutating the original afterward does not change
an outstanding execution. Configure/capture itself must not race with mutation.
Immutable value objects and caller-owned Stream handles are retained, not cloned
or disposed; stream position/lifetime remains the caller's responsibility.

```csharp
var dump = new PgDump("/opt/postgresql/18/bin/pg_dump", PostgreSqlMajorVersion.V18);
PgDumpResult result = await dump.ExecuteAsync(
    configureOptions: options =>
    {
        options.Database = "appdb";
        options.Format = PgDumpFormat.Custom;
        options.Schemas.Add("public");
    },
    output: PgDumpOutput.ToFile("appdb.dump"));
IPgExecutionResult metadata = result;
Console.WriteLine(metadata.Duration);
```

### Offline validation and command generation

| Operation | Behavior |
|---|---|
| `Validate(options, ...)` / lambda equivalent | Return zero errors or the first option failure, with stable `Code`, `OptionNames`, known `PropertyNames`, and localized `Message`. Programmer argument errors propagate. |
| `CreateCommand(options, ...)` / lambda equivalent | Validate using the same validators and serialize using the same builders as execution; invalid options throw the established typed exceptions. Capture exact read-only arguments/environment. |
| `Psql.ValidateForSession` / `CreateSessionCommand` | Apply redirected-session constraints, including rejection of finite command/file actions. |
| Server wrappers' `CreateHelpCommand()` | Capture `--help` without probing or starting the executable. |

These operations do not start even `--version`, check executable existence,
create/open files or consume stream contents. They can run on a machine without
PostgreSQL installed. Existing platform and ambient-environment validation is
retained. They do not establish executable-build, filesystem, stream-lifetime,
server or cluster validity. Execution still validates the actual executable.

```csharp
var dump = new PgDump(@"C:\Program Files\PostgreSQL\18\bin\pg_dump.exe", PostgreSqlMajorVersion.V18);
var options = new PgDumpOptions { Database = "appdb", Format = PgDumpFormat.Custom };
options.Schemas.Add("public");
PgDumpOutput output = PgDumpOutput.ToFile(@"C:\Backups\appdb.dump");
PgValidationResult validation = dump.Validate(options, output);
if (!validation.IsValid)
    Console.WriteLine(validation.Errors[0].Message);
else
{
    PgCommand command = dump.CreateCommand(options, output);
    Console.WriteLine(command); // Redacted diagnostic string.
    string copyable = command.ToCommandLine(PgCommandLineStyle.PowerShell, includeSensitiveValues: true);
    Console.WriteLine(copyable);
}
```

The dump/restore trio additionally accepts `Version? executableVersion` for an
**asserted**, independently known numeric patch version. Without it, requested
patch-sensitive options have `RequiresExecutableVersionCheck == true` on the
validation result/command. `IsValid` means the offline checks passed; it does not
mean deferred checks passed. Supplying a version checks its major and known patch
bounds but never probes or bypasses execution's actual version check. Other build
capabilities and upstream state remain upstream checks.

### Actual command export and secrets

`Arguments` contains exact individual tokens. `EnvironmentVariables` contains
exact process-only overrides; inherited variables are not captured. These explicit
raw-data accesses can expose passwords, connection strings, SQL and forwarded
server fragments. `ToString()` and `ToCommandLine(style)` conservatively redact
**every** argument/environment value. Such diagnostic strings are not the actual
invocation. Only explicit `includeSensitiveValues: true` emits actual values.

Select `PosixShell` for POSIX-shaped quoting or `PowerShell` for PowerShell 7.5+
with `$PSNativeCommandArgumentPassing = 'Standard'`. PowerShell export uses a child
script scope and restores overridden environment values in `finally`. Legacy
PowerShell 5.1 native argument marshaling cannot faithfully preserve every empty
or quoted token and is not covered by the export guarantee. `cmd.exe` syntax is
not generated. PgCliSharp execution remains direct and shell-free on every target.

`UsesStandardInput`, `UsesStandardOutput` and `UsesStandardError` report requested
stream routing. The generated string does not serialize a Stream or invent a
file/pipeline/redirection. Supply such routing separately when copying an invocation.
For `--filter=-` or archive stdin, the generated CLI argument is exact but the input
bytes must still be supplied. Tool-owned file/directory destinations are already
encoded in arguments; stdout archives remain binary.

### Awaited session completion and result metadata

`await session.CompleteAsync(cancellationToken)` signals EOF after accepted input
and waits for process completion/output drain. Cancellation requests the existing
process-tree cleanup and preserves the supplied token. The session's original
process timeout still applies. With neither timeout nor cancellation, upstream
can keep graceful completion waiting. `Cancel` and `Dispose` retain their existing
forceful behavior; caller streams remain caller-owned. No new async-disposal
interface/runtime dependency is required for legacy consumers.

`IPgExecutionResult` exposes `ExitCode`, `Duration`, `ExecutableVersion` and
`RawExecutableVersion` across all result families. Readiness, psql, pgbench and
pg_ctl semantic statuses stay tool-specific; the interface has no universal
`IsSuccess` and does not force stderr/output buffering.

## 日本語

全25ラッパーで、既存の Options 指定とラムダ設定を利用できます。ラムダ設定は
ラッパーと同じ名前空間の拡張メソッドで、`tool.ExecuteAsync(options => ...)` と
呼び出せます。呼び出すたびに新規 Options を作成し、同期的に一度だけ設定します。
共通設定には `Action<PgDumpOptions>` を再利用でき、可変 Options の共有は不要です。
コールバックの例外は同期的に伝播し、null は引数エラーです。既存の null Options
呼び出しも、従来のインスタンスメソッドに解決されます。

実行は最初の非同期処理前に、スカラー・順序付きコレクション・環境変数をコピーします。
その後の元 Options の変更は実行中の処理に影響しません。設定・コピー中の同時変更には
対応しません。不変の値オブジェクトとストリームの参照を保持し、ストリームの複製・
破棄・位置の固定は行いません。

`Validate` は、エラーなし、または最初のオプションエラーを返します。安定した
`Code`、CLI の `OptionNames`、判明している `PropertyNames` と日英の `Message` を
取得でき、メッセージの解析は不要です。全エラーの一括列挙ではなく、引数エラーは
そのまま送出します。`CreateCommand` は実行時と同じ検証・引数生成を使い、無効な
設定では既存の型付き例外を送出します。psql はセッション専用の検証・コマンド生成、
サーバー系は `CreateHelpCommand` も提供します。

いずれも `--version` を含むプロセス起動・実行ファイルの存在確認・ファイル作成／
オープン・ストリーム読み取りを行いません。PostgreSQL 未導入環境でも生成できます。
既存の OS・環境変数依存の検証は維持します。実行ファイルのビルド設定、ファイルシステム、
ストリームの寿命、サーバーやクラスタの状態を保証する機能ではありません。

dump／restore の3ツールでは、既知の正確な版を `executableVersion` に申告できます。
未指定でパッチ版依存オプションを使う場合は `RequiresExecutableVersionCheck` が true
になります。`IsValid` はオフラインで検証できた範囲の合格を意味し、保留された検証の
合格を意味しません。申告版で生成しても、実行時の実バージョン確認は省略しません。

`Arguments` と `EnvironmentVariables` は正確な生データで、秘密情報を含み得ます。
継承する環境変数は含めません。`ToString()` と既定の `ToCommandLine(style)` は
すべての引数・環境変数値を伏せた診断表示です。実際のコマンドが必要な場合は
`includeSensitiveValues: true` を明示します。POSIX または PowerShell 形式を選べ、
PowerShell は7.5以降の Standard 引数渡しを対象にします。環境変数の上書きは終了後に
復元します。5.1固有のネイティブ引数処理、cmd.exe 形式は対象外です。ライブラリの
実行自体は引き続きシェルを経由しません。

`UsesStandardInput/Output/Error` はストリーム接続の要求を示します。任意の Stream を
コマンド文字列へ変換せず、リダイレクトや入力バイト列は別途供給します。ファイルや
ディレクトリ出力をツール自身が扱う場合は、そのパスを引数へ含めます。

`await session.CompleteAsync(cancellationToken)` は EOF 通知から終了・出力転送完了まで
待ちます。キャンセルは既存のプロセスツリー終了処理を要求し、指定トークンを維持します。
セッション開始時のタイムアウトも有効です。両方未指定なら upstream の動作によっては
待ち続けます。`Dispose`／`Cancel` の既存動作と呼び出し側のストリーム所有権は維持し、
新たな非同期破棄インターフェイスやランタイム依存は追加しません。

全結果型は `IPgExecutionResult` で終了コード・実行時間・解析済み実行版・元の数値版を
共通参照できます。各ツール固有の状態は保持し、一律の `IsSuccess` や stderr の
バッファリングは要求しません。英語節のコード例は両言語で同じ API を利用します。
