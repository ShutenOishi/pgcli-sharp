using PgCliSharp.Internal.Vacuumdb;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class VacuumDb
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(VacuumDbOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(VacuumDbOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        VacuumdbValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, VacuumdbArgumentBuilder.Build(options), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--all" => new[] { "AllDatabases" },
            "--analyze" => new[] { "Analyze" },
            "--analyze-in-stages" => new[] { "AnalyzeInStages" },
            "--analyze-only" => new[] { "AnalyzeOnly" },
            "--buffer-usage-limit" => new[] { "BufferUsageLimit" },
            "--dbname" => new[] { "DatabaseName" },
            "--disable-page-skipping" => new[] { "DisablePageSkipping" },
            "--echo" => new[] { "Echo" },
            "--exclude-schema" => new[] { "ExcludedSchemas" },
            "--force-index-cleanup" => new[] { "IndexCleanup" },
            "--freeze" => new[] { "Freeze" },
            "--full" => new[] { "Full" },
            "--host" => new[] { "Host" },
            "--jobs" => new[] { "Jobs" },
            "--maintenance-db" => new[] { "MaintenanceDatabase" },
            "--min-mxid-age" => new[] { "MinMultiXactIdAge" },
            "--min-xid-age" => new[] { "MinXidAge" },
            "--missing-stats-only" => new[] { "MissingStatsOnly" },
            "--no-index-cleanup" => new[] { "IndexCleanup" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-process-main" => new[] { "NoProcessMain" },
            "--no-process-toast" => new[] { "NoProcessToast" },
            "--no-truncate" => new[] { "NoTruncate" },
            "--parallel" => new[] { "ParallelWorkers" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--quiet" => new[] { "Quiet" },
            "--schema" => new[] { "Schemas" },
            "--skip-locked" => new[] { "SkipLocked" },
            "--table" => new[] { "Tables" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbose" },
            "-F" => new[] { "Freeze" },
            "-N" => new[] { "ExcludedSchemas" },
            "-P" => new[] { "ParallelWorkers" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-Z" => new[] { "AnalyzeOnly" },
            "-a" => new[] { "AllDatabases" },
            "-d" => new[] { "DatabaseName" },
            "-e" => new[] { "Echo" },
            "-f" => new[] { "Full" },
            "-h" => new[] { "Host" },
            "-j" => new[] { "Jobs" },
            "-n" => new[] { "Schemas" },
            "-p" => new[] { "Port" },
            "-q" => new[] { "Quiet" },
            "-t" => new[] { "Tables" },
            "-v" => new[] { "Verbose" },
            "-w" => new[] { "PasswordPrompt" },
            "-z" => new[] { "Analyze" },
            "<dbname>" => new[] { "DatabaseName" },
            _ => Array.Empty<string>(),
        };
    }
}
