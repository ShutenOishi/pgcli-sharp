using PgCliSharp.Internal.Configuration;

namespace PgCliSharp;

/// <summary><para>EN: Lambda configuration conveniences preserving existing Options instance-call overload resolution.</para><para>JA: 既存の Options 指定インスタンス呼び出しのオーバーロード解決を維持するラムダ設定の便利な API です。</para></summary>
public static class PgCliConfigurationExtensions
{
    /// <summary><para>EN: Configures fresh PgDumpAllOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgDumpAllOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgDumpAllResult> ExecuteAsync(this PgDumpAll tool, Action<PgDumpAllOptions> configureOptions, PgDumpAllOutput output, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), output, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgDumpAll tool, Action<PgDumpAllOptions> configureOptions, PgDumpAllOutput output, Version? executableVersion = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), output, executableVersion);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgDumpAll tool, Action<PgDumpAllOptions> configureOptions, PgDumpAllOutput output, Version? executableVersion = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), output, executableVersion);
    }

    /// <summary><para>EN: Configures fresh PgReceiveWalOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgReceiveWalOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgReceiveWalResult> ExecuteAsync(this PgReceiveWal tool, Action<PgReceiveWalOptions> configureOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgReceiveWal tool, Action<PgReceiveWalOptions> configureOptions)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions));
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgReceiveWal tool, Action<PgReceiveWalOptions> configureOptions)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions));
    }

    /// <summary><para>EN: Configures fresh PgVerifyBackupOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgVerifyBackupOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgVerifyBackupResult> ExecuteAsync(this PgVerifyBackup tool, Action<PgVerifyBackupOptions> configureOptions, PgVerifyBackupInput input, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), input, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgVerifyBackup tool, Action<PgVerifyBackupOptions> configureOptions, PgVerifyBackupInput input)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), input);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgVerifyBackup tool, Action<PgVerifyBackupOptions> configureOptions, PgVerifyBackupInput input)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), input);
    }

    /// <summary><para>EN: Configures fresh PgRestoreOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgRestoreOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgRestoreResult> ExecuteAsync(this PgRestore tool, Action<PgRestoreOptions> configureOptions, PgRestoreInput input, PgRestoreOutput output, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), input, output, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgRestore tool, Action<PgRestoreOptions> configureOptions, PgRestoreInput input, PgRestoreOutput output, Version? executableVersion = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), input, output, executableVersion);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgRestore tool, Action<PgRestoreOptions> configureOptions, PgRestoreInput input, PgRestoreOutput output, Version? executableVersion = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), input, output, executableVersion);
    }

    /// <summary><para>EN: Configures fresh CreateDbOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: CreateDbOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this CreateDb tool, Action<CreateDbOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this CreateDb tool, Action<CreateDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this CreateDb tool, Action<CreateDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgAmcheckOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgAmcheckOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this PgAmcheck tool, Action<PgAmcheckOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgAmcheck tool, Action<PgAmcheckOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgAmcheck tool, Action<PgAmcheckOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgBaseBackupOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgBaseBackupOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgBaseBackupResult> ExecuteAsync(this PgBaseBackup tool, Action<PgBaseBackupOptions> configureOptions, PgBaseBackupDestination destination, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), destination, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgBaseBackup tool, Action<PgBaseBackupOptions> configureOptions, PgBaseBackupDestination destination)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), destination);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgBaseBackup tool, Action<PgBaseBackupOptions> configureOptions, PgBaseBackupDestination destination)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), destination);
    }

    /// <summary><para>EN: Configures fresh PgDumpOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgDumpOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgDumpResult> ExecuteAsync(this PgDump tool, Action<PgDumpOptions> configureOptions, PgDumpOutput output, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), output, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgDump tool, Action<PgDumpOptions> configureOptions, PgDumpOutput output, Version? executableVersion = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), output, executableVersion);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgDump tool, Action<PgDumpOptions> configureOptions, PgDumpOutput output, Version? executableVersion = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), output, executableVersion);
    }

    /// <summary><para>EN: Configures fresh ClusterDbOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: ClusterDbOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this ClusterDb tool, Action<ClusterDbOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this ClusterDb tool, Action<ClusterDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this ClusterDb tool, Action<ClusterDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh DropDbOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: DropDbOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this DropDb tool, Action<DropDbOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this DropDb tool, Action<DropDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this DropDb tool, Action<DropDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgRecvLogicalOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgRecvLogicalOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgRecvLogicalResult> ExecuteAsync(this PgRecvLogical tool, Action<PgRecvLogicalOptions> configureOptions, PgRecvLogicalOutput? output = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), output, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgRecvLogical tool, Action<PgRecvLogicalOptions> configureOptions, PgRecvLogicalOutput? output = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), output);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgRecvLogical tool, Action<PgRecvLogicalOptions> configureOptions, PgRecvLogicalOutput? output = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), output);
    }

    /// <summary><para>EN: Configures fresh PsqlOptions synchronously once, then invokes StartSessionAsync. Existing Options overloads remain available.</para><para>JA: PsqlOptions を新規作成し同期的に一度設定して StartSessionAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PsqlSession> StartSessionAsync(this Psql tool, Action<PsqlOptions> configureOptions, PsqlSessionIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.StartSessionAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline ValidateForSession; callback exceptions propagate.</para><para>JA: オフラインの ValidateForSession 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult ValidateForSession(this Psql tool, Action<PsqlOptions> configureOptions, PsqlSessionIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ValidateForSession(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateSessionCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateSessionCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateSessionCommand(this Psql tool, Action<PsqlOptions> configureOptions, PsqlSessionIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateSessionCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PsqlOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PsqlOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PsqlResult> ExecuteAsync(this Psql tool, Action<PsqlOptions> configureOptions, PsqlIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this Psql tool, Action<PsqlOptions> configureOptions, PsqlIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this Psql tool, Action<PsqlOptions> configureOptions, PsqlIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgBenchOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgBenchOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgBenchResult> ExecuteAsync(this PgBench tool, Action<PgBenchOptions> configureOptions, PgBenchIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgBench tool, Action<PgBenchOptions> configureOptions, PgBenchIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgBench tool, Action<PgBenchOptions> configureOptions, PgBenchIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgCombineBackupOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgCombineBackupOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgCombineBackupResult> ExecuteAsync(this PgCombineBackup tool, Action<PgCombineBackupOptions> configureOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgCombineBackup tool, Action<PgCombineBackupOptions> configureOptions)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions));
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgCombineBackup tool, Action<PgCombineBackupOptions> configureOptions)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions));
    }

    /// <summary><para>EN: Configures fresh VacuumDbOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: VacuumDbOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this VacuumDb tool, Action<VacuumDbOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this VacuumDb tool, Action<VacuumDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this VacuumDb tool, Action<VacuumDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgIsReadyOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgIsReadyOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgIsReadyResult> ExecuteAsync(this PgIsReady tool, Action<PgIsReadyOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgIsReady tool, Action<PgIsReadyOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgIsReady tool, Action<PgIsReadyOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh CreateUserOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: CreateUserOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this CreateUser tool, Action<CreateUserOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this CreateUser tool, Action<CreateUserOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this CreateUser tool, Action<CreateUserOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh DropUserOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: DropUserOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this DropUser tool, Action<DropUserOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this DropUser tool, Action<DropUserOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this DropUser tool, Action<DropUserOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh ReindexDbOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: ReindexDbOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgMaintenanceResult> ExecuteAsync(this ReindexDb tool, Action<ReindexDbOptions> configureOptions, PgMaintenanceIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this ReindexDb tool, Action<ReindexDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this ReindexDb tool, Action<ReindexDbOptions> configureOptions, PgMaintenanceIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }
}
