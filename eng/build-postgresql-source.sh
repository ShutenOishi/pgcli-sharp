#!/usr/bin/env bash
set -euo pipefail

pg_major="${1:?PostgreSQL major required}"
pg_prefix_root="${PGCLI_PREFIX_ROOT:?Explicit CI installation prefix required}"
test "${pg_prefix_root:0:1}" = /
test "$pg_prefix_root" != /
pg_row="$(jq -ce --argjson major "$pg_major" '.versions[] | select(.major == $major)' eng/postgresql-source-versions.json)"
pg_version="$(jq -r .version <<< "$pg_row")"
pg_sha="$(jq -r .sha256 <<< "$pg_row")"
pg_prefix="$pg_prefix_root/$pg_major"

if [ -f "$pg_prefix/bin/postgres" ]; then
  test "$("$pg_prefix/bin/postgres" --version)" = "postgres (PostgreSQL) $pg_version"
  jq -e --arg version "$pg_version" --arg sha "$pg_sha" '.version == $version and .sha256 == $sha' "$pg_prefix/BUILD-SOURCE.json" >/dev/null
  echo "Verified cached PostgreSQL $pg_version"
  exit 0
fi

pg_build_dir="$(mktemp -d "${RUNNER_TEMP:?}/pgcli-build-$pg_major.XXXXXX")"
pg_archive="postgresql-$pg_version.tar.bz2"
pg_url="https://ftp.postgresql.org/pub/source/v$pg_version/$pg_archive"
curl --fail --location --retry 3 "$pg_url" --output "$pg_build_dir/$pg_archive"
echo "$pg_sha  $pg_build_dir/$pg_archive" | sha256sum --check --strict
tar -xjf "$pg_build_dir/$pg_archive" -C "$pg_build_dir"
pushd "$pg_build_dir/postgresql-$pg_version"
./configure --prefix="$pg_prefix" --without-readline --without-icu > "$pg_build_dir/configure.log" 2>&1 || { cat "$pg_build_dir/configure.log"; exit 1; }
make -j2 > "$pg_build_dir/build.log" 2>&1 || { tail -100 "$pg_build_dir/build.log"; exit 1; }
make install > "$pg_build_dir/install.log" 2>&1 || { tail -100 "$pg_build_dir/install.log"; exit 1; }
popd
test "$("$pg_prefix/bin/postgres" --version)" = "postgres (PostgreSQL) $pg_version"
jq -n --arg version "$pg_version" --arg sha256 "$pg_sha" --arg url "$pg_url" --arg compiler "$(cc --version | head -1)" \
  '{version:$version,sha256:$sha256,url:$url,compiler:$compiler,configure:["--without-readline","--without-icu"],ssl:false}' > "$pg_prefix/BUILD-SOURCE.json"
echo "Built and verified PostgreSQL $pg_version"
