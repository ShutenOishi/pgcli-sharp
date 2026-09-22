using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using PgCliSharp.Internal.PgDump;
using PgCliSharp.Internal.PgDumpAll;
using PgCliSharp.Internal.PgRestore;

namespace PgCliSharp.Tests;

public sealed class CompatibilitySpecCoverageTests
{
    private static readonly Dictionary<string, HashSet<string>> AllowedSpecialBindings =
        new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            ["pg_dump"] = new HashSet<string>(
                new[] { "PgDumpOutput", "not-normal-options", "wrapper-version-probe" },
                StringComparer.Ordinal),
            ["pg_restore"] = new HashSet<string>(
                new[]
                {
                    "Executable version probe / utility command",
                    "PgRestoreOutput script/list destination",
                    "PgRestoreOutput.ToDatabase",
                    "Utility command",
                },
                StringComparer.Ordinal),
            ["pg_dumpall"] = new HashSet<string>(
                new[]
                {
                    "Executable version probe / utility command",
                    "PgDumpAllOutput",
                    "Utility command",
                },
                StringComparer.Ordinal),
        };

    [Theory]
    [InlineData("pg_dump", typeof(PgDumpOptions))]
    [InlineData("pg_restore", typeof(PgRestoreOptions))]
    [InlineData("pg_dumpall", typeof(PgDumpAllOptions))]
    public void InventoryEntries_HaveResolvableApiBindings(
        string tool,
        Type optionsType)
    {
        ToolSpec spec = ReadSpec(tool);

        foreach (OptionSpec option in spec.Options)
        {
            bool hasProperty = !string.IsNullOrWhiteSpace(option.Api.Property);
            bool hasSpecialBinding = !string.IsNullOrWhiteSpace(option.Api.Binding);

            Assert.True(
                hasProperty || hasSpecialBinding,
                $"Spec option '{tool}:{option.Id}' has no API binding.");

            if (hasProperty)
            {
                Assert.NotNull(optionsType.GetProperty(option.Api.Property!));
            }

            if (hasSpecialBinding)
            {
                Assert.True(
                    AllowedSpecialBindings.TryGetValue(tool, out HashSet<string>? allowed) &&
                    allowed.Contains(option.Api.Binding!),
                    $"Spec option '{tool}:{option.Id}' uses unknown special binding '{option.Api.Binding}'.");
            }
        }
    }

    [Fact]
    public void PgDump_VersionVaryingSpecMatchesRuntimeCatalog()
    {
        AssertAvailabilityMatches(
            ReadSpec("pg_dump"),
            PgDumpOptionAvailabilityCatalog.All.Select(
                item => new RuntimeAvailability(
                    item.OptionName,
                    item.Since,
                    item.Until,
                    item.MinimumVersions)));
    }

    [Fact]
    public void PgRestore_VersionVaryingSpecMatchesRuntimeCatalog()
    {
        AssertAvailabilityMatches(
            ReadSpec("pg_restore"),
            PgRestoreOptionAvailabilityCatalog.All.Select(
                item => new RuntimeAvailability(
                    item.OptionName,
                    item.Since,
                    item.Until,
                    item.MinimumVersions)));
    }

    [Fact]
    public void PgDumpAll_VersionVaryingSpecMatchesRuntimeCatalog()
    {
        AssertAvailabilityMatches(
            ReadSpec("pg_dumpall"),
            PgDumpAllOptionAvailabilityCatalog.All.Select(
                item => new RuntimeAvailability(
                    item.OptionName,
                    item.Since,
                    item.Until,
                    item.MinimumVersions)));
    }

    private static void AssertAvailabilityMatches(
        ToolSpec spec,
        IEnumerable<RuntimeAvailability> runtimeItems)
    {
        Dictionary<string, RuntimeAvailability> runtime =
            runtimeItems.ToDictionary(
                item => item.OptionName,
                StringComparer.Ordinal);

        foreach (OptionSpec option in spec.Options)
        {
            AvailabilitySpec availability = option.Availability;
            bool varies =
                availability.MajorSince != 10 ||
                availability.MajorUntil != 18 ||
                (availability.MinimumExecutableVersionByMajor?.Count ?? 0) > 0 ||
                (availability.MinimumPatchByMajor?.Count ?? 0) > 0;

            if (!varies)
            {
                continue;
            }

            string optionName = option.Spellings.Long[0];

            Assert.True(
                runtime.TryGetValue(optionName, out RuntimeAvailability? actual),
                $"No runtime availability entry exists for '{spec.Tool}:{optionName}'.");

            Assert.NotNull(actual);
            Assert.Equal(availability.MajorSince, (int)actual!.Since);
            Assert.Equal(availability.MajorUntil, (int)actual.Until);

            Dictionary<string, string> expectedMinimums =
                availability.MinimumExecutableVersionByMajor ??
                availability.MinimumPatchByMajor ??
                new Dictionary<string, string>(StringComparer.Ordinal);

            Dictionary<string, string> runtimeMinimums =
                actual.MinimumVersions.ToDictionary(
                    pair => ((int)pair.Key).ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    pair => pair.Value.ToString(),
                    StringComparer.Ordinal);

            Assert.Equal(
                expectedMinimums.OrderBy(pair => pair.Key),
                runtimeMinimums.OrderBy(pair => pair.Key));
        }
    }

    private static ToolSpec ReadSpec(string tool)
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "spec",
            tool + ".json");

        using FileStream stream = File.OpenRead(path);
        var serializer = new DataContractJsonSerializer(
            typeof(ToolSpec),
            new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            });

        return Assert.IsType<ToolSpec>(serializer.ReadObject(stream));
    }

    private sealed class RuntimeAvailability
    {
        internal RuntimeAvailability(
            string optionName,
            PostgreSqlMajorVersion since,
            PostgreSqlMajorVersion until,
            IReadOnlyDictionary<PostgreSqlMajorVersion, Version> minimumVersions)
        {
            OptionName = optionName;
            Since = since;
            Until = until;
            MinimumVersions = minimumVersions;
        }

        internal string OptionName { get; }

        internal PostgreSqlMajorVersion Since { get; }

        internal PostgreSqlMajorVersion Until { get; }

        internal IReadOnlyDictionary<PostgreSqlMajorVersion, Version> MinimumVersions { get; }
    }

    [DataContract]
    private sealed class ToolSpec
    {
        [DataMember(Name = "tool")]
        public string Tool { get; set; } = string.Empty;

        [DataMember(Name = "options")]
        public List<OptionSpec> Options { get; set; } = new List<OptionSpec>();
    }

    [DataContract]
    private sealed class OptionSpec
    {
        [DataMember(Name = "id")]
        public string Id { get; set; } = string.Empty;

        [DataMember(Name = "spellings")]
        public SpellingSpec Spellings { get; set; } = new SpellingSpec();

        [DataMember(Name = "availability")]
        public AvailabilitySpec Availability { get; set; } =
            new AvailabilitySpec();

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

        [DataMember(Name = "minimumExecutableVersionByMajor")]
        public Dictionary<string, string>? MinimumExecutableVersionByMajor { get; set; }

        [DataMember(Name = "minimumPatchByMajor")]
        public Dictionary<string, string>? MinimumPatchByMajor { get; set; }
    }

    [DataContract]
    private sealed class ApiSpec
    {
        [DataMember(Name = "property")]
        public string? Property { get; set; }

        [DataMember(Name = "binding")]
        public string? Binding { get; set; }
    }
}
