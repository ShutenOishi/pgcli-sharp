#!/usr/bin/env bash
set -euo pipefail

artifacts_dir="${1:-artifacts}"
manifest=".github/nuget-release.json"
package_id="$(jq -r '.package_id' "$manifest")"
version="$(jq -r '.version' "$manifest")"
expected_commit="${EXPECTED_COMMIT:-}"

nupkg="$artifacts_dir/$package_id.$version.nupkg"
snupkg="$artifacts_dir/$package_id.$version.snupkg"

test -f "$nupkg"
test -f "$snupkg"

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

mkdir -p "$work/nupkg" "$work/snupkg"
unzip -q "$nupkg" -d "$work/nupkg"
unzip -q "$snupkg" -d "$work/snupkg"

nuspec="$work/nupkg/$package_id.nuspec"
test -f "$nuspec"

grep -F "<id>$package_id</id>" "$nuspec" >/dev/null
grep -F "<version>$version</version>" "$nuspec" >/dev/null
grep -F '<repository type="git" url="https://github.com/ShutenOishi/pgcli-sharp"' "$nuspec" >/dev/null

repository_line="$(grep -o '<repository[^>]*/>' "$nuspec" | head -n1)"
test -n "$repository_line"

if [ -n "$expected_commit" ]; then
  grep -F "commit=\"$expected_commit\"" <<<"$repository_line" >/dev/null
else
  grep -Eq 'commit="[0-9a-f]{40}"' <<<"$repository_line"
fi

for tfm in netstandard2.0 net8.0 net10.0; do
  test -f "$work/nupkg/lib/$tfm/PgCliSharp.dll"
  test -f "$work/nupkg/lib/$tfm/PgCliSharp.xml"
  test -f "$work/snupkg/lib/$tfm/PgCliSharp.pdb"
done

test -f "$work/nupkg/README.md"
test -f "$work/nupkg/README.ja.md"

# Clean local-consumer smoke test for every advertised TFM.
consumer="$work/consumer"
mkdir -p "$consumer"
artifact_source="$(cd "$artifacts_dir" && pwd)"

for tfm in netstandard2.0 net8.0 net10.0; do
  dir="$consumer/$tfm"
  mkdir -p "$dir"
  cat > "$dir/Smoke.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$tfm</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="$package_id" Version="$version" />
  </ItemGroup>
</Project>
EOF
  cat > "$dir/Smoke.cs" <<'EOF'
using PgCliSharp;

public static class Smoke
{
    public static PostgreSqlMajorVersion Version => PostgreSqlMajorVersion.V18;
}
EOF
  dotnet restore "$dir/Smoke.csproj" --source "$artifact_source" --source "https://api.nuget.org/v3/index.json"
  dotnet build "$dir/Smoke.csproj" --configuration Release --no-restore
done

echo "Verified $nupkg and $snupkg"
