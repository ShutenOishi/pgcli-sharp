import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import audit_development_dependencies as audit


class DevelopmentDependencyTests(unittest.TestCase):
    def test_rejects_retained_cliwrap_runtime(self):
        records = [{"package": name, "runtime_frameworks": ["legacy"]} for name in audit.EXPECTED]
        records.append({"package": "CliWrap/3.10.5", "runtime_frameworks": ["legacy"]})
        with patch.object(audit, "audit_dependencies", return_value=records):
            with self.assertRaises(AssertionError):
                audit.audit("consumer", "unused.json")

    def test_rejects_execution_dependency_in_modern_asset(self):
        legacy = [{"package": name, "runtime_frameworks": ["legacy"]} for name in audit.EXPECTED]
        modern = [{"package": "System.Management/10.0.10", "runtime_frameworks": ["modern"]}]
        with patch.object(audit, "audit_dependencies", side_effect=[legacy, modern]):
            with self.assertRaises(AssertionError):
                audit.audit("consumer", "unused.json")

    def test_ignores_reference_only_packages_and_marks_nonpublication(self):
        legacy = [{"package": name, "runtime_frameworks": ["legacy"]} for name in audit.EXPECTED]
        reference = {"package": "NETStandard.Library/2.0.3", "runtime_frameworks": []}
        with tempfile.TemporaryDirectory() as folder:
            output = Path(folder) / "audit.json"
            with patch.object(audit, "audit_dependencies", side_effect=[legacy + [reference], [], []]):
                audit.audit("consumer", output)
            self.assertIs(json.loads(output.read_text())["publication_candidate"], False)


if __name__ == "__main__":
    unittest.main()
