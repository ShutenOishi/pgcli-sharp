using PgCliSharp.Internal.Psql;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class Psql
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PsqlOptions options, PsqlIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PsqlOptions options, PsqlIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        PsqlValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, PsqlArgumentBuilder.Build(options), options.EnvironmentVariables,
            io?.StandardInput is not null, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--command" => new[] { "Actions" },
            "--csv" => new[] { "OutputFormat" },
            "--dbname" => new[] { "Database" },
            "--echo-all" => new[] { "EchoAll" },
            "--echo-errors" => new[] { "EchoErrors" },
            "--echo-hidden" => new[] { "EchoHidden" },
            "--echo-queries" => new[] { "EchoQueries" },
            "--expanded" => new[] { "Expanded" },
            "--field-separator" => new[] { "FieldSeparator" },
            "--field-separator-zero" => new[] { "FieldSeparator" },
            "--file" => new[] { "Actions" },
            "--host" => new[] { "Host" },
            "--html" => new[] { "OutputFormat" },
            "--list" => new[] { "ListDatabases" },
            "--log-file" => new[] { "LogFile" },
            "--no-align" => new[] { "OutputFormat" },
            "--no-password" => new[] { "PasswordPrompt" },
            "--no-psqlrc" => new[] { "NoPsqlRc" },
            "--no-readline" => new[] { "NoReadline" },
            "--output" => new[] { "OutputFile" },
            "--password" => new[] { "PasswordPrompt" },
            "--port" => new[] { "Port" },
            "--pset" => new[] { "PrintSettings" },
            "--quiet" => new[] { "Quiet" },
            "--record-separator" => new[] { "RecordSeparator" },
            "--record-separator-zero" => new[] { "RecordSeparator" },
            "--set" => new[] { "Variables" },
            "--single-line" => new[] { "SingleLine" },
            "--single-step" => new[] { "SingleStep" },
            "--single-transaction" => new[] { "SingleTransaction" },
            "--table-attr" => new[] { "TableAttributes" },
            "--tuples-only" => new[] { "TuplesOnly" },
            "--username" => new[] { "Username" },
            "--variable" => new[] { "Variables" },
            "-0" => new[] { "RecordSeparator" },
            "-1" => new[] { "SingleTransaction" },
            "-A" => new[] { "OutputFormat" },
            "-E" => new[] { "EchoHidden" },
            "-F" => new[] { "FieldSeparator" },
            "-H" => new[] { "OutputFormat" },
            "-L" => new[] { "LogFile" },
            "-P" => new[] { "PrintSettings" },
            "-R" => new[] { "RecordSeparator" },
            "-S" => new[] { "SingleLine" },
            "-T" => new[] { "TableAttributes" },
            "-U" => new[] { "Username" },
            "-W" => new[] { "PasswordPrompt" },
            "-X" => new[] { "NoPsqlRc" },
            "-a" => new[] { "EchoAll" },
            "-b" => new[] { "EchoErrors" },
            "-c" => new[] { "Actions" },
            "-d" => new[] { "Database" },
            "-e" => new[] { "EchoQueries" },
            "-f" => new[] { "Actions" },
            "-h" => new[] { "Host" },
            "-l" => new[] { "ListDatabases" },
            "-n" => new[] { "NoReadline" },
            "-o" => new[] { "OutputFile" },
            "-p" => new[] { "Port" },
            "-q" => new[] { "Quiet" },
            "-s" => new[] { "SingleStep" },
            "-t" => new[] { "TuplesOnly" },
            "-v" => new[] { "Variables" },
            "-w" => new[] { "PasswordPrompt" },
            "-x" => new[] { "Expanded" },
            "-z" => new[] { "FieldSeparator" },
            _ => Array.Empty<string>(),
        };
    }
}
