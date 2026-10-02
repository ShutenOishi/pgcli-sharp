using PgCliSharp.Internal.PgDumpAll;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgDumpAll
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgDumpAllOptions options, PgDumpAllOutput output, Version? executableVersion = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, output, executableVersion), ValidationProperties, options.RestrictKey is not null && executableVersion is null);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgDumpAllOptions options, PgDumpAllOutput output, Version? executableVersion = null)
    {
        options = OptionsSnapshot.Copy(options);
        ConfigurationGuard.NotNull(output, nameof(output));
        PgDumpAllValidator.Validate(options, output, Version, OfflineValidation.ExactVersion(ExecutablePath, Version, executableVersion));
        return new PgCommand(ExecutablePath, PgDumpAllArgumentBuilder.Build(options, output), options.EnvironmentVariables,
            PgDumpAllValidator.GetStandardInput(options) is not null, output.Kind == PgDumpAllOutputKind.StandardOutput, false, options.RestrictKey is not null && executableVersion is null);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--attribute-inserts" => new[] { "ColumnInserts" },
            "--binary-upgrade" => new[] { "BinaryUpgrade" },
            "--clean" => new[] { "Clean" },
            "--column-inserts" => new[] { "ColumnInserts" },
            "--data-only" => new[] { "ContentMode" },
            "--database" => new[] { "InitialDatabase" },
            "--dbname" => new[] { "ConnectionString" },
            "--disable-dollar-quoting" => new[] { "DisableDollarQuoting" },
            "--disable-triggers" => new[] { "DisableTriggers" },
            "--encoding" => new[] { "Encoding" },
            "--exclude-database" => new[] { "ExcludedDatabases" },
            "--extra-float-digits" => new[] { "ExtraFloatDigits" },
            "--filter" => new[] { "Filters" },
            "--globals-only" => new[] { "Scope" },
            "--host" => new[] { "Host" },
            "--if-exists" => new[] { "IfExists" },
            "--inserts" => new[] { "Inserts" },
            "--load-via-partition-root" => new[] { "LoadViaPartitionRoot" },
            "--lock-wait-timeout" => new[] { "LockWaitTimeout" },
            "--no-acl" => new[] { "NoPrivileges" },
            "--no-comments" => new[] { "NoComments" },
            "--no-data" => new[] { "NoData" },
            "--no-owner" => new[] { "NoOwner" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-policies" => new[] { "NoPolicies" },
            "--no-privileges" => new[] { "NoPrivileges" },
            "--no-publications" => new[] { "NoPublications" },
            "--no-role-passwords" => new[] { "NoRolePasswords" },
            "--no-schema" => new[] { "NoSchema" },
            "--no-security-labels" => new[] { "NoSecurityLabels" },
            "--no-statistics" => new[] { "NoStatistics" },
            "--no-subscriptions" => new[] { "NoSubscriptions" },
            "--no-sync" => new[] { "NoSync" },
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
            "--roles-only" => new[] { "Scope" },
            "--rows-per-insert" => new[] { "RowsPerInsert" },
            "--schema-only" => new[] { "ContentMode" },
            "--sequence-data" => new[] { "SequenceData" },
            "--statistics" => new[] { "Statistics" },
            "--statistics-only" => new[] { "ContentMode" },
            "--superuser" => new[] { "Superuser" },
            "--tablespaces-only" => new[] { "Scope" },
            "--use-set-session-authorization" => new[] { "UseSetSessionAuthorization" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbosity" },
            "-E" => new[] { "Encoding" },
            "-O" => new[] { "NoOwner" },
            "-S" => new[] { "Superuser" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-a" => new[] { "ContentMode" },
            "-c" => new[] { "Clean" },
            "-d" => new[] { "ConnectionString" },
            "-g" => new[] { "Scope" },
            "-h" => new[] { "Host" },
            "-l" => new[] { "InitialDatabase" },
            "-o" => new[] { "IncludeOids" },
            "-p" => new[] { "Port" },
            "-r" => new[] { "Scope" },
            "-s" => new[] { "ContentMode" },
            "-t" => new[] { "Scope" },
            "-v" => new[] { "Verbosity" },
            "-w" => new[] { "PasswordPrompt" },
            "-x" => new[] { "NoPrivileges" },
            _ => Array.Empty<string>(),
        };
    }
}
