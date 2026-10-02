using System.Reflection;
using Xunit.Abstractions;

namespace PgCliSharp.Tests;

public sealed class Phase8PublicApiTests(ITestOutputHelper output)
{
    private static readonly string[] ConstructorParameterNames = { "executablePath", "version" };
    [Fact]
    public void PublicAndProtectedContract_MatchesReviewedBaseline()
    {
        string[] actual = PublicApiSnapshot.Capture(PublicApiSnapshot.ContractTypes(typeof(PostgreSqlMajorVersion).Assembly));
        string[] expected = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "api", "PublicApi.txt"))
            .Where(line => line.Length > 0 && line[0] != '#').ToArray();
        if (!expected.SequenceEqual(actual, StringComparer.Ordinal))
        {
            // Intentional mismatch diagnostics, never an automatic baseline rewrite.
            output.WriteLine("API_BASELINE_BEGIN");
            foreach (string line in actual) output.WriteLine("API_BASELINE|" + line);
            output.WriteLine("API_BASELINE_END");
        }
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Snapshot_TracksNamedDefaultsProtectedMethodsAndEnumValues()
    {
        string[] lines = PublicApiSnapshot.Capture(new[] { typeof(SnapshotFixture), typeof(SnapshotStatus) });
        Assert.Contains(lines, line => line.Contains("amount:System.Int32") && line.Contains("default=7"));
        Assert.Contains(lines, line => line.Contains(".ProtectedOperation ") && line.Contains("Family"));
        Assert.Contains(lines, line => line.Contains(".Ready ") && line.Contains("value=3"));
        Assert.Contains(lines, line => line.Contains("System.String") && line.Contains("Nullable"));
        Assert.Contains(lines, line => line.StartsWith("TYPE ", StringComparison.Ordinal) && line.Contains("base=System.Enum interfaces="));
        Assert.DoesNotContain(lines, line => line.Contains("System.ISpanFormattable"));
        Assert.Contains(PublicApiSnapshot.ContractTypes(typeof(SnapshotFixture).Assembly), type => type.Name == "ProtectedTypeFixture");
        Assert.Equal(lines, PublicApiSnapshot.Capture(new[] { typeof(SnapshotStatus), typeof(SnapshotFixture) }));
    }

    public class SnapshotFixture
    {
        protected class ProtectedTypeFixture { }
        public virtual string? Operation(int amount = 7) => amount == 7 ? null : "value";
        protected virtual void ProtectedOperation() { }
    }

    public enum SnapshotStatus { Ready = 3 }

    [Fact]
    public void All25Wrappers_UseReviewedConstructorAndAsyncConventions()
    {
        Type[] wrappers =
        {
            typeof(PgDump), typeof(PgRestore), typeof(PgDumpAll), typeof(PgBaseBackup),
            typeof(PgReceiveWal), typeof(PgRecvLogical), typeof(PgVerifyBackup), typeof(PgCombineBackup),
            typeof(CreateDb), typeof(DropDb), typeof(CreateUser), typeof(DropUser),
            typeof(VacuumDb), typeof(ReindexDb), typeof(ClusterDb), typeof(PgIsReady), typeof(PgAmcheck),
            typeof(Psql), typeof(PgBench), typeof(ServerApplications.InitDb), typeof(ServerApplications.PgCtl),
            typeof(ServerApplications.PgUpgrade), typeof(ServerApplications.PgRewind),
            typeof(ServerApplications.PgChecksums), typeof(ServerApplications.PgResetWal),
        };
        Assembly assembly = typeof(PgDump).Assembly;
        // Fail if a new wrapper is omitted from this explicitly reviewed inventory.
        Assert.Equal(wrappers.OrderBy(type => type.FullName), assembly.GetExportedTypes()
            .Where(type => type.GetProperty("ExecutablePath") is not null && type.GetProperty("Version")?.PropertyType == typeof(PostgreSqlMajorVersion))
            .OrderBy(type => type.FullName));
        foreach (Type wrapper in wrappers)
        {
            ConstructorInfo constructor = Assert.Single(wrapper.GetConstructors());
            Assert.Equal(new[] { typeof(string), typeof(PostgreSqlMajorVersion) }, constructor.GetParameters().Select(parameter => parameter.ParameterType));
            Assert.Equal(ConstructorParameterNames, constructor.GetParameters().Select(parameter => parameter.Name));
            Assert.Equal(typeof(string), wrapper.GetProperty("ExecutablePath")!.PropertyType);
            Type options = assembly.GetType(wrapper.FullName + "Options", throwOnError: true)!;
            Assert.True(options.IsSealed);
            foreach (MethodInfo method in wrapper.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName))
            {
                Assert.EndsWith("Async", method.Name);
                Assert.True(method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
                ParameterInfo[] parameters = method.GetParameters();
                Assert.Equal(typeof(CancellationToken), parameters[parameters.Length - 1].ParameterType);
                Assert.Equal("cancellationToken", parameters[parameters.Length - 1].Name);
                Assert.True(parameters[parameters.Length - 1].IsOptional);
                ParameterInfo timeout = Assert.Single(parameters, parameter => parameter.Name == "timeout");
                Assert.Equal(typeof(TimeSpan?), timeout.ParameterType);
                Assert.True(timeout.IsOptional);
                if (method.Name == "ExecuteAsync") Assert.Equal(options, parameters[0].ParameterType);
            }
        }
    }
}
