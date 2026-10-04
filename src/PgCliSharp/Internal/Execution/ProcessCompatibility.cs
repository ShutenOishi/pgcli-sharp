using System.ComponentModel;
using System.Diagnostics;
#if NETSTANDARD2_0
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
#endif

namespace PgCliSharp.Internal.Execution;

internal static class ProcessCompatibility
{
    internal static void SetArguments(ProcessStartInfo startInfo, IEnumerable<string> arguments)
    {
#if NETSTANDARD2_0
        startInfo.Arguments = string.Join(" ", arguments.Select(QuoteArgument));
#else
        foreach (string argument in arguments) startInfo.ArgumentList.Add(argument);
#endif
    }

    internal static Task WaitForExitAsync(Process process)
    {
#if NETSTANDARD2_0
        return WaitForLegacyExitAsync(process);
#else
        return process.WaitForExitAsync(CancellationToken.None);
#endif
    }

    internal static void TryTerminateProcessTree(Process process)
    {
        try
        {
            if (process.HasExited) return;
#if NETSTANDARD2_0
            // A netstandard asset can also run on a runtime with the modern API.
            var killTree = typeof(Process).GetMethod("Kill", new[] { typeof(bool) });
            if (killTree is not null)
            {
                killTree.Invoke(process, new object[] { true });
                return;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var visited = new HashSet<int>();
                KillLegacyWindowsTree(process.Id, visited);
            }
            else
            {
                // Same legacy Unix limitation as the former dependency's polyfill.
                process.Kill();
            }
#else
            process.Kill(entireProcessTree: true);
#endif
        }
        catch (Exception)
        {
            // Tree discovery/permissions can fail. Still attempt the owned child.
            try { if (!process.HasExited) process.Kill(); }
            catch (Exception) { }
        }
    }

#if NETSTANDARD2_0
    private static async Task WaitForLegacyExitAsync(Process process)
    {
        var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler handler = (_, _) => exited.TrySetResult(true);
        process.Exited += handler;
        try
        {
            process.EnableRaisingEvents = true;
            // Subscribe before this check: exit between checking and subscription
            // must never strand the completion task.
            if (process.HasExited) exited.TrySetResult(true);
            await exited.Task.ConfigureAwait(false);
        }
        finally
        {
            process.Exited -= handler;
        }
    }

    private static void KillLegacyWindowsTree(int processId, HashSet<int> visited)
    {
        if (!visited.Add(processId)) return;
        var descendants = new List<int>();
        using (var searcher = new ManagementObjectSearcher(
            "SELECT ProcessID FROM Win32_Process WHERE ParentProcessID=" +
            processId.ToString(System.Globalization.CultureInfo.InvariantCulture)))
        using (ManagementObjectCollection results = searcher.Get())
        {
            foreach (ManagementObject child in results)
            {
                using (child) descendants.Add(Convert.ToInt32(child["ProcessID"], System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        try
        {
            using (Process parent = Process.GetProcessById(processId))
                if (!parent.HasExited) parent.Kill();
        }
        catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException || exception is Win32Exception)
        {
            // A process can exit between discovery and termination.
        }
        foreach (int childId in descendants)
        {
            try { KillLegacyWindowsTree(childId, visited); }
            catch (Exception) { /* Best effort per branch; continue other descendants. */ }
        }
    }

    // Adapted from .NET Foundation PasteArguments (MIT); see THIRD-PARTY-NOTICES.
    // This is native argument serialization, never shell quoting.
    private static string QuoteArgument(string argument)
    {
        if (argument.Length > 0 && argument.All(c => !char.IsWhiteSpace(c) && c != '"'))
            return argument;
        var buffer = new StringBuilder();
        buffer.Append('"');
        for (int index = 0; index < argument.Length;)
        {
            char value = argument[index++];
            if (value == '\\')
            {
                int count = 1;
                while (index < argument.Length && argument[index] == '\\') { index++; count++; }
                if (index == argument.Length) buffer.Append('\\', count * 2);
                else if (argument[index] == '"') { buffer.Append('\\', count * 2 + 1).Append('"'); index++; }
                else buffer.Append('\\', count);
            }
            else if (value == '"') buffer.Append('\\').Append('"');
            else buffer.Append(value);
        }
        return buffer.Append('"').ToString();
    }
#endif
}
