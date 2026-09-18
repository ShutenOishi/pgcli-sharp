# Phase 0 - Foundation

Phase 0 establishes the reusable foundation for PgCliSharp.

## Included

- `PgCliSharp.slnx` solution format and .NET 10 SDK pinning.
- Accepted target framework matrix: `netstandard2.0;net8.0;net10.0`.
- Strict nullable/analyzer configuration, XML documentation, SourceLink, and NuGet metadata.
- English/Japanese localized runtime diagnostics and structured exception types.
- PostgreSQL 10-18 version/lifecycle metadata.
- Shell-free process execution with cancellation, timeout handling, binary stdout streaming, and stderr capture.
- Modern .NET process-tree termination through the BCL.
- Conditional CliWrap compatibility backend for `netstandard2.0`.
- Executable `--version` parsing, caching, and major-version validation.
- Linux, Windows, and macOS CI.
- Windows .NET Framework 4.8 compatibility execution against the `netstandard2.0` library asset.
- Regression coverage for process-tree termination and binary output preservation.

## Attached artifacts

The Phase Release workflow attaches:

- `pgcli-sharp-phase-0.zip` — source snapshot for the exact Phase 0 merge commit.
- `PgCliSharp.0.1.0-alpha.0.nupkg` — NuGet package produced from that commit.
- `PgCliSharp.0.1.0-alpha.0.snupkg` — symbol package produced from that commit.

This Phase release records a development milestone. It does not by itself publish the package to nuget.org.
