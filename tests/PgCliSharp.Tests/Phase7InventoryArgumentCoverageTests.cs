using PgCliSharp.Internal.ServerApplications;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Tests;

public sealed class Phase7InventoryArgumentCoverageTests
{
    [Fact]
    public void InitDb_Pgdata_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.DataDirectory = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--pgdata");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_Encoding_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.Encoding = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--encoding");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_Locale_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.Locale = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--locale");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_LcCollate_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LcCollate = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--lc-collate");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_LcCtype_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LcCtype = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--lc-ctype");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_LcMonetary_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LcMonetary = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--lc-monetary");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_LcNumeric_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LcNumeric = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--lc-numeric");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_LcTime_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LcTime = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--lc-time");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_LcMessages_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LcMessages = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--lc-messages");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_NoLocale_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.NoLocale = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-locale", args);
    }

    [Fact]
    public void InitDb_TextSearchConfig_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.TextSearchConfiguration = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--text-search-config");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_Auth_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.Authentication = PgInitDbAuthentication.ScramSha256;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--auth");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("scram-sha-256", args[index + 1]);
    }

    [Fact]
    public void InitDb_AuthLocal_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LocalAuthentication = PgInitDbAuthentication.ScramSha256;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--auth-local");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("scram-sha-256", args[index + 1]);
    }

    [Fact]
    public void InitDb_AuthHost_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.HostAuthentication = PgInitDbAuthentication.ScramSha256;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--auth-host");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("scram-sha-256", args[index + 1]);
    }

    [Fact]
    public void InitDb_Pwprompt_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.PasswordPrompt = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--pwprompt", args);
    }

    [Fact]
    public void InitDb_Pwfile_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.PasswordFile = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--pwfile");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_Username_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.Username = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--username");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_Debug_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.Debug = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--debug", args);
    }

    [Fact]
    public void InitDb_Show_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.ShowSettings = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--show", args);
    }

    [Fact]
    public void InitDb_Noclean_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.NoClean = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-clean", args);
    }

    [Fact]
    public void InitDb_NoClean_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.NoClean = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-clean", args);
    }

    [Fact]
    public void InitDb_Nosync_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.NoSync = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-sync", args);
    }

    [Fact]
    public void InitDb_NoSync_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.NoSync = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-sync", args);
    }

    [Fact]
    public void InitDb_SyncOnly_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.SyncOnly = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--sync-only", args);
    }

    [Fact]
    public void InitDb_Waldir_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.WalDirectory = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--waldir");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_DataChecksums_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.DataChecksums = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--data-checksums", args);
    }

    [Fact]
    public void InitDb_InputDirectory_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.InputDirectory = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-L");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_WalSegsize_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.WalSegmentSizeMegabytes = 16;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--wal-segsize");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void InitDb_AllowGroupAccess_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.AllowGroupAccess = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--allow-group-access", args);
    }

    [Fact]
    public void InitDb_NoInstructions_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.NoInstructions = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-instructions", args);
    }

    [Fact]
    public void InitDb_DiscardCaches_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.DiscardCaches = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--discard-caches", args);
    }

    [Fact]
    public void InitDb_LocaleProvider_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.LocaleProvider = PgInitDbLocaleProvider.Icu;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--locale-provider");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("icu", args[index + 1]);
    }

    [Fact]
    public void InitDb_IcuLocale_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.IcuLocale = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--icu-locale");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_Set_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.Settings.Add(new PgServerSetting("shared_buffers", "128 MB"));
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--set");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("shared_buffers=128 MB", args[index + 1]);
    }

    [Fact]
    public void InitDb_IcuRules_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.IcuRules = "token with spaces";
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--icu-rules");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void InitDb_BuiltinLocale_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.BuiltinLocale = PgBuiltinLocale.CUtf8;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--builtin-locale");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("C.UTF-8", args[index + 1]);
    }

    [Fact]
    public void InitDb_SyncMethod_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.SyncMethod = PgFileSyncMethod.Fsync;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--sync-method");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("fsync", args[index + 1]);
    }

    [Fact]
    public void InitDb_NoDataChecksums_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.DataChecksums = false;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-data-checksums", args);
    }

    [Fact]
    public void InitDb_NoSyncDataFiles_HasItsAuditedArgumentBinding()
    {
        var o = new InitDbOptions();
        o.NoSyncDataFiles = true;
        IReadOnlyList<string> args = InitDbImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-sync-data-files", args);
    }

    [Fact]
    public void PgCtl_Log_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.LogFile = "token with spaces";
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--log");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgCtl_Mode_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.ShutdownMode = PgCtlShutdownMode.Immediate;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--mode");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("immediate", args[index + 1]);
    }

    [Fact]
    public void PgCtl_Pgdata_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.DataDirectory = "token with spaces";
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--pgdata");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgCtl_Options_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.ForwardedOptions.Add("first fragment");
        o.ForwardedOptions.Add("second fragment");
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--options");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("first fragment", args[index + 1]);
        Assert.True(args.ToList().IndexOf("first fragment") < args.ToList().IndexOf("second fragment"));
    }

    [Fact]
    public void PgCtl_Silent_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.Silent = true;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--silent", args);
    }

    [Fact]
    public void PgCtl_Timeout_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.WaitTimeoutSeconds = 16;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--timeout");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgCtl_CoreFiles_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.CoreFiles = true;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--core-files", args);
    }

    [Fact]
    public void PgCtl_Wait_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.Wait = true;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--wait", args);
    }

    [Fact]
    public void PgCtl_NoWait_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.Wait = false;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-wait", args);
    }

    [Fact]
    public void PgCtl_Executable_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.ServerExecutablePath = "token with spaces";
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-p");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgCtl_ServiceName_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.ServiceName = "token with spaces";
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-N");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgCtl_ServicePassword_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.ServicePassword = "token with spaces";
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-P");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgCtl_ServiceUsername_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.ServiceUsername = "token with spaces";
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-U");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgCtl_ServiceStart_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.ServiceStart = PgCtlServiceStart.Demand;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-S");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("demand", args[index + 1]);
    }

    [Fact]
    public void PgCtl_EventSource_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.EventSource = "token with spaces";
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-e");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgCtl_Command_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.Command = PgCtlCommand.Reload;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("reload", args);
    }

    [Fact]
    public void PgCtl_Signal_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.Command = PgCtlCommand.Kill; o.Signal = PgCtlSignal.Interrupt; o.ProcessId = 123;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("INT", args);
    }

    [Fact]
    public void PgCtl_Pid_HasItsAuditedArgumentBinding()
    {
        var o = new PgCtlOptions { Command = PgCtlCommand.Status };
        o.Command = PgCtlCommand.Kill; o.Signal = PgCtlSignal.Kill; o.ProcessId = 123;
        IReadOnlyList<string> args = PgCtlImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("123", args);
    }

    [Fact]
    public void PgUpgrade_OldDatadir_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.OldDataDirectory = "token with spaces";
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--old-datadir");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_NewDatadir_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.NewDataDirectory = "token with spaces";
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--new-datadir");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_OldBindir_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.OldBinaryDirectory = "token with spaces";
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--old-bindir");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_NewBindir_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.NewBinaryDirectory = "token with spaces";
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--new-bindir");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_OldOptions_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.OldServerOptions.Add("first fragment");
        o.OldServerOptions.Add("second fragment");
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--old-options");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("first fragment", args[index + 1]);
        Assert.True(args.ToList().IndexOf("first fragment") < args.ToList().IndexOf("second fragment"));
    }

    [Fact]
    public void PgUpgrade_NewOptions_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.NewServerOptions.Add("first fragment");
        o.NewServerOptions.Add("second fragment");
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--new-options");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("first fragment", args[index + 1]);
        Assert.True(args.ToList().IndexOf("first fragment") < args.ToList().IndexOf("second fragment"));
    }

    [Fact]
    public void PgUpgrade_OldPort_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.OldPort = 16;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--old-port");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_NewPort_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.NewPort = 16;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--new-port");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_Username_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.Username = "token with spaces";
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--username");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_Check_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.CheckOnly = true;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--check", args);
    }

    [Fact]
    public void PgUpgrade_Link_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.TransferMode = PgUpgradeTransferMode.Link;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--link", args);
    }

    [Fact]
    public void PgUpgrade_Retain_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.Retain = true;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--retain", args);
    }

    [Fact]
    public void PgUpgrade_Jobs_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.Jobs = 16;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--jobs");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_Verbose_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.Verbose = true;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--verbose", args);
    }

    [Fact]
    public void PgUpgrade_Socketdir_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.SocketDirectory = "token with spaces";
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--socketdir");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_Clone_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.TransferMode = PgUpgradeTransferMode.Clone;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--clone", args);
    }

    [Fact]
    public void PgUpgrade_NoSync_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.NoSync = true;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-sync", args);
    }

    [Fact]
    public void PgUpgrade_Copy_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.TransferMode = PgUpgradeTransferMode.Copy;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--copy", args);
    }

    [Fact]
    public void PgUpgrade_CopyFileRange_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.TransferMode = PgUpgradeTransferMode.CopyFileRange;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--copy-file-range", args);
    }

    [Fact]
    public void PgUpgrade_SyncMethod_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.SyncMethod = PgFileSyncMethod.Fsync;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--sync-method");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("fsync", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_NoStatistics_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.NoStatistics = true;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-statistics", args);
    }

    [Fact]
    public void PgUpgrade_SetCharSignedness_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.CharSignedness = PgCharSignedness.UnsignedValue;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--set-char-signedness");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("unsigned", args[index + 1]);
    }

    [Fact]
    public void PgUpgrade_Swap_HasItsAuditedArgumentBinding()
    {
        var o = new PgUpgradeOptions();
        o.TransferMode = PgUpgradeTransferMode.Swap;
        IReadOnlyList<string> args = PgUpgradeImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--swap", args);
    }

    [Fact]
    public void PgRewind_TargetPgdata_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.TargetDataDirectory = "token with spaces";
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--target-pgdata");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgRewind_SourcePgdata_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.SourceDataDirectory = "token with spaces";
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--source-pgdata");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgRewind_SourceServer_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.SourceConnectionString = "token with spaces";
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--source-server");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgRewind_DryRun_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.DryRun = true;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--dry-run", args);
    }

    [Fact]
    public void PgRewind_Progress_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.Progress = true;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--progress", args);
    }

    [Fact]
    public void PgRewind_Debug_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.Debug = true;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--debug", args);
    }

    [Fact]
    public void PgRewind_NoSync_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.NoSync = true;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-sync", args);
    }

    [Fact]
    public void PgRewind_WriteRecoveryConf_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.WriteRecoveryConfiguration = true;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--write-recovery-conf", args);
    }

    [Fact]
    public void PgRewind_NoEnsureShutdown_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.NoEnsureShutdown = true;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-ensure-shutdown", args);
    }

    [Fact]
    public void PgRewind_RestoreTargetWal_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.RestoreTargetWal = true;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--restore-target-wal", args);
    }

    [Fact]
    public void PgRewind_ConfigFile_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.ConfigurationFile = "token with spaces";
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--config-file");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgRewind_SyncMethod_HasItsAuditedArgumentBinding()
    {
        var o = new PgRewindOptions();
        o.SyncMethod = PgFileSyncMethod.Fsync;
        IReadOnlyList<string> args = PgRewindImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--sync-method");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("fsync", args[index + 1]);
    }

    [Fact]
    public void PgChecksums_Check_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.Mode = PgChecksumsMode.Check;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--check", args);
    }

    [Fact]
    public void PgChecksums_Pgdata_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.DataDirectory = "token with spaces";
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--pgdata");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgChecksums_Disable_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.Mode = PgChecksumsMode.Disable;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--disable", args);
    }

    [Fact]
    public void PgChecksums_Enable_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.Mode = PgChecksumsMode.Enable;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--enable", args);
    }

    [Fact]
    public void PgChecksums_Filenode_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.FileNode = 16;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--filenode");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgChecksums_NoSync_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.NoSync = true;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--no-sync", args);
    }

    [Fact]
    public void PgChecksums_Progress_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.Progress = true;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--progress", args);
    }

    [Fact]
    public void PgChecksums_Verbose_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.Verbose = true;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("--verbose", args);
    }

    [Fact]
    public void PgChecksums_SyncMethod_HasItsAuditedArgumentBinding()
    {
        var o = new PgChecksumsOptions();
        o.SyncMethod = PgFileSyncMethod.Fsync;
        IReadOnlyList<string> args = PgChecksumsImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--sync-method");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("fsync", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_Pgdata_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.DataDirectory = "token with spaces";
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-D");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("token with spaces", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_CommitTimestampIds_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.CommitTimestampIds = new PgCommitTimestampIds(3, 9);
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-c");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("3,9", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_Epoch_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.TransactionIdEpoch = 16;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-e");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_Force_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.Force = true;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("-f", args);
    }

    [Fact]
    public void PgResetWal_NextWalFile_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.NextWalFile = new PgWalSegmentName("000000010000000000000001");
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-l");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("000000010000000000000001", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_MultixactIds_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.MultiTransactionIds = new PgMultiTransactionIds(5, 1);
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-m");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("5,1", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_DryRun_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.DryRun = true;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        Assert.Contains("-n", args);
    }

    [Fact]
    public void PgResetWal_NextOid_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.NextObjectId = 16;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-o");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_MultixactOffset_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.MultiTransactionOffset = 16;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-O");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_OldestTransactionId_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.OldestTransactionId = 16;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-u");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_NextTransactionId_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.NextTransactionId = 16;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("-x");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_WalSegsize_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.WalSegmentSizeMegabytes = 16;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--wal-segsize");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("16", args[index + 1]);
    }

    [Fact]
    public void PgResetWal_CharSignedness_HasItsAuditedArgumentBinding()
    {
        var o = new PgResetWalOptions();
        o.CharSignedness = PgCharSignedness.UnsignedValue;
        IReadOnlyList<string> args = PgResetWalImplementation.Build(o, PostgreSqlMajorVersion.V18);
        int index = args.ToList().IndexOf("--char-signedness");
        Assert.True(index >= 0 && index + 1 < args.Count);
        Assert.Equal("unsigned", args[index + 1]);
    }

}
