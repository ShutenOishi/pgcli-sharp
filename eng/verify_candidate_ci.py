"""Read-only exact-source main CI gate for the reviewed preview."""
import json
import os
from pathlib import Path
import urllib.request

from validate_candidate import ROOT, validate

REPOSITORY = 'ShutenOishi/pgcli-sharp'


def check(run, jobs, candidate):
    assert run['head_sha'] == candidate['source_commit'], 'Wrong source CI SHA'
    assert run['event'] == 'push' and run['head_branch'] == 'main'
    assert run['path'] == '.github/workflows/ci.yml'
    assert run['status'] == 'completed' and run['conclusion'] == 'success'
    required = {f'Build and test ({os})' for os in ('ubuntu-latest', 'windows-latest', 'macos-latest')}
    required.update(f'Real PostgreSQL {major} (Linux/net10.0)' for major in range(10, 19))
    if candidate.get('dependency_profile') == 'process-compat-v1':
        required.update(f'Native PostgreSQL 18 ({os}/net10.0)' for os in ('windows-latest', 'macos-latest'))
        required.update(f'Unpublished candidate audit ({os})' for os in ('ubuntu-latest', 'windows-latest', 'macos-latest'))
        required.add('Release preflight / Verify reviewed preview without publication')
        assert run['run_attempt'] == 1, 'RC source requires first-attempt evidence'
        assert len(jobs) == len({job['name'] for job in jobs}), 'Duplicate source CI jobs'
    assert required <= {job['name'] for job in jobs}
    assert any(job['name'].startswith('Unpublished candidate audit (') for job in jobs)
    assert len(jobs) >= 13 and all(job['conclusion'] == 'success' for job in jobs)
    return {'run_id': run['id'], 'source_commit': run['head_sha'], 'attempt': run['run_attempt'],
            'jobs': [{'name': job['name'], 'conclusion': job['conclusion']} for job in jobs]}


def get(url):
    headers = {'Accept': 'application/vnd.github+json', 'User-Agent': 'PgCliSharp-candidate-preflight'}
    token = os.environ.get('GH_TOKEN')
    if token:
        headers['Authorization'] = 'Bearer ' + token
    with urllib.request.urlopen(urllib.request.Request(url, headers=headers), timeout=30) as response:
        return json.load(response)


def main():
    if not __debug__:
        raise RuntimeError('CI gate requires assertions; do not use -O.')
    c = validate(json.loads((ROOT / '.github/release-candidate.json').read_text(encoding='utf-8')))
    base = f'https://api.github.com/repos/{REPOSITORY}/actions/runs/{c["source_main_ci"]}'
    run = get(base)
    page = get(base + '/jobs?per_page=100')
    assert page['total_count'] == len(page['jobs']), 'Incomplete job pagination'
    report = check(run, page['jobs'], c)
    Path('source-ci-verification.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print('Exact-source main CI verified:', run['id'])


if __name__ == '__main__':
    main()
