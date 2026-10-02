using PgCliSharp.Internal.PgVerifyBackup;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgVerifyBackup
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgVerifyBackupOptions options, PgVerifyBackupInput input)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, input), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgVerifyBackupOptions options, PgVerifyBackupInput input)
    {
        options = OptionsSnapshot.Copy(options);
        ConfigurationGuard.NotNull(input, nameof(input));
        PgVerifyBackupValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, PgVerifyBackupArgumentBuilder.Build(options, input), options.EnvironmentVariables,
            false, false, false, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--exit-on-error" => new[] { "ExitOnError" },
            "--format" => new[] { "Format" },
            "--ignore" => new[] { "IgnoredPaths" },
            "--manifest-path" => new[] { "ManifestPath" },
            "--no-parse-wal" => new[] { "NoParseWal" },
            "--progress" => new[] { "Progress" },
            "--quiet" => new[] { "Quiet" },
            "--skip-checksums" => new[] { "SkipChecksums" },
            "--wal-directory" => new[] { "WalDirectory" },
            "-F" => new[] { "Format" },
            "-P" => new[] { "Progress" },
            "-e" => new[] { "ExitOnError" },
            "-i" => new[] { "IgnoredPaths" },
            "-m" => new[] { "ManifestPath" },
            "-n" => new[] { "NoParseWal" },
            "-q" => new[] { "Quiet" },
            "-s" => new[] { "SkipChecksums" },
            "-w" => new[] { "WalDirectory" },
            _ => Array.Empty<string>(),
        };
    }
}
