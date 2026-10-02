using PgCliSharp.Internal.PgCombineBackup;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgCombineBackup
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgCombineBackupOptions options)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgCombineBackupOptions options)
    {
        options = OptionsSnapshot.Copy(options);
        PgCombineBackupValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, PgCombineBackupArgumentBuilder.Build(options, Version), options.EnvironmentVariables,
            false, false, false, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--clone" => new[] { "CopyMethod" },
            "--copy" => new[] { "CopyMethod" },
            "--copy-file-range" => new[] { "CopyMethod" },
            "--debug" => new[] { "Debug" },
            "--dry-run" => new[] { "DryRun" },
            "--link" => new[] { "CopyMethod" },
            "--manifest-checksums" => new[] { "ManifestChecksums" },
            "--no-manifest" => new[] { "NoManifest" },
            "--no-sync" => new[] { "NoSync" },
            "--output" => new[] { "OutputDirectory" },
            "--sync-method" => new[] { "SyncMethod" },
            "--tablespace-mapping" => new[] { "TablespaceMappings" },
            "-N" => new[] { "NoSync" },
            "-T" => new[] { "TablespaceMappings" },
            "-d" => new[] { "Debug" },
            "-k" => new[] { "CopyMethod" },
            "-n" => new[] { "DryRun" },
            "-o" => new[] { "OutputDirectory" },
            "<backup-directory>..." => new[] { "InputDirectories" },
            "--backup-directories" => new[] { "InputDirectories" },
            "--copy-method" => new[] { "CopyMethod" },
            _ => Array.Empty<string>(),
        };
    }
}
