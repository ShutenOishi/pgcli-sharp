using PgCliSharp.Internal.Reindexdb;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class ReindexDb
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(ReindexDbOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(ReindexDbOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        ReindexdbValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, ReindexdbArgumentBuilder.Build(options), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--all" => new[] { "AllDatabases" },
            "--concurrently" => new[] { "Concurrently" },
            "--dbname" => new[] { "DatabaseName" },
            "--echo" => new[] { "Echo" },
            "--host" => new[] { "Host" },
            "--index" => new[] { "Indexes" },
            "--jobs" => new[] { "Jobs" },
            "--maintenance-db" => new[] { "MaintenanceDatabase" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--quiet" => new[] { "Quiet" },
            "--schema" => new[] { "Schemas" },
            "--system" => new[] { "SystemCatalogs" },
            "--table" => new[] { "Tables" },
            "--tablespace" => new[] { "Tablespace" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbose" },
            "-S" => new[] { "Schemas" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-a" => new[] { "AllDatabases" },
            "-d" => new[] { "DatabaseName" },
            "-e" => new[] { "Echo" },
            "-h" => new[] { "Host" },
            "-i" => new[] { "Indexes" },
            "-j" => new[] { "Jobs" },
            "-p" => new[] { "Port" },
            "-q" => new[] { "Quiet" },
            "-s" => new[] { "SystemCatalogs" },
            "-t" => new[] { "Tables" },
            "-v" => new[] { "Verbose" },
            "-w" => new[] { "PasswordPrompt" },
            "<dbname>" => new[] { "DatabaseName" },
            _ => Array.Empty<string>(),
        };
    }
}
