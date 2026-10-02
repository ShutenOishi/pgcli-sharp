# ADR-0019: Adopt the MIT license

- Status: Accepted
- Date: 2026-10-03
- Supersedes: None
- Complements: ADR-0008, ADR-0012

## Context

The owner selected MIT after considering, but not adopting, MPL 2.0.
Commercial and proprietary use and redistribution of modifications should be
allowed without a source-disclosure requirement. No accepted license decision
previously existed in this repository.

## Decision

- License original PgCliSharp code and documentation under the standard MIT
  license in the root LICENSE; retain third-party terms and notices separately.
- Use the existing package author/copyright identity, ShutenOishi, with year 2026.
- Declare PackageLicenseExpression=MIT and include LICENSE and the bilingual
  licensing guide in development packages. Verify their presence/content in CI.
- PostgreSQL executables are caller-supplied external programs, not bundled or
  relicensed by this project. Official PostgreSQL permits use, modification and
  redistribution under the PostgreSQL License. If binaries/source are bundled
  later, retain PostgreSQL notices and audit bundled third-party components.
- CliWrap remains a separately resolved netstandard2.0 dependency; its package
  declares MIT. A final candidate audit must still inspect actual resolved
  transitive dependencies and redistributed notices. Do not claim it complete.
- No source-disclosure obligation is added to original PgCliSharp code. MIT's
  copyright/permission notice retention and warranty disclaimer remain intact.
- Keep the preserved Phase 3 source/version and publication manifests unchanged
  and disabled. This license adoption neither retroactively edits old artifacts
  nor selects a new publication candidate nor authorizes external publication.

## Consequences and validation

MIT is intentionally simpler than copyleft licensing. The project does not
require users to contribute modifications back. Metadata, packaged license text,
bilingual explanations and disabled-publication provenance are checked in CI.
No runtime dependency or public API is changed.

## 日本語

所有者の選択により、独自コード・文書へMITを採用します。MPLは検討案であり、
採用済みライセンスからの変更ではありません。商用利用・改変・非公開での配布を
認め、ソース提供義務を求めません。著作権表示と許諾文の保持・無保証条項は維持します。
PostgreSQL CLIは同梱せず外部プロセスとして利用し、その独自ライセンスは変更しません。
将来同梱する場合はPostgreSQLおよび第三者の表示・条件を別途遵守します。
公開候補・推移的依存関係の最終監査は残し、公開設定や旧Phase 3候補は変更しません。

## References

- https://opensource.org/license/mit
- https://www.postgresql.org/about/licence/
- https://www.nuget.org/packages/CliWrap/3.10.5
- [Licensing guide](../licensing.md)
