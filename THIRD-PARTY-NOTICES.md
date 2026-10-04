# Third-party notices / 第三者の権利表示

## English

PgCliSharp packages contain PgCliSharp assemblies and documentation, not
PostgreSQL, libpq, .NET runtime binaries or dependency assemblies. NuGet resolves
the following dependencies separately for netstandard2.0. This inventory records
the reviewed lock; applications must preserve notices for their actual resolution
and redistributed files. Modern net8.0/net10.0 assets have no execution runtime dependencies. This inventory
applies to the ADR-0024 development source; frozen alpha.2 retains its own notices
and lock at its immutable source.

| Package | Reviewed version | License | Package copyright |
|---|---|---|---|
| System.Buffers | 4.6.1 | MIT | © Microsoft Corporation. All rights reserved. |
| System.CodeDom | 10.0.10 | MIT | © Microsoft Corporation. All rights reserved. |
| System.Management | 10.0.10 | MIT | © Microsoft Corporation. All rights reserved. |
| System.Memory | 4.6.3 | MIT | © Microsoft Corporation. All rights reserved. |
| System.Numerics.Vectors | 4.6.1 | MIT | © Microsoft Corporation. All rights reserved. |
| System.Runtime.CompilerServices.Unsafe | 6.1.2 | MIT | © Microsoft Corporation. All rights reserved. |

The expressions and copyright strings are from the exact NuGet packages.
The standard MIT permission/disclaimer below accompanies these notices; it does
not replace an upstream package's own files or grant additional rights.
The .NET packages also retain attribution to the .NET Foundation and Contributors.
System.CodeDom and System.Management supply the same
upstream [additional notices](docs/third-party/dotnet-notices.txt), preserved byte
for byte (SHA256 `6d15e10a101c6bfff2ab4429ed061bf76c456fc4b23ad6b03e0d0f8377148a21`).
Keeping that entire upstream notice does not assert all its components are used.

Build-only NETStandard.Library 2.0.3 includes MIT LICENSE.TXT. Its legacy
Microsoft.NETCore.Platforms 1.1.0 dependency contains Microsoft .NET Library
license terms, not MIT. Neither contributes executable/compile binaries in the
inspected candidate's runtime distribution; those packages and their runtime.json
are not redistributed inside PgCliSharp. Their terms still apply to separately
obtaining/using or redistributing them. The candidate audit retains the original
text/hash and rejects unexpected shipped files. This review covers the PgCliSharp
nupkg/snupkg boundary, not every downstream or self-contained application.

### Attributed native argument serialization

ProcessCompatibility.QuoteArgument adapts the .NET Foundation PasteArguments
algorithm, replacing its internal buffer with StringBuilder. Source:
https://github.com/dotnet/runtime/blob/9a50493f9f1125fda5e2212b9d6718bc7cdbc5c0/src/libraries/System.Private.CoreLib/src/System/PasteArguments.cs

Copyright (c) .NET Foundation and Contributors. Licensed under MIT. The following
permission and disclaimer apply to this adapted source as well as the MIT
packages listed above. No CliWrap/PolyShim implementation is vendored.

### MIT permission text

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## 日本語

PgCliSharpは外部CLI・libpq・.NETランタイム・依存DLLを同梱しません。
表はnetstandard2.0で別途解決される実行時依存の確認版・ライセンス・著作権表示です。
アプリへ依存物を同梱する場合は、実際の解決版に付属する本文・表示を保持してください。
表のMIT許諾文と.NETの追加表示を同梱しますが、元の依存物の条件を変更するものではありません。
古いMicrosoft独自ライセンスの参照パッケージはMITと読み替えず、原文・hashを監査記録へ残します。
今回の配布物にそれらが入らないことを確認し、その範囲で配布境界のレビューを完了します。
別のアプリや.NET／PostgreSQLを同梱する配布物の条件までは代行しません。

引数整形は.NET FoundationのPasteArgumentsをStringBuilder向けに改変したMITコードです。
上記の著作権・許諾文を保持します。CliWrap/PolyShimのコードは取り込みません。
固定alpha.2の旧依存・表示はimmutable sourceに保持し、新しい開発ソースと区別します。
