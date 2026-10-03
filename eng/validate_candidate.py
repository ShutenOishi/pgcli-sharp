"""Validate the final preview provenance unit; never enable or publish it."""
import argparse
import json
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parent.parent


def validate(candidate):
    assert candidate['package_id'] == 'PgCliSharp'
    assert re.fullmatch(r'\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?', candidate['version'])
    assert candidate['tag'] == 'v' + candidate['version']
    assert candidate['prerelease'] == ('-' in candidate['version'])
    assert re.fullmatch(r'[0-9a-f]{40}', candidate['source_commit'])
    assert isinstance(candidate['source_main_ci'], int) and candidate['source_main_ci'] > 0
    assert re.fullmatch(r'\d+\.\d+\.\d+', candidate['sdk_version'])
    assert re.fullmatch(r'[A-Za-z0-9_-]+', candidate['nuget_user'])
    assert candidate['github_environment'] == 'release'
    assert isinstance(candidate['publication_enabled'], bool)
    for field in ('title', 'package_release_notes'):
        value = candidate[field]
        assert value.strip() and '\n' not in value and '\r' not in value
        assert re.search(r'[\u3040-\u30ff\u4e00-\u9fff]', value), field + ' must include Japanese'
    notes = Path(candidate['notes_file'])
    assert not notes.is_absolute() and '..' not in notes.parts
    assert notes.parts[:2] == ('docs', 'releases')
    text = (ROOT / notes).read_text(encoding='utf-8')
    assert text.index('## English') < text.index('## 日本語')
    return candidate


def main():
    if not __debug__:
        raise RuntimeError('Validation requires assertions; do not use -O.')
    parser = argparse.ArgumentParser()
    parser.add_argument('--require-enabled', action='store_true')
    parser.add_argument('--outputs', action='store_true')
    args = parser.parse_args()
    c = validate(json.loads((ROOT / '.github/release-candidate.json').read_text(encoding='utf-8')))
    if args.require_enabled:
        assert c['publication_enabled'], 'Publication remains disabled; a reviewed enablement is required.'
    if args.outputs:
        # GitHub output files require UTF-8, including on Windows redirect pipes.
        sys.stdout.reconfigure(encoding="utf-8")
        for key in ('source_commit', 'sdk_version', 'version', 'tag', 'title', 'nuget_user', 'prerelease'):
            value = c[key]
            print(key + '=' + (str(value).lower() if isinstance(value, bool) else value))
    else:
        print('Candidate metadata valid; publication_enabled=' + str(c['publication_enabled']).lower())


if __name__ == '__main__':
    main()
