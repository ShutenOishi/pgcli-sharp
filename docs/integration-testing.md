# Real PostgreSQL integration testing / 実 PostgreSQL 統合テスト

## English

ADR-0022 adds required native Windows/macOS PostgreSQL 18/net10.0 representative
execution alongside the Linux matrix. See [CP-05 native evidence](phase-8-native-os.md)
for owned fixtures, exact-source/CLI/server/TRX checks and retained exclusions.
Completion requires exact PR/main CI and artifact inspection; no native success
is inferred merely from adding the job. Publication remains postponed.

PgCliSharp keeps deterministic unit and compatibility tests independent of local PostgreSQL installations, but those tests are not counted as real PostgreSQL execution evidence.

CI has a separate ubuntu-24.04/net10.0 matrix for pinned source builds of PostgreSQL 10-18. Each job verifies the official archive SHA256, builds an isolated server/client toolchain and runs public APIs against owned socket-only clusters. [CP-02 matrix and exclusions](phase-8-real-binary-matrix.md) and ADR-0016 define the scope.

| Scenario | Wrapper path exercised |
|---|---|
| seed and query | finite `Psql` |
| custom archive | `PgDump` |
| direct restore | `PgRestore` |
| post-start command exchange and EOF | redirected `PsqlSession` |
| initialization and one-transaction benchmark | `PgBench` |

The job records actual CLI/server versions, binary hashes, OS/TFM, build metadata, TRX outcomes and exclusions in JSON development artifacts. Required configuration validation prevents omissions from becoming passes. It never connects to a user-owned environment. Artifact sourceCommit records the checkout/Actions SHA; testedHeadSha records the PR head separately because checkout may use GitHub's temporary PR merge ref.

Phase 8 expands this evidence toward PostgreSQL 10-18 where reproducible maintained runners are available. A major/patch or OS combination that cannot be reproduced in maintained CI must be listed with its exclusion reason and corresponding official-source/specification plus deterministic regression evidence. Mock/fake-runner tests are never relabeled as real PostgreSQL evidence.

The test class is gated by `PGCLI_REAL_PG_*` environment variables so normal unit-test runs remain installation-independent.

## 日本語

ADR-0022により、Linuxに加えてWindows／macOSのPostgreSQL 18/net10.0代表シナリオを
1.0の必須条件にします。CP-05に所有環境・版／hash／実結果・除外を記録し、PR／main CIと
成果物確認で完了とします。job追加だけで合格とはせず、公開は延期します。

通常の unit/compatibility test は PostgreSQL のローカル導入に依存させませんが、それらを実 PostgreSQL 実行の証拠として数えることもしません。

CI には PostgreSQL 10〜18 の固定ソースを対象とする ubuntu-24.04/net10.0 の matrix を設けます。公式 SHA256 を確認してビルドし、TCP無効の使い捨てクラスタで公開 API を実行します。利用者所有環境には接続せず、CP-02 文書と ADR-0016 に範囲・除外を記録します。

有限 `Psql`、`PgDump`／`PgRestore`、redirected `PsqlSession`、`PgBench`、クラスタ smoke に加え、13以降の実巻き戻しと16→18コピー移行を検証します。実 CLI／server 版・hash・OS／TFM・結果・除外を JSON/TRX に残し、必須設定やテスト欠落を合格にしません。

Phase 8 では、再現可能な runner が用意できる範囲で PostgreSQL 10〜18 へ証拠を拡張します。維持可能な CI で再現できない major/patch/OS は除外理由と、公式 source/specification および決定論的 regression test による補完証拠を明記します。fake/mock の合格を実 PostgreSQL 合格として扱いません。

通常 test は `PGCLI_REAL_PG_*` 環境変数が無い場合に real-PG シナリオを実行しないため、PostgreSQL の導入を要求しません。

## Server application smoke and migration scenarios

`RealPostgreSqlServerApplicationsTests` runs alongside the existing real-binary
scenarios in PostgreSQL 10-18 Linux jobs. It creates a unique disposable cluster,
checks/disables/enables checksums from 12, executes pg_resetwal with DryRun, and exercises
pg_ctl Start/Status/Stop with an owned log and socket directory. Cleanup checks
status before stopping a possibly running server and deletes only the owned
directory. It never resets production WAL or invokes an upgrade/rewind against
external clusters. CP-02 adds scoped divergent rewind (13+) and a 16-to-18 Copy
migration. Windows/macOS real binaries and Windows service lifecycle remain untested.

日本語: Phase 7 の実バイナリ検証は、専用の破棄可能なクラスタで初期化・チェックサム操作・
WAL リセット dry-run・起動／状態確認／停止を実施します。CP-02 に掲載した巻き戻し・移行は
専用クラスタで検証し、Windows／macOS実機・サービスや他の除外は未検証として残します。
