# ADR-0018: Add lambda configuration, offline commands and execution snapshots

- Status: Accepted
- Date: 2026-10-02
- Supersedes: None
- Complements: ADR-0002, ADR-0003, ADR-0004, ADR-0007, ADR-0008, ADR-0013, ADR-0014, ADR-0017

## Context

The user approved the complete API improvement direction and requested command
creation without execution. Mutable Options previously could change while the
first executable-version probe was awaiting. Existing validators and deterministic
argument builders already implement PostgreSQL 10-18 semantics for 25 tools.

## Decision

- Preserve existing instance Options APIs, parameter names/defaults, dedicated I/O,
  status semantics, .NET Standard 2.0/net8/net10 assets and runtime dependencies.
- Add lambda conveniences for every execution, validation and command operation
  as extension methods in the wrapper's namespace. Invoke Action<TOptions> once,
  synchronously, with a new Options instance. Callback exceptions propagate.
  Extensions preserve resolution of existing ExecuteAsync(null, ...) calls;
  adding same-position instance Action overloads would make them ambiguous.
- Copy every mutable scalar, list and environment dictionary before the first
  await in both Options and lambda execution. Immutable value objects and stream
  handles are retained; caller streams are neither copied nor disposed. Concurrent
  mutation during capture/configuration is unsupported; afterward the original
  Options can be changed without affecting the in-flight invocation. Configuration
  is mutable; the internal execution copy has no external owner and is not mutated.
- Add synchronous Validate and CreateCommand to all 25 wrappers, plus psql's
  ValidateForSession/CreateSessionCommand and server CreateHelpCommand operations.
  Reuse existing validators/builders, without spawning even --version, checking
  executable existence, opening/creating files or consuming stream contents.
  Existing ambient environment/platform-sensitive validation stays applicable;
  no claim of server, cluster, filesystem or installed-build validity is made.
- Validation returns zero errors or the first known option failure. Stable enum
  codes, original CLI names/labels, known property bindings and localized text
  support forms without parsing messages. Programmer argument errors propagate.
- The three dump/restore tools accept an optional asserted numeric executable
  Version for patch validation. Null means unknown: perform major/combination
  checks and explicitly flag requested patch-sensitive checks as deferred. Never
  invent a patch number. Execution independently probes the actual executable;
  an asserted version cannot bypass that check. These are distinct guarantees.
- PgCommand freezes exact tokens/environment overrides in read-only collections.
  Explicit raw access can contain secrets. ToString and ToCommandLine redact every
  argument/environment value by default, including SQL, URIs and forwarded server
  fragments. Explicit includeSensitiveValues:true renders the actual values.
- Rendering requires a selected POSIX-shell or PowerShell style, quoting every
  token including executable paths and empty/Unicode values. Environment overrides
  are included; PowerShell restores its invoking process environment in finally.
  PowerShell native argument fidelity requires PowerShell 7.5+ with Standard
  native argument passing. This is export syntax, not a new execution backend.
  Caller-owned stdin/stdout/stderr routing is reported as metadata and must be
  supplied separately. Arbitrary Stream objects cannot be serialized as shell
  pipelines, files or contents. No cmd.exe/bash invocation is added to execution.
- Add IPgExecutionResult only for the four existing common metadata properties.
  Do not impose uniform IsSuccess, stderr buffering or rebased result classes.
- Add PsqlSession.CompleteAsync(CancellationToken): signal EOF and await existing
  completion/output drain; cancellation requests the existing process-tree cleanup
  and carries the supplied token. The original session timeout remains effective.
  Dispose/Cancel/Completion/CompleteInput remain. No IAsyncDisposable dependency or
  target-specific public surface is added. With no timeout/token, normal graceful
  completion may wait indefinitely for upstream behavior, like Completion today.
- Review the compiled API diff under ADR-0017 and update the checked baseline
  explicitly. No automatic acceptance switch is added. This is additive source/API
  work; caller reliance on mutation during an outstanding await intentionally stops.
- Keep deferred publication disabled and Phase 3 provenance unchanged. This change
  does not finish Phase 8 CP-04 or choose/publish a release candidate.

## Validation

Across all 25 wrappers, compare offline tokens to execution tokens while mutating
Options and environment during a deliberately suspended version probe. Exercise
lambda creation, first-error localization/bindings, exact patch boundaries, shell
quoting/round trips, stream metadata/ownership, session completion/cancellation and
existing version matrices. Compile and compare the public contract on all targets,
compile EN/JA README examples, and retain three-OS plus pinned real-binary CI gates.

## 日本語

既存 Options API と互換性を維持し、ラムダ設定・実行前の設定コピー・オフライン検証・
コマンド生成を全25ラッパーへ追加します。ラムダは新規 Options に同期的に一度だけ適用し、
拡張メソッドで既存 null 呼び出しの曖昧化を避けます。コマンド生成では実行ファイルの
確認やファイル作成を行わず、既存の検証・引数生成を再利用します。未確認パッチ版は
未確認と明示し、実行時の実バージョン確認を省略しません。文字列は既定で秘密値を伏せ、
明示指定時に実際の値を出力します。ストリームは別途接続します。共通結果インターフェイスと
psql の非同期 EOF／終了待ちを追加し、3対象・日英・公開 API 検証・公開延期を維持します。

## References

- [PowerShell quoting](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_quoting_rules)
- [PowerShell native argument passing](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_preference_variables#psnativeargumentpassing)
- [POSIX-shaped single quoting](https://www.gnu.org/software/bash/manual/html_node/Single-Quotes.html)
- [Consumer guide](../configuration-and-commands.md)
