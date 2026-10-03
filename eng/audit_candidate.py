"""Build and inspect an unpublished candidate from an explicit immutable source."""
import argparse
import hashlib
import html
import json
import os
import re
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET
import zipfile
from validate_candidate import validate

ROOT = Path(__file__).resolve().parent.parent
FRAMEWORKS = ("netstandard2.0", "net8.0", "net10.0")
LEGACY_LICENSES = {
    "NETStandard.Library/2.0.3": ("LICENSE.TXT", "fc95de1436a321aadbfafd21f8a8506a2f4678bfc003bd806d6b59d3088efadc", "MIT"),
    "Microsoft.NETCore.Platforms/1.1.0": ("dotnet_library_license.txt", "f1db688d8481c91a452fabcea5060a23da9ea5088329b58c478a040e2e426297", "LicenseRef-Microsoft-DotNet-Library"),
}


def msbuild_value(value):
    return value.replace("%", "%25").replace(",", "%2C").replace(";", "%3B")


def run(*args, cwd=None):
    subprocess.run(args, cwd=cwd, check=True)


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def metadata(data):
    root = ET.fromstring(data)
    for node in root.iter():
        node.tag = node.tag.rsplit("}", 1)[-1]
    return root.find("metadata")


def audit_package(package, source, candidate):
    with zipfile.ZipFile(package) as archive:
        meta = metadata(archive.read("PgCliSharp.nuspec"))
        assert meta.findtext("id") == "PgCliSharp"
        assert meta.findtext("version") == candidate["version"]
        assert meta.findtext("releaseNotes") == candidate["package_release_notes"]
        assert meta.find("repository").get("commit") == candidate["source_commit"]
        assert meta.find("license").get("type") == "expression"
        assert meta.findtext("license") == "MIT"
        docs = ("LICENSE", "README.md", "README.ja.md", "docs/licensing.md",
                "THIRD-PARTY-NOTICES.md", "docs/third-party/dotnet-notices.txt")
        for name in docs:
            assert archive.read(name) == (source / name).read_bytes(), name
        actual_libs = {name for name in archive.namelist() if name.startswith("lib/")}
        expected_libs = {f"lib/{tfm}/PgCliSharp.{ext}" for tfm in FRAMEWORKS for ext in ("dll", "xml")}
        expected_libs.update(f"lib/{tfm}/ja/PgCliSharp.resources.dll" for tfm in FRAMEWORKS)
        assert actual_libs == expected_libs, "Unexpected/missing library payload"
        payload = archive.namelist()
        assert len(payload) == len(set(payload)), "Duplicate archive entries"
        core = [name for name in payload if name.startswith("package/services/metadata/core-properties/") and name.endswith(".psmdcp")]
        assert len(core) == 1
        assert set(payload) == expected_libs | set(docs) | {
            "PgCliSharp.nuspec", "_rels/.rels", "[Content_Types].xml", core[0]}, "Unexpected shipped file"
        groups = meta.findall("dependencies/group")
        assert len(groups) == 3
        assert {group.get("targetFramework") for group in groups} == {".NETStandard2.0", "net8.0", "net10.0"}
        for group in groups:
            deps = [(node.get("id"), node.get("version")) for node in group.findall("dependency")]
            assert deps == ([("CliWrap", "3.10.5")] if group.get("targetFramework") == ".NETStandard2.0" else [])
        return {"file": package.name, "sha256": sha256(package.read_bytes()), "payload": archive.namelist()}


def audit_dependencies(assets_path):
    assets = json.loads(assets_path.read_text(encoding="utf-8"))
    records = []
    for identity, item in sorted(assets["libraries"].items()):
        if item["type"] != "package":
            continue
        package = next((Path(folder) / item["path"] for folder in assets["packageFolders"]
                        if (Path(folder) / item["path"]).is_dir()), None)
        assert package is not None, identity
        nuspec = next(package.glob("*.nuspec"))
        meta = metadata(nuspec.read_bytes())
        license_node = meta.find("license")
        evidence = "nuspec license expression"
        if license_node is None:
            assert identity in LEGACY_LICENSES, f"Unreviewed legacy license: {identity}"
            filename, expected_hash, expression = LEGACY_LICENSES[identity]
            assert sha256((package / filename).read_bytes()) == expected_hash, identity
            evidence = "exact packaged text: " + filename
        else:
            assert license_node.get("type") == "expression", identity
            expression = license_node.text
            assert expression == "MIT", f"Unreviewed license: {identity}: {expression}"
        frameworks = [tfm for tfm, target in assets["targets"].items() if identity in target]
        runtime_frameworks = [tfm for tfm in frameworks if any(
            not path.endswith("/_._") for key in ("runtime", "native", "runtimeTargets")
            for path in assets["targets"][tfm][identity].get(key, {}))]
        notices = []
        for file in sorted(package.iterdir()):
            normalized = file.name.lower().replace("-", "").replace("_", "")
            if file.is_file() and any(word in normalized for word in ("license", "notice")):
                notices.append({"file": file.name, "sha256": sha256(file.read_bytes()),
                                "text": file.read_text(encoding="utf-8-sig")})
        assert expression == "MIT" or not runtime_frameworks, "Non-MIT runtime dependency requires review: " + identity
        if identity == "Microsoft.NETCore.Platforms/1.1.0":
            assert all(path.endswith("/_._") for tfm in frameworks
                       for key in ("compile", "runtime", "native", "runtimeTargets")
                       for path in assets["targets"][tfm][identity].get(key, {})), "Legacy reference binary requires review"
        records.append({"package": identity, "license": expression, "license_evidence": evidence,
                        "copyright": meta.findtext("copyright"), "runtime_frameworks": runtime_frameworks,
                        "frameworks": frameworks, "nuget_sha512": item["sha512"], "notices": notices})
    return records


def audit_symbols(package, candidate):
    with zipfile.ZipFile(package) as archive:
        meta = metadata(archive.read("PgCliSharp.nuspec"))
        assert meta.findtext("id") == candidate["package_id"]
        assert meta.findtext("version") == candidate["version"]
        assert meta.find("repository").get("commit") == candidate["source_commit"]
        pdbs = [name for name in archive.namelist() if name.endswith(".pdb")]
        assert len(pdbs) == 3
        assert set(pdbs) == {f"lib/{tfm}/PgCliSharp.pdb" for tfm in FRAMEWORKS}


def audit_consumer(dotnet, output, candidate, common):
    """Compile all assets and execute offline wrapper code, never PostgreSQL."""
    with tempfile.TemporaryDirectory(prefix="pgclisharp-candidate-consumer-") as directory:
        folder = Path(directory)
        project = folder / "Consumer.csproj"
        frameworks = list(FRAMEWORKS) + (["net48"] if os.name == "nt" else [])
        reference = '<PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies.net48" Version="[1.0.3]" PrivateAssets="all" />' if os.name == "nt" else ''
        project.write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFrameworks>' +
            ';'.join(frameworks) + '</TargetFrameworks><LangVersion>latest</LangVersion>' +
            '<OutputType Condition="\'$(TargetFramework)\' != \'netstandard2.0\'">Exe</OutputType>' +
            '<TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>' +
            '<PackageReference Include="PgCliSharp" Version="[' + html.escape(candidate['version']) +
            ']" />' + reference + '</ItemGroup></Project>', encoding="utf-8")
        (folder / "Smoke.cs").write_text('''using System;
using System.Reflection;
using System.Runtime.Versioning;
using PgCliSharp;
internal static class Smoke {
    public static void Main() {
        PgCommand command = new PgDump("/not-executed/pg_dump", PostgreSqlMajorVersion.V18)
            .CreateCommand(options => options.Database = "appdb", PgDumpOutput.ToFile("appdb.dump"));
        if (command.ExecutablePath != "/not-executed/pg_dump") throw new Exception("Wrong offline command.");
#if NET48 || NETSTANDARD2_0
        string expected = ".NETStandard,Version=v2.0";
#elif NET8_0
        string expected = ".NETCoreApp,Version=v8.0";
#else
        string expected = ".NETCoreApp,Version=v10.0";
#endif
        string actual = typeof(PgDump).Assembly.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
        if (actual != expected) throw new Exception("Wrong package asset: " + actual);
        Console.WriteLine("Verified offline runtime consumer: " + actual);
    }
}
''', encoding="utf-8")
        run(dotnet, "restore", str(project), "--source", str(output), "--source",
            "https://api.nuget.org/v3/index.json", "--packages", str(folder / "packages"), *common, cwd=output)
        assets_path = folder / "obj/project.assets.json"
        assets = json.loads(assets_path.read_text(encoding="utf-8"))
        for tfm in FRAMEWORKS:
            target = assets["targets"][".NETStandard,Version=v2.0" if tfm == "netstandard2.0" else tfm]
            assert "PgCliSharp/" + candidate['version'] in target
        run(dotnet, "build", str(project), "-c", "Release", "--no-restore", *common, cwd=output)
        runtime_frameworks = [tfm for tfm in frameworks if tfm != "netstandard2.0"]
        for tfm in runtime_frameworks:
            run(dotnet, "run", "--project", str(project), "-c", "Release", "-f", tfm,
                "--no-build", "--no-restore", cwd=output)
        reference_tools = []
        for identity in list(assets["libraries"]):
            if identity.startswith("Microsoft.NETFramework.ReferenceAssemblies"):
                assert identity in ("Microsoft.NETFramework.ReferenceAssemblies/1.0.3",
                                    "Microsoft.NETFramework.ReferenceAssemblies.net48/1.0.3")
                assert all(not target[identity].get(key) for target in assets['targets'].values()
                           if identity in target for key in ('runtime', 'native', 'runtimeTargets'))
                item = assets['libraries'].pop(identity)
                path = folder / 'packages' / item['path']
                nuspec = next(path.glob('*.nuspec'))
                meta = metadata(nuspec.read_bytes())
                reference_tools.append({'package': identity, 'role': 'consumer build reference assemblies only',
                    'nuget_sha512': item['sha512'], 'license_url': meta.findtext('licenseUrl'),
                    'nuspec_sha256': sha256(nuspec.read_bytes()), 'redistributed': False})
        # Exclude the package under audit; it is already verified independently.
        del assets["libraries"]["PgCliSharp/" + candidate['version']]
        assets_path.write_text(json.dumps(assets), encoding="utf-8")
        return {"frameworks": frameworks, "runtime_frameworks": runtime_frameworks, "postgresql_execution": False,
                "reference_build_tools": reference_tools,
                "dependencies": audit_dependencies(assets_path)}


def main():
    if not __debug__:
        raise RuntimeError("Candidate audit requires Python assertions; do not use -O.")
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--dotnet", default="dotnet")
    args = parser.parse_args()
    source, output = args.source.resolve(), args.output.resolve()
    candidate = json.loads((ROOT / ".github/release-candidate.json").read_text(encoding="utf-8"))
    validate(candidate)
    actual = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=source, text=True).strip()
    assert actual == candidate["source_commit"], "Wrong candidate source"
    run("git", "diff", "--exit-code", "HEAD", cwd=source)
    untracked = subprocess.check_output(["git", "ls-files", "--others", "--exclude-standard"], cwd=source, text=True).splitlines()
    assert set(untracked) <= {"src/PgCliSharp/packages.lock.json"}, "Untracked candidate source files"
    output.mkdir(parents=True, exist_ok=True)
    (output / "global.json").write_text(json.dumps({"sdk": {"version": candidate["sdk_version"],
        "rollForward": "disable", "allowPrerelease": False}}) + "\n", encoding="utf-8")
    sdk = subprocess.check_output([args.dotnet, "--version"], cwd=output, text=True).strip()
    assert sdk == candidate["sdk_version"], "Wrong candidate SDK"
    project = source / "src/PgCliSharp/PgCliSharp.csproj"
    common = ("-m:1", "-nodeReuse:false", "-p:UseSharedCompilation=false")
    pinned_lock = ROOT / "eng/candidate.packages.lock.json"
    (project.parent / "packages.lock.json").write_bytes(pinned_lock.read_bytes())
    run(args.dotnet, "restore", str(project), "--locked-mode", *common, cwd=output)
    run(args.dotnet, "pack", str(project), "-c", "Release", "--no-restore", "-o", str(output),
        "-p:Version=" + candidate["version"],
        "-p:PackageReleaseNotes=" + msbuild_value(candidate["package_release_notes"]), *common, cwd=output)
    package = output / f"PgCliSharp.{candidate['version']}.nupkg"
    symbols = output / f"PgCliSharp.{candidate['version']}.snupkg"
    result = audit_package(package, source, candidate)
    audit_symbols(symbols, candidate)
    dependencies = audit_dependencies(project.parent / "obj/project.assets.json")
    consumer = audit_consumer(args.dotnet, output, candidate, common)
    reviewed = {name + '/' + version for name, version in re.findall(
        r'^\| ([A-Za-z0-9.]+) \| ([0-9.]+) \| MIT \|',
        (source / 'THIRD-PARTY-NOTICES.md').read_text(encoding='utf-8'), re.M)}
    runtime = {record['package'] for record in consumer['dependencies'] if record['runtime_frameworks']}
    assert runtime == reviewed, 'Runtime dependency notice inventory mismatch'
    assert sha256((source / 'docs/third-party/dotnet-notices.txt').read_bytes()) == '6d15e10a101c6bfff2ab4429ed061bf76c456fc4b23ad6b03e0d0f8377148a21'
    run(args.dotnet, "run", "--project", str(ROOT / "eng/SourceLinkAudit"), "-c", "Release",
        "-p:UseSharedCompilation=false", "--", candidate["source_commit"], str(symbols), str(package), cwd=output)
    audit_tool = ROOT / 'eng/SourceLinkAudit/bin/Release/net10.0/SourceLinkAudit.dll'
    invalid = subprocess.run([args.dotnet, str(audit_tool), '0' * 40, str(symbols), str(package)],
                             cwd=output, capture_output=True, text=True)
    assert invalid.returncode != 0 and 'SourceLink does not reference' in invalid.stderr, 'Wrong-SHA negative test did not fail'
    with tempfile.TemporaryDirectory(prefix='pgcli-symbol-pair-') as directory:
        mismatch = Path(directory) / 'mismatch.nupkg'
        with zipfile.ZipFile(package) as original, zipfile.ZipFile(mismatch, 'w') as changed:
            for entry in original.infolist():
                data = original.read('lib/netstandard2.0/PgCliSharp.dll' if entry.filename == 'lib/net10.0/PgCliSharp.dll' else entry.filename)
                changed.writestr(entry, data)
        invalid = subprocess.run([args.dotnet, str(audit_tool), candidate['source_commit'], str(symbols), str(mismatch)],
                                 cwd=output, capture_output=True, text=True)
        assert invalid.returncode != 0 and 'PDB does not match' in invalid.stderr, 'Mismatched-DLL negative test did not fail'
    lock = project.parent / "packages.lock.json"
    (output / "packages.lock.json").write_bytes(lock.read_bytes())
    (output / "release-notes.md").write_bytes((ROOT / candidate["notes_file"]).read_bytes())
    result.update({"candidate": candidate, "symbols_sha256": sha256(symbols.read_bytes()),
                   "lock_sha256": sha256(lock.read_bytes()), "dependencies": dependencies, "consumer": consumer,
                   "sdk": sdk,
                   "workflow_run_id": os.environ.get("GITHUB_RUN_ID"),
                   "schema_version": 2, "technical_audit_passed": True, "publication_performed": False, "publication_ready": False,
                   "source_link": {"verified_portable_pdbs": 3, "dll_pdb_pairs_verified": True,
                       "wrong_sha_rejected": True, "mismatched_dll_rejected": True, "url":
                       "https://raw.githubusercontent.com/ShutenOishi/pgcli-sharp/" + candidate["source_commit"] + "/*"},
                   "distribution_scope_review": {"complete": True, "dependency_binaries_embedded": False,
                       "legacy_reference_package_embedded": False, "downstream_distribution_cleared": False},
                   "remaining_license_review": [],
                   "remaining_publication_gates": ["explicit approval and reviewed enablement", "nuget.org Trusted Publishing policy verification"],
                   "control_commit": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
                   "os": os.name})
    (output / "candidate-audit.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("Candidate package, dependency licenses and SourceLink verified:", package.name)


if __name__ == "__main__":
    main()
