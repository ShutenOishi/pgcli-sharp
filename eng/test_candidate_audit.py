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

    def test_candidate_keeps_publication_disabled_and_preserves_candidate_history(self):
        candidate = json.loads((audit.ROOT / ".github/release-candidate.json").read_text())
        preserved_phase3 = json.loads((audit.ROOT / ".github/nuget-release.json").read_text())
        self.assertIs(candidate["publication_enabled"], False)
        self.assertIs(preserved_phase3["publication_enabled"], False)
        self.assertNotEqual(candidate["version"], preserved_phase3["version"])

        # The current RC supersedes the immediately previous unpublished
        # candidate (alpha.2), while the Phase 3 manifest independently
        # preserves alpha.1 and its original source. These are distinct
        # provenance records and must not be collapsed into one identity.
        superseded = candidate["supersedes_candidate"]
        self.assertEqual("0.1.0-alpha.2", superseded["version"])
        self.assertEqual(
            "0b7a2bb2e4dc1c1ed016181b2598645a1e3a8d5d",
            superseded["source_commit"])
        self.assertEqual(37104736018, superseded["source_main_ci"])
        self.assertEqual("0.1.0-alpha.1", preserved_phase3["version"])
        self.assertEqual(
            "cccf8d9fbe1f2e1104676ab94a7863209c0220dd",
            preserved_phase3["release_source_commit"])
        self.assertNotEqual(
            superseded["source_commit"],
            preserved_phase3["release_source_commit"])

        history = candidate["selection_history"]
        self.assertTrue(any(
            item["version"] == superseded["version"]
            and item["source_commit"] == superseded["source_commit"]
            and item["source_main_ci"] == superseded["source_main_ci"]
            for item in history))
        self.assertTrue((audit.ROOT / candidate["notes_file"]).is_file())

    def test_package_rejects_wrong_provenance_license_and_missing_satellite(self):
        candidate = {"version": "0.1.0-alpha.2", "source_commit": "a" * 40, "package_release_notes": "notes"}
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            names = ("LICENSE", "README.md", "README.ja.md", "docs/licensing.md",
                     "THIRD-PARTY-NOTICES.md", "docs/third-party/dotnet-notices.txt")
            for name in names:
                file = folder / name
                file.parent.mkdir(parents=True, exist_ok=True)
                file.write_bytes(b"source")
            entries = {name: b"source" for name in names}
            entries.update({'_rels/.rels': b'metadata', '[Content_Types].xml': b'metadata',
                            'package/services/metadata/core-properties/test.psmdcp': b'metadata'})
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
            candidate['dependency_profile'] = 'process-compat-v1'
            process_xml = xml.replace('id="CliWrap" version="3.10.5"', 'id="System.Management" version="10.0.10"')
            check(entries, process_xml)
            with self.assertRaises(AssertionError):
                check(entries, xml)
            candidate['dependency_profile'] = 'unknown'
            with self.assertRaisesRegex(AssertionError, 'Unknown candidate dependency profile'):
                check(entries, process_xml)
            del candidate['dependency_profile']
            with self.assertRaises(AssertionError):
                check(entries, process_xml)
            for invalid_xml in (xml.replace(candidate['source_commit'], "b" * 40), xml.replace('>MIT<', '>GPL-3.0-only<')):
                with self.assertRaises(AssertionError):
                    check(entries, invalid_xml)
            missing = copy.copy(entries)
            del missing['lib/net8.0/ja/PgCliSharp.resources.dll']
            with self.assertRaises(AssertionError):
                check(missing, xml)
            extra = copy.copy(entries)
            extra['runtime.json'] = b'unexpected third-party payload'
            with self.assertRaisesRegex(AssertionError, 'Unexpected shipped file'):
                check(extra, xml)

    def test_symbols_reject_extra_and_duplicate_payload(self):
        candidate = {"package_id": "PgCliSharp", "version": "0.1.0-alpha.2", "source_commit": "a" * 40}
        with tempfile.TemporaryDirectory() as directory:
            package = Path(directory) / "test.snupkg"
            entries = [(f"lib/{tfm}/PgCliSharp.pdb", b"pdb") for tfm in audit.FRAMEWORKS]
            entries += [("PgCliSharp.nuspec", '<package><metadata><id>PgCliSharp</id><version>0.1.0-alpha.2</version><repository commit="' + candidate["source_commit"] + '" /></metadata></package>'),
                        ("_rels/.rels", b"metadata"), ("[Content_Types].xml", b"metadata"),
                        ("package/services/metadata/core-properties/test.psmdcp", b"metadata")]

            def check(files):
                with zipfile.ZipFile(package, "w") as archive:
                    for name, content in files:
                        archive.writestr(name, content)
                audit.audit_symbols(package, candidate)

            check(entries)
            for extra in ("runtime.json", "lib/net8.0/Dependency.dll"):
                with self.assertRaisesRegex(AssertionError, "Unexpected symbol payload"):
                    check(entries + [(extra, b"unexpected")])
            with self.assertWarns(UserWarning):
                with self.assertRaisesRegex(AssertionError, "Duplicate symbol archive entries"):
                    check(entries + [("_rels/.rels", b"duplicate")])

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
