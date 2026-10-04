# Windows net48 completion investigation / 完了待ち調査

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

No production execution behavior, public API, frozen preview source or publication
settings change. 1.0 acceptance and external publication remain pending.

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
成功した場合も原因解決とは扱いません。実行実装・公開API・固定候補・公開設定は
変更せず、1.0判定と外部公開は保留します。
