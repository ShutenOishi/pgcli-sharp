using PgCliSharp.Internal.ServerApplications;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp.ServerApplications;

public sealed partial class PgChecksums
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgChecksumsOptions options, PgServerIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgChecksumsOptions options, PgServerIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        PgChecksumsImplementation.Validate(options, Version);
        return new PgCommand(ExecutablePath, PgChecksumsImplementation.Build(options, Version), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--check" => new[] { "Mode" },
            "--disable" => new[] { "Mode" },
            "--enable" => new[] { "Mode" },
            "--filenode" => new[] { "FileNode" },
            "--no-sync" => new[] { "NoSync" },
            "--pgdata" => new[] { "DataDirectory" },
            "--progress" => new[] { "Progress" },
            "--sync-method" => new[] { "SyncMethod" },
            "--verbose" => new[] { "Verbose" },
            "-D" => new[] { "DataDirectory" },
            "-N" => new[] { "NoSync" },
            "-P" => new[] { "Progress" },
            "-c" => new[] { "Mode" },
            "-d" => new[] { "Mode" },
            "-e" => new[] { "Mode" },
            "-f" => new[] { "FileNode" },
            "-v" => new[] { "Verbose" },
            _ => Array.Empty<string>(),
        };
    }

    /// <summary><para>EN: Captures upstream help tokens without probing or executing the tool.</para><para>JA: ツールの確認や実行を行わず upstream ヘルプの引数を取得します。</para></summary>
    public PgCommand CreateHelpCommand(PgServerIo? io = null) => new PgCommand(ExecutablePath, HelpArguments, new Dictionary<string, string>(),
        io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null);
}
