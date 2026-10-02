using PgCliSharp.Internal.PgDump;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgDump
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgDumpOptions options, PgDumpOutput output, Version? executableVersion = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, output, executableVersion), ValidationProperties, options.RestrictKey is not null && executableVersion is null);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgDumpOptions options, PgDumpOutput output, Version? executableVersion = null)
    {
        options = OptionsSnapshot.Copy(options);
        ConfigurationGuard.NotNull(output, nameof(output));
        PgDumpValidator.Validate(options, output, Version, OfflineValidation.ExactVersion(ExecutablePath, Version, executableVersion));
        return new PgCommand(ExecutablePath, PgDumpArgumentBuilder.Build(options, output, Version), options.EnvironmentVariables,
            PgDumpValidator.GetStandardInput(options) is not null, output.Kind == PgDumpOutputKind.StandardOutput, false, options.RestrictKey is not null && executableVersion is null);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--attribute-inserts" => new[] { "ColumnInserts" },
            "--binary-upgrade" => new[] { "BinaryUpgrade" },
            "--blobs" => new[] { "LargeObjects" },
            "--clean" => new[] { "Clean" },
            "--column-inserts" => new[] { "ColumnInserts" },
            "--compress" => new[] { "Compression" },
            "--create" => new[] { "Create" },
            "--data-only" => new[] { "DataOnly" },
            "--dbname" => new[] { "Database" },
            "--disable-dollar-quoting" => new[] { "DisableDollarQuoting" },
            "--disable-triggers" => new[] { "DisableTriggers" },
            "--enable-row-security" => new[] { "EnableRowSecurity" },
            "--encoding" => new[] { "Encoding" },
            "--exclude-extension" => new[] { "ExcludedExtensions" },
            "--exclude-schema" => new[] { "ExcludedSchemas" },
            "--exclude-table" => new[] { "ExcludedTables" },
            "--exclude-table-and-children" => new[] { "ExcludedTablesAndChildren" },
            "--exclude-table-data" => new[] { "ExcludedTableData" },
            "--exclude-table-data-and-children" => new[] { "ExcludedTableDataAndChildren" },
            "--extension" => new[] { "Extensions" },
            "--extra-float-digits" => new[] { "ExtraFloatDigits" },
            "--filter" => new[] { "Filters" },
            "--format" => new[] { "Format" },
            "--host" => new[] { "Host" },
            "--if-exists" => new[] { "IfExists" },
            "--include-foreign-data" => new[] { "IncludedForeignData" },
            "--inserts" => new[] { "Inserts" },
            "--jobs" => new[] { "Jobs" },
            "--large-objects" => new[] { "LargeObjects" },
            "--load-via-partition-root" => new[] { "LoadViaPartitionRoot" },
            "--lock-wait-timeout" => new[] { "LockWaitTimeout" },
            "--no-acl" => new[] { "NoPrivileges" },
            "--no-blobs" => new[] { "LargeObjects" },
            "--no-comments" => new[] { "NoComments" },
            "--no-data" => new[] { "NoData" },
            "--no-large-objects" => new[] { "LargeObjects" },
            "--no-owner" => new[] { "NoOwner" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-policies" => new[] { "NoPolicies" },
            "--no-privileges" => new[] { "NoPrivileges" },
            "--no-publications" => new[] { "NoPublications" },
            "--no-reconnect" => new[] { "NoReconnect" },
            "--no-schema" => new[] { "NoSchema" },
            "--no-security-labels" => new[] { "NoSecurityLabels" },
            "--no-statistics" => new[] { "NoStatistics" },
            "--no-subscriptions" => new[] { "NoSubscriptions" },
            "--no-sync" => new[] { "NoSync" },
            "--no-synchronized-snapshots" => new[] { "NoSynchronizedSnapshots" },
            "--no-table-access-method" => new[] { "NoTableAccessMethod" },
            "--no-tablespaces" => new[] { "NoTablespaces" },
            "--no-toast-compression" => new[] { "NoToastCompression" },
            "--no-unlogged-table-data" => new[] { "NoUnloggedTableData" },
            "--oids" => new[] { "IncludeOids" },
            "--on-conflict-do-nothing" => new[] { "OnConflictDoNothing" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--quote-all-identifiers" => new[] { "QuoteAllIdentifiers" },
            "--restrict-key" => new[] { "RestrictKey" },
            "--role" => new[] { "Role" },
            "--rows-per-insert" => new[] { "RowsPerInsert" },
            "--schema" => new[] { "Schemas" },
            "--schema-only" => new[] { "SchemaOnly" },
            "--section" => new[] { "Sections" },
            "--sequence-data" => new[] { "SequenceData" },
            "--serializable-deferrable" => new[] { "SerializableDeferrable" },
            "--snapshot" => new[] { "Snapshot" },
            "--statistics" => new[] { "Statistics" },
            "--statistics-only" => new[] { "StatisticsOnly" },
            "--strict-names" => new[] { "StrictNames" },
            "--superuser" => new[] { "Superuser" },
            "--sync-method" => new[] { "SyncMethod" },
            "--table" => new[] { "Tables" },
            "--table-and-children" => new[] { "TablesAndChildren" },
            "--use-set-session-authorization" => new[] { "UseSetSessionAuthorization" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbosity" },
            "-B" => new[] { "LargeObjects" },
            "-C" => new[] { "Create" },
            "-E" => new[] { "Encoding" },
            "-F" => new[] { "Format" },
            "-N" => new[] { "ExcludedSchemas" },
            "-O" => new[] { "NoOwner" },
            "-R" => new[] { "NoReconnect" },
            "-S" => new[] { "Superuser" },
            "-T" => new[] { "ExcludedTables" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-Z" => new[] { "Compression" },
            "-a" => new[] { "DataOnly" },
            "-b" => new[] { "LargeObjects" },
            "-c" => new[] { "Clean" },
            "-d" => new[] { "Database" },
            "-e" => new[] { "Extensions" },
            "-h" => new[] { "Host" },
            "-j" => new[] { "Jobs" },
            "-n" => new[] { "Schemas" },
            "-o" => new[] { "IncludeOids" },
            "-p" => new[] { "Port" },
            "-s" => new[] { "SchemaOnly" },
            "-t" => new[] { "Tables" },
            "-v" => new[] { "Verbosity" },
            "-w" => new[] { "PasswordPrompt" },
            "-x" => new[] { "NoPrivileges" },
            _ => Array.Empty<string>(),
        };
    }
}
