"""Record actual binaries/server/TRX outcomes; never infer pass from missing tests."""
import json
import hashlib
import os
import platform
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def require(condition, *details):
    if not condition:
        raise ValueError(str(details))


def record(output):
    major = int(os.environ["PGCLI_REAL_PG_MAJOR"])
    binary = Path(os.environ["PGCLI_REAL_PG_BIN"])
    evidence = {
        "schemaVersion": 1,
        "sourceCommit": os.environ.get("GITHUB_SHA"),
        "testedHeadSha": os.environ.get("PGCLI_EVIDENCE_HEAD_SHA"),
        "workflowRunId": os.environ.get("GITHUB_RUN_ID"),
        "major": major,
        "os": platform.platform(),
        "runner": "ubuntu-24.04",
        "runnerImage": os.environ.get("ImageVersion"),
        "architecture": platform.machine(),
        "tfm": "net10.0",
        "result": "failed",
        "exclusions": {
            "Windows/macOS real binaries": "Not configured in this Linux source-build matrix; 3-OS unit tests are separate evidence.",
            "Windows service lifecycle": "Requires a separate privileged Windows service fixture; not exercised here.",
            "TLS/ICU/TTY": "Source builds omit SSL, ICU and Readline; redirected-pipe tests do not prove these features.",
            "other patches/upgrade pairs": "Only pinned patches and the explicit 16-to-18 copy-upgrade pair are exercised.",
        },
    }
    try:
        manifest = json.loads(Path("eng/postgresql-source-versions.json").read_text())
        pinned = next(row for row in manifest["versions"] if row["major"] == major)
        evidence["sourceBuild"] = json.loads((binary.parent / "BUILD-SOURCE.json").read_text())
        require(evidence["sourceBuild"]["sha256"] == pinned["sha256"], "source hash mismatch")
        tools = ["postgres", "psql", "pg_dump", "pg_restore", "pgbench", "initdb", "pg_ctl", "pg_resetwal", "pg_rewind", "pg_basebackup", "pg_upgrade"]
        if major >= 12:
            tools.append("pg_checksums")
        else:
            evidence["exclusions"]["checksum check/disable/enable"] = "pg_checksums does not exist before PostgreSQL 12."
        evidence["cliVersions"] = {}
        for tool in tools:
            raw = subprocess.check_output([str(binary / tool), "--version"], text=True, timeout=15).strip()
            numeric = re.search(r"\(PostgreSQL\) (\d+\.\d+)", raw).group(1)
            require(numeric == pinned["version"], tool, raw, pinned)
            evidence["cliVersions"][tool] = {"numeric": numeric, "raw": raw, "binarySha256": hashlib.sha256((binary / tool).read_bytes()).hexdigest()}
        sql = subprocess.check_output([
            str(binary / "psql"), "-X", "-h", os.environ["PGCLI_REAL_PG_HOST"],
            "-p", os.environ["PGCLI_REAL_PG_PORT"], "-U", os.environ["PGCLI_REAL_PG_USER"],
            "-d", "postgres", "-Atqc", "SELECT current_setting('server_version'), current_setting('server_version_num')",
        ], text=True, timeout=15).strip().split("|")
        evidence["serverVersion"] = sql[0]
        evidence["serverVersionNumber"] = int(sql[1])
        require(int(sql[1]) == major * 10000 + int(pinned["version"].split(".")[1]), "server version mismatch", sql)
        results = {}
        for trx in Path(output).glob("*.trx"):
            for item in ET.parse(trx).getroot().iter():
                if item.tag.rsplit("}", 1)[-1] == "UnitTestResult":
                    results[item.attrib["testName"]] = item.attrib["outcome"]
        expected = [
            "RepresentativeRealPostgreSql_BackupRestorePsqlSessionAndPgBench",
            "OwnedDisposableCluster_InitializesChecksumsDryRunAndServerLifecycle",
        ]
        if major >= 13:
            expected.append("DivergentOwnedCluster_RewindsAndPreservesSourceRows")
        else:
            evidence["exclusions"]["divergent rewind"] = "This fixture uses --write-recovery-conf (13+); manual historical recovery configuration is not implemented."
        if major == 18:
            expected.append("OwnedClusters_CopyUpgrade16To18PreservesRows")
            old_binary = Path(os.environ["PGCLI_REAL_PG_UPGRADE_FROM_BIN"])
            evidence["upgradeSourceBuild"] = json.loads((old_binary.parent / "BUILD-SOURCE.json").read_text())
            old_pin = next(row for row in manifest["versions"] if row["major"] == 16)
            require(evidence["upgradeSourceBuild"]["sha256"] == old_pin["sha256"], "old source hash mismatch")
            evidence["upgradeSourceCli"] = subprocess.check_output([str(old_binary / "postgres"), "--version"], text=True, timeout=15).strip()
            require(evidence["upgradeSourceCli"] == "postgres (PostgreSQL) " + old_pin["version"], "old CLI mismatch")
        evidence["scenarios"] = {}
        for name in expected:
            outcomes = [value for key, value in results.items() if key.endswith(name)]
            require(outcomes == ["Passed"], name, outcomes)
            evidence["scenarios"][name] = "passed"
        evidence["result"] = "passed"
    except Exception as error:
        evidence["failure"] = str(error)
    Path(output).mkdir(parents=True, exist_ok=True)
    Path(output, "real-postgresql-evidence.json").write_text(json.dumps(evidence, indent=2) + "\n")
    print(json.dumps(evidence, indent=2))
    return 0 if evidence["result"] == "passed" else 1


if __name__ == "__main__":
    sys.exit(record(sys.argv[1]))
