using PgCliSharp.Internal.Configuration;

namespace PgCliSharp.ServerApplications;

/// <summary><para>EN: Lambda configuration conveniences preserving existing Options instance-call overload resolution.</para><para>JA: 既存の Options 指定インスタンス呼び出しのオーバーロード解決を維持するラムダ設定の便利な API です。</para></summary>
public static class PgServerConfigurationExtensions
{
    /// <summary><para>EN: Configures fresh PgChecksumsOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgChecksumsOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgServerResult> ExecuteAsync(this PgChecksums tool, Action<PgChecksumsOptions> configureOptions, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgChecksums tool, Action<PgChecksumsOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgChecksums tool, Action<PgChecksumsOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgCtlOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgCtlOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgCtlResult> ExecuteAsync(this PgCtl tool, Action<PgCtlOptions> configureOptions, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgCtl tool, Action<PgCtlOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgCtl tool, Action<PgCtlOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgRewindOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgRewindOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgServerResult> ExecuteAsync(this PgRewind tool, Action<PgRewindOptions> configureOptions, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgRewind tool, Action<PgRewindOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgRewind tool, Action<PgRewindOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgResetWalOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgResetWalOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgServerResult> ExecuteAsync(this PgResetWal tool, Action<PgResetWalOptions> configureOptions, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgResetWal tool, Action<PgResetWalOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgResetWal tool, Action<PgResetWalOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh PgUpgradeOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: PgUpgradeOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgServerResult> ExecuteAsync(this PgUpgrade tool, Action<PgUpgradeOptions> configureOptions, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this PgUpgrade tool, Action<PgUpgradeOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this PgUpgrade tool, Action<PgUpgradeOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh InitDbOptions synchronously once, then invokes ExecuteAsync. Existing Options overloads remain available.</para><para>JA: InitDbOptions を新規作成し同期的に一度設定して ExecuteAsync を呼び出します。既存の Options 指定も利用できます。</para></summary>
    public static Task<PgServerResult> ExecuteAsync(this InitDb tool, Action<InitDbOptions> configureOptions, PgServerIo? io = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.ExecuteAsync(OfflineValidation.Configure(configureOptions), io, timeout, cancellationToken);
    }

    /// <summary><para>EN: Configures fresh options once for offline Validate; callback exceptions propagate.</para><para>JA: オフラインの Validate 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgValidationResult Validate(this InitDb tool, Action<InitDbOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.Validate(OfflineValidation.Configure(configureOptions), io);
    }

    /// <summary><para>EN: Configures fresh options once for offline CreateCommand; callback exceptions propagate.</para><para>JA: オフラインの CreateCommand 用に新規オプションを一度設定します。コールバック内の例外はそのまま伝播します。</para></summary>
    public static PgCommand CreateCommand(this InitDb tool, Action<InitDbOptions> configureOptions, PgServerIo? io = null)
    {
        ConfigurationGuard.NotNull(tool, nameof(tool));
        return tool.CreateCommand(OfflineValidation.Configure(configureOptions), io);
    }
}
