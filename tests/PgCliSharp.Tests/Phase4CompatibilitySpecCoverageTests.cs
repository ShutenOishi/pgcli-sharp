using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using PgCliSharp.Internal.BackupWal;
using PgCliSharp.Internal.PgBaseBackup;
using PgCliSharp.Internal.PgCombineBackup;
using PgCliSharp.Internal.PgReceiveWal;
using PgCliSharp.Internal.PgRecvLogical;
using PgCliSharp.Internal.PgVerifyBackup;

namespace PgCliSharp.Tests;

public sealed class Phase4CompatibilitySpecCoverageTests
{
    [Theory]
    [InlineData("pg_basebackup", typeof(PgBaseBackupOptions))]
    [InlineData("pg_receivewal", typeof(PgReceiveWalOptions))]
    [InlineData("pg_recvlogical", typeof(PgRecvLogicalOptions))]
    [InlineData("pg_verifybackup", typeof(PgVerifyBackupOptions))]
    [InlineData("pg_combinebackup", typeof(PgCombineBackupOptions))]
    public void InventoryEntries_HaveResolvableApiBindings(string tool, Type optionsType)
    {
        ToolSpec spec = ReadSpec(tool);

        foreach (OptionSpec option in spec.Options)
        {
            bool hasProperty = !string.IsNullOrWhiteSpace(option.Api.Property);
            bool hasBinding = !string.IsNullOrWhiteSpace(option.Api.Binding);

            Assert.True(hasProperty || hasBinding, $"Spec option '{tool}:{option.Id}' has no API binding.");

            if (hasProperty)
                Assert.NotNull(optionsType.GetProperty(option.Api.Property!));
        }
    }

    [Theory]
    [InlineData("pg_basebackup")]
    [InlineData("pg_receivewal")]
    [InlineData("pg_recvlogical")]
    [InlineData("pg_verifybackup")]
    [InlineData("pg_combinebackup")]
    public void ToolAvailabilityAndPerMajorInventory_AreExplicit(string tool)
    {
        ToolSpec spec = ReadSpec(tool);

        for (int major = 10; major <= 18; major++)
        {
            string key = major.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Assert.True(spec.Versions.TryGetValue(key, out VersionSpec? version));
            Assert.NotNull(version);

            bool shouldExist = major >= spec.Scope.ToolAvailableSince;
            Assert.Equal(shouldExist, version!.ToolAvailable);

            if (!shouldExist)
                Assert.Empty(version.OptionIdsInCurrentMajorDocumentation);
        }
    }

    [Fact]
    public void PgBaseBackup_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_basebackup"), PgBaseBackupOptionAvailabilityCatalog.All);

    [Fact]
    public void PgReceiveWal_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_receivewal"), PgReceiveWalOptionAvailabilityCatalog.All);

    [Fact]
    public void PgRecvLogical_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_recvlogical"), PgRecvLogicalOptionAvailabilityCatalog.All);

    [Fact]
    public void PgVerifyBackup_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_verifybackup"), PgVerifyBackupOptionAvailabilityCatalog.All);

    [Fact]
    public void PgCombineBackup_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_combinebackup"), PgCombineBackupOptionAvailabilityCatalog.All);

    [Fact]
    public void ToolAvailabilityBoundaries_AreEnforcedBeforeProcessStartup()
    {
        Assert.Throws<PgUnsupportedToolException>(
            () => new PgVerifyBackup("/fake/pg_verifybackup", PostgreSqlMajorVersion.V12));
        Assert.Throws<PgUnsupportedToolException>(
            () => new PgCombineBackup("/fake/pg_combinebackup", PostgreSqlMajorVersion.V16));

        _ = new PgVerifyBackup("/fake/pg_verifybackup", PostgreSqlMajorVersion.V13);
        _ = new PgCombineBackup("/fake/pg_combinebackup", PostgreSqlMajorVersion.V17);
    }

    private static void AssertAvailabilityMatches(ToolSpec spec, IReadOnlyList<BackupWalOptionAvailabilityInfo> runtimeItems)
    {
        Dictionary<string, BackupWalOptionAvailabilityInfo> runtime =
            runtimeItems.ToDictionary(item => item.OptionName, StringComparer.Ordinal);

        foreach (OptionSpec option in spec.Options)
        {
            bool variesBeyondToolBaseline =
                option.Availability.MajorSince != spec.Scope.ToolAvailableSince ||
                option.Availability.MajorUntil != 18;

            if (!variesBeyondToolBaseline)
                continue;

            string? longName = option.Spellings.Long.FirstOrDefault(x => x.StartsWith("--", StringComparison.Ordinal));
            if (longName is null)
                continue;

            Assert.True(runtime.TryGetValue(longName, out BackupWalOptionAvailabilityInfo? actual),
                $"No runtime availability entry exists for '{spec.Tool}:{longName}'.");

            Assert.Equal(option.Availability.MajorSince, (int)actual!.Since);
            Assert.Equal(option.Availability.MajorUntil, (int)actual.Until);
        }
    }

    private static ToolSpec ReadSpec(string tool)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "spec", tool + ".json");
        using FileStream stream = File.OpenRead(path);
        var serializer = new DataContractJsonSerializer(
            typeof(ToolSpec),
            new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
        return Assert.IsType<ToolSpec>(serializer.ReadObject(stream));
    }

    [DataContract]
    private sealed class ToolSpec
    {
        [DataMember(Name = "tool")]
        public string Tool { get; set; } = string.Empty;

        [DataMember(Name = "scope")]
        public ScopeSpec Scope { get; set; } = new ScopeSpec();

        [DataMember(Name = "options")]
        public List<OptionSpec> Options { get; set; } = new List<OptionSpec>();

        [DataMember(Name = "versions")]
        public Dictionary<string, VersionSpec> Versions { get; set; } = new Dictionary<string, VersionSpec>();
    }

    [DataContract]
    private sealed class ScopeSpec
    {
        [DataMember(Name = "toolAvailableSince")]
        public int ToolAvailableSince { get; set; } = 10;
    }

    [DataContract]
    private sealed class OptionSpec
    {
        [DataMember(Name = "id")]
        public string Id { get; set; } = string.Empty;

        [DataMember(Name = "spellings")]
        public SpellingSpec Spellings { get; set; } = new SpellingSpec();

        [DataMember(Name = "availability")]
        public AvailabilitySpec Availability { get; set; } = new AvailabilitySpec();

        [DataMember(Name = "api")]
        public ApiSpec Api { get; set; } = new ApiSpec();
    }

    [DataContract]
    private sealed class SpellingSpec
    {
        [DataMember(Name = "long")]
        public List<string> Long { get; set; } = new List<string>();
    }

    [DataContract]
    private sealed class AvailabilitySpec
    {
        [DataMember(Name = "majorSince")]
        public int MajorSince { get; set; }

        [DataMember(Name = "majorUntil")]
        public int MajorUntil { get; set; }
    }

    [DataContract]
    private sealed class ApiSpec
    {
        [DataMember(Name = "property")]
        public string? Property { get; set; }

        [DataMember(Name = "binding")]
        public string? Binding { get; set; }
    }

    [DataContract]
    private sealed class VersionSpec
    {
        [DataMember(Name = "toolAvailable")]
        public bool ToolAvailable { get; set; } = true;

        [DataMember(Name = "optionIdsInCurrentMajorDocumentation")]
        public List<string> OptionIdsInCurrentMajorDocumentation { get; set; } = new List<string>();
    }
}
