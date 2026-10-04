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
if (args.Length == 2 && args[0] == "gate")
{
    // The parent releases this owned fixture only after startup has returned.
    while (!File.Exists(args[1])) Thread.Sleep(10);
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
using Stream input = Console.OpenStandardInput();
using Stream output = Console.OpenStandardOutput();
Record("copy-enter");
input.CopyTo(output);
Record("eof");
Console.Error.WriteLine("eof");
output.Flush();
Console.Error.WriteLine("drained");
Record("drained");
