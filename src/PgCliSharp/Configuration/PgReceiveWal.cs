using PgCliSharp.Internal.PgReceiveWal;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgReceiveWal
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgReceiveWalOptions options)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgReceiveWalOptions options)
    {
        options = OptionsSnapshot.Copy(options);
        PgReceiveWalValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, PgReceiveWalArgumentBuilder.Build(options, Version), options.EnvironmentVariables,
            false, false, false, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--compress" => new[] { "Compression" },
            "--create-slot" => new[] { "Action" },
            "--dbname" => new[] { "ConnectionString" },
            "--directory" => new[] { "Directory" },
            "--drop-slot" => new[] { "Action" },
            "--endpos" => new[] { "EndPosition" },
            "--host" => new[] { "Host" },
            "--if-not-exists" => new[] { "IfNotExists" },
            "--no-loop" => new[] { "NoLoop" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-sync" => new[] { "NoSync" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--slot" => new[] { "Slot" },
            "--status-interval" => new[] { "StatusInterval" },
            "--synchronous" => new[] { "Synchronous" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbose" },
            "-D" => new[] { "Directory" },
            "-E" => new[] { "EndPosition" },
            "-S" => new[] { "Slot" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-Z" => new[] { "Compression" },
            "-d" => new[] { "ConnectionString" },
            "-h" => new[] { "Host" },
            "-n" => new[] { "NoLoop" },
            "-p" => new[] { "Port" },
            "-s" => new[] { "StatusInterval" },
            "-v" => new[] { "Verbose" },
            "-w" => new[] { "PasswordPrompt" },
            _ => Array.Empty<string>(),
        };
    }
}
