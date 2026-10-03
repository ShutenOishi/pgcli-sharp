using System.IO.Compression;
using System.Reflection.Metadata;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("Expected source SHA and snupkg path.");
string expected = "https://raw.githubusercontent.com/ShutenOishi/pgcli-sharp/" + args[0] + "/*";
using ZipArchive archive = ZipFile.OpenRead(args[1]);
int count = 0;
foreach (ZipArchiveEntry entry in archive.Entries.Where(entry => entry.FullName.EndsWith(".pdb", StringComparison.Ordinal)))
{
    using var bytes = new MemoryStream();
    using (Stream input = entry.Open()) input.CopyTo(bytes);
    bytes.Position = 0;
    using MetadataReaderProvider provider = MetadataReaderProvider.FromPortablePdbStream(bytes);
    MetadataReader reader = provider.GetMetadataReader();
    var links = reader.CustomDebugInformation.Select(reader.GetCustomDebugInformation)
        .Where(item => reader.GetGuid(item.Kind) == new Guid("CC110556-A091-4D38-9FEC-25AB9A351A6A")).ToArray();
    if (links.Length != 1) throw new InvalidDataException("Missing or duplicate SourceLink: " + entry.FullName);
    using JsonDocument json = JsonDocument.Parse(reader.GetBlobBytes(links[0].Value));
    var mappings = json.RootElement.GetProperty("documents").EnumerateObject().ToArray();
    if (mappings.Length != 1 || mappings[0].Value.GetString() != expected)
        throw new InvalidDataException("SourceLink does not reference selected source: " + entry.FullName);
    Console.WriteLine("Verified SourceLink: " + entry.FullName);
    count++;
}
if (count != 3) throw new InvalidDataException("Expected three library PDBs.");
