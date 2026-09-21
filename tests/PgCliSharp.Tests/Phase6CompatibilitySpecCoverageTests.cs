using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.PgBench;
using PgCliSharp.Internal.Psql;

namespace PgCliSharp.Tests;

public sealed class Phase6CompatibilitySpecCoverageTests
{
    [Theory]
    [InlineData("psql", typeof(PsqlOptions))]
    [InlineData("pgbench", typeof(PgBenchOptions))]
    public void InventoryEntries_HaveResolvableApiBindings(string tool, Type optionsType)
    {
        ToolSpec spec = ReadSpec(tool);
        foreach (OptionSpec option in spec.Options)
        {
            bool hasProperty = !string.IsNullOrWhiteSpace(option.Api.Property);
            bool hasBinding = !string.IsNullOrWhiteSpace(option.Api.Binding);
            Assert.True(hasProperty || hasBinding, $"Spec option '{tool}:{option.Id}' has no API binding.");
            if (hasProperty) Assert.NotNull(optionsType.GetProperty(option.Api.Property!));
        }
    }

    [Theory]
    [InlineData("psql")]
    [InlineData("pgbench")]
    public void PerMajorInventories_ArePresent(string tool)
    {
        ToolSpec spec = ReadSpec(tool);
        for (int major = 10; major <= 18; major++)
        {
            string key = major.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Assert.True(spec.Versions.TryGetValue(key, out VersionSpec? version));
            Assert.NotNull(version);
            Assert.True(version!.ToolAvailable);
            Assert.NotEmpty(version.OptionIdsInCurrentMajorDocumentation);
        }
    }

    [Fact]
    public void Psql_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("psql"), PsqlOptionAvailabilityCatalog.All);

    [Fact]
    public void PgBench_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pgbench"), PgBenchOptionAvailabilityCatalog.All);

    private static void AssertAvailabilityMatches(ToolSpec spec, IReadOnlyList<MaintenanceOptionAvailabilityInfo> runtimeItems)
    {
        Dictionary<string, MaintenanceOptionAvailabilityInfo> runtime =
            runtimeItems.ToDictionary(item => item.OptionName, StringComparer.Ordinal);

        foreach (OptionSpec option in spec.Options)
        {
            bool varies = option.Availability.MajorSince != 10 || option.Availability.MajorUntil != 18;
            if (!varies) continue;

            string? longName = option.Spellings.Long.FirstOrDefault(x => x.StartsWith("--", StringComparison.Ordinal));
            if (longName is null) continue;

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
        [DataMember(Name = "tool")] public string Tool { get; set; } = string.Empty;
        [DataMember(Name = "options")] public List<OptionSpec> Options { get; set; } = new List<OptionSpec>();
        [DataMember(Name = "versions")] public Dictionary<string, VersionSpec> Versions { get; set; } = new Dictionary<string, VersionSpec>();
    }

    [DataContract]
    private sealed class OptionSpec
    {
        [DataMember(Name = "id")] public string Id { get; set; } = string.Empty;
        [DataMember(Name = "spellings")] public SpellingSpec Spellings { get; set; } = new SpellingSpec();
        [DataMember(Name = "availability")] public AvailabilitySpec Availability { get; set; } = new AvailabilitySpec();
        [DataMember(Name = "api")] public ApiSpec Api { get; set; } = new ApiSpec();
    }

    [DataContract]
    private sealed class SpellingSpec
    {
        [DataMember(Name = "long")] public List<string> Long { get; set; } = new List<string>();
    }

    [DataContract]
    private sealed class AvailabilitySpec
    {
        [DataMember(Name = "majorSince")] public int MajorSince { get; set; }
        [DataMember(Name = "majorUntil")] public int MajorUntil { get; set; }
    }

    [DataContract]
    private sealed class ApiSpec
    {
        [DataMember(Name = "property")] public string? Property { get; set; }
        [DataMember(Name = "binding")] public string? Binding { get; set; }
    }

    [DataContract]
    private sealed class VersionSpec
    {
        [DataMember(Name = "toolAvailable")] public bool ToolAvailable { get; set; } = true;
        [DataMember(Name = "optionIdsInCurrentMajorDocumentation")] public List<string> OptionIdsInCurrentMajorDocumentation { get; set; } = new List<string>();
    }
}
