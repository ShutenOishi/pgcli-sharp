# Licensing / ライセンス

## English

Original PgCliSharp code and documentation are licensed under [MIT](../LICENSE).
Commercial use, modification and redistribution in proprietary applications are
allowed without disclosing source. Keep the copyright and permission notice in
copies or substantial portions. The software is provided without warranty;
the license text, rather than this summary, defines the terms.

### PostgreSQL and third-party boundaries

PgCliSharp does not bundle PostgreSQL executables or libpq. It launches the
external executable supplied by the caller. PostgreSQL is independently licensed
under the [PostgreSQL License](https://www.postgresql.org/about/licence/), which
permits use, copying, modification and distribution with retained notices.
Selecting MIT for this wrapper does not change PostgreSQL's license. If you
redistribute PostgreSQL binaries/source, retain its copyright/license text and
audit any additional components in the particular distribution. Calling an
external CLI is not a substitute for a distribution's third-party obligations.

New netstandard2.0 assets reference System.Management 10.0.10 for best-effort
Windows descendant discovery under ADR-0024; CliWrap is removed. Modern assets
have no execution runtime dependencies. The reviewed legacy runtime inventory
and .NET additional notices remain in [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md).
Native argument serialization adapts .NET Foundation MIT source with retained
attribution and permission text. No CliWrap/PolyShim source is vendored. Frozen
alpha.2 still contains its original CliWrap backend and independently pinned
notices/dependency lock. SourceLink and tests remain build/test dependencies.

The preserved Phase 3 artifacts/source are not rewritten. Adopting MIT does not
enable publication or claim Phase 8 completion. The separate [unpublished
candidate audit](phase-8-candidate-audit.md) records actual resolved dependencies,
including one legacy Microsoft license. Its exact text/hash are preserved and
it is excluded from PgCliSharp's nupkg/snupkg: no dependency DLL, reference
package or runtime.json is bundled. This completes the distribution-scope
review for this wrapper; the legacy package remains independently licensed.
It does not clear a downstream application's different dependency/runtime bundle.

## 日本語

PgCliSharp独自のコード・文書は[MIT](../LICENSE)です。商用利用・改変・非公開アプリ
への組み込み・再配布ができ、ソース提供義務はありません。コピーや重要な部分を
配布する場合は著作権表示と許諾文を保持してください。無保証であり、正確な条件は
英語のライセンス本文に従います。

PostgreSQL実行ファイルやlibpqは同梱せず、呼び出し側が指定した外部CLIを起動します。
PostgreSQLは独自のPostgreSQL Licenseのままで、MITへ変更するものではありません。
PostgreSQL本体を再配布する場合は、その著作権表示・ライセンス文と、対象配布物に
含まれる第三者コンポーネントの条件を別途保持・確認してください。

新しいnetstandard2.0資産はWindows子孫終了用System.Management 10.0.10に依存し、
CliWrap依存を外します。modern対象には実行用の依存を追加しません。.NET Foundationの
MIT引数整形を改変し、出典・著作権・許諾文を保持します。CliWrap/PolyShimを取り込まず、
固定alpha.2は旧実装・依存lock・表示を元のソースに保持します。現在の依存一覧と表示は
[THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md)へ保存して同梱します。
旧Phase 3の履歴は書き換えず、公開設定は無効のままです。
[未公開候補監査](phase-8-candidate-audit.md)は確定した推移的依存と原文を記録します。
古いMicrosoft独自ライセンスは原文・hashを残し、参照パッケージ・依存DLL・runtime.jsonを
本ライブラリへ同梱しない配布境界を確認します。その範囲でレビューを完了し、独自条件は変更しません。
利用側が別の依存版・.NET／PostgreSQLを同梱する場合の確認まで完了したとは扱いません。
