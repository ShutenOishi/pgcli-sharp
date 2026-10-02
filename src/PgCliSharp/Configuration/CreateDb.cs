using PgCliSharp.Internal.Createdb;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class CreateDb
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(CreateDbOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(CreateDbOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        CreatedbValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, CreatedbArgumentBuilder.Build(options, Version), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--builtin-locale" => new[] { "BuiltinLocale" },
            "--echo" => new[] { "Echo" },
            "--encoding" => new[] { "Encoding" },
            "--host" => new[] { "Host" },
            "--icu-locale" => new[] { "IcuLocale" },
            "--icu-rules" => new[] { "IcuRules" },
            "--lc-collate" => new[] { "LcCollate" },
            "--lc-ctype" => new[] { "LcCtype" },
            "--locale" => new[] { "Locale" },
            "--locale-provider" => new[] { "LocaleProvider" },
            "--maintenance-db" => new[] { "MaintenanceDatabase" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--owner" => new[] { "Owner" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--strategy" => new[] { "Strategy" },
            "--tablespace" => new[] { "Tablespace" },
            "--template" => new[] { "Template" },
            "--username" => new[] { "Username" },
            "-D" => new[] { "Tablespace" },
            "-E" => new[] { "Encoding" },
            "-O" => new[] { "Owner" },
            "-S" => new[] { "Strategy" },
            "-T" => new[] { "Template" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-e" => new[] { "Echo" },
            "-h" => new[] { "Host" },
            "-l" => new[] { "Locale" },
            "-p" => new[] { "Port" },
            "-w" => new[] { "PasswordPrompt" },
            "<dbname>" => new[] { "DatabaseName" },
            "<description>" => new[] { "Description" },
            _ => Array.Empty<string>(),
        };
    }
}
