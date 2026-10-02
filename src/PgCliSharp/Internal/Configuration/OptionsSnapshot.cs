using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.Configuration;

internal static class OptionsSnapshot
{
    internal static ClusterDbOptions Copy(ClusterDbOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new ClusterDbOptions
        {
            DatabaseName = options.DatabaseName,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Echo = options.Echo,
            Quiet = options.Quiet,
            AllDatabases = options.AllDatabases,
            Verbose = options.Verbose,
            MaintenanceDatabase = options.MaintenanceDatabase,
        };
        foreach (var item in options.Tables) copy.Tables.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static CreateDbOptions Copy(CreateDbOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new CreateDbOptions
        {
            DatabaseName = options.DatabaseName,
            Description = options.Description,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Echo = options.Echo,
            Owner = options.Owner,
            Tablespace = options.Tablespace,
            Template = options.Template,
            Encoding = options.Encoding,
            Strategy = options.Strategy,
            LcCollate = options.LcCollate,
            LcCtype = options.LcCtype,
            Locale = options.Locale,
            MaintenanceDatabase = options.MaintenanceDatabase,
            LocaleProvider = options.LocaleProvider,
            BuiltinLocale = options.BuiltinLocale,
            IcuLocale = options.IcuLocale,
            IcuRules = options.IcuRules,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static CreateUserOptions Copy(CreateUserOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new CreateUserOptions
        {
            RoleName = options.RoleName,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Echo = options.Echo,
            CanCreateDatabase = options.CanCreateDatabase,
            IsSuperuser = options.IsSuperuser,
            CanCreateRole = options.CanCreateRole,
            Inherit = options.Inherit,
            Login = options.Login,
            Replication = options.Replication,
            BypassRls = options.BypassRls,
            Interactive = options.Interactive,
            ConnectionLimit = options.ConnectionLimit,
            PromptForRolePassword = options.PromptForRolePassword,
            Encrypted = options.Encrypted,
            ValidUntil = options.ValidUntil,
        };
        foreach (var item in options.MemberOfRoles) copy.MemberOfRoles.Add(item);
        foreach (var item in options.AdminOfRoles) copy.AdminOfRoles.Add(item);
        foreach (var item in options.Members) copy.Members.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static DropDbOptions Copy(DropDbOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new DropDbOptions
        {
            DatabaseName = options.DatabaseName,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Echo = options.Echo,
            Interactive = options.Interactive,
            IfExists = options.IfExists,
            MaintenanceDatabase = options.MaintenanceDatabase,
            Force = options.Force,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static DropUserOptions Copy(DropUserOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new DropUserOptions
        {
            RoleName = options.RoleName,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Echo = options.Echo,
            Interactive = options.Interactive,
            IfExists = options.IfExists,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static InitDbOptions Copy(InitDbOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new InitDbOptions
        {
            DataDirectory = options.DataDirectory,
            Encoding = options.Encoding,
            Locale = options.Locale,
            LcCollate = options.LcCollate,
            LcCtype = options.LcCtype,
            LcMessages = options.LcMessages,
            LcMonetary = options.LcMonetary,
            LcNumeric = options.LcNumeric,
            LcTime = options.LcTime,
            NoLocale = options.NoLocale,
            TextSearchConfiguration = options.TextSearchConfiguration,
            Authentication = options.Authentication,
            LocalAuthentication = options.LocalAuthentication,
            HostAuthentication = options.HostAuthentication,
            PasswordPrompt = options.PasswordPrompt,
            PasswordFile = options.PasswordFile,
            Username = options.Username,
            Debug = options.Debug,
            ShowSettings = options.ShowSettings,
            NoClean = options.NoClean,
            NoSync = options.NoSync,
            NoInstructions = options.NoInstructions,
            SyncOnly = options.SyncOnly,
            WalDirectory = options.WalDirectory,
            WalSegmentSizeMegabytes = options.WalSegmentSizeMegabytes,
            DataChecksums = options.DataChecksums,
            AllowGroupAccess = options.AllowGroupAccess,
            DiscardCaches = options.DiscardCaches,
            LocaleProvider = options.LocaleProvider,
            IcuLocale = options.IcuLocale,
            IcuRules = options.IcuRules,
            BuiltinLocale = options.BuiltinLocale,
            SyncMethod = options.SyncMethod,
            NoSyncDataFiles = options.NoSyncDataFiles,
            InputDirectory = options.InputDirectory,
        };
        foreach (var item in options.Settings) copy.Settings.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgAmcheckOptions Copy(PgAmcheckOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgAmcheckOptions
        {
            DatabaseName = options.DatabaseName,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            MaintenanceDatabase = options.MaintenanceDatabase,
            AllDatabases = options.AllDatabases,
            Echo = options.Echo,
            Jobs = options.Jobs,
            Progress = options.Progress,
            Verbose = options.Verbose,
            NoDependentIndexes = options.NoDependentIndexes,
            NoDependentToast = options.NoDependentToast,
            ExcludeToastPointers = options.ExcludeToastPointers,
            OnErrorStop = options.OnErrorStop,
            Skip = options.Skip,
            StartBlock = options.StartBlock,
            EndBlock = options.EndBlock,
            RootDescend = options.RootDescend,
            StrictNames = options.StrictNames,
            HeapAllIndexed = options.HeapAllIndexed,
            ParentCheck = options.ParentCheck,
            InstallMissing = options.InstallMissing,
            CheckUnique = options.CheckUnique,
        };
        foreach (var item in options.DatabasePatterns) copy.DatabasePatterns.Add(item);
        foreach (var item in options.ExcludedDatabasePatterns) copy.ExcludedDatabasePatterns.Add(item);
        foreach (var item in options.IndexPatterns) copy.IndexPatterns.Add(item);
        foreach (var item in options.ExcludedIndexPatterns) copy.ExcludedIndexPatterns.Add(item);
        foreach (var item in options.RelationPatterns) copy.RelationPatterns.Add(item);
        foreach (var item in options.ExcludedRelationPatterns) copy.ExcludedRelationPatterns.Add(item);
        foreach (var item in options.SchemaPatterns) copy.SchemaPatterns.Add(item);
        foreach (var item in options.ExcludedSchemaPatterns) copy.ExcludedSchemaPatterns.Add(item);
        foreach (var item in options.TablePatterns) copy.TablePatterns.Add(item);
        foreach (var item in options.ExcludedTablePatterns) copy.ExcludedTablePatterns.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgBaseBackupOptions Copy(PgBaseBackupOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgBaseBackupOptions
        {
            Format = options.Format,
            Checkpoint = options.Checkpoint,
            CreateSlot = options.CreateSlot,
            MaxRate = options.MaxRate,
            WriteRecoveryConf = options.WriteRecoveryConf,
            Slot = options.Slot,
            WalMethod = options.WalMethod,
            Compression = options.Compression,
            Label = options.Label,
            NoClean = options.NoClean,
            NoSync = options.NoSync,
            ConnectionString = options.ConnectionString,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            StatusInterval = options.StatusInterval,
            Verbose = options.Verbose,
            Progress = options.Progress,
            WalDirectory = options.WalDirectory,
            NoSlot = options.NoSlot,
            NoVerifyChecksums = options.NoVerifyChecksums,
            NoEstimateSize = options.NoEstimateSize,
            NoManifest = options.NoManifest,
            ManifestForceEncode = options.ManifestForceEncode,
            ManifestChecksums = options.ManifestChecksums,
            IncrementalManifest = options.IncrementalManifest,
            SyncMethod = options.SyncMethod,
        };
        foreach (var item in options.TablespaceMappings) copy.TablespaceMappings.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgBenchOptions Copy(PgBenchOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgBenchOptions
        {
            Database = options.Database,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            Initialize = options.Initialize,
            Clients = options.Clients,
            ConnectPerTransaction = options.ConnectPerTransaction,
            Debug = options.Debug,
            FillFactor = options.FillFactor,
            Jobs = options.Jobs,
            LogTransactions = options.LogTransactions,
            LatencyLimitMilliseconds = options.LatencyLimitMilliseconds,
            NoVacuum = options.NoVacuum,
            ProgressSeconds = options.ProgressSeconds,
            Protocol = options.Protocol,
            Quiet = options.Quiet,
            ReportPerCommand = options.ReportPerCommand,
            Rate = options.Rate,
            Scale = options.Scale,
            SelectOnly = options.SelectOnly,
            SkipSomeUpdates = options.SkipSomeUpdates,
            DurationSeconds = options.DurationSeconds,
            TransactionsPerClient = options.TransactionsPerClient,
            VacuumAll = options.VacuumAll,
            UnloggedTables = options.UnloggedTables,
            Tablespace = options.Tablespace,
            IndexTablespace = options.IndexTablespace,
            SamplingRate = options.SamplingRate,
            AggregateIntervalSeconds = options.AggregateIntervalSeconds,
            ProgressTimestamp = options.ProgressTimestamp,
            LogPrefix = options.LogPrefix,
            ForeignKeys = options.ForeignKeys,
            RandomSeed = options.RandomSeed,
            ShowScript = options.ShowScript,
            Partitions = options.Partitions,
            PartitionMethod = options.PartitionMethod,
            FailuresDetailed = options.FailuresDetailed,
            MaxTries = options.MaxTries,
            VerboseErrors = options.VerboseErrors,
            ExitOnAbort = options.ExitOnAbort,
        };
        foreach (var item in options.Scripts) copy.Scripts.Add(item);
        foreach (var item in options.Variables) copy.Variables.Add(item);
        foreach (var item in options.InitializationSteps) copy.InitializationSteps.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgChecksumsOptions Copy(PgChecksumsOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgChecksumsOptions
        {
            DataDirectory = options.DataDirectory,
            Mode = options.Mode,
            FileNode = options.FileNode,
            NoSync = options.NoSync,
            Progress = options.Progress,
            Verbose = options.Verbose,
            SyncMethod = options.SyncMethod,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgCombineBackupOptions Copy(PgCombineBackupOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgCombineBackupOptions
        {
            OutputDirectory = options.OutputDirectory,
            Debug = options.Debug,
            DryRun = options.DryRun,
            NoSync = options.NoSync,
            CopyMethod = options.CopyMethod,
            ManifestChecksums = options.ManifestChecksums,
            NoManifest = options.NoManifest,
            SyncMethod = options.SyncMethod,
        };
        foreach (var item in options.InputDirectories) copy.InputDirectories.Add(item);
        foreach (var item in options.TablespaceMappings) copy.TablespaceMappings.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgCtlOptions Copy(PgCtlOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgCtlOptions
        {
            DataDirectory = options.DataDirectory,
            Command = options.Command,
            LogFile = options.LogFile,
            ShutdownMode = options.ShutdownMode,
            Silent = options.Silent,
            WaitTimeoutSeconds = options.WaitTimeoutSeconds,
            CoreFiles = options.CoreFiles,
            Wait = options.Wait,
            ServerExecutablePath = options.ServerExecutablePath,
            Signal = options.Signal,
            ProcessId = options.ProcessId,
            ServiceName = options.ServiceName,
            ServicePassword = options.ServicePassword,
            ServiceUsername = options.ServiceUsername,
            ServiceStart = options.ServiceStart,
            EventSource = options.EventSource,
        };
        foreach (var item in options.ForwardedOptions) copy.ForwardedOptions.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgDumpAllOptions Copy(PgDumpAllOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgDumpAllOptions
        {
            Scope = options.Scope,
            ContentMode = options.ContentMode,
            ConnectionString = options.ConnectionString,
            InitialDatabase = options.InitialDatabase,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Role = options.Role,
            Clean = options.Clean,
            Encoding = options.Encoding,
            IncludeOids = options.IncludeOids,
            NoOwner = options.NoOwner,
            Superuser = options.Superuser,
            Verbosity = options.Verbosity,
            NoPrivileges = options.NoPrivileges,
            BinaryUpgrade = options.BinaryUpgrade,
            ColumnInserts = options.ColumnInserts,
            DisableDollarQuoting = options.DisableDollarQuoting,
            DisableTriggers = options.DisableTriggers,
            IfExists = options.IfExists,
            Inserts = options.Inserts,
            LockWaitTimeout = options.LockWaitTimeout,
            NoTablespaces = options.NoTablespaces,
            QuoteAllIdentifiers = options.QuoteAllIdentifiers,
            UseSetSessionAuthorization = options.UseSetSessionAuthorization,
            NoPublications = options.NoPublications,
            NoRolePasswords = options.NoRolePasswords,
            NoSecurityLabels = options.NoSecurityLabels,
            NoSubscriptions = options.NoSubscriptions,
            NoSync = options.NoSync,
            NoUnloggedTableData = options.NoUnloggedTableData,
            LoadViaPartitionRoot = options.LoadViaPartitionRoot,
            NoComments = options.NoComments,
            ExtraFloatDigits = options.ExtraFloatDigits,
            OnConflictDoNothing = options.OnConflictDoNothing,
            RowsPerInsert = options.RowsPerInsert,
            RestrictKey = options.RestrictKey,
            NoToastCompression = options.NoToastCompression,
            NoTableAccessMethod = options.NoTableAccessMethod,
            NoData = options.NoData,
            NoPolicies = options.NoPolicies,
            NoSchema = options.NoSchema,
            NoStatistics = options.NoStatistics,
            Statistics = options.Statistics,
            SequenceData = options.SequenceData,
        };
        foreach (var item in options.ExcludedDatabases) copy.ExcludedDatabases.Add(item);
        foreach (var item in options.Filters) copy.Filters.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgDumpOptions Copy(PgDumpOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgDumpOptions
        {
            Database = options.Database,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Role = options.Role,
            DataOnly = options.DataOnly,
            LargeObjects = options.LargeObjects,
            Clean = options.Clean,
            Create = options.Create,
            Encoding = options.Encoding,
            Format = options.Format,
            Jobs = options.Jobs,
            IncludeOids = options.IncludeOids,
            NoOwner = options.NoOwner,
            NoReconnect = options.NoReconnect,
            SchemaOnly = options.SchemaOnly,
            Superuser = options.Superuser,
            Verbosity = options.Verbosity,
            NoPrivileges = options.NoPrivileges,
            Compression = options.Compression,
            BinaryUpgrade = options.BinaryUpgrade,
            ColumnInserts = options.ColumnInserts,
            DisableDollarQuoting = options.DisableDollarQuoting,
            DisableTriggers = options.DisableTriggers,
            EnableRowSecurity = options.EnableRowSecurity,
            IfExists = options.IfExists,
            Inserts = options.Inserts,
            LockWaitTimeout = options.LockWaitTimeout,
            NoPublications = options.NoPublications,
            NoSecurityLabels = options.NoSecurityLabels,
            NoSubscriptions = options.NoSubscriptions,
            NoSync = options.NoSync,
            NoSynchronizedSnapshots = options.NoSynchronizedSnapshots,
            NoTablespaces = options.NoTablespaces,
            NoUnloggedTableData = options.NoUnloggedTableData,
            QuoteAllIdentifiers = options.QuoteAllIdentifiers,
            SerializableDeferrable = options.SerializableDeferrable,
            Snapshot = options.Snapshot,
            StrictNames = options.StrictNames,
            UseSetSessionAuthorization = options.UseSetSessionAuthorization,
            LoadViaPartitionRoot = options.LoadViaPartitionRoot,
            NoComments = options.NoComments,
            ExtraFloatDigits = options.ExtraFloatDigits,
            OnConflictDoNothing = options.OnConflictDoNothing,
            RowsPerInsert = options.RowsPerInsert,
            RestrictKey = options.RestrictKey,
            NoToastCompression = options.NoToastCompression,
            NoTableAccessMethod = options.NoTableAccessMethod,
            SyncMethod = options.SyncMethod,
            NoData = options.NoData,
            NoPolicies = options.NoPolicies,
            NoSchema = options.NoSchema,
            NoStatistics = options.NoStatistics,
            SequenceData = options.SequenceData,
            Statistics = options.Statistics,
            StatisticsOnly = options.StatisticsOnly,
        };
        foreach (var item in options.Schemas) copy.Schemas.Add(item);
        foreach (var item in options.ExcludedSchemas) copy.ExcludedSchemas.Add(item);
        foreach (var item in options.Tables) copy.Tables.Add(item);
        foreach (var item in options.ExcludedTables) copy.ExcludedTables.Add(item);
        foreach (var item in options.ExcludedTableData) copy.ExcludedTableData.Add(item);
        foreach (var item in options.Sections) copy.Sections.Add(item);
        foreach (var item in options.IncludedForeignData) copy.IncludedForeignData.Add(item);
        foreach (var item in options.Extensions) copy.Extensions.Add(item);
        foreach (var item in options.TablesAndChildren) copy.TablesAndChildren.Add(item);
        foreach (var item in options.ExcludedTablesAndChildren) copy.ExcludedTablesAndChildren.Add(item);
        foreach (var item in options.ExcludedTableDataAndChildren) copy.ExcludedTableDataAndChildren.Add(item);
        foreach (var item in options.ExcludedExtensions) copy.ExcludedExtensions.Add(item);
        foreach (var item in options.Filters) copy.Filters.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgIsReadyOptions Copy(PgIsReadyOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgIsReadyOptions
        {
            Database = options.Database,
            Host = options.Host,
            Port = options.Port,
            Quiet = options.Quiet,
            ConnectTimeoutSeconds = options.ConnectTimeoutSeconds,
            Username = options.Username,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgReceiveWalOptions Copy(PgReceiveWalOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgReceiveWalOptions
        {
            Directory = options.Directory,
            ConnectionString = options.ConnectionString,
            EndPosition = options.EndPosition,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            NoLoop = options.NoLoop,
            PasswordPrompt = options.PasswordPrompt,
            StatusInterval = options.StatusInterval,
            Slot = options.Slot,
            Verbose = options.Verbose,
            Compression = options.Compression,
            Action = options.Action,
            IfNotExists = options.IfNotExists,
            Synchronous = options.Synchronous,
            NoSync = options.NoSync,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgRecvLogicalOptions Copy(PgRecvLogicalOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgRecvLogicalOptions
        {
            FsyncInterval = options.FsyncInterval,
            NoLoop = options.NoLoop,
            Verbose = options.Verbose,
            EnableTwoPhase = options.EnableTwoPhase,
            EnableFailover = options.EnableFailover,
            Database = options.Database,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            StartPosition = options.StartPosition,
            EndPosition = options.EndPosition,
            Plugin = options.Plugin,
            StatusInterval = options.StatusInterval,
            Slot = options.Slot,
            Action = options.Action,
            IfNotExists = options.IfNotExists,
        };
        foreach (var item in options.PluginOptions) copy.PluginOptions.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgResetWalOptions Copy(PgResetWalOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgResetWalOptions
        {
            DataDirectory = options.DataDirectory,
            CommitTimestampIds = options.CommitTimestampIds,
            TransactionIdEpoch = options.TransactionIdEpoch,
            Force = options.Force,
            NextWalFile = options.NextWalFile,
            MultiTransactionIds = options.MultiTransactionIds,
            DryRun = options.DryRun,
            NextObjectId = options.NextObjectId,
            MultiTransactionOffset = options.MultiTransactionOffset,
            OldestTransactionId = options.OldestTransactionId,
            NextTransactionId = options.NextTransactionId,
            WalSegmentSizeMegabytes = options.WalSegmentSizeMegabytes,
            CharSignedness = options.CharSignedness,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgRestoreOptions Copy(PgRestoreOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgRestoreOptions
        {
            Mode = options.Mode,
            ContentMode = options.ContentMode,
            Clean = options.Clean,
            Create = options.Create,
            ExitOnError = options.ExitOnError,
            ArchiveFormat = options.ArchiveFormat,
            Jobs = options.Jobs,
            NoPrivileges = options.NoPrivileges,
            NoOwner = options.NoOwner,
            NoReconnect = options.NoReconnect,
            Superuser = options.Superuser,
            Verbosity = options.Verbosity,
            TransactionMode = options.TransactionMode,
            DisableTriggers = options.DisableTriggers,
            EnableRowSecurity = options.EnableRowSecurity,
            IfExists = options.IfExists,
            NoDataForFailedTables = options.NoDataForFailedTables,
            NoTablespaces = options.NoTablespaces,
            UseSetSessionAuthorization = options.UseSetSessionAuthorization,
            StrictNames = options.StrictNames,
            NoComments = options.NoComments,
            NoPublications = options.NoPublications,
            NoSecurityLabels = options.NoSecurityLabels,
            NoSubscriptions = options.NoSubscriptions,
            RestrictKey = options.RestrictKey,
            NoTableAccessMethod = options.NoTableAccessMethod,
            NoData = options.NoData,
            NoPolicies = options.NoPolicies,
            NoSchema = options.NoSchema,
            NoStatistics = options.NoStatistics,
            Statistics = options.Statistics,
            UseListFile = options.UseListFile,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Role = options.Role,
        };
        foreach (var item in options.Schemas) copy.Schemas.Add(item);
        foreach (var item in options.ExcludedSchemas) copy.ExcludedSchemas.Add(item);
        foreach (var item in options.Functions) copy.Functions.Add(item);
        foreach (var item in options.Indexes) copy.Indexes.Add(item);
        foreach (var item in options.Tables) copy.Tables.Add(item);
        foreach (var item in options.Triggers) copy.Triggers.Add(item);
        foreach (var item in options.Sections) copy.Sections.Add(item);
        foreach (var item in options.Filters) copy.Filters.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgRewindOptions Copy(PgRewindOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgRewindOptions
        {
            TargetDataDirectory = options.TargetDataDirectory,
            SourceDataDirectory = options.SourceDataDirectory,
            SourceConnectionString = options.SourceConnectionString,
            WriteRecoveryConfiguration = options.WriteRecoveryConfiguration,
            NoEnsureShutdown = options.NoEnsureShutdown,
            ConfigurationFile = options.ConfigurationFile,
            RestoreTargetWal = options.RestoreTargetWal,
            DryRun = options.DryRun,
            NoSync = options.NoSync,
            Progress = options.Progress,
            Debug = options.Debug,
            SyncMethod = options.SyncMethod,
        };
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgUpgradeOptions Copy(PgUpgradeOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgUpgradeOptions
        {
            OldDataDirectory = options.OldDataDirectory,
            NewDataDirectory = options.NewDataDirectory,
            OldBinaryDirectory = options.OldBinaryDirectory,
            NewBinaryDirectory = options.NewBinaryDirectory,
            OldPort = options.OldPort,
            NewPort = options.NewPort,
            Username = options.Username,
            CheckOnly = options.CheckOnly,
            TransferMode = options.TransferMode,
            Retain = options.Retain,
            Jobs = options.Jobs,
            SocketDirectory = options.SocketDirectory,
            Verbose = options.Verbose,
            NoSync = options.NoSync,
            SyncMethod = options.SyncMethod,
            NoStatistics = options.NoStatistics,
            CharSignedness = options.CharSignedness,
        };
        foreach (var item in options.OldServerOptions) copy.OldServerOptions.Add(item);
        foreach (var item in options.NewServerOptions) copy.NewServerOptions.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PgVerifyBackupOptions Copy(PgVerifyBackupOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PgVerifyBackupOptions
        {
            ExitOnError = options.ExitOnError,
            Format = options.Format,
            ManifestPath = options.ManifestPath,
            NoParseWal = options.NoParseWal,
            Progress = options.Progress,
            Quiet = options.Quiet,
            SkipChecksums = options.SkipChecksums,
            WalDirectory = options.WalDirectory,
        };
        foreach (var item in options.IgnoredPaths) copy.IgnoredPaths.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static PsqlOptions Copy(PsqlOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new PsqlOptions
        {
            Database = options.Database,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            OutputFormat = options.OutputFormat,
            EchoAll = options.EchoAll,
            EchoErrors = options.EchoErrors,
            EchoQueries = options.EchoQueries,
            EchoHidden = options.EchoHidden,
            FieldSeparator = options.FieldSeparator,
            RecordSeparator = options.RecordSeparator,
            ListDatabases = options.ListDatabases,
            LogFile = options.LogFile,
            NoReadline = options.NoReadline,
            SingleTransaction = options.SingleTransaction,
            OutputFile = options.OutputFile,
            Quiet = options.Quiet,
            SingleStep = options.SingleStep,
            SingleLine = options.SingleLine,
            TuplesOnly = options.TuplesOnly,
            TableAttributes = options.TableAttributes,
            Expanded = options.Expanded,
            NoPsqlRc = options.NoPsqlRc,
        };
        foreach (var item in options.Actions) copy.Actions.Add(item);
        foreach (var item in options.Variables) copy.Variables.Add(item);
        foreach (var item in options.PrintSettings) copy.PrintSettings.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static ReindexDbOptions Copy(ReindexDbOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new ReindexDbOptions
        {
            DatabaseName = options.DatabaseName,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Echo = options.Echo,
            Quiet = options.Quiet,
            AllDatabases = options.AllDatabases,
            SystemCatalogs = options.SystemCatalogs,
            Jobs = options.Jobs,
            Verbose = options.Verbose,
            Concurrently = options.Concurrently,
            MaintenanceDatabase = options.MaintenanceDatabase,
            Tablespace = options.Tablespace,
        };
        foreach (var item in options.Schemas) copy.Schemas.Add(item);
        foreach (var item in options.Tables) copy.Tables.Add(item);
        foreach (var item in options.Indexes) copy.Indexes.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

    internal static VacuumDbOptions Copy(VacuumDbOptions options)
    {
        ConfigurationGuard.NotNull(options, nameof(options));
        var copy = new VacuumDbOptions
        {
            DatabaseName = options.DatabaseName,
            Host = options.Host,
            Port = options.Port,
            Username = options.Username,
            PasswordPrompt = options.PasswordPrompt,
            Echo = options.Echo,
            Quiet = options.Quiet,
            Analyze = options.Analyze,
            AnalyzeOnly = options.AnalyzeOnly,
            Freeze = options.Freeze,
            AllDatabases = options.AllDatabases,
            Full = options.Full,
            Verbose = options.Verbose,
            Jobs = options.Jobs,
            ParallelWorkers = options.ParallelWorkers,
            MaintenanceDatabase = options.MaintenanceDatabase,
            AnalyzeInStages = options.AnalyzeInStages,
            DisablePageSkipping = options.DisablePageSkipping,
            SkipLocked = options.SkipLocked,
            MinXidAge = options.MinXidAge,
            MinMultiXactIdAge = options.MinMultiXactIdAge,
            IndexCleanup = options.IndexCleanup,
            NoTruncate = options.NoTruncate,
            NoProcessToast = options.NoProcessToast,
            NoProcessMain = options.NoProcessMain,
            BufferUsageLimit = options.BufferUsageLimit,
            MissingStatsOnly = options.MissingStatsOnly,
        };
        foreach (var item in options.Tables) copy.Tables.Add(item);
        foreach (var item in options.Schemas) copy.Schemas.Add(item);
        foreach (var item in options.ExcludedSchemas) copy.ExcludedSchemas.Add(item);
        foreach (var item in options.EnvironmentVariables) copy.EnvironmentVariables.Add(item.Key, item.Value);
        return copy;
    }

}
