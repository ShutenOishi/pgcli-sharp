import contextlib
import io
import json
import os
import platform
import tempfile
from pathlib import Path
import unittest
from unittest.mock import patch

from native_postgresql import SCENARIO, check_trx, collect, pin, stage


class NativeEvidenceTests(unittest.TestCase):
    def test_staging_preserves_owned_text_evidence_and_rejects_foreign_logs(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            build = root / 'build'
            fixture = root / 'fixture'
            evidence = root / 'native-evidence'
            for directory in (build, fixture, evidence):
                directory.mkdir()
            build_owner = {'root': str(build), 'run': '123'}
            owner = {'root': str(fixture), 'binary': str(root / 'bin'), 'run': '123'}
            (build / 'BUILD-OWNER.json').write_text(json.dumps(build_owner))
            (fixture / 'OWNER.json').write_text(json.dumps(owner))
            (build / 'build.log').write_text('build log')
            (fixture / 'server.log').write_text('server log')
            (evidence / 'report.json').write_text('{}')
            (evidence / 'report.trx').write_text('<TestRun />')
            for directory in (build, fixture, evidence):
                (directory / 'do-not-ship.dll').write_bytes(b'not evidence')
            env = {'RUNNER_TEMP': str(root), 'GITHUB_RUN_ID': '123', 'PGCLI_NATIVE_BUILD_ROOT': str(build),
                   'PGCLI_REAL_PG_ROOT': str(fixture), 'PGCLI_REAL_PG_BIN': owner['binary']}
            target = root / 'staged'
            with patch.dict(os.environ, env):
                stage(target)
                self.assertEqual({path.relative_to(target).as_posix() for path in target.rglob('*') if path.is_file()},
                                 {'evidence/report.json', 'evidence/report.trx', 'build/BUILD-OWNER.json',
                                  'build/build.log', 'fixture/OWNER.json', 'fixture/server.log'})
                for path, receipt in ((build / 'BUILD-OWNER.json', build_owner), (fixture / 'OWNER.json', owner)):
                    with self.subTest(receipt=path.name):
                        path.write_text(json.dumps(dict(receipt, run='foreign')))
                        with self.assertRaises(ValueError):
                            stage(root / ('foreign-' + path.name))
                        path.write_text(json.dumps(receipt))

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
