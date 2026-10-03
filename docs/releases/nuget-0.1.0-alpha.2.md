# PgCliSharp 0.1.0-alpha.2 — Full wrapper preview / 全ラッパープレビュー

## English

Unpublished preview candidate; not authorization to publish.

- All 25 typed PostgreSQL CLI wrappers with PostgreSQL 10–18 syntax support.
- Lambda configuration, execution snapshots, offline validation and POSIX/PowerShell command export.
- Awaited psql completion and shared execution-result metadata.
- MIT original code; PostgreSQL executables supplied separately by the caller.
- netstandard2.0, net8.0 and net10.0 assets, bilingual XML/diagnostics and current local-feed README.
- Runtime dependency attribution/permission notices and unmodified .NET additional notices included.

Selected source: `0b7a2bb2e4dc1c1ed016181b2598645a1e3a8d5d`; first-attempt main CI 37104736018
passed all 16 jobs. Version/PackageReleaseNotes overrides, SDK 10.0.100 and reviewed
locked restore are recorded in the candidate audit. Payload, dependency terms,
package hashes, actual PDB SourceLink and DLL/PDB pairing are verified. Isolated
consumers compile all assets and run offline wrapper code on .NET 8/10; Windows
also runs net48, which is cross-compiled on every OS. No dependency or PostgreSQL/runtime binaries are embedded.
Legacy Microsoft reference terms remain independently recorded; the wrapper's
actual distribution scope is reviewed, not downstream bundled distributions.

The historical PowerShell completion timeout cause is unknown. A direct managed
fixture keeps the 15-second deadline and adds binary/EOF/drain regression stress
with first-failure reporting. Source main Windows TRX contains 360 successful
binary completions across net48/net8/net10. This is bounded fixture evidence.
Representative real PostgreSQL 10–18 coverage remains Linux/net10.0; native
Windows/macOS, service/TTY/TLS/ICU and ADR-0016 exclusions remain untested.
No 1.0/RC stability claim is made.

CI and manual release preflight share exact-source/package verification. Previous
unpublished selections and disabled Phase 3 manifests are retained. NuGet push,
public tags/Releases require explicit approval, reviewed candidate enablement and
account-side Trusted Publishing verification. This candidate is available only
as review artifacts/local feed; no nuget.org install is currently advertised.

## 日本語

未公開プレビュー候補です。公開承認を意味しません。

全25の型付きCLIラッパー、PostgreSQL 10〜18の構文対応、ラムダ設定、実行前の設定コピー、
オフライン検証、POSIX／PowerShell形式のコマンド生成、psql終了待ちと共通結果を含みます。
MITを採用し、実行ファイルは利用側で指定します。3アセットと日英文書・診断を提供し、
利用側依存の権利表示と原文.NET追加表示を同梱します。

固定ソースは `0b7a2bb2e4dc1c1ed016181b2598645a1e3a8d5d`、main CI 37104736018は
初回で全16ジョブ成功です。固定SDK・lockで生成した候補のソース・同梱物・原文条件・hash・
PDB／DLLの対応を監査し、利用側でコンパイルとnet8/net10、Windowsのnet48実行を検証します。
依存DLL・PostgreSQL・.NETランタイムは同梱しません。古いMicrosoft参照用の独自条件を
MITと読み替えず、その非同梱の境界でレビューを閉じます。下流の別配布は対象外です。

過去のPowerShellタイムアウト原因は未確定です。制限15秒を保持した直接起動の.NET
子プロセスでバイナリ・EOF・転送終了の反復を追加し、ソースmainのWindows3対象で
360回成功しました。実PostgreSQLの代表検証はLinux/net10.0に限り、掲載した実CLIの
除外範囲を維持します。1.0・RCの安定性保証とは扱いません。

CIと手動公開前チェックは同じ固定ソース監査を使い、以前の選定履歴と旧公開設定は保持します。
NuGet・タグ・Releaseの公開は明示承認、レビュー済み有効化、Trusted Publishingの確認が
必要です。候補成果物／ローカルfeedで確認でき、現在nuget.orgからの導入は案内しません。
