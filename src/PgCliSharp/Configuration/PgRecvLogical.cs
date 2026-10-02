using PgCliSharp.Internal.PgRecvLogical;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgRecvLogical
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgRecvLogicalOptions options, PgRecvLogicalOutput? output = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, output), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgRecvLogicalOptions options, PgRecvLogicalOutput? output = null)
    {
        options = OptionsSnapshot.Copy(options);
        PgRecvLogicalValidator.Validate(options, output, Version);
        return new PgCommand(ExecutablePath, PgRecvLogicalArgumentBuilder.Build(options, output, Version), options.EnvironmentVariables,
            false, output?.Kind == PgRecvLogicalOutputKind.StandardOutput, false, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--create-slot" => new[] { "Action" },
            "--dbname" => new[] { "Database" },
            "--drop-slot" => new[] { "Action" },
            "--enable-failover" => new[] { "EnableFailover" },
            "--enable-two-phase" => new[] { "EnableTwoPhase" },
            "--endpos" => new[] { "EndPosition" },
            "--fsync-interval" => new[] { "FsyncInterval" },
            "--host" => new[] { "Host" },
            "--if-not-exists" => new[] { "IfNotExists" },
            "--no-loop" => new[] { "NoLoop" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--option" => new[] { "PluginOptions" },
            "--password" => new[] { "PasswordPrompt" },
            "--plugin" => new[] { "Plugin" },
            "--port" => new[] { "Port" },
            "--slot" => new[] { "Slot" },
            "--start" => new[] { "Action" },
            "--startpos" => new[] { "StartPosition" },
            "--status-interval" => new[] { "StatusInterval" },
            "--two-phase" => new[] { "EnableTwoPhase" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbose" },
            "-E" => new[] { "EndPosition" },
            "-F" => new[] { "FsyncInterval" },
            "-I" => new[] { "StartPosition" },
            "-P" => new[] { "Plugin" },
            "-S" => new[] { "Slot" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-d" => new[] { "Database" },
            "-h" => new[] { "Host" },
            "-n" => new[] { "NoLoop" },
            "-o" => new[] { "PluginOptions" },
            "-p" => new[] { "Port" },
            "-s" => new[] { "StatusInterval" },
            "-t" => new[] { "EnableTwoPhase" },
            "-v" => new[] { "Verbose" },
            "-w" => new[] { "PasswordPrompt" },
            "--action" => new[] { "Action" },
            _ => Array.Empty<string>(),
        };
    }
}
