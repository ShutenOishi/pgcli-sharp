using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.ServerApplications;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Tests;

public sealed class Phase7CompatibilitySpecCoverageTests
{
    [Theory]
    [InlineData("initdb", typeof(InitDbOptions))]
    [InlineData("pg_ctl", typeof(PgCtlOptions))]
    [InlineData("pg_upgrade", typeof(PgUpgradeOptions))]
    [InlineData("pg_rewind", typeof(PgRewindOptions))]
    [InlineData("pg_checksums", typeof(PgChecksumsOptions))]
    [InlineData("pg_resetwal", typeof(PgResetWalOptions))]
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

            if (hasBinding)
            {
                Assert.True(
                    option.Api.Binding is "Executable version probe" or "GetHelpAsync",
                    $"Spec option '{tool}:{option.Id}' uses unknown special binding '{option.Api.Binding}'.");
            }
        }
    }

    [Theory]
    [InlineData("initdb")]
    [InlineData("pg_ctl")]
    [InlineData("pg_upgrade")]
    [InlineData("pg_rewind")]
    [InlineData("pg_checksums")]
    [InlineData("pg_resetwal")]
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
    public void InitDb_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("initdb"), InitDbOptionAvailabilityCatalog.All);

    [Fact]
    public void PgCtl_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_ctl"), PgCtlOptionAvailabilityCatalog.All);

    [Fact]
    public void PgUpgrade_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_upgrade"), PgUpgradeOptionAvailabilityCatalog.All);

    [Fact]
    public void PgRewind_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_rewind"), PgRewindOptionAvailabilityCatalog.All);

    [Fact]
    public void PgChecksums_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_checksums"), PgChecksumsOptionAvailabilityCatalog.All);

    [Fact]
    public void PgResetWal_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_resetwal"), PgResetWalOptionAvailabilityCatalog.All);

    [Fact]
    public void PgChecksums_WholeToolBoundaryIsBeforeStartup()
    {
        Assert.Throws<PgUnsupportedToolException>(() => new PgChecksums("/fake/pg_checksums", PostgreSqlMajorVersion.V11));
        _ = new PgChecksums("/fake/pg_checksums", PostgreSqlMajorVersion.V12);
    }

    private static void AssertAvailabilityMatches(ToolSpec spec, IReadOnlyList<MaintenanceOptionAvailabilityInfo> runtimeItems)
    {
        Dictionary<string, MaintenanceOptionAvailabilityInfo> runtime =
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

            Assert.True(runtime.TryGetValue(longName, out MaintenanceOptionAvailabilityInfo? actual),
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
