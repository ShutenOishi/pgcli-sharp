using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using PgCliSharp.Internal.Clusterdb;
using PgCliSharp.Internal.Createdb;
using PgCliSharp.Internal.Createuser;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.Internal.Dropdb;
using PgCliSharp.Internal.Dropuser;
using PgCliSharp.Internal.PgAmcheck;
using PgCliSharp.Internal.PgIsReady;
using PgCliSharp.Internal.Reindexdb;
using PgCliSharp.Internal.Vacuumdb;

namespace PgCliSharp.Tests;

public sealed class Phase5CompatibilitySpecCoverageTests
{
    [Theory]
    [InlineData("createdb", typeof(CreateDbOptions))]
    [InlineData("dropdb", typeof(DropDbOptions))]
    [InlineData("createuser", typeof(CreateUserOptions))]
    [InlineData("dropuser", typeof(DropUserOptions))]
    [InlineData("vacuumdb", typeof(VacuumDbOptions))]
    [InlineData("reindexdb", typeof(ReindexDbOptions))]
    [InlineData("clusterdb", typeof(ClusterDbOptions))]
    [InlineData("pg_isready", typeof(PgIsReadyOptions))]
    [InlineData("pg_amcheck", typeof(PgAmcheckOptions))]
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
    [InlineData("createdb")]
    [InlineData("dropdb")]
    [InlineData("createuser")]
    [InlineData("dropuser")]
    [InlineData("vacuumdb")]
    [InlineData("reindexdb")]
    [InlineData("clusterdb")]
    [InlineData("pg_isready")]
    [InlineData("pg_amcheck")]
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
    public void CreateDb_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("createdb"), CreatedbOptionAvailabilityCatalog.All);

    [Fact]
    public void DropDb_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("dropdb"), DropdbOptionAvailabilityCatalog.All);

    [Fact]
    public void CreateUser_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("createuser"), CreateuserOptionAvailabilityCatalog.All);

    [Fact]
    public void DropUser_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("dropuser"), DropuserOptionAvailabilityCatalog.All);

    [Fact]
    public void VacuumDb_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("vacuumdb"), VacuumdbOptionAvailabilityCatalog.All);

    [Fact]
    public void ReindexDb_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("reindexdb"), ReindexdbOptionAvailabilityCatalog.All);

    [Fact]
    public void ClusterDb_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("clusterdb"), ClusterdbOptionAvailabilityCatalog.All);

    [Fact]
    public void PgIsReady_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_isready"), PgIsReadyOptionAvailabilityCatalog.All);

    [Fact]
    public void PgAmcheck_VersionVaryingOptionsMatchRuntimeCatalog() =>
        AssertAvailabilityMatches(ReadSpec("pg_amcheck"), PgAmcheckOptionAvailabilityCatalog.All);

    [Fact]
    public void PgAmcheck_ToolAvailabilityBoundary_IsEnforcedBeforeProcessStartup()
    {
        Assert.Throws<PgUnsupportedToolException>(
            () => new PgAmcheck("/fake/pg_amcheck", PostgreSqlMajorVersion.V13));

        _ = new PgAmcheck("/fake/pg_amcheck", PostgreSqlMajorVersion.V14);
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
