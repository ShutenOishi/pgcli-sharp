#!/usr/bin/env bash
set -euo pipefail

manifest="${1:-.github/phase-release.json}"

test -f "$manifest"

jq -e '
  (.phase | type == "number") and
  (.tag | type == "string" and test("^phase-[0-9]+$")) and
  (.title | type == "string" and length > 0) and
  (.notes_file | type == "string" and startswith("docs/releases/")) and
  (.prerelease | type == "boolean") and
  (.publication_enabled | type == "boolean") and
  (.release_source_commit | type == "string" and test("^[0-9a-f]{40}$")) and
  (.deferred_until_phase | type == "number" and . >= 3)
' "$manifest" >/dev/null

phase="$(jq -r '.phase' "$manifest")"
tag="$(jq -r '.tag' "$manifest")"
notes_file="$(jq -r '.notes_file' "$manifest")"

test "$tag" = "phase-$phase"
test -f "$notes_file"
test -f README.ja.md
grep -F 'README.ja.md' README.md >/dev/null
grep -F '## 日本語' "$notes_file" >/dev/null
grep -F '## English' "$notes_file" >/dev/null

english_line="$(grep -n -m1 '^## English$' "$notes_file" | cut -d: -f1)"
japanese_line="$(grep -n -m1 '^## 日本語$' "$notes_file" | cut -d: -f1)"

test -n "$english_line"
test -n "$japanese_line"

if [ "$phase" -ge 1 ]; then
  test "$english_line" -lt "$japanese_line"
fi

if [ "$(jq -r '.publication_enabled' "$manifest")" != "true" ]; then
  echo "Phase external publication: deferred/disabled"
fi
