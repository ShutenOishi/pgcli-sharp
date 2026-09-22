# Phase 0–6 review and change plan / Phase 0〜6 見直し・変更計画

## English

Review date: 2026-09-22. Baseline: `6c85389946602b200d2822f7667eeef4ada49c48` (main, Phase 6 merge).

This file began as a documentation-only review backlog at commit `746a510363ba74bf214314ff2acab8ddbdcd0143`. After separate authorization, PR #12 was expanded to implement the recorded follow-ups while preserving the completed Phase 0-6 status and ADR-0012 publication deferral. The original findings and acceptance criteria remain below as audit history; the follow-up status section records the resulting changes.

The review found three priority areas: process/I/O failure supervision, bounded legacy-session input buffering, and alignment of the completion workflow with ADR-0012. Other work concerns spec/runtime drift detection, localization regression gates, real PostgreSQL integration evidence, package provenance, and final phase-evidence indexing.

Modern runners do not supervise I/O task failures while awaiting process exit. Failed termination or blocked output draining can also leave cancellation/timeout completion unbounded. These are source-grounded conditional risks; no runtime reproduction was performed. The netstandard2.0 session uses an unbounded input queue, a confirmed implementation characteristic with load-dependent memory risk.

Read-only checks covered 19 specifications and 574 option entries: no duplicate option IDs, missing major records (10–18), unknown referenced IDs, or missing API-binding declarations were found. This does not prove parser semantics or actual public binding correctness. The 24 neutral/Japanese resource entries had matching keys and numeric placeholder sets in this snapshot. Existing main CI [35691571528](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/35691571528) was verified successful for Linux, macOS, and Windows at the exact baseline SHA. No new build, test suite, PostgreSQL execution, or complete upstream semantic re-audit was run.

The original priorities below remain useful as review history. The authorized follow-up preserves ADR-0012, historical Phase 0–2 releases, the disabled publication manifests, and the deferred Phase 3 source identity.

## 日本語

### 1. 対象・判断基準・制約

- 対象リポジトリ: `ShutenOishi/pgcli-sharp`。
- 基準: `main@6c85389946602b200d2822f7667eeef4ada49c48`。記録前に main が同一 SHA であることを再確認。
- 対象範囲: Phase 0〜6。Phase 7 の実装、公開操作、既存の完了状態の取り消しは対象外。
- AGENTS、Accepted ADR、architecture、localization、roadmap、tool implementation workflow を基準に、共通実行基盤・互換性検査・各 Phase の証跡を重点レビュー。
- 本文は当初「変更が必要な箇所と今後の確認計画」として commit `746a510363ba74bf214314ff2acab8ddbdcd0143` に記録した。その後、別途の実施承認を受けて PR #12 内で是正を実装した。元の指摘・受入条件は監査履歴として残し、末尾の実施状況で変更結果を追跡する。
- 確認済みのコード構造、そこから推定される条件付きリスク、今後の検証課題を区別する。全 574 オプションの正しさを保証する監査ではない。

### 2. Phase 別の確認範囲

| Phase | 今回確認した範囲 | 主な後続項目 |
|---|---|---|
| 0 基盤 | Accepted ADR、version provider、modern/CliWrap runner、実プロセステスト、resources、TFM/CI | R01、R05、R06 |
| 1 pg_dump | 75 option の spec 構造、API-binding 宣言、既存 availability テストと spec 比較テストの差 | R04、R06 |
| 2 pg_restore / pg_dumpall | 109 option の spec 構造、spec→runtime 比較方式、API-binding 検査 | R04、R06 |
| 3 パッケージ・公開準備 | csproj、CI pack、公開 manifest、ADR-0012 の固定 source/version 方針 | R03、R07 |
| 4 Backup/WAL | 107 option、whole-tool 境界、coverage テスト、FakeRunner を用いた execution テスト、完了記録 | R01、R04、R06、R08 |
| 5 管理・保守 | 197 option、whole-tool 境界、coverage/execution テスト構成、完了記録 | R01、R04、R06、R08 |
| 6 psql / pgbench | 86 option、public session API、session runner 全体、有限実行共通基盤、実プロセステスト、完了記録 | R01、R02、R04、R06、R08 |

全 19 spec を読み、ID 重複、10〜18 の version record、参照 ID の存在、API binding 宣言を読み取り専用スクリプトで検査した。異常なし。これは既存の `eng/validate-compatibility-specs.sh` 全体を実行した結果ではなく、限定した独立検査である。

### 3. 優先度一覧

P1: 次の共通基盤拡張前に優先して設計・再現確認する。P2: Phase 8 / 公開前までに閉じる。P3: 保守性・追跡性の改善。

| ID | 優先度 | 判定 | 変更対象（将来） |
|---|---|---|---|
| R01 | P1 | 待機構造は確認済み。ハングは条件付き・未再現 | modern runner/session の異常系監督・終了処理 |
| R02 | P1 | 無制限キューは確認済み。メモリ増大は負荷依存 | netstandard2.0 session 入力制御・契約 |
| R03 | P1 | 現行文書間の不整合を確認 | Phase 完了手順と ADR 相互参照 |
| R04 | P2 | pg_dump の spec 比較不足を確認 | spec/API/runtime の回帰検査 |
| R05 | P2 | 専用 localization テストに placeholder 比較なし | resource 検査の恒常化 |
| R06 | P2 | 確認した CI に実 PG 環境準備なし | 実バイナリ統合検証計画 |
| R07 | P2 | 同一 package version と異なる source の CI 生成を確認 | 開発 artifact と公開候補の識別 |
| R08 | P3 | 完了記録が最終 merge 証跡を直接収録していない | 最終 SHA/CI の証跡索引 |

### R01 — I/O 失敗と終了処理を含めたライフサイクル監督

**根拠**

- [ProcessRunner.cs](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/src/PgCliSharp/Internal/Execution/ProcessRunner.cs) の `RunWithProcessAsync`、特に 181〜213 行。待ち合わせ対象は process exit、cancel、timeout のみであり、stdin/stdout/stderr copy の fault は含まれない。
- [ProcessSessionRunner.cs](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/src/PgCliSharp/Internal/Execution/ProcessSessionRunner.cs) の `ModernProcessSession.CompleteAsync`、257〜284 行も同様。
- 両 runner の `TryTerminateProcessTree` は終了操作の例外を吸収するが、その後 `waitTask` を期限なしで await する。出力転送には `CancellationToken.None` を渡し、プロセス終了後も転送を待つ。
- [ProcessRunnerTests](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/tests/PgCliSharp.Tests/ProcessRunnerTests.cs) と [ProcessSessionRunnerTests](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/tests/PgCliSharp.Tests/ProcessSessionRunnerTests.cs) は通常の timeout/cancel/EOF を検証するが、失敗・停止する利用者 stream や kill 失敗を扱うテストは今回確認したファイルにない。

**影響・確度**

出力先が例外を投げ、その後子プロセスが pipe 容量を超える出力を続けると、読み手が止まったまま子が終了せず、I/O 例外の通知まで進まない可能性がある。stdin 読み取り失敗時も EOF 待ちになる可能性がある。kill 失敗、終了後の pipe drain、停止した利用者 stream によって、指定 timeout が API 完了時間の上限にならない。通常経路で必ず発生するとは主張しない。

**変更計画**

1. I/O fault、プロセス終了、cancel、timeout を一つの監督対象として扱う設計を作る。
2. I/O 失敗時の子プロセス終了、全タスクの例外観測、元の失敗原因と cleanup 失敗の優先順位を定義。
3. 終了・drain に期限を設けるか、呼出者へ残存プロセス状態を通知するかを決定。kill 成功を保証したかのように扱わない。
4. 利用者 stream は勝手に Dispose しない。キャンセル非協調 stream を強制停止できるとは約束せず、保証範囲を公開文書に明記。
5. CliWrap 側は同じ故障入力で振る舞いを比較する。modern の問題がそのまま CliWrap にあるとは断定しない。

**受け入れ条件**

stdout/stderr の write fault、stdin read fault、子の早期終了、kill 失敗の注入、停止 stream、子孫が pipe を保持する場合を検証。外側の watchdog でテスト自身の無限待機を防ぎ、残存プロセスを回収する。3 OS と Windows net48 で、終了状態・例外・所有 stream の扱いを確認する。

### R02 — 旧.NET セッション入力キューの容量制限と配送契約

**根拠**

[ProcessSessionRunner.cs](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/src/PgCliSharp/Internal/Execution/ProcessSessionRunner.cs) の `SessionInputPipe`（435 行以降）は `ConcurrentQueue<byte[]>` を持ち、`Enqueue` が全 byte をコピーして蓄積する。`SessionInputStream.WriteAsync`（556 行以降）は enqueue 後に即完了し、`Flush/FlushAsync` は配送完了を待たない。modern backend は OS pipe へ直接書き込む。

**影響・確度**

子が遅い／stdin を読まない状態でも旧.NET では送信側を抑制しないため、入力の蓄積量に応じてメモリが増える。負荷時の backend 差であり、今回 OOM は発生させていない。Flush の無処理だけを直ちに API 違反とは断定せず、配送契約が未整理な点として扱う。

**変更計画**

byte 単位の上限を持つ入力バッファとキャンセル可能な待機を検討。write 完了、Flush、CompleteInput、cancel/dispose、子の早期終了の意味を両 backend で定義する。EOF 前の投入済み byte 順序を保持し、満杯待ちを終了時に必ず解放する。公開契約を変更する場合は ADR-0013/0008 への追補・superseding が必要か判断する。

**受け入れ条件**

読まない子／低速の子に上限を超えるデータを送り、キューが規定範囲内に収まること、待機 write が cancel/exit/dispose で解放されること、通常 EOF が欠落・重複なく配送されること。特に net48 で実行する。

### R03 — Phase 完了ゲートを公開延期方針に合わせる

**根拠**

[tool-implementation-workflow.md](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/docs/tool-implementation-workflow.md) §13（243 行以降）は Release、tag、assets を確認して初めて Phase 完了としている。一方 [ADR-0012](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/docs/adr/0012-defer-external-publication-until-final-phase.md) は Phase 3 以降を実装完了と公開に分離する。ADR-0011 §9 に旧ゲートが残る一方、ADR-0012 は ADR-0011 を Preserves としている。architecture §16 の「First NuGet preview and publication pipeline validation」も現在の保留状態が読み取りにくい。

**変更計画**

現行 workflow を「Phase 0〜2 の履歴」「Phase 3〜7 の実装完了」「Phase 8 の明示的公開承認」に分ける。実装完了は exact head/main CI と repo 証跡で判定し、公開 manifest を最新 Phase 番号に書き換えない。ADR-0011 の本文を履歴を隠す形で修正せず、ADR-0012 による §9 の適用変更を索引・注記で明示する。方針自体を変更する場合だけ新 ADR を作る。

**受け入れ条件**

同じ Phase 6 の状態をロードマップ・workflow・ADR から読んで同じ結論になること。Phase 3 source SHA と publication_enabled=false を保持し、公開操作なしで実装完了を説明できること。

### R04 — pg_dump の spec→runtime 照合と binding 検査強化

**根拠**

[CompatibilitySpecCoverageTests](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/tests/PgCliSharp.Tests/CompatibilitySpecCoverageTests.cs) は API property の存在検査には pg_dump を含むが、spec→runtime availability 比較は pg_restore / pg_dumpall のみ。[PgDumpOptionAvailabilityCatalogTests](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/tests/PgCliSharp.Tests/PgDumpOptionAvailabilityCatalogTests.cs) は手書きの境界値を検証しており、pg_dump.json と直接比較していない。Phase 4〜6 には各 catalog 比較がある。

全 Phase の binding 検査は `api.binding` が空でないことを確かめるが、任意の special-binding 名の実在・動作までは証明しない。また availability 比較は主に spec→catalog の片方向である。

**変更計画**

pg_dump に同等の自動照合を追加し、major/patch 境界、spelling と feature の区別を維持する。special binding は許可リストと対応テストへ結び付ける。catalog 側の余剰エントリは一律に拒否せず、合成 feature・値レベル境界等の正当な例外を明示する。

**受け入れ条件**

pg_dump の spec と catalog の片側だけを変える mutation で失敗すること。restrict-key の patch 前後と large-object spelling の回帰が検出されること。不明な special binding を追加すると失敗し、正当な I/O/version/help 等の binding は通ること。現時点で pg_dump の境界値が誤りだとは主張しない。

### R05 — localization placeholder 検査を恒常化

**根拠**

[localization policy](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/docs/localization.md) §9 は日英 placeholder の一致を要求する。[LocalizationTests](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/tests/PgCliSharp.Tests/LocalizationTests.cs) は MessageKeys の取得、日英差、fallback、代表例外の構造を検査するが、placeholder index の比較はない。今回の読み取り専用比較では neutral/ja 各 24 resource の key と numeric placeholder に不一致はなかった。

**変更計画**

resx の key 集合と composite-format placeholder を CI で比較する。fallback が欠落翻訳を隠さないよう各 resource を直接検査。エスケープ brace・format/alignment の扱いを含め、単純な文字列差分にしない。

**受け入れ条件**

日本語 key 削除、`{0}`→`{1}`、不正 format を混入すると失敗し、引数順の翻訳上の入れ替えは許容する。既存の culture/structured-data テストも維持する。

### R06 — 実 PostgreSQL 検証を実行可能な計画へ具体化

**根拠**

[CI](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/.github/workflows/ci.yml) は 3 OS の .NET build/test/pack を行うが、PG 10〜18 のバイナリ・サーバー準備や専用 integration job はない。Phase 4〜6 execution テストは FakeRunner/FakeSessionRunner による契約検証であり、session の実プロセステストは cat / sleep / PowerShell / ping を使用している。これは有用なプロセス検証だが PostgreSQL の E2E 証拠ではない。roadmap Phase 8 に実 PG マトリクスは既に予定されている。

**変更計画**

新たな欠落 Phase を作らず、既存 Phase 8 計画を具体化する。まず代表 major で backup→restore、psql finite/session、pgbench 最小実行を行い、PG 10〜18 の再現可能な範囲へ拡張。Phase 4/5 の破壊的・権限を伴う操作は専用の使い捨て環境に隔離する。patch 境界の実バイナリ検証と、再現不能時の公式 source＋unit 証拠を分ける。

**受け入れ条件**

CLI major/patch、server version、OS/TFM、シナリオ、結果、除外理由が表で追えること。mock 合格を real PG 合格と数えない。利用者環境に接続しない。全 major × 全 OS の実行を無条件必須とせず、ADR-0011 に従って再現性と補完証拠を明記する。

### R07 — 開発 package と保留公開候補の識別

**根拠**

[PgCliSharp.csproj](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/src/PgCliSharp/PgCliSharp.csproj) は version `0.1.0-alpha.1`、release notes は backup trio の初回 preview のまま。現行 CI は現在の SHA を pack するが、[NuGet manifest](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/.github/nuget-release.json) は Phase 3 の `cccf8d9fbe1f2e1104676ab94a7863209c0220dd` を固定 source とする。CI は EXPECTED_COMMIT を使うため出所の検証が全くないわけではない。外部公開の誤りを発見したという意味ではない。

**変更計画**

開発 CI artifact に SHA・非公開候補である旨を明示し、必要なら開発用 version 方針を別途決める。Phase 3 候補の version/source を黙って更新しない。Phase 8 では固定候補を公開するか、後続候補で supersede するかを明記し、選択した source と notes/package 内容を一致させる。

**受け入れ条件**

開発 artifact と固定公開候補を区別でき、package 内 SourceLink/repository commit がビルド元と一致すること。manifest の固定 SHA・無効状態を維持すること。今回 version/notes/manifest を変更しない。

### R08 — 最終 merge/CI 証跡の repo 内索引

**根拠**

[Phase 4 completion](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/docs/phase-4-completion.md)、[Phase 5 completion](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/docs/phase-5-completion.md)、[Phase 6 completion](https://github.com/ShutenOishi/pgcli-sharp/blob/6c85389946602b200d2822f7667eeef4ada49c48/docs/phase-6-completion.md) は documentation 変更前の head/CI を載せ、最後の検証を PR report/checks に委ねる。これは証跡なしという意味ではないが、repo 文書単体では最終 merge の追跡に追加検索が必要になる。

**変更計画**

履歴を消さず、最終 PR head、merge SHA、各 CI URL、3 OS 結果を completion 索引へ追記する後続文書更新を計画する。文書を後から更新した SHA と、当該 Phase の完了 SHA を混同しない。

**受け入れ条件**

CI の head_sha と記録 SHA が一致し、PR と main 両方の結果が直接追跡できること。今回 Phase 6 main のみ直接確認済み: `6c85389946602b200d2822f7667eeef4ada49c48` / run `35691571528`、Linux/macOS/Windows success。Phase 4/5 の最終 CI は追記時に再確認する。

### 4. 当初の推奨実施順序（履歴）

1. R03: 現行完了ゲートの誤読を防ぐ文書整合。公開方針そのものは維持する。
2. R01/R02: 異常系再現テストと session 契約の設計を先行。実装修正は別承認後。
3. R04/R05: 仕様・翻訳が増えても抜けを検知できる回帰ゲート。
4. R06: 既存 Phase 8 の実 PG 検証を、環境と完了条件のある作業単位へ具体化。
5. R07/R08: 公開前の provenance と証跡整理。R07 の候補選択は公開承認と分離して記録する。

各修正では必要な source/spec/tests/docs を一貫した変更単位にし、最終 head の CI と merge 後の exact main CI を検証する。Phase 7 や外部公開はこの follow-up の対象外とする。

### 5. 維持する判断・今回の限界

維持するもの: per-tool typed options、explicit executable path/version、PG CLI compatibility と upstream EOL の区別、spec-first、shell 非介在、binary streaming、pipe session と TTY/PTY の区別、日英文書、ADR-0012 の公開延期。

当初レビュー時点で未実施だった build/test、異常系再現、実 PostgreSQL 実行のうち、follow-up で対象化したものは下記の実施状況へ移した。全 574 option の upstream semantic 再監査、全公開 XML の意味対訳確認、外部公開状態の網羅監査は今回の follow-up 範囲には拡張しない。

### 6. Follow-up 実施状況

| ID | 実施結果 |
|---|---|
| R01 | modern one-shot/session の I/O fault を process/cancel/timeout と同じ lifecycle 監督対象にし、異常 cleanup を 2 秒の内部 grace で有界化。write/read fault と cancellation 非協調 output stream を watchdog 付き real-process test で検証。caller-owned stream は Dispose しない。 |
| R02 | netstandard2.0 session input bridge を 64 KiB × 16 segment（約 1 MiB）に制限し、同期/非同期 write に backpressure を導入。NET48 test で write cancellation と session end による blocked writer 解放を検証。 |
| R03 | Phase 0-2 の履歴 release、Phase 3-7 の implementation completion、Phase 8 の external publication を workflow 上で分離。ADR-0012 を維持。 |
| R04 | pg_dump spec→runtime availability の直接比較、patch-level minimum、special binding allowlist を regression gate に追加。 |
| R05 | neutral/ja RESX を直接読み、key 集合と composite-format placeholder index を比較する test を追加。 |
| R06 | disposable PostgreSQL 16/18 Linux CI と public wrapper E2E（PgDump/PgRestore、finite/redirected Psql、PgBench）を追加。Phase 8 の 10-18 拡張規則を文書化。 |
| R07 | CI package artifact に exact SHA と `publication_candidate=false` を持つ provenance file を同梱。version と Phase 3 publication source は変更しない。 |
| R08 | Phase 4/5/6 completion docs に final PR head、merge SHA、PR/main CI を追記。 |

実行基盤の契約変更は Accepted ADR-0014 に記録した。PR #12 の最終 head CI と merge 後 exact-main CI を completion gate とし、publication manifest、tag、Release、NuGet push はこの follow-up では変更・実行しない。
