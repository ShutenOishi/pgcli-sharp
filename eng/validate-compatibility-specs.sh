#!/usr/bin/env bash
set -euo pipefail

shopt -s nullglob
specs=(spec/postgresql/*.json)

if [ "${#specs[@]}" -eq 0 ]; then
  echo "No PostgreSQL compatibility specifications found." >&2
  exit 1
fi

expected_majors='[10,11,12,13,14,15,16,17,18]'

for spec in "${specs[@]}"; do
  echo "Validating $spec"

  jq -e --argjson expected "$expected_majors" '
    (.schemaVersion | type == "number") and
    (.tool | type == "string" and length > 0) and
    (.researchedAt | type == "string" and length > 0) and
    (.scope.postgresqlMajors == $expected) and
    (.sources.perMajorDocumentation | type == "object") and
    (.options | type == "array" and length > 0) and
    (.versions | type == "object") and
    all(.options[];
      (.id | type == "string" and length > 0) and
      (.spellings | type == "object") and
      (.argumentMode == "none" or .argumentMode == "required") and
      (.repeatable | type == "boolean") and
      (.availability | type == "object") and
      (.defaults.wrapper | type == "string" and length > 0) and
      (.defaults.upstream | type == "string" and length > 0) and
      (((.api.property? // "") | length > 0) or ((.api.binding? // "") | length > 0))
    )
  ' "$spec" >/dev/null

  duplicate_ids="$(
    jq -r '.options[].id' "$spec" |
      sort |
      uniq -d
  )"
  if [ -n "$duplicate_ids" ]; then
    echo "Duplicate option IDs in $spec:" >&2
    echo "$duplicate_ids" >&2
    exit 1
  fi

  for major in {10..18}; do
    major_key="$major"

    jq -e --arg major "$major_key" '
      (.sources.perMajorDocumentation[$major] | type == "string" and length > 0) and
      (.versions[$major].documentation | type == "string" and length > 0) and
      (.versions[$major].optionIdsInCurrentMajorDocumentation | type == "array") and
      (
        .versions[$major].optionIdsInCurrentMajorDocumentation
        | length == (unique | length)
      )
    ' "$spec" >/dev/null
  done

  jq -e '
    (.options | map(.id)) as $ids |
    all(
      .versions[].optionIdsInCurrentMajorDocumentation[];
      . as $id | ($ids | index($id)) != null
    )
  ' "$spec" >/dev/null
done
