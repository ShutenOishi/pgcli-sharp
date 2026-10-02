import contextlib
import io
import json
import os
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from record_real_postgresql_evidence import record


class EvidenceTests(unittest.TestCase):
    def fixture(self, major=18, missing=False, outcome="Passed", wrong_version=False):
        manifest = json.loads(Path("eng/postgresql-source-versions.json").read_text())
        pinned = next(row for row in manifest["versions"] if row["major"] == major)
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            binary = root / str(major) / "bin"
            binary.mkdir(parents=True)
            (binary.parent / "BUILD-SOURCE.json").write_text(json.dumps(pinned))
            for tool in ["postgres", "psql", "pg_dump", "pg_restore", "pgbench", "initdb", "pg_ctl", "pg_resetwal", "pg_rewind", "pg_basebackup", "pg_upgrade", "pg_checksums"]:
                (binary / tool).write_bytes(b"synthetic test fixture, not a real executable")
            old = root / "16" / "bin"
            old.mkdir(parents=True, exist_ok=True)
            old_pin = next(row for row in manifest["versions"] if row["major"] == 16)
            (old.parent / "BUILD-SOURCE.json").write_text(json.dumps(old_pin))
            results = root / "results"
            results.mkdir()
            names = ["RepresentativeRealPostgreSql_BackupRestorePsqlSessionAndPgBench", "OwnedDisposableCluster_InitializesChecksumsDryRunAndServerLifecycle"]
            if major >= 13:
                names.append("DivergentOwnedCluster_RewindsAndPreservesSourceRows")
            if major == 18:
                names.append("OwnedClusters_CopyUpgrade16To18PreservesRows")
            if missing:
                names.pop()
            (results / "test.trx").write_text('<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">' + ''.join(
                f'<UnitTestResult testName="PgCliSharp.Tests.{name}" outcome="{outcome}" />' for name in names) + '</TestRun>')

            def run(arguments, **_):
                if "--version" not in arguments:
                    return pinned["version"] + "|" + str(major * 10000 + int(pinned["version"].split(".")[1]))
                version = old_pin["version"] if Path(arguments[0]).parent == old else pinned["version"]
                if wrong_version:
                    version = "18.0"
                return Path(arguments[0]).name + " (PostgreSQL) " + version

            environment = {
                "PGCLI_REAL_PG_MAJOR": str(major), "PGCLI_REAL_PG_BIN": str(binary),
                "PGCLI_REAL_PG_UPGRADE_FROM_BIN": str(old), "PGCLI_REAL_PG_HOST": str(root),
                "PGCLI_REAL_PG_PORT": "5432", "PGCLI_REAL_PG_USER": "fixture",
            }
            with patch.dict(os.environ, environment), patch("subprocess.check_output", side_effect=run), contextlib.redirect_stdout(io.StringIO()):
                status = record(str(results))
            return status, json.loads((results / "real-postgresql-evidence.json").read_text())

    def test_pinned_manifest_covers_exact_supported_majors(self):
        rows = json.loads(Path("eng/postgresql-source-versions.json").read_text())["versions"]
        self.assertEqual(list(range(10, 19)), [row["major"] for row in rows])
        for row in rows:
            self.assertTrue(row["version"].startswith(str(row["major"]) + "."))
            self.assertRegex(row["sha256"], r"^[0-9a-f]{64}$")

    def test_actual_expected_scenarios_pass(self):
        status, evidence = self.fixture()
        self.assertEqual(0, status)
        self.assertEqual("passed", evidence["result"])
        self.assertEqual(4, len(evidence["scenarios"]))

    def test_legacy_exclusions_are_not_reported_as_passes(self):
        status, evidence = self.fixture(major=10)
        self.assertEqual(0, status)
        self.assertEqual(2, len(evidence["scenarios"]))
        self.assertIn("checksum check/disable/enable", evidence["exclusions"])
        self.assertIn("divergent rewind", evidence["exclusions"])

    def test_missing_test_fails(self):
        self.assertEqual(1, self.fixture(missing=True)[0])

    def test_failed_test_fails(self):
        self.assertEqual(1, self.fixture(outcome="Failed")[0])

    def test_wrong_binary_version_fails(self):
        self.assertEqual(1, self.fixture(wrong_version=True)[0])


if __name__ == "__main__":
    unittest.main()
