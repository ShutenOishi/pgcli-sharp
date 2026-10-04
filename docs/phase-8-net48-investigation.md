# Windows net48 completion investigation / 完了待ち調査

## Replacement follow-up / 実行基盤の置換

Replacement final-candidate CI 37192663590 at
`2a1b77d0f9320f34addc5dcbb086f97957aa77e8` passes all normal full suites,
native Windows/macOS PostgreSQL and the corrected clean-consumer package audit.
However, its Windows stress step remains reported in progress beyond its
10-minute step cap, with no downloadable job log or Windows artifact at the
observation point. This is unresolved runner/command-lifecycle evidence, not a
proved recurrence of the library stall and not a successful completion receipt.
The earlier replacement CI 37192226343 completed all 30/3/3 rounds but failed its
initial dependency-audit expectation; neither incomplete run clears the merge gate.

The next diagnostic revision independently bounds each owned dotnet test command
to 180 seconds, including host/collector shutdown, records UTC/PID/TFM/round,
elapsed duration and actual exit in stress-commands.jsonl, and stops on the first
nonzero/timeout outcome. Timeout terminates only the newly started invocation's
tree and is recorded as failure 124. The original session 15-second deadlines,
testcase two-minute blame watchdog, total 10-minute step cap and 30/3/3 counts
remain. No local VSTest execution or browser fallback is used. Existing workflow
concurrency supersedes the unresponsive run when this meaningful diagnostic
revision is published; old run outcomes remain separate evidence.

置換案の最終候補は通常テスト・両native・依存監査を通過しましたが、Windows反復stepが
10分上限後も実行中表示で、ログ／Windows成果物を取得できません。ライブラリ再停止とは
断定せず、マージを保留します。新しい診断では各自所有のdotnet testコマンドの終了を
180秒で制限し、PID・対象・回数・所要時間・実終了値を記録します。期限切れは失敗とし、
再試行せず止めます。既存の15秒／2分／10分期限と反復回数は変えません。

Nested-diagnostic CI 37191060046 at source
`3859d4eb31fa71e0b19dc87d23e35c5dee72d9fe` reproduces a five-byte stall in the
initial net48 full suite (44 completed, aborted before stress); modern suites
pass 554 each. Actual owned CLR4 smoke succeeds with int 1729, short 37, bool true.
Artifact 11298503924 was independently checked against SHA-256
`3a7916571d76ea7fb82626dfc318ffeb919e208b0825cd68b9772ab578e76ae3` and contains no DMP.
The active promise version and consumer token both equal 3; WhenEach state is -4,
ExecuteAsync state is 2, and all four lower tasks have flags 0x7000400. The promise
is PolyShim's internal lazy-TCS compatibility implementation. Its unsynchronized
TCS initialization is a potential race in pinned primary source, not a root cause
proven by this capture; the TCS continuation graph was not decoded.

The maintainer authorizes considering replacement. [ADR-0024](adr/0024-share-process-backend-with-legacy-compatibility.md)
adopts a shared Process lifecycle, removes the dependency's iterator/promise path,
and preserves explicit tokens, binary streams, original deadlines and the 1 MiB
input bridge. Legacy exit waiting subscribes before checking HasExited; Windows
descendant termination retains the already-reviewed System.Management version.
New tests cover exact troublesome tokens/environment, already-exited processes,
nonzero status/stderr and early session exit releasing blocked input. Existing
stream-fault/noncooperative-output tests now also run on net48. The actual new
development dependency graph is audited separately from frozen alpha.2.

Final replacement PR and exact-main CI, Windows stress/actual artifacts and all
three-OS/native evidence are pending; no successful retry is claimed as a fix.
PR #25 retains the failed partial-fix evidence. Publication remains postponed.

追加診断でも通常net48テストが停止しました。promiseと消費側tokenは一致し、下位4Taskは
正常完了ですが上位は保留です。互換promiseの遅延TCS初期化に競合の可能性を確認しましたが、
継続グラフは未取得で原因の断定はしません。ADR-0024で既存Process実装を共有して依存の
列挙経路を外し、引数・終了通知・Windows子孫終了だけを補います。入力上限・EOF・所有権・
期限を保持し、net48のI/O障害、実引数、即時終了と入力待ち解除を検証します。
最終PRと正確なmainのCI／成果物確認前には1.0完了と扱わず、公開延期を保持します。

## English

CP-05 is complete through [PR #23](https://github.com/ShutenOishi/pgcli-sharp/pull/23):
main `f5f7ecbb4f5d04647bc3b2eb570d4e1aa2c503b3`,
[CI 37115096538](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/37115096538),
all 18 jobs at attempt 1, with independently inspected native Windows/macOS and
Linux 10–18 reports. Its separate exclusions remain in [CP-05](phase-8-native-os.md).

One 1.0 stabilization risk remains open. Windows net48 stalled in
[run 37111416924](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/37111416924)
in the zero-byte completion case and
[run 37112048520](https://github.com/ShutenOishi/pgcli-sharp/actions/runs/37112048520)
in the five-byte case during stress round 2. The latter parent trace records
`complete-returned` but no `completion-finished`; its child records `eof` and
`drained`. These prove neither child process exit nor the identity of the pending
task. Later successful runs do not establish a root-cause fix.

PR #24 passed all 18 PR CI jobs (37187722869), but exact-main CI 37188258263
at `1e794140c80b2d0092f2d1a4ae0ceb8d85d790fc` reproduced the zero-byte stall
in net48 stress round 3. The failed run is retained without retry.
Artifact 11297931986, SHA-256
`fb7e8b8a9a479e52035240cbda75dae890df0bfa2712a02a314c7874d299613b`,
was independently downloaded and checked. The actual CLR4 dump reader succeeded;
all four CliWrap stdIn/stdOut/stdErr/process tasks had flags `0x7000400`
(RanToCompletion set, Faulted/Canceled unset), while its ExecuteAsync state machine
remained at state 2 and wrapper completion flags were `0x2000400` (pending).
Thus, for this capture, unfinished process/pipe tasks do not explain the stall.
Reference-source Task flag definitions and pinned CliWrap/PolyShim source inform
the next investigation; they do not alone identify a historical root cause.

A follow-up adds deterministic session/one-shot regressions: start under a context
that queues callbacks without pumping, restore the caller's original context,
then require completion within five seconds without any posts to the occupied
startup context. Cleanup releases queued baseline continuations. Start-failure
coverage separately requires caller-context restoration. Before changing runtime
behavior, Windows CI must demonstrate the failing baseline. Baseline CI 37188939390
at PR #25 head `b18a9a7b8187408a232ad4d4d6753a04b00820bd` confirms two net48
failures: both controlled-context completions time out with two posts each.
Modern net8/net10 suites each pass 554 tests, and the start-error restoration test
passes on all targets. This is a deterministic context-dependence defect.

ADR-0023 fixes it by initializing the existing CliWrap command task synchronously
under a null SynchronizationContext and restoring the caller's context in finally.
Both finite and session runners use the same internal adapter. No Task.Run or
wider ExecutionContext suppression is used. Final exact PR/main CI and artifact
inspection remain the acceptance gate for this change. The known defect and the
historical captured stall are distinguished; no claim covers every past timeout.
The current [PR #25 validation receipt](https://github.com/ShutenOishi/pgcli-sharp/pull/25)
records the exact final PR/main commits, CI runs and independently inspected
diagnostic artifacts, preserving failures and superseded revisions. This avoids
an untested follow-up source change solely to embed the CI's own source SHA.

The context-isolation revision `c3d53b4bb23f3f7ee00638ce677028d8d5531f4e`
passed the full Windows suites and 26 net48 stress rounds in CI 37189457631,
then stalled in round 27 on the 262144-byte case. Its controlled-context tests
pass, so the confirmed startup-context fix is not sufficient for the intermittent
defect. The PR remains Draft; this failed run is not retried or merged.
Artifact 11297554452, SHA-256
`a4d56daa20274368b2f22c25f7fa4c246095ef6f0d2767ea4c176d543d07fded`,
was independently checked. Again, all four lower tasks are normally completed;
the outer ExecuteAsync is at state 2, while the internalized WhenEach iterator
is at state -4 with a completed stdout task as its current yielded value.

Further diagnosis reads bounded nested value-type fields (awaiters and the
ManualResetValueTaskSourceCore promise), including short tokens, completion flags
and captured-context type references, with three-level nesting and a global
10,000-field limit. ExecutionContext fields are decoded only as scalar flags and
type/address references; string/environment contents are still never read.
Iterator state -2 is retained rather than treated as an ordinary completed async
method. This is an evidence improvement, not another claimed runtime fix.
The owned CLR4 smoke fixture now keeps a nested struct alive and requires its
actual int/short/bool values (1729/37/true) from the dump, so a decoder that cannot
read interior field addresses cannot silently pass the readiness gate.

The prepared ClrMD reader had not previously compiled or read a dump in CI.
Windows now first captures an owned net48 `wait` helper using `MiniDumpWriteDump`
and requires CLR4, thread and fixture-managed frames from the actual dump.
Only that newly started child's PID is captured; it is terminated in cleanup.
The reader's temporary NuGet dependency remains outside the library/package.
Raw dumps are deleted even on analysis failures and excluded from artifact upload.
Only bounded stack/type/address/task-state diagnostics are retained; strings and
environment values are not printed. Relevant PolyShim and Process fields are
included without attributing the historical stall to either component.

The net48 completion/session stress loop is bounded to 30 rounds, stopping on the
first failure; modern-target loops stay at three rounds. Existing 15-second
session deadlines and two-minute blame watchdogs remain unchanged. Fresh final
PR/main CI and inspection of actual smoke/stress artifacts are required before
this diagnostic change is accepted. A successful bounded run is preparedness and
regression evidence, not proof that the intermittent defect is resolved.

The initial PR #24 diagnostics did not change production execution. The PR #25
follow-up deliberately changes only legacy startup context capture under ADR-0023.
Public API, runtime dependencies, frozen preview source and publication settings
remain unchanged. 1.0 acceptance and external publication remain pending.

## 日本語

CP-05はPR #23、main `f5f7ecbb4f5d04647bc3b2eb570d4e1aa2c503b3`の
CI 37115096538全18ジョブ成功と成果物の独立確認で完了しました。
Windows net48の間欠停止は引き続き未解明です。過去の0／5バイトケースが停止し、
5バイト時は子がEOF／flushを記録した後も親の完了が戻りませんでした。
子の実際の終了や停止中のTaskはこの記録だけでは分かりません。

解析器を毎回、専用に起動したnet48子プロセスの実ダンプで自己検証します。
CLR4・スレッド・fixtureの管理フレームを必須とし、子は後片付けで停止します。
net48反復は30回を上限に最初の失敗で止め、既存のタイムアウトは維持します。
生ダンプは削除しアップロードからも除外します。文字列や環境値を出力せず、
スタック・型・アドレス・Task状態のテキストを保持します。
PR #24のmainで再現した停止の実ダンプを解析し、下位4Taskの正常完了と上位処理の
保留を確認しました。PR #25の修正前CI 37188939390では、処理しない同期コンテキストを
使う回帰テストがnet48の通常実行・セッション双方で失敗し、それぞれ2回のPostを
観測しました。modern対象と起動失敗時のコンテキスト保持は成功しました。
ADR-0023に従い、CliWrap起動だけを同期コンテキストnullで初期化し、finallyで
呼び出し元の状態を戻します。同期的な起動エラー・実行コンテキスト・公開API・
依存・固定候補・公開設定は保持します。確認できた依存の欠陥を修正しますが、
過去の全停止の原因とは断定しません。最終PR／main CIと成果物確認を必須とし、
正確なcommit・run・成果物をPR #25の検証記録に保持します。
1.0判定と外部公開は保留します。
同期コンテキスト修正後も反復27回目で停止が再現し、PRはDraftのままです。
awaiter／promiseの構造体内部と継続先の型を上限付きで採取し、専用CLR4子の
既知のint／short／boolを実ダンプから読めることも必須にします。
