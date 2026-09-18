# PostgreSQL CLI compatibility specifications

This directory contains machine-readable compatibility research used by PgCliSharp.

## pg_dump

[`pg_dump.json`](pg_dump.json) is the Phase 1 canonical compatibility inventory for PostgreSQL 10 through 18.

It records:

- official per-major documentation URLs;
- the resolved option inventory for each major;
- short/long spellings and aliases;
- option argument/value kinds;
- repeatability;
- major and patch-level availability;
- defaults and constraints;
- archive/output/compression behavior;
- connection/environment relationships;
- intended typed .NET binding;
- validation rules and version-specific semantic changes.

Do not infer historical support by applying the newest PostgreSQL option list to old versions. Update the per-major source comparison first.

A notable example is `--restrict-key`: current historical-major documentation contains it because it was security-backported. Its minimum executable versions are therefore recorded per major instead of as a simple major-only `since` value.

Human-readable research and API planning are in [`docs/pg-dump-phase-1.md`](../../docs/pg-dump-phase-1.md).
