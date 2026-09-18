#!/usr/bin/env bash
set -euo pipefail

manifest="${1:-.github/nuget-release.json}"
project="src/PgCliSharp/PgCliSharp.csproj"

test -f "$manifest"
test -f "$project"

jq -e '
  (.package_id | type == "string" and length > 0) and
  (.version | type == "string" and test("^[0-9]+\\.[0-9]+\\.[0-9]+-[0-9A-Za-z.-]+$")) and
  (.tag | type == "string" and test("^v[0-9]+\\.[0-9]+\\.[0-9]+-[0-9A-Za-z.-]+$")) and
  (.title | type == "string" and length > 0) and
  (.notes_file | type == "string" and startswith("docs/releases/")) and
  (.nuget_user | type == "string" and length > 0) and
  (.github_environment == "release") and
  (.prerelease | type == "boolean")
' "$manifest" >/dev/null

package_id="$(jq -r '.package_id' "$manifest")"
version="$(jq -r '.version' "$manifest")"
tag="$(jq -r '.tag' "$manifest")"
notes_file="$(jq -r '.notes_file' "$manifest")"

test "$tag" = "v$version"
test -f "$notes_file"

project_package_id="$(sed -n 's:.*<PackageId>\(.*\)</PackageId>.*:\1:p' "$project" | head -n1)"
project_version="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$project" | head -n1)"

test "$project_package_id" = "$package_id"
test "$project_version" = "$version"

grep -F '<Title>PgCliSharp</Title>' "$project" >/dev/null
grep -F '<PackageReadmeFile>README.md</PackageReadmeFile>' "$project" >/dev/null
grep -F '<GenerateDocumentationFile>true</GenerateDocumentationFile>' "$project" >/dev/null
grep -F '<IncludeSymbols>true</IncludeSymbols>' "$project" >/dev/null
grep -F '<SymbolPackageFormat>snupkg</SymbolPackageFormat>' "$project" >/dev/null
grep -F '<PublishRepositoryUrl>true</PublishRepositoryUrl>' "$project" >/dev/null
grep -F '<RepositoryType>git</RepositoryType>' "$project" >/dev/null
grep -F '<RepositoryUrl>https://github.com/ShutenOishi/pgcli-sharp</RepositoryUrl>' "$project" >/dev/null
grep -F 'Microsoft.SourceLink.GitHub' "$project" >/dev/null
grep -F 'README.ja.md' "$project" >/dev/null

grep -F '## English' "$notes_file" >/dev/null
grep -F '## 日本語' "$notes_file" >/dev/null

english_line="$(grep -n -m1 '^## English$' "$notes_file" | cut -d: -f1)"
japanese_line="$(grep -n -m1 '^## 日本語$' "$notes_file" | cut -d: -f1)"

test -n "$english_line"
test -n "$japanese_line"
test "$english_line" -lt "$japanese_line"

if grep -Eq '<PackageLicense(Expression|File)>' "$project"; then
  echo "NuGet license metadata: configured"
else
  echo "NuGet license metadata: not configured (no accepted repository license decision exists yet)"
fi
