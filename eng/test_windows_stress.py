import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import Mock, patch
import run_windows_stress as stress


class WindowsStressTests(unittest.TestCase):
    def test_command_timeout_targets_only_started_pid_and_is_a_failure(self):
        process = Mock(pid=1729)
        process.wait.side_effect = [subprocess.TimeoutExpired("owned", 180), 1]
        process.poll.return_value = None
        with tempfile.TemporaryDirectory() as folder, patch.object(stress.subprocess, "Popen", return_value=process), \
             patch.object(stress.subprocess, "run") as terminate, patch.dict(stress.os.environ, {"SystemRoot": "fixture-root"}):
            self.assertEqual(124, stress.one_round(folder, "net48", 1))
            self.assertEqual(terminate.call_args.args[0][1:], ["/PID", "1729", "/T", "/F"])
            process.kill.assert_called_once()
            records = [json.loads(line) for line in (Path(folder) / "stress-commands.jsonl").read_text().splitlines()]
            self.assertEqual(["starting", "started", "command-timeout", "finished"], [r["event"] for r in records])
            self.assertEqual(124, records[-1]["exitCode"])

    def test_nonzero_command_stops_before_any_retry_or_next_round(self):
        with tempfile.TemporaryDirectory() as folder, patch.object(stress, "one_round", side_effect=[0, 1]) as execute:
            self.assertEqual(1, stress.run(folder))
            self.assertEqual(2, execute.call_count)
            self.assertEqual([1, 2], [c.args[2] for c in execute.call_args_list])

    def test_successful_command_retains_actual_exit_and_elapsed_record(self):
        process = Mock(pid=37)
        process.wait.return_value = 0
        with tempfile.TemporaryDirectory() as folder, patch.object(stress.subprocess, "Popen", return_value=process):
            self.assertEqual(0, stress.one_round(folder, "net48", 1))
            records = [json.loads(line) for line in (Path(folder) / "stress-commands.jsonl").read_text().splitlines()]
            self.assertEqual(0, records[-1]["exitCode"])
            process.kill.assert_not_called()


if __name__ == "__main__":
    unittest.main()
