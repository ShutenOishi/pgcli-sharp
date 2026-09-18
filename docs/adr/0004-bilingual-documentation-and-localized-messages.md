# ADR-0004: Provide bilingual XML documentation and localized runtime messages

- Status: Accepted
- Date: 2026-09-18
- Decision owners: PgCliSharp maintainers
- Supersedes: None
- Superseded by: None

## Context

PgCliSharp is intended for both English- and Japanese-speaking .NET users. Public API documentation must be available in ordinary IDE IntelliSense, while runtime errors should follow application/user UI culture without forcing callers to parse or translate strings.

XML documentation localization and runtime resource localization have different tooling characteristics.

## Decision

### Public XML documentation

All public NuGet-facing APIs use bilingual XML documentation in the same XML documentation output.

The canonical ordering is:

1. `EN:`
2. `JA:`

Example:

```csharp
/// <summary>
/// <para>EN: Executes pg_dump with the specified options.</para>
/// <para>JA: 指定されたオプションで pg_dump を実行します。</para>
/// </summary>
```

Public enums and enum members are included in this requirement.

Internal/private implementation comments do not require mandatory bilingual duplication.

### Runtime diagnostics and exceptions

User-facing PgCliSharp runtime messages are resource-based:

```text
Resources/
  Messages.resx       # neutral/default English
  Messages.ja.resx    # Japanese
```

English is the neutral/default resource language. Runtime selection follows `CultureInfo.CurrentUICulture` by default.

An explicit UI-culture override may be offered at an appropriate library/execution scope if useful.

Exceptions expose structured properties in addition to localized text so program logic never depends on parsing the message.

PostgreSQL executable stderr is preserved as external original output and is not machine-translated by default. PgCliSharp may wrap it with its own localized context.

## Alternatives considered

### English-only documentation/messages

Rejected because Japanese support is a product requirement.

### Japanese-only documentation/messages

Rejected because NuGet/.NET ecosystem interoperability and broader reuse require English.

### Separate public API assemblies/packages per language

Rejected as unnecessarily complex for initial IntelliSense and runtime behavior.

### Parse localized exception messages in caller code

Rejected; localization must not alter machine-readable failure semantics.

## Consequences

### Positive

- Both languages are visible in common IDE IntelliSense.
- Runtime messages can follow UI culture.
- Caller logic remains culture-independent.
- PostgreSQL's own diagnostic text is preserved accurately.

### Negative / trade-offs

- Public XML comments are longer.
- Every resource key needs synchronized translations and placeholder validation.
- Documentation review must check semantic parity between languages.

## Implementation notes

The detailed operational rules live in `docs/localization.md`.

Localized command/error rendering must redact secrets.

## Validation

Tests must verify:

- neutral and Japanese resource keys match;
- format placeholders match;
- English, Japanese, and fallback resource behavior;
- structured exception properties are invariant across UI cultures;
- public API XML documentation coverage as practical.
