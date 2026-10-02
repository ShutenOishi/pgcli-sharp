using PgCliSharp.Internal.PgBench;
using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

public sealed partial class PgBench
{
    /// <summary><para>EN: Validates settings offline without probing the executable, starting a process, opening files or consuming streams. Returns the first option failure; actual executable/server state is not verified.</para><para>JA: 実行ファイル確認・プロセス起動・ファイルオープン・ストリーム読み取りを行わず設定を検証します。最初のオプションエラーを返し、実際の実行ファイルやサーバー状態は確認しません。</para></summary>
    public PgValidationResult Validate(PgBenchOptions options, PgBenchIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        return OfflineValidation.Check(() => CreateCommand(options, io), ValidationProperties, false);
    }

    /// <summary><para>EN: Validates and captures exact command tokens/environment without execution or side effects. Optional exact versions are caller assertions, never probes. Stream routing remains external; raw data can contain secrets.</para><para>JA: 実行や副作用を伴わず、検証済みの正確なコマンド引数・環境変数を取得します。任意の正確なバージョンは呼び出し側の申告で、実行ファイル確認は行いません。ストリームは別途接続し、生データには秘密情報が含まれ得ます。</para></summary>
    public PgCommand CreateCommand(PgBenchOptions options, PgBenchIo? io = null)
    {
        options = OptionsSnapshot.Copy(options);
        PgBenchValidator.Validate(options, Version);
        return new PgCommand(ExecutablePath, PgBenchArgumentBuilder.Build(options, Version), options.EnvironmentVariables,
            false, io?.StandardOutput is not null, io?.StandardError is not null, false);
    }

    private static IEnumerable<string> ValidationProperties(string optionName)
    {
        return optionName switch
        {
            "--aggregate-interval" => new[] { "AggregateIntervalSeconds" },
            "--builtin" => new[] { "Scripts" },
            "--client" => new[] { "Clients" },
            "--connect" => new[] { "ConnectPerTransaction" },
            "--dbname" => new[] { "Database" },
            "--debug" => new[] { "Debug" },
            "--define" => new[] { "Variables" },
            "--exit-on-abort" => new[] { "ExitOnAbort" },
            "--failures-detailed" => new[] { "FailuresDetailed" },
            "--file" => new[] { "Scripts" },
            "--fillfactor" => new[] { "FillFactor" },
            "--foreign-keys" => new[] { "ForeignKeys" },
            "--host" => new[] { "Host" },
            "--index-tablespace" => new[] { "IndexTablespace" },
            "--init-steps" => new[] { "InitializationSteps" },
            "--initialize" => new[] { "Initialize" },
            "--jobs" => new[] { "Jobs" },
            "--latency-limit" => new[] { "LatencyLimitMilliseconds" },
            "--log" => new[] { "LogTransactions" },
            "--log-prefix" => new[] { "LogPrefix" },
            "--max-tries" => new[] { "MaxTries" },
            "--no-vacuum" => new[] { "NoVacuum" },
            "--partition-method" => new[] { "PartitionMethod" },
            "--partitions" => new[] { "Partitions" },
            "--port" => new[] { "Port" },
            "--progress" => new[] { "ProgressSeconds" },
            "--progress-timestamp" => new[] { "ProgressTimestamp" },
            "--protocol" => new[] { "Protocol" },
            "--quiet" => new[] { "Quiet" },
            "--random-seed" => new[] { "RandomSeed" },
            "--rate" => new[] { "Rate" },
            "--report-latencies" => new[] { "ReportPerCommand" },
            "--report-per-command" => new[] { "ReportPerCommand" },
            "--sampling-rate" => new[] { "SamplingRate" },
            "--scale" => new[] { "Scale" },
            "--select-only" => new[] { "SelectOnly" },
            "--show-script" => new[] { "ShowScript" },
            "--skip-some-updates" => new[] { "SkipSomeUpdates" },
            "--tablespace" => new[] { "Tablespace" },
            "--time" => new[] { "DurationSeconds" },
            "--transactions" => new[] { "TransactionsPerClient" },
            "--unlogged-tables" => new[] { "UnloggedTables" },
            "--username" => new[] { "Username" },
            "--vacuum-all" => new[] { "VacuumAll" },
            "--verbose-errors" => new[] { "VerboseErrors" },
            "-C" => new[] { "ConnectPerTransaction" },
            "-D" => new[] { "Variables" },
            "-F" => new[] { "FillFactor" },
            "-I" => new[] { "InitializationSteps" },
            "-L" => new[] { "LatencyLimitMilliseconds" },
            "-M" => new[] { "Protocol" },
            "-N" => new[] { "SkipSomeUpdates" },
            "-P" => new[] { "ProgressSeconds" },
            "-R" => new[] { "Rate" },
            "-S" => new[] { "SelectOnly" },
            "-T" => new[] { "DurationSeconds" },
            "-U" => new[] { "Username" },
            "-b" => new[] { "Scripts" },
            "-c" => new[] { "Clients" },
            "-d" => new[] { "Database", "Debug" },
            "-f" => new[] { "Scripts" },
            "-h" => new[] { "Host" },
            "-i" => new[] { "Initialize" },
            "-j" => new[] { "Jobs" },
            "-l" => new[] { "LogTransactions" },
            "-n" => new[] { "NoVacuum" },
            "-p" => new[] { "Port" },
            "-q" => new[] { "Quiet" },
            "-r" => new[] { "ReportPerCommand" },
            "-s" => new[] { "Scale" },
            "-t" => new[] { "TransactionsPerClient" },
            "-v" => new[] { "VacuumAll" },
            "<dbname>" => new[] { "Database" },
            _ => Array.Empty<string>(),
        };
    }
}
