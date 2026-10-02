#!/usr/bin/env bash
set -euo pipefail

pg_major="${PGCLI_REAL_PG_MAJOR:?}"
pg_bin="${PGCLI_REAL_PG_BIN:?}"
pg_root="$(mktemp -d "${RUNNER_TEMP:?}/pgcli-real-$pg_major.XXXXXX")"
pg_user="$(id -un)"
{
  echo "PGCLI_REAL_PG_ROOT=$pg_root"
  echo "PGCLI_REAL_PG_HOST=$pg_root"
  echo "PGCLI_REAL_PG_PORT=5432"
  echo "PGCLI_REAL_PG_USER=$pg_user"
  echo "PGCLI_REAL_PG_SOURCE_DB=pgclisharp_source"
  echo "PGCLI_REAL_PG_TARGET_DB=pgclisharp_target"
  echo "PGCLI_REAL_PG_BENCH_DB=pgclisharp_bench"
} >> "${GITHUB_ENV:?}"
# Register ownership before startup so an unsuccessful setup can still be stopped.
"$pg_bin/initdb" -D "$pg_root/data" -A trust -U "$pg_user" --locale=C --encoding=UTF8
"$pg_bin/pg_ctl" -D "$pg_root/data" -l "$pg_root/server.log" -t 30 -w -o "-c listen_addresses='' -k '$pg_root' -p 5432" start
for database in pgclisharp_source pgclisharp_target pgclisharp_bench; do
  "$pg_bin/createdb" -h "$pg_root" -p 5432 -U "$pg_user" "$database"
done
