using PgCliSharp.Internal.Createuser;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class CreateUser
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(CreateUserOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(CreateUserOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        CreateuserValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, CreateuserArgumentBuilder.Build(options, Version), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--adduser" => new[] { "IsSuperuser" },
            "--bypassrls" => new[] { "BypassRls" },
            "--connection-limit" => new[] { "ConnectionLimit" },
            "--createdb" => new[] { "CanCreateDatabase" },
            "--createrole" => new[] { "CanCreateRole" },
            "--echo" => new[] { "Echo" },
            "--encrypted" => new[] { "Encrypted" },
            "--host" => new[] { "Host" },
            "--inherit" => new[] { "Inherit" },
            "--interactive" => new[] { "Interactive" },
            "--login" => new[] { "Login" },
            "--member-of" => new[] { "MemberOfRoles" },
            "--no-adduser" => new[] { "IsSuperuser" },
            "--no-bypassrls" => new[] { "BypassRls" },
            "--no-createdb" => new[] { "CanCreateDatabase" },
            "--no-createrole" => new[] { "CanCreateRole" },
            "--no-inherit" => new[] { "Inherit" },
            "--no-login" => new[] { "Login" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-replication" => new[] { "Replication" },
            "--no-superuser" => new[] { "IsSuperuser" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--pwprompt" => new[] { "PromptForRolePassword" },
            "--replication" => new[] { "Replication" },
            "--role" => new[] { "MemberOfRoles" },
            "--superuser" => new[] { "IsSuperuser" },
            "--username" => new[] { "Username" },
            "--valid-until" => new[] { "ValidUntil" },
            "--with-admin" => new[] { "AdminOfRoles" },
            "--with-member" => new[] { "Members" },
            "-D" => new[] { "CanCreateDatabase" },
            "-E" => new[] { "Encrypted" },
            "-I" => new[] { "Inherit" },
            "-L" => new[] { "Login" },
            "-P" => new[] { "PromptForRolePassword" },
            "-R" => new[] { "CanCreateRole" },
            "-S" => new[] { "IsSuperuser" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-a" => new[] { "AdminOfRoles" },
            "-c" => new[] { "ConnectionLimit" },
            "-d" => new[] { "CanCreateDatabase" },
            "-e" => new[] { "Echo" },
            "-g" => new[] { "MemberOfRoles" },
            "-h" => new[] { "Host" },
            "-i" => new[] { "Inherit" },
            "-l" => new[] { "Login" },
            "-m" => new[] { "Members" },
            "-p" => new[] { "Port" },
            "-r" => new[] { "CanCreateRole" },
            "-s" => new[] { "IsSuperuser" },
            "-v" => new[] { "ValidUntil" },
            "-w" => new[] { "PasswordPrompt" },
            "<rolename>" => new[] { "RoleName" },
            "--role-name" => new[] { "RoleName" },
            _ => Array.Empty<string>(),
        };
    }
}
