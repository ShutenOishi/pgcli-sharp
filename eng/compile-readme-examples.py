"""Compile every EN/JA README C# block; never execute its database operations."""
import html
import os
from pathlib import Path
import re
import subprocess
import tempfile


def main():
    root = Path(__file__).resolve().parent.parent
    blocks = []
    for name in ("README.md", "README.ja.md"):
        snippets = re.findall(r"```csharp\n(.*?)\n```", (root / name).read_text(), re.S)
        if not snippets:
            raise ValueError("Missing C# examples: " + name)
        for index, snippet in enumerate(snippets, 1):
            # Namespace imports become file-scoped imports; using declarations stay.
            snippet = re.sub(r"^using [\w.]+;\n", "", snippet, flags=re.M)
            blocks.append("static async Task Example" + str(len(blocks) + 1) + "() {\n" + snippet + "\n}")
            print("Compile-only: " + name + " C# block " + str(index), flush=True)
    frameworks = "net8.0;net10.0"
    legacy = ""
    if os.name == "nt":
        frameworks = "net48;" + frameworks
        legacy = '<PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies.net48" Version="1.0.3" PrivateAssets="All" />'
    with tempfile.TemporaryDirectory(prefix="pgclisharp-readme-") as directory:
        folder = Path(directory)
        project = folder / "ReadmeExamples.csproj"
        reference = html.escape(str(root / "src/PgCliSharp/PgCliSharp.csproj"), quote=True)
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>' + frameworks +
            '</TargetFrameworks><LangVersion>latest</LangVersion><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>' +
            '<IsPackable>false</IsPackable></PropertyGroup><ItemGroup><ProjectReference Include="' + reference + '" />' +
            legacy + '</ItemGroup></Project>')
        (folder / "Examples.cs").write_text("using System;\nusing System.IO;\nusing System.Text;\nusing System.Threading.Tasks;\nusing PgCliSharp;\nusing PgCliSharp.ServerApplications;\ninternal static class Examples {\n" + "\n".join(blocks) + "\n}")
        subprocess.run(["dotnet", "build", str(project), "--configuration", "Release", "--nologo"], cwd=root, check=True)


if __name__ == "__main__":
    main()
