# Localization and Documentation Policy

> Consolidated current policy. The decision and rationale are recorded in [ADR-0004](adr/0004-bilingual-documentation-and-localized-messages.md).

## 1. Goals

PgCliSharp supports English and Japanese for:

1. public C# XML documentation;
2. runtime user-facing diagnostics and exception messages;
3. human-facing project documentation and release surfaces.

Code identifiers remain English.

## 2. Public XML documentation

Every public API intended for NuGet consumers should have bilingual English/Japanese XML documentation.

Use English first, Japanese second, in the same XML documentation file so both are available in ordinary IDE IntelliSense without requiring language-specific package selection.

Recommended style:

```csharp
/// <summary>
/// <para>EN: Executes pg_dump with the specified options.</para>
/// <para>JA: 指定されたオプションで pg_dump を実行します。</para>
/// </summary>
/// <param name="options">
/// <para>EN: The typed pg_dump options.</para>
/// <para>JA: 型付きの pg_dump オプションです。</para>
/// </param>
/// <returns>
/// <para>EN: The command execution result.</para>
/// <para>JA: コマンドの実行結果です。</para>
/// </returns>
public Task<PgCommandResult> ExecuteAsync(PgDumpOptions options);
```

Use the same order everywhere:

1. `EN:`
2. `JA:`

Do not duplicate the same `<param name="...">` tag twice. Put both languages inside the same element.

Public enums and enum members should also be documented bilingually because option meaning is part of the typed API.

Private/internal implementation comments do not need mandatory bilingual duplication. Prefer concise English internal comments unless Japanese materially improves maintainability for a particular complex rule.

## 3. XML documentation quality

Translations must describe the same behavior. Japanese text is not allowed to add guarantees that are absent from the English text, and vice versa.

Documentation should state:

- semantics;
- version restrictions where relevant;
- units/ranges;
- null/default behavior;
- important PostgreSQL CLI constraints.

Avoid merely repeating the member name.

## 4. Runtime localization

Do not hard-code complete user-facing exception/error strings throughout the source.

Use .NET resources:

```text
Resources/
  Messages.resx       # neutral/default English
  Messages.ja.resx    # Japanese
```

The neutral resource language is English.

Use stable resource keys, for example:

```text
ExecutableNotFound
ExecutableVersionMismatch
OptionNotSupported
InvalidOptionCombination
ProcessExitedWithError
ProcessTimedOut
```

Parameterized messages should use placeholders rather than string concatenation.

Example intent:

English:

```text
Option '{0}' is not supported by PostgreSQL {1}. It is supported from PostgreSQL {2}.
```

Japanese:

```text
オプション '{0}' は PostgreSQL {1} ではサポートされていません。PostgreSQL {2} 以降で使用できます。
```

## 5. Culture selection

Default runtime localization should follow `CultureInfo.CurrentUICulture`.

The design should also allow an explicit caller-provided UI culture override at an appropriate library/execution scope if implementation experience shows this is useful.

Do not bind the library permanently to the machine OS language.

Tests must be able to select a culture deterministically.

## 6. Machine-readable exception data

Localized text is for humans. Program logic must not depend on it.

Exception types should expose structured information where practical, for example:

```csharp
public PostgreSqlMajorVersion SelectedVersion { get; }
public string OptionName { get; }
public PostgreSqlMajorVersion? SupportedSince { get; }
```

The message may be English or Japanese, but callers should inspect properties/types rather than parse text.

## 7. PostgreSQL stderr

Text produced directly by PostgreSQL executables is external output and must not be falsely presented as a PgCliSharp translation.

PgCliSharp may wrap it with a localized message such as "pg_dump exited with code 1", while preserving the original PostgreSQL stderr separately.

Do not machine-translate PostgreSQL stderr by default.

## 8. Logging and secrets

Localized diagnostics must never expose passwords or other secrets.

If a command representation is included in an exception/log, redact secret-bearing environment values and other sensitive fields.

## 9. Resource tests

Tests should verify at minimum:

- every required neutral English resource key has a Japanese counterpart;
- formatting placeholders match between English and Japanese;
- resource lookup works under `en`, `ja`, and a fallback culture;
- representative exceptions expose identical structured properties regardless of UI culture.

## 10. Human-facing repository and release content

Prominent content that a human user is expected to read directly must provide both English and Japanese.

This includes at minimum:

- the repository root README;
- GitHub Release titles and release notes;
- user-oriented setup/usage guides;
- prominent notices that affect package consumers.

The root `README.md` may remain primarily English for ecosystem interoperability, but it must place a clear Japanese link near the top. The maintained Japanese document is `README.ja.md`.

Phase release notes under `docs/releases/` must contain both Japanese and English in the same file so the GitHub Release body is bilingual without requiring navigation to another page.

GitHub Release tags, target commits, and attached artifacts remain immutable according to the phase-release ADR. Human-readable Release metadata such as title and notes may be synchronized later when adding/fixing translations or correcting non-semantic documentation errors.

Internal engineering documents such as ADRs may remain English-first unless a Japanese version materially improves usability. They are not required to duplicate every paragraph in both languages.

## 11. NuGet documentation

NuGet packages should include generated XML documentation files.

The package README should expose a conspicuous path to Japanese documentation. Public IntelliSense documentation remains bilingual according to this policy.

## 12. Updating localization policy

If IDE rendering or package tooling demonstrates a better way to deliver language-specific XML IntelliSense, this policy can be revised. Any material change to the Accepted localization decision must use the ADR superseding workflow in `docs/adr/README.md`, and this consolidated document must be updated in the same change.
