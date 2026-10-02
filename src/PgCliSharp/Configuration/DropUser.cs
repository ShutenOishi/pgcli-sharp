using PgCliSharp.Internal.Dropuser;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class DropUser
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(DropUserOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(DropUserOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        DropuserValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, DropuserArgumentBuilder.Build(options), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--echo" => new[] { "Echo" },
            "--host" => new[] { "Host" },
            "--if-exists" => new[] { "IfExists" },
            "--interactive" => new[] { "Interactive" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--username" => new[] { "Username" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-e" => new[] { "Echo" },
            "-h" => new[] { "Host" },
            "-i" => new[] { "Interactive" },
            "-p" => new[] { "Port" },
            "-w" => new[] { "PasswordPrompt" },
            "<rolename>" => new[] { "RoleName" },
            "--role-name" => new[] { "RoleName" },
            _ => Array.Empty<string>(),
        };
    }
}
