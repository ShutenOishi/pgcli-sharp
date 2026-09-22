# Real PostgreSQL integration testing / 実 PostgreSQL 統合テスト

## English

PgCliSharp keeps deterministic unit and compatibility tests independent of local PostgreSQL installations, but those tests are not counted as real PostgreSQL execution evidence.

CI therefore has a separate Linux integration matrix for representative PostgreSQL majors 16 and 18. Each job installs an isolated PostgreSQL server/client toolchain and runs the public PgCliSharp API against disposable databases.

| Scenario | Wrapper path exercised |
|---|---|
| seed and query | finite `Psql` |
| custom archive | `PgDump` |
| direct restore | `PgRestore` |
| post-start command exchange and EOF | redirected `PsqlSession` |
| initialization and one-transaction benchmark | `PgBench` |

The job records the actual CLI and server versions in the workflow log/summary. It uses `net10.0` on `ubuntu-latest` and never connects to a user-owned PostgreSQL environment.

Phase 8 expands this evidence toward PostgreSQL 10-18 where reproducible maintained runners are available. A major/patch or OS combination that cannot be reproduced in maintained CI must be listed with its exclusion reason and corresponding official-source/specification plus deterministic regression evidence. Mock/fake-runner tests are never relabeled as real PostgreSQL evidence.

The test class is gated by `PGCLI_REAL_PG_*` environment variables so normal unit-test runs remain installation-independent.

## 日本語

通常の unit/compatibility test は PostgreSQL のローカル導入に依存させませんが、それらを実 PostgreSQL 実行の証拠として数えることもしません。

CI には代表バージョン PostgreSQL 16 / 18 を対象とする Linux 統合テスト matrix を分離して設けます。各 job は使い捨ての PostgreSQL server/client 環境を構築し、利用者所有環境には接続せず、公開 PgCliSharp API から専用 database を操作します。

検証シナリオは有限 `Psql` による seed/query、`PgDump` custom archive、`PgRestore` direct restore、起動後に書き込みと EOF を行う redirected `PsqlSession`、`PgBench` の初期化と最小 benchmark です。実際の CLI/server version は workflow log/summary に残します。

Phase 8 では、再現可能な runner が用意できる範囲で PostgreSQL 10〜18 へ証拠を拡張します。維持可能な CI で再現できない major/patch/OS は除外理由と、公式 source/specification および決定論的 regression test による補完証拠を明記します。fake/mock の合格を実 PostgreSQL 合格として扱いません。

通常 test は `PGCLI_REAL_PG_*` 環境変数が無い場合に real-PG シナリオを実行しないため、PostgreSQL の導入を要求しません。
