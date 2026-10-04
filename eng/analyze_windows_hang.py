"""Read test-only Windows hang dumps on their originating runner; retain text only."""
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import time

PROJECT = '''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
<ItemGroup><PackageReference Include="Microsoft.Diagnostics.Runtime" Version="3.1.512801" /></ItemGroup>
</Project>'''

PROGRAM = r'''
using Microsoft.Diagnostics.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;
if (args[0] == "--capture")
{
    using Process process = Process.GetProcessById(int.Parse(args[1]));
    using FileStream file = File.Create(args[2]);
    if (!Native.MiniDumpWriteDump(process.Handle, process.Id, file.SafeFileHandle, 2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
        throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    return;
}
using DataTarget target = DataTarget.LoadDump(args[0]);
if (target.ClrVersions.Length == 0) throw new InvalidDataException("No CLR found in dump.");
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
        if (!(name.Contains("CliWrap.Command+") || name.Contains("ProcessSessionRunner+") || name.Contains("PsqlSession+") || name.Contains("PolyShim.") || name.StartsWith("MemberPolyfills_") || name == "System.Diagnostics.Process")) continue;
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
                else if (field.ElementType == ClrElementType.Boolean)
                    Console.WriteLine($"  {field.Name} = {field.Read<bool>(obj.Address, false)}");
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
internal static class Native
{
    [DllImport("dbghelp.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool MiniDumpWriteDump(IntPtr process, int processId, Microsoft.Win32.SafeHandles.SafeFileHandle file, uint type, IntPtr exception, IntPtr userStream, IntPtr callback);
}
'''


def smoke(reader, folder, helper):
    """Capture only a child started here, then require real CLR4 managed stacks."""
    trace = Path(folder) / "stack-reader-smoke.child.log"
    dump = Path(folder) / "stack-reader-smoke.dmp"
    trace.unlink(missing_ok=True)
    child = subprocess.Popen([str(Path(helper).resolve()), "wait"], stdin=subprocess.DEVNULL,
                             stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                             env={**os.environ, "PGCLI_TEST_TRACE_FILE": str(trace.resolve())})
    try:
        deadline = time.monotonic() + 10
        while not trace.exists() or "ready" not in trace.read_text(encoding="utf-8"):
            if child.poll() is not None or time.monotonic() >= deadline:
                raise RuntimeError("Owned CLR4 smoke child did not become ready.")
            time.sleep(0.05)
        subprocess.run(["dotnet", str(reader), "--capture", str(child.pid), str(dump)], check=True, timeout=30)
        log = Path(folder) / "stack-reader-smoke.stacks.log"
        with log.open("w", encoding="utf-8") as output:
            subprocess.run(["dotnet", str(reader), str(dump)], stdout=output, stderr=subprocess.STDOUT, check=True, timeout=90)
        stacks = log.read_text(encoding="utf-8")
        if "CLR 4." not in stacks or "THREAD " not in stacks or "Program." not in stacks:
            raise RuntimeError("Owned CLR4 smoke dump did not yield expected managed frames.")
    finally:
        if child.poll() is None:
            child.kill()
        child.wait(timeout=10)
        dump.unlink(missing_ok=True)


def main(folder, helper=None):
    Path(folder).mkdir(parents=True, exist_ok=True)
    dumps = list(Path(folder).rglob("*.dmp"))
    if not dumps and helper is None:
        return
    try:
        with tempfile.TemporaryDirectory(prefix="pgcli-stack-reader-", dir=os.environ["RUNNER_TEMP"]) as work:
            project = Path(work) / "StackReader.csproj"
            project.write_text(PROJECT, encoding="utf-8")
            (Path(work) / "Program.cs").write_text(PROGRAM, encoding="utf-8")
            build = subprocess.run(["dotnet", "build", str(project), "-c", "Release", "-o", str(Path(work) / "out")], capture_output=True, text=True, timeout=180)
            (Path(folder) / "stack-reader-build.log").write_text(build.stdout + build.stderr, encoding="utf-8")
            build.check_returncode()
            reader = Path(work) / "out/StackReader.dll"
            if helper is not None:
                smoke(reader, folder, helper)
            for dump in sorted(dumps, key=lambda p: ("testhost" not in p.name, p.name))[:2]:
                with dump.with_suffix(".stacks.log").open("w", encoding="utf-8") as output:
                    result = subprocess.run(["dotnet", str(reader), str(dump)], stdout=output, stderr=subprocess.STDOUT, timeout=90)
                    result.check_returncode()
    finally:
        # Raw memory is an intermediate diagnostic, never part of uploaded artifacts.
        for dump in dumps:
            dump.unlink(missing_ok=True)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
