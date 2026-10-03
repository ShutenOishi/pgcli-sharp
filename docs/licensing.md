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

The netstandard2.0 asset references [CliWrap 3.10.5](https://www.nuget.org/packages/CliWrap/3.10.5),
whose package declares MIT and Copyright (C) Oleksii Holub. It is resolved as a
separate NuGet dependency, not embedded or relicensed as PgCliSharp code. Modern
net8.0/net10.0 assets do not reference CliWrap. Preserve upstream notices whenever
redistributing dependencies. SourceLink and test tools are build/test dependencies,
not shipped runtime dependencies. Final publication requires a resolved-package
license/notice audit including transitive packages; this guide is not that audit.

The preserved Phase 3 artifacts/source are not rewritten. Adopting MIT does not
enable publication or claim Phase 8 completion. The separate [unpublished
candidate audit](phase-8-candidate-audit.md) records actual resolved dependencies,
including one legacy Microsoft license requiring pre-publication review; it is
not blanket MIT classification or distribution clearance.

## 日本語

PgCliSharp独自のコード・文書は[MIT](../LICENSE)です。商用利用・改変・非公開アプリ
への組み込み・再配布ができ、ソース提供義務はありません。コピーや重要な部分を
配布する場合は著作権表示と許諾文を保持してください。無保証であり、正確な条件は
英語のライセンス本文に従います。

PostgreSQL実行ファイルやlibpqは同梱せず、呼び出し側が指定した外部CLIを起動します。
PostgreSQLは独自のPostgreSQL Licenseのままで、MITへ変更するものではありません。
PostgreSQL本体を再配布する場合は、その著作権表示・ライセンス文と、対象配布物に
含まれる第三者コンポーネントの条件を別途保持・確認してください。

netstandard2.0のみCliWrap 3.10.5へ依存します。同パッケージの表示はMIT、
著作権者はOleksii Holubです。NuGetが別途解決する依存であり、PgCliSharpへ埋め込んで
権利表示を置き換えません。依存物を再配布する際は元の表示を保持してください。
推移的依存も含む確定パッケージのライセンス監査は公開前に実施します。
旧Phase 3の履歴は書き換えず、公開設定は無効のままです。
[未公開候補監査](phase-8-candidate-audit.md)は確定した推移的依存と原文を記録します。
古いMicrosoft独自ライセンスの公開前確認を残し、依存全件をMIT・配布承認済みとは扱いません。
