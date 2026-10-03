"""Fail-closed regression coverage for unpublished candidate auditing."""
import copy
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import zipfile

import audit_candidate as audit


class CandidateAuditTests(unittest.TestCase):
    def test_msbuild_separator_escaping(self):
        self.assertEqual(audit.msbuild_value("a,b;c%"), "a%2Cb%3Bc%25")

    def test_candidate_keeps_publication_disabled_and_distinct_from_history(self):
        candidate = json.loads((audit.ROOT / ".github/release-candidate.json").read_text())
        preserved = json.loads((audit.ROOT / ".github/nuget-release.json").read_text())
        self.assertIs(candidate["publication_enabled"], False)
        self.assertIs(preserved["publication_enabled"], False)
        self.assertNotEqual(candidate["version"], preserved["version"])
        self.assertEqual(candidate["supersedes_candidate"]["source_commit"], preserved["release_source_commit"])
        self.assertTrue((audit.ROOT / candidate["notes_file"]).is_file())

    def test_package_rejects_wrong_provenance_license_and_missing_satellite(self):
        candidate = {"version": "0.1.0-alpha.2", "source_commit": "a" * 40, "package_release_notes": "notes"}
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            for name in ("LICENSE", "README.md", "README.ja.md", "docs/licensing.md"):
                file = folder / name
                file.parent.mkdir(exist_ok=True)
                file.write_bytes(b"source")
            entries = {name: b"source" for name in ("LICENSE", "README.md", "README.ja.md", "docs/licensing.md")}
            for tfm in audit.FRAMEWORKS:
                for name in ("PgCliSharp.dll", "PgCliSharp.xml", "ja/PgCliSharp.resources.dll"):
                    entries[f"lib/{tfm}/{name}"] = b"payload"
            xml = '<package><metadata><id>PgCliSharp</id><version>0.1.0-alpha.2</version><releaseNotes>notes</releaseNotes>' + \
                '<repository commit="' + candidate['source_commit'] + '" /><license type="expression">MIT</license>' + \
                '<dependencies><group targetFramework=".NETStandard2.0"><dependency id="CliWrap" version="3.10.5" /></group>' + \
                '<group targetFramework="net8.0" /><group targetFramework="net10.0" /></dependencies></metadata></package>'

            def check(files, metadata):
                package = folder / "test.nupkg"
                with zipfile.ZipFile(package, "w") as archive:
                    for name, content in files.items():
                        archive.writestr(name, content)
                    archive.writestr("PgCliSharp.nuspec", metadata)
                return audit.audit_package(package, folder, candidate)

            check(entries, xml)
            for invalid_xml in (xml.replace(candidate['source_commit'], "b" * 40), xml.replace('>MIT<', '>GPL-3.0-only<')):
                with self.assertRaises(AssertionError):
                    check(entries, invalid_xml)
            missing = copy.copy(entries)
            del missing['lib/net8.0/ja/PgCliSharp.resources.dll']
            with self.assertRaises(AssertionError):
                check(missing, xml)

    def test_unknown_legacy_license_fails(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            package = folder / "unknown/1.0.0"
            package.mkdir(parents=True)
            (package / "unknown.nuspec").write_text('<package><metadata><licenseUrl>https://example.invalid</licenseUrl></metadata></package>')
            assets = {"libraries": {"Unknown/1.0.0": {"type": "package", "path": "unknown/1.0.0"}},
                      "packageFolders": {str(folder): {}}, "targets": {}}
            file = folder / "assets.json"
            file.write_text(json.dumps(assets))
            with self.assertRaisesRegex(AssertionError, "Unreviewed legacy"):
                audit.audit_dependencies(file)

    def test_legacy_text_is_hashed_and_non_mit_runtime_assets_fail(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            package = folder / "legacy/1.0.0"
            package.mkdir(parents=True)
            (package / "legacy.nuspec").write_text('<package><metadata /></package>')
            license_file = package / "dotnet_library_license.txt"
            license_file.write_bytes(b"reviewed terms")
            (package / "ThirdPartyNotices.txt").write_text("retained notice")
            assets = {"libraries": {"Legacy/1.0.0": {"type": "package", "path": "legacy/1.0.0", "sha512": "hash"}},
                      "packageFolders": {str(folder): {}}, "targets": {"net8.0": {"Legacy/1.0.0": {}}}}
            file = folder / "assets.json"
            file.write_text(json.dumps(assets))
            with patch.dict(audit.LEGACY_LICENSES, {"Legacy/1.0.0":
                    (license_file.name, audit.sha256(b"reviewed terms"), "LicenseRef-Test")}):
                result = audit.audit_dependencies(file)
                self.assertEqual(len(result[0]["notices"]), 2)
                license_file.write_bytes(b"changed terms")
                with self.assertRaises(AssertionError):
                    audit.audit_dependencies(file)
                license_file.write_bytes(b"reviewed terms")
                assets['targets']['net8.0']['Legacy/1.0.0']['runtime'] = {'lib/net8.0/Legacy.dll': {}}
                file.write_text(json.dumps(assets))
                with self.assertRaisesRegex(AssertionError, "Non-MIT runtime"):
                    audit.audit_dependencies(file)


if __name__ == "__main__":
    unittest.main()
