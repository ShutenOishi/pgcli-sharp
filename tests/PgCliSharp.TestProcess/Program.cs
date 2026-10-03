// Minimal binary pipe fixture. Never invokes a shell or PostgreSQL.
Console.Error.WriteLine("ready");
using Stream input = Console.OpenStandardInput();
using Stream output = Console.OpenStandardOutput();
input.CopyTo(output);
Console.Error.WriteLine("eof");
output.Flush();
Console.Error.WriteLine("drained");
