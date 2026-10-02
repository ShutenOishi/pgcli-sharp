using PgCliSharp.Internal.PgAmcheck;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgAmcheck
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgAmcheckOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgAmcheckOptions options, PgMaintenanceIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        PgAmcheckValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, PgAmcheckArgumentBuilder.Build(options), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--all" => new[] { "AllDatabases" },
            "--checkunique" => new[] { "CheckUnique" },
            "--database" => new[] { "DatabasePatterns" },
            "--echo" => new[] { "Echo" },
            "--endblock" => new[] { "EndBlock" },
            "--exclude-database" => new[] { "ExcludedDatabasePatterns" },
            "--exclude-index" => new[] { "ExcludedIndexPatterns" },
            "--exclude-relation" => new[] { "ExcludedRelationPatterns" },
            "--exclude-schema" => new[] { "ExcludedSchemaPatterns" },
            "--exclude-table" => new[] { "ExcludedTablePatterns" },
            "--exclude-toast-pointers" => new[] { "ExcludeToastPointers" },
            "--heapallindexed" => new[] { "HeapAllIndexed" },
            "--host" => new[] { "Host" },
            "--index" => new[] { "IndexPatterns" },
            "--install-missing" => new[] { "InstallMissing" },
            "--jobs" => new[] { "Jobs" },
            "--maintenance-db" => new[] { "MaintenanceDatabase" },
            "--no-dependent-indexes" => new[] { "NoDependentIndexes" },
            "--no-dependent-toast" => new[] { "NoDependentToast" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-strict-names" => new[] { "StrictNames" },
            "--on-error-stop" => new[] { "OnErrorStop" },
            "--parent-check" => new[] { "ParentCheck" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--progress" => new[] { "Progress" },
            "--relation" => new[] { "RelationPatterns" },
            "--rootdescend" => new[] { "RootDescend" },
            "--schema" => new[] { "SchemaPatterns" },
            "--skip" => new[] { "Skip" },
            "--startblock" => new[] { "StartBlock" },
            "--table" => new[] { "TablePatterns" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbose" },
            "-D" => new[] { "ExcludedDatabasePatterns" },
            "-I" => new[] { "ExcludedIndexPatterns" },
            "-P" => new[] { "Progress" },
            "-R" => new[] { "ExcludedRelationPatterns" },
            "-S" => new[] { "ExcludedSchemaPatterns" },
            "-T" => new[] { "ExcludedTablePatterns" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-a" => new[] { "AllDatabases" },
            "-d" => new[] { "DatabasePatterns" },
            "-e" => new[] { "Echo" },
            "-h" => new[] { "Host" },
            "-i" => new[] { "IndexPatterns" },
            "-j" => new[] { "Jobs" },
            "-p" => new[] { "Port" },
            "-r" => new[] { "RelationPatterns" },
            "-s" => new[] { "SchemaPatterns" },
            "-t" => new[] { "TablePatterns" },
            "-v" => new[] { "Verbose" },
            "-w" => new[] { "PasswordPrompt" },
            "<dbname>" => new[] { "DatabaseName" },
            _ => Array.Empty<string>(),
        };
    }
}
