using PgCliSharp.Internal.PgRestore;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgRestore
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgRestoreOptions options, PgRestoreInput input, PgRestoreOutput output, Version? executableVersion = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, input, output, executableVersion), ValidationProperties, options.RestrictKey is not null && executableVersion is null);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgRestoreOptions options, PgRestoreInput input, PgRestoreOutput output, Version? executableVersion = null)
    {
        options = OptionsSnapshot.Copy(options);
        ConfigurationGuard.NotNull(input, nameof(input));
        ConfigurationGuard.NotNull(output, nameof(output));
        PgRestoreValidator.Validate(options, input, output, Version, OfflineValidation.ExactVersion(ExecutablePath, Version, executableVersion));
        return new PgCommand(ExecutablePath, PgRestoreArgumentBuilder.Build(options, input, output), options.EnvironmentVariables,
            PgRestoreValidator.GetStandardInput(options, input) is not null, output.Kind == PgRestoreOutputKind.StandardOutput, false, options.RestrictKey is not null && executableVersion is null);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--clean" => new[] { "Clean" },
            "--create" => new[] { "Create" },
            "--data-only" => new[] { "ContentMode" },
            "--disable-triggers" => new[] { "DisableTriggers" },
            "--enable-row-security" => new[] { "EnableRowSecurity" },
            "--exclude-schema" => new[] { "ExcludedSchemas" },
            "--exit-on-error" => new[] { "ExitOnError" },
            "--filter" => new[] { "Filters" },
            "--format" => new[] { "ArchiveFormat" },
            "--function" => new[] { "Functions" },
            "--host" => new[] { "Host" },
            "--if-exists" => new[] { "IfExists" },
            "--index" => new[] { "Indexes" },
            "--jobs" => new[] { "Jobs" },
            "--list" => new[] { "Mode" },
            "--no-acl" => new[] { "NoPrivileges" },
            "--no-comments" => new[] { "NoComments" },
            "--no-data" => new[] { "NoData" },
            "--no-data-for-failed-tables" => new[] { "NoDataForFailedTables" },
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
            "--no-table-access-method" => new[] { "NoTableAccessMethod" },
            "--no-tablespaces" => new[] { "NoTablespaces" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--restrict-key" => new[] { "RestrictKey" },
            "--role" => new[] { "Role" },
            "--schema" => new[] { "Schemas" },
            "--schema-only" => new[] { "ContentMode" },
            "--section" => new[] { "Sections" },
            "--single-transaction" => new[] { "TransactionMode" },
            "--statistics" => new[] { "Statistics" },
            "--statistics-only" => new[] { "ContentMode" },
            "--strict-names" => new[] { "StrictNames" },
            "--superuser" => new[] { "Superuser" },
            "--table" => new[] { "Tables" },
            "--transaction-size" => new[] { "TransactionMode" },
            "--trigger" => new[] { "Triggers" },
            "--use-list" => new[] { "UseListFile" },
            "--use-set-session-authorization" => new[] { "UseSetSessionAuthorization" },
            "--username" => new[] { "Username" },
            "--verbose" => new[] { "Verbosity" },
            "-1" => new[] { "TransactionMode" },
            "-C" => new[] { "Create" },
            "-F" => new[] { "ArchiveFormat" },
            "-I" => new[] { "Indexes" },
            "-L" => new[] { "UseListFile" },
            "-N" => new[] { "ExcludedSchemas" },
            "-O" => new[] { "NoOwner" },
            "-P" => new[] { "Functions" },
            "-R" => new[] { "NoReconnect" },
            "-S" => new[] { "Superuser" },
            "-T" => new[] { "Triggers" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-a" => new[] { "ContentMode" },
            "-c" => new[] { "Clean" },
            "-e" => new[] { "ExitOnError" },
            "-h" => new[] { "Host" },
            "-j" => new[] { "Jobs" },
            "-l" => new[] { "Mode" },
            "-n" => new[] { "Schemas" },
            "-p" => new[] { "Port" },
            "-s" => new[] { "ContentMode" },
            "-t" => new[] { "Tables" },
            "-v" => new[] { "Verbosity" },
            "-w" => new[] { "PasswordPrompt" },
            "-x" => new[] { "NoPrivileges" },
            _ => Array.Empty<string>(),
        };
    }
}
