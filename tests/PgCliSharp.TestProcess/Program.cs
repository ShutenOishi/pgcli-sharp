// Minimal binary pipe fixture. Never invokes a shell or PostgreSQL.
string? trace = Environment.GetEnvironmentVariable("PGCLI_TEST_TRACE_FILE");
void Record(string stage)
{
    if (trace is not null) File.AppendAllText(trace, $"{DateTime.UtcNow:O} {stage}\n");
}
Record("entry");
Console.Error.WriteLine("ready");
Console.Error.Flush();
Record("ready");
if (args.Length > 0 && args[0] == "arguments")
{
    foreach (string argument in args.Skip(1))
        Console.WriteLine(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(argument)));
    Console.WriteLine("environment:" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("PGCLI_TEST_VALUE") ?? string.Empty)));
    return;
}
if (args.Length == 1 && args[0] == "exit")
{
    Console.Error.WriteLine("fixture-exit");
    Environment.ExitCode = 17;
    return;
}
if (args.Length == 2 && (args[0] == "gate" || args[0] == "gate-exit"))
{
    // The parent releases this owned fixture only after startup has returned.
    while (!File.Exists(args[1])) Thread.Sleep(10);
    if (args[0] == "gate-exit")
    {
        Environment.ExitCode = 17;
        return;
    }
}
if (args.Length == 1 && args[0] == "produce")
{
    using Stream producer = Console.OpenStandardOutput();
    byte[] buffer = new byte[4096];
    while (true)
    {
        producer.Write(buffer, 0, buffer.Length);
        producer.Flush();
    }
}
if (args.Length == 1 && args[0] == "wait")
{
    // The test owns this child's lifetime. Do not read stdin or exit on a timer.
    Thread.Sleep(Timeout.Infinite);
    return;
}
if (args.Length == 1 && args[0] == "diagnostic-wait")
{
    var fixture = new StackReaderSmokeFixture();
    Record("diagnostic-ready");
    Thread.Sleep(Timeout.Infinite);
    GC.KeepAlive(fixture);
    return;
}
using Stream input = Console.OpenStandardInput();
using Stream output = Console.OpenStandardOutput();
Record("copy-enter");
input.CopyTo(output);
Record("eof");
Console.Error.WriteLine("eof");
output.Flush();
Console.Error.WriteLine("drained");
Record("drained");

// Kept alive in the owned smoke process to verify nested CLR4 field addresses.
internal sealed class StackReaderSmokeFixture
{
    internal SmokePromise Promise = new() { Marker = 1729, Awaiter = new() { Token = 37, Completed = true } };
}

internal struct SmokePromise
{
    internal int Marker;
    internal SmokeAwaiter Awaiter;
}

internal struct SmokeAwaiter
{
    internal short Token;
    internal bool Completed;
}
