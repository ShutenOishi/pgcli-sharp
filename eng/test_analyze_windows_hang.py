"""Failure-path checks for test-only memory diagnostics."""
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import MagicMock, patch

import analyze_windows_hang as analyzer


class HangReaderTests(unittest.TestCase):
    def test_build_timeout_removes_all_raw_dumps(self):
        with tempfile.TemporaryDirectory() as root:
            folder = Path(root) / "results"
            folder.mkdir()
            dumps = [folder / "testhost.dmp", folder / "child.dmp"]
            for dump in dumps:
                dump.write_bytes(b"private-test-memory")
            with patch.dict(os.environ, {"RUNNER_TEMP": root}), patch.object(analyzer.subprocess, "run", side_effect=subprocess.TimeoutExpired("dotnet", 180)):
                with self.assertRaises(subprocess.TimeoutExpired):
                    analyzer.main(folder)
            self.assertFalse(any(dump.exists() for dump in dumps))

    def test_invalid_smoke_stacks_fail_and_clean_up_owned_child(self):
        with tempfile.TemporaryDirectory() as root:
            child = MagicMock(pid=12345)
            child.poll.return_value = None

            def start(*args, **kwargs):
                Path(kwargs["env"]["PGCLI_TEST_TRACE_FILE"]).write_text("ready", encoding="utf-8")
                return child

            def run(command, **kwargs):
                if "--capture" in command:
                    Path(command[-1]).write_bytes(b"private-test-memory")
                else:
                    kwargs["stdout"].write("CLR 4.0\nTHREAD 1\n")

            with patch.object(analyzer.subprocess, "Popen", side_effect=start), patch.object(analyzer.subprocess, "run", side_effect=run):
                with self.assertRaisesRegex(RuntimeError, "expected managed frames"):
                    analyzer.smoke(Path(root) / "reader.dll", root, Path(root) / "owned.exe")
            child.kill.assert_called_once()
            child.wait.assert_called_once_with(timeout=10)
            self.assertFalse((Path(root) / "stack-reader-smoke.dmp").exists())


if __name__ == "__main__":
    unittest.main()
