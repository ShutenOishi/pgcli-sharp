namespace PgCliSharp.Tests;

internal static class ManagedTestProcess
{
    internal static (string Executable, string[] Arguments) Command(params string[] arguments)
    {
        string helper = Path.Combine(AppContext.BaseDirectory, "test-process", "PgCliSharp.TestProcess");
#if NET48
        return (helper + ".exe", arguments);
#else
        string executable = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
        return (executable, new[] { helper + ".dll" }.Concat(arguments).ToArray());
#endif
    }
}
