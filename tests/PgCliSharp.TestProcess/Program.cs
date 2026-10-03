// Minimal binary pipe fixture. Never invokes a shell or PostgreSQL.
Console.Error.WriteLine("ready");
Console.Error.Flush();
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
input.CopyTo(output);
Console.Error.WriteLine("eof");
output.Flush();
Console.Error.WriteLine("drained");
