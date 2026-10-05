"""Reject incorrect CI provenance, missing jobs and enabled-publication mistakes."""
import copy
import json
import os
import subprocess
import sys
import unittest
from validate_candidate import ROOT, validate
from verify_candidate_ci import check


class PreflightTests(unittest.TestCase):
    def test_rc_requires_process_profile_and_pinned_lock(self):
        c = json.loads((ROOT / '.github/release-candidate.json').read_text())
        c.update(version='1.0.0-rc.1', tag='v1.0.0-rc.1',
                 dependency_profile='process-compat-v1', lock_sha256='a' * 64,
                 notes_file='docs/releases/nuget-1.0.0-rc.1.md')
        validate(c)
        for changes in ({'dependency_profile': 'legacy-cliwrap'},
                        {'dependency_profile': 'unknown'}, {'lock_sha256': 'bad'}):
            with self.assertRaises(AssertionError):
                validate(dict(c, **changes))

    def test_rc_source_requires_all_18_first_attempt_jobs(self):
        c = {'source_commit': 'a' * 40, 'dependency_profile': 'process-compat-v1'}
        run = {'id': 123, 'head_sha': c['source_commit'], 'event': 'push', 'head_branch': 'main',
               'path': '.github/workflows/ci.yml', 'status': 'completed', 'conclusion': 'success', 'run_attempt': 1}
        names = [f'Build and test ({os})' for os in ('ubuntu-latest', 'windows-latest', 'macos-latest')]
        names += [f'Real PostgreSQL {major} (Linux/net10.0)' for major in range(10, 19)]
        names += [f'Native PostgreSQL 18 ({os}/net10.0)' for os in ('windows-latest', 'macos-latest')]
        names += [f'Unpublished candidate audit ({os})' for os in ('ubuntu-latest', 'windows-latest', 'macos-latest')]
        names += ['Release preflight / Verify reviewed preview without publication']
        jobs = [{'name': name, 'conclusion': 'success'} for name in names]
        check(run, jobs, c)
        for index in range(len(jobs)):
            with self.assertRaises(AssertionError):
                check(run, jobs[:index] + jobs[index + 1:], c)
        with self.assertRaisesRegex(AssertionError, 'first-attempt'):
            check(dict(run, run_attempt=2), jobs, c)
        with self.assertRaisesRegex(AssertionError, 'Duplicate'):
            check(run, jobs + [jobs[0]], c)

    def test_manifest_identity_and_inputs(self):
        c = json.loads((ROOT / '.github/release-candidate.json').read_text())
        validate(c)
        for key, value in (('source_commit', 'main'), ('tag', 'v0.1.0-alpha.1'),
                           ('notes_file', '../secret'), ('sdk_version', '10.0.x'),
                           ('publication_enabled', 'false'), ('nuget_user', 'bad\nvalue')):
            invalid = copy.deepcopy(c)
            invalid[key] = value
            with self.assertRaises(AssertionError):
                validate(invalid)

    def test_exact_main_ci_and_required_jobs(self):
        c = {'source_commit': 'a' * 40}
        run = {'id': 123, 'head_sha': c['source_commit'], 'event': 'push', 'head_branch': 'main',
               'path': '.github/workflows/ci.yml', 'status': 'completed', 'conclusion': 'success', 'run_attempt': 1}
        names = [f'Build and test ({os})' for os in ('ubuntu-latest', 'windows-latest', 'macos-latest')]
        names += [f'Real PostgreSQL {major} (Linux/net10.0)' for major in range(10, 19)]
        names += ['Unpublished candidate audit (Linux)']
        jobs = [{'name': name, 'conclusion': 'success'} for name in names]
        check(run, jobs, c)
        for key, value in (('head_sha', 'b' * 40), ('event', 'pull_request'),
                           ('head_branch', 'feature'), ('conclusion', 'failure')):
            invalid = dict(run, **{key: value})
            with self.assertRaises(AssertionError):
                check(invalid, jobs, c)
        for invalid_jobs in (jobs[:-1], jobs[:-2], [dict(jobs[0], conclusion='skipped')] + jobs[1:]):
            with self.assertRaises(AssertionError):
                check(run, invalid_jobs, c)

    def test_bilingual_github_outputs_survive_windows_legacy_encoding(self):
        result = subprocess.run([sys.executable, str(ROOT / 'eng/validate_candidate.py'), '--outputs'],
            env=dict(os.environ, PYTHONIOENCODING='cp1252'), capture_output=True)
        self.assertEqual(result.returncode, 0, result.stderr)
        title = json.loads((ROOT / '.github/release-candidate.json').read_text())['title']
        self.assertIn('title=' + title, result.stdout.decode('utf-8'))

    def test_disabled_publication_fails_before_outputs(self):
        result = subprocess.run([sys.executable, str(ROOT / 'eng/validate_candidate.py'),
                                 '--require-enabled', '--outputs'], capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('Publication remains disabled', result.stderr)
        self.assertEqual(result.stdout, '')

    def test_publication_is_manual_and_separate_from_shared_ci_preflight(self):
        release = (ROOT / '.github/workflows/release.yml').read_text()
        preflight = (ROOT / '.github/workflows/candidate-preflight.yml').read_text()
        ci = (ROOT / '.github/workflows/ci.yml').read_text()
        # The only job allowed to login/push must be gated by both explicit dispatch
        # and successful preflight, whose validator requires reviewed enablement.
        trigger = release.split('on:\n', 1)[1].split('permissions:', 1)[0]
        self.assertIn('workflow_dispatch:', trigger)
        self.assertIn('default: false', trigger)
        for event in ('push:', 'pull_request:', 'schedule:'):
            self.assertNotIn(event, trigger)
        gate, publisher = release.split('\n  publish:\n', 1)
        self.assertIn('uses: ./.github/workflows/candidate-preflight.yml', gate)
        self.assertIn('require_publication_enabled: ${{ inputs.publish }}', gate)
        self.assertIn('if: ${{ inputs.publish == true }}', publisher)
        self.assertIn('needs: preflight', publisher)
        self.assertIn('environment: release', publisher)
        self.assertNotIn('NuGet/login', gate)
        self.assertIn('python eng/validate_candidate.py --require-enabled', preflight)
        for text in (preflight, ci):
            for forbidden in ('id-token:', 'contents: write', 'dotnet nuget push', 'NuGet/login', 'gh release create'):
                self.assertNotIn(forbidden, text)
        self.assertIn('uses: ./.github/workflows/candidate-preflight.yml', ci)
        self.assertIn('require_publication_enabled: false', ci)
        for name in ('nuget-release.json', 'phase-release.json', 'release-candidate.json'):
            self.assertIs(json.loads((ROOT / '.github' / name).read_text())['publication_enabled'], False)


if __name__ == '__main__':
    unittest.main()
