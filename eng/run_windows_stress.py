"""Bound each owned VSTest invocation, including host/collector shutdown."""
import json
import os
from pathlib import Path
import subprocess
import sys
import time
from datetime import datetime, timezone


def record(folder, **fields):
    with (Path(folder) / "stress-commands.jsonl").open("a", encoding="utf-8") as output:
        output.write(json.dumps({"utc": datetime.now(timezone.utc).isoformat(), **fields}) + "\n")


def one_round(folder, tfm, number):
    result_folder = Path(folder) / f"stress-{tfm}-{number}"
    result_folder.mkdir(parents=True, exist_ok=True)
    record(folder, event="starting", tfm=tfm, round=number)
    command = ["dotnet", "test", "tests/PgCliSharp.Tests/PgCliSharp.Tests.csproj", "-c", "Release", "-f", tfm,
               "--no-build", "--no-restore", "--filter", "FullyQualifiedName~PsqlCompletionTests|FullyQualifiedName~ProcessSessionRunnerTests",
               "--logger", "trx", "--results-directory", str(result_folder), "--blame-hang-timeout", "2m",
               "--blame-hang-dump-type", "full", "--diag", str(result_folder / "vstest.log")]
    started = time.monotonic()
    process = subprocess.Popen(command)
    record(folder, event="started", tfm=tfm, round=number, pid=process.pid)
    try:
        result = process.wait(timeout=180)
    except subprocess.TimeoutExpired:
        # Only the invocation started here and its descendants are targeted.
        # Session deadlines and the existing testcase blame timeout are unchanged.
        record(folder, event="command-timeout", tfm=tfm, round=number, pid=process.pid)
        try:
            if process.poll() is None:
                killer = Path(os.environ["SystemRoot"]) / "System32/taskkill.exe"
                subprocess.run([str(killer), "/PID", str(process.pid), "/T", "/F"],
                               check=False, timeout=20, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        finally:
            if process.poll() is None:
                process.kill()
            process.wait(timeout=10)
        result = 124
    record(folder, event="finished", tfm=tfm, round=number, pid=process.pid,
           exitCode=result, seconds=round(time.monotonic() - started, 3))
    return result


def run(folder):
    Path(folder).mkdir(parents=True, exist_ok=True)
    for tfm, rounds in (("net48", 30), ("net8.0", 3), ("net10.0", 3)):
        for number in range(1, rounds + 1):
            result = one_round(folder, tfm, number)
            if result != 0:
                return result
    return 0


if __name__ == "__main__":
    if os.name != "nt":
        raise SystemExit("Windows stress must run on Windows.")
    sys.exit(run(sys.argv[1]))
