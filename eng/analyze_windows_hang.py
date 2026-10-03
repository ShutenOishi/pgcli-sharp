"""Read test-only Windows hang dumps on their originating runner; retain text only."""
import os
from pathlib import Path
import subprocess
import sys
import tempfile

PROJECT = '''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
<ItemGroup><PackageReference Include="Microsoft.Diagnostics.Runtime" Version="3.1.512801" /></ItemGroup>
</Project>'''

PROGRAM = r'''
using Microsoft.Diagnostics.Runtime;
using DataTarget target = DataTarget.LoadDump(args[0]);
foreach (ClrInfo info in target.ClrVersions)
{
    string dac = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET", "Framework64", "v4.0.30319", "mscordacwks.dll");
    using ClrRuntime runtime = info.Version.Major == 4 ? info.CreateRuntime(dac) : info.CreateRuntime();
    Console.WriteLine($"CLR {info.Version}");
    foreach (ClrThread thread in runtime.Threads.Where(t => t.IsAlive))
    {
        Console.WriteLine($"THREAD {thread.OSThreadId:x} {thread.State}");
        foreach (ClrStackFrame frame in thread.EnumerateStackTrace().Take(100))
            Console.WriteLine($"  {frame}");
    }
    // Only relevant async state/pipe fields; do not print environment, strings or arbitrary memory.
    int count = 0;
    foreach (ClrObject obj in runtime.Heap.EnumerateObjects())
    {
        string name = obj.Type?.Name ?? "";
        if (!(name.Contains("CliWrap.Command+") || name.Contains("ProcessSessionRunner+") || name.Contains("PsqlSession+"))) continue;
        ClrInstanceField? state = obj.Type!.GetFieldByName("<>1__state");
        if (state is not null && state.Read<int>(obj.Address, false) == -2) continue;
        if (++count > 500) break;
        Console.WriteLine($"OBJECT {obj.Address:x} {name}");
        foreach (ClrInstanceField field in obj.Type.Fields)
        {
            try
            {
                if (field.ElementType == ClrElementType.Int32)
                    Console.WriteLine($"  {field.Name} = {field.Read<int>(obj.Address, false)}");
                else if (field.IsObjectReference)
                {
                    ClrObject value = field.ReadObject(obj.Address, false);
                    Console.WriteLine($"  {field.Name} = {value.Address:x} {value.Type?.Name}");
                    ClrInstanceField? flags = value.Type?.GetFieldByName("m_stateFlags");
                    if (flags is not null) Console.WriteLine($"    taskFlags = {flags.Read<int>(value.Address, false):x}");
                }
            }
            catch (Exception e) { Console.WriteLine($"  {field.Name}: {e.GetType().Name}"); }
        }
    }
}
'''


def main(folder):
    dumps = list(Path(folder).rglob("*.dmp"))
    if not dumps:
        return
    try:
        with tempfile.TemporaryDirectory(prefix="pgcli-stack-reader-", dir=os.environ["RUNNER_TEMP"]) as work:
            project = Path(work) / "StackReader.csproj"
            project.write_text(PROJECT, encoding="utf-8")
            (Path(work) / "Program.cs").write_text(PROGRAM, encoding="utf-8")
            build = subprocess.run(["dotnet", "build", str(project), "-c", "Release", "-o", str(Path(work) / "out")], capture_output=True, text=True, timeout=180)
            (Path(folder) / "stack-reader-build.log").write_text(build.stdout + build.stderr, encoding="utf-8")
            build.check_returncode()
            for dump in sorted(dumps, key=lambda p: ("testhost" not in p.name, p.name))[:2]:
                with dump.with_suffix(".stacks.log").open("w", encoding="utf-8") as output:
                    result = subprocess.run(["dotnet", str(Path(work) / "out/StackReader.dll"), str(dump)], stdout=output, stderr=subprocess.STDOUT, timeout=90)
                    result.check_returncode()
    finally:
        # Raw memory is an intermediate diagnostic, never part of uploaded artifacts.
        for dump in dumps:
            dump.unlink(missing_ok=True)


if __name__ == "__main__":
    main(sys.argv[1])
