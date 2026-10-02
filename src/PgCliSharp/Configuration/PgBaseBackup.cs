using PgCliSharp.Internal.PgBaseBackup;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgBaseBackup
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgBaseBackupOptions options, PgBaseBackupDestination destination)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, destination), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgBaseBackupOptions options, PgBaseBackupDestination destination)
    {
        options = OptionsSnapshot.Copy(options);
        ConfigurationGuard.NotNull(destination, nameof(destination));
        PgBaseBackupValidator.Validate(options, destination, Version);
        return new PgCommand(ExecutablePath, PgBaseBackupArgumentBuilder.Build(options, destination, Version), options.EnvironmentVariables,
            false, destination.Kind == PgBaseBackupDestinationKind.StandardOutput, false, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--checkpoint" => new[] { "Checkpoint" },
            "--compress" => new[] { "Compression" },
            "--create-slot" => new[] { "CreateSlot" },
            "--dbname" => new[] { "ConnectionString" },
            "--format" => new[] { "Format" },
            "--gzip" => new[] { "Compression" },
            "--host" => new[] { "Host" },
            "--incremental" => new[] { "IncrementalManifest" },
            "--label" => new[] { "Label" },
            "--manifest-checksums" => new[] { "ManifestChecksums" },
            "--manifest-force-encode" => new[] { "ManifestForceEncode" },
            "--max-rate" => new[] { "MaxRate" },
            "--no-clean" => new[] { "NoClean" },
            "--no-estimate-size" => new[] { "NoEstimateSize" },
            "--no-manifest" => new[] { "NoManifest" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-slot" => new[] { "NoSlot" },
            "--no-sync" => new[] { "NoSync" },
            "--no-verify-checksums" => new[] { "NoVerifyChecksums" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--progress" => new[] { "Progress" },
            "--slot" => new[] { "Slot" },
            "--status-interval" => new[] { "StatusInterval" },
            "--sync-method" => new[] { "SyncMethod" },
            "--tablespace-mapping" => new[] { "TablespaceMappings" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbose" },
            "--wal-method" => new[] { "WalMethod" },
            "--waldir" => new[] { "WalDirectory" },
            "--write-recovery-conf" => new[] { "WriteRecoveryConf" },
            "-C" => new[] { "CreateSlot" },
            "-F" => new[] { "Format" },
            "-N" => new[] { "NoSync" },
            "-P" => new[] { "Progress" },
            "-R" => new[] { "WriteRecoveryConf" },
            "-S" => new[] { "Slot" },
            "-T" => new[] { "TablespaceMappings" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-X" => new[] { "WalMethod" },
            "-Z" => new[] { "Compression" },
            "-c" => new[] { "Checkpoint" },
            "-d" => new[] { "ConnectionString" },
            "-h" => new[] { "Host" },
            "-i" => new[] { "IncrementalManifest" },
            "-k" => new[] { "NoVerifyChecksums" },
            "-l" => new[] { "Label" },
            "-n" => new[] { "NoClean" },
            "-p" => new[] { "Port" },
            "-r" => new[] { "MaxRate" },
            "-s" => new[] { "StatusInterval" },
            "-v" => new[] { "Verbose" },
            "-w" => new[] { "PasswordPrompt" },
            "-z" => new[] { "Compression" },
            _ => Array.Empty<string>(),
        };
    }
}
