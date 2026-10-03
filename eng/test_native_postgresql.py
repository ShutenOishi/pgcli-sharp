import contextlib
import io
import json
import os
import platform
import tempfile
from pathlib import Path
import unittest
from unittest.mock import patch

from native_postgresql import SCENARIO, check_trx, collect, pin


class NativeEvidenceTests(unittest.TestCase):
    def fixture(self, invalid=None):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            binary = root / 'prefix/bin'
            binary.mkdir(parents=True)
            row = pin()
            build = dict(row, system=platform.system())
            if invalid == 'source':
                build['sha256'] = '0' * 64
            (binary.parent / 'BUILD-SOURCE.json').write_text(json.dumps(build))
            for name in ('postgres', 'initdb', 'pg_ctl', 'createdb', 'psql', 'pg_dump', 'pg_restore', 'pgbench'):
                (binary / name).write_bytes(b'synthetic, not a native executable')
            owner = {'root': str(root), 'binary': str(binary), 'run': '123'}
            if invalid == 'owner':
                owner['run'] = '456'
            (root / 'OWNER.json').write_text(json.dumps(owner))
            evidence = root / 'results'
            evidence.mkdir()
            (evidence / 'results.trx').write_text(f'<TestRun><UnitTestResult testName="{SCENARIO}" outcome="Passed" /></TestRun>')
            env = {'PGCLI_REAL_PG_BIN': str(binary), 'PGCLI_REAL_PG_ROOT': str(root), 'GITHUB_RUN_ID': '123',
                   'PGCLI_REAL_PG_HOST': str(root), 'PGCLI_REAL_PG_PORT': '5432', 'PGCLI_REAL_PG_USER': 'fixture'}
            def fake(*args):
                if '--version' in args:
                    return 'fixture (PostgreSQL) ' + ('18.0' if invalid == 'cli' else row['version'])
                data = root / ('other' if invalid == 'data' else 'data')
                version = 180000 if invalid == 'server' else 180000 + int(row['version'].split('.')[1])
                return str(version) + '|' + str(data)
            with patch.dict(os.environ, env), patch('native_postgresql.run', side_effect=fake), contextlib.redirect_stdout(io.StringIO()):
                status = collect(evidence)
            return status, json.loads((evidence / 'native-postgresql-evidence.json').read_text())

    def test_valid_synthetic_receipt_and_failed_provenance(self):
        status, report = self.fixture()
        self.assertEqual(status, 0)
        self.assertEqual(report['scenarios'], {SCENARIO: 'passed'})
        self.assertEqual(len(report['cliVersions']), 8)
        for invalid in ('source', 'owner', 'cli', 'server', 'data'):
            with self.subTest(invalid=invalid):
                status, report = self.fixture(invalid)
                self.assertEqual(status, 1)
                self.assertEqual(report['result'], 'failed')
                self.assertIn('failure', report)

    def test_absent_failed_and_duplicate_results_are_not_passes(self):
        with tempfile.TemporaryDirectory() as folder:
            file = Path(folder) / "results.trx"
            def write(outcomes):
                file.write_text('<TestRun>' + ''.join(f'<UnitTestResult testName="{SCENARIO}" outcome="{value}" />' for value in outcomes) + '</TestRun>')
            write(["Passed"])
            check_trx(folder)
            for outcomes in ([], ["Failed"], ["Passed", "Passed"], ["NotExecuted"]):
                write(outcomes)
                with self.assertRaises(ValueError):
                    check_trx(folder)


if __name__ == "__main__":
    unittest.main()
