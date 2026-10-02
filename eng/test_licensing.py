"""Repository licensing gates; package contents are verified separately in CI."""
import json
from pathlib import Path
import unittest
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parent.parent


class LicensingTests(unittest.TestCase):
    def test_mit_metadata_and_packed_notices(self):
        project = ET.parse(ROOT / "src/PgCliSharp/PgCliSharp.csproj").getroot()
        self.assertEqual(project.findtext(".//PackageLicenseExpression"), "MIT")
        self.assertIsNone(project.find(".//PackageLicenseFile"))
        packed = {node.attrib["Include"] for node in project.findall(".//None")
                  if node.attrib.get("Pack") == "true"}
        self.assertIn("../../LICENSE", packed)
        self.assertIn("../../docs/licensing.md", packed)

    def test_license_identity_and_obligations(self):
        license_text = (ROOT / "LICENSE").read_text(encoding="utf-8")
        self.assertTrue(license_text.startswith("MIT License\n"))
        self.assertIn("Copyright (c) 2026 ShutenOishi", license_text)
        self.assertIn("The above copyright notice and this permission notice", license_text)
        self.assertIn('THE SOFTWARE IS PROVIDED "AS IS"', license_text)

    def test_publication_is_still_disabled_and_preserved(self):
        for filename in ("nuget-release.json", "phase-release.json"):
            manifest = json.loads((ROOT / ".github" / filename).read_text(encoding="utf-8"))
            self.assertIs(manifest["publication_enabled"], False)
            self.assertEqual(manifest["release_source_commit"],
                             "cccf8d9fbe1f2e1104676ab94a7863209c0220dd")

    def test_guides_are_bilingual_and_readmes_have_no_control_characters(self):
        guide = (ROOT / "docs/licensing.md").read_text(encoding="utf-8")
        self.assertLess(guide.index("## English"), guide.index("## 日本語"))
        for filename in ("README.md", "README.ja.md"):
            text = (ROOT / filename).read_text(encoding="utf-8")
            self.assertIn("(LICENSE)", text)
            self.assertIn("docs/licensing.md", text)
            self.assertFalse(any(ord(char) < 32 and char not in "\n\r\t" for char in text))


if __name__ == "__main__":
    unittest.main()
