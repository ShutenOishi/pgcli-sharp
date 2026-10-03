"""Owned PostgreSQL 18 native fixture; no publication or user-cluster access."""
import csv
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import socket
import subprocess
import sys
import tarfile
import tempfile
import urllib.request
import zipfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
SCENARIO = "RepresentativeRealPostgreSql_BackupRestorePsqlSessionAndPgBench"
WIN_FLEX_SHA = "8d324b62be33604b2c45ad1dd34ab93d722534448f55a16ca7292de32b6ac135"


def pin():
    return next(row for row in json.loads((ROOT / "eng/postgresql-source-versions.json").read_text())["versions"] if row["major"] == 18)


def run(*args):
    return subprocess.check_output([str(arg) for arg in args], text=True, timeout=120).strip()


def executable(binary, name):
    return Path(binary) / (name + (".exe" if os.name == "nt" else ""))


def download(url, path, expected):
    with urllib.request.urlopen(url, timeout=60) as response:
        path.write_bytes(response.read())
    if hashlib.sha256(path.read_bytes()).hexdigest() != expected:
        raise ValueError("Downloaded source/tool hash mismatch")


def build():
    row = pin()
    work = Path(tempfile.mkdtemp(prefix="pgcli-native-build-", dir=os.environ["RUNNER_TEMP"]))
    prefix = Path(os.environ["RUNNER_TEMP"]) / "pgcli-native-prefix"
    url = f'https://ftp.postgresql.org/pub/source/v{row["version"]}/postgresql-{row["version"]}.tar.bz2'
    archive = work / "source.tar.bz2"
    download(url, archive, row["sha256"])
    with tarfile.open(archive) as source:
        source.extractall(work, filter="data")
    if os.name == "nt":
        tools = work / "winflexbison"
        tools.mkdir()
        package = work / "winflexbison.zip"
        download("https://github.com/lexxmark/winflexbison/releases/download/v2.5.25/win_flex_bison-2.5.25.zip", package, WIN_FLEX_SHA)
        with zipfile.ZipFile(package) as source:
            if any(Path(n).is_absolute() or ".." in Path(n).parts for n in source.namelist()):
                raise ValueError("Unsafe tool archive")
            source.extractall(tools)
        os.environ["PATH"] = str(tools) + os.pathsep + os.environ["PATH"]
    options = ["--buildtype=release", "-Dssl=none"] + [f"-D{name}=disabled" for name in
        ("icu", "readline", "zlib", "zstd", "lz4", "plperl", "plpython", "pltcl", "nls", "docs", "docs_pdf", "tap_tests", "gssapi", "ldap", "libxml", "libxslt", "libcurl", "liburing")]
    if os.name == "nt":
        options.append("--vsenv")
    setup = ["meson", "setup", str(work / "build"), str(work / ('postgresql-' + row['version'])), "--prefix=" + str(prefix), *options]
    for name, command in (("configure", setup), ("build", ["meson", "compile", "-C", str(work / "build"), "-j", "2"]), ("install", ["meson", "install", "-C", str(work / "build")])):
        log = work / (name + ".log")
        with log.open("w") as output:
            result = subprocess.run(command, stdout=output, stderr=subprocess.STDOUT, timeout=900)
        if result.returncode:
            print(log.read_text(errors="replace")[-16000:])
            raise RuntimeError(name + " failed; " + str(log))
    binary = prefix / "bin"
    if run(executable(binary, "postgres"), "--version") != "postgres (PostgreSQL) " + row["version"]:
        raise ValueError("Built executable version mismatch")
    compiler = json.loads((work / "build/meson-info/intro-compilers.json").read_text())
    receipt = dict(row, url=url, system=platform.system(), architecture=platform.machine(), compiler=compiler,
                   meson=run("meson", "--version"), ninja=run("ninja", "--version"), options=options,
                   winFlexBisonSha256=WIN_FLEX_SHA if os.name == "nt" else None)
    (prefix / "BUILD-SOURCE.json").write_text(json.dumps(receipt, indent=2), encoding="utf-8")
    export({"PGCLI_REAL_PG_BIN": str(binary), "PGCLI_REAL_PG_MAJOR": "18", "PGCLI_REAL_PG_REQUIRED": "true"})


def export(values):
    with open(os.environ["GITHUB_ENV"], "a", encoding="utf-8") as output:
        for name, value in values.items():
            if "\n" in value or "\r" in value:
                raise ValueError("Invalid environment value")
            output.write(name + "=" + value + "\n")


def setup():
    root = Path(tempfile.mkdtemp(prefix="pgcli-native-", dir=os.environ["RUNNER_TEMP"] if os.name == "nt" else "/tmp"))
    owner = {"root": str(root), "binary": os.environ["PGCLI_REAL_PG_BIN"], "run": os.environ["GITHUB_RUN_ID"]}
    (root / "OWNER.json").write_text(json.dumps(owner), encoding="utf-8")
    export({"PGCLI_REAL_PG_ROOT": str(root)})
    if os.name == "nt":
        # mkdtemp's OWNER RIGHTS ACL may resolve to the Administrators owner.
        # PostgreSQL removes that group from its token: grant the actual user SID
        # explicitly, on this newly owned directory only, for the restricted child.
        sid = next(csv.reader([run("whoami", "/user", "/fo", "csv", "/nh")]))[1]
        if not re.fullmatch(r"S-\d+(?:-\d+)+", sid):
            raise ValueError("Invalid current Windows user SID")
        run("icacls", root, "/grant", "*" + sid + ":(OI)(CI)F")
        (root / "WINDOWS-ACL.json").write_text(json.dumps({"userSid": sid, "acl": run("icacls", root)}, indent=2), encoding="utf-8")
    binary = Path(owner["binary"])
    user = "pgcli_owned"
    run(executable(binary, "initdb"), "-D", root / "data", "-A", "trust", "-U", user, "--locale=C", "--encoding=UTF8")
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        port = str(listener.getsockname()[1])
    host = "127.0.0.1" if os.name == "nt" else str(root)
    options = f"-c listen_addresses={'127.0.0.1' if os.name == 'nt' else chr(39) + chr(39)} -p {port}"
    if os.name != "nt":
        options += " -k " + str(root)
    run(executable(binary, "pg_ctl"), "-D", root / "data", "-l", root / "server.log", "-t", "30", "-w", "-o", options, "start")
    for database in ("pgclisharp_source", "pgclisharp_target", "pgclisharp_bench"):
        run(executable(binary, "createdb"), "-h", host, "-p", port, "-U", user, database)
    export({"PGCLI_REAL_PG_HOST": host, "PGCLI_REAL_PG_PORT": port, "PGCLI_REAL_PG_USER": user,
            "PGCLI_REAL_PG_SOURCE_DB": "pgclisharp_source", "PGCLI_REAL_PG_TARGET_DB": "pgclisharp_target", "PGCLI_REAL_PG_BENCH_DB": "pgclisharp_bench"})


def owned():
    root = Path(os.environ["PGCLI_REAL_PG_ROOT"])
    receipt = json.loads((root / "OWNER.json").read_text())
    if receipt != {"root": str(root), "binary": os.environ["PGCLI_REAL_PG_BIN"], "run": os.environ["GITHUB_RUN_ID"]}:
        raise ValueError("Fixture ownership mismatch")
    return root


def check_trx(folder):
    results = []
    for file in Path(folder).glob("*.trx"):
        results.extend(item.attrib for item in ET.parse(file).getroot().iter() if item.tag.rsplit("}", 1)[-1] == "UnitTestResult")
    matches = [item["outcome"] for item in results if item["testName"].endswith(SCENARIO)]
    if matches != ["Passed"] or any(item["outcome"] != "Passed" for item in results):
        raise ValueError("Required native scenario missing, duplicate or failed")


def collect(folder):
    output = Path(folder)
    output.mkdir(parents=True, exist_ok=True)
    evidence = {"schemaVersion": 1, "sourceCommit": os.environ.get("GITHUB_SHA"), "testedHeadSha": os.environ.get("PGCLI_EVIDENCE_HEAD_SHA"),
                "workflowRunId": os.environ.get("GITHUB_RUN_ID"), "system": platform.system(), "architecture": platform.machine(),
                "runnerImage": os.environ.get("ImageVersion"), "tfm": "net10.0", "major": 18, "result": "failed",
                "exclusions": ["other majors/patches on Windows/macOS", "native net8/net48 real CLI", "service lifecycle", "TLS/ICU/TTY", "compression libraries", "native migration/rewind and unlisted destructive scenarios"]}
    try:
        root = owned()
        row = pin()
        binary = Path(os.environ["PGCLI_REAL_PG_BIN"])
        build_receipt = json.loads((binary.parent / "BUILD-SOURCE.json").read_text())
        if any(build_receipt[key] != row[key] for key in ("major", "version", "sha256")) or build_receipt["system"] != platform.system():
            raise ValueError("Source/platform provenance mismatch")
        evidence["sourceBuild"] = build_receipt
        if os.name == "nt":
            evidence["ownedWindowsAcl"] = json.loads((root / "WINDOWS-ACL.json").read_text())
        evidence["cliVersions"] = {}
        for name in ("postgres", "initdb", "pg_ctl", "createdb", "psql", "pg_dump", "pg_restore", "pgbench"):
            path = executable(binary, name)
            raw = run(path, "--version")
            if not raw.endswith("(PostgreSQL) " + row["version"]):
                raise ValueError("CLI version mismatch: " + name)
            evidence["cliVersions"][name] = {"raw": raw, "binarySha256": hashlib.sha256(path.read_bytes()).hexdigest()}
        sql = run(executable(binary, "psql"), "-X", "-h", os.environ["PGCLI_REAL_PG_HOST"], "-p", os.environ["PGCLI_REAL_PG_PORT"], "-U", os.environ["PGCLI_REAL_PG_USER"], "-d", "postgres", "-Atqc", "SELECT current_setting('server_version_num'), current_setting('data_directory')")
        version, data = sql.split("|", 1)
        if int(version) != 180000 + int(row["version"].split(".")[1]) or Path(data).resolve() != (root / "data").resolve():
            raise ValueError("Server version/owned data directory mismatch")
        check_trx(output)
        evidence.update(serverVersionNumber=int(version), transport="loopback TCP" if os.name == "nt" else "Unix socket", scenarios={SCENARIO: "passed"}, result="passed")
    except Exception as error:
        evidence["failure"] = str(error)
    (output / "native-postgresql-evidence.json").write_text(json.dumps(evidence, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(evidence, indent=2))
    return 0 if evidence["result"] == "passed" else 1


def stop():
    if not os.environ.get("PGCLI_REAL_PG_ROOT"):
        return
    root = owned()
    if (root / "data/postmaster.pid").exists():
        run(executable(os.environ["PGCLI_REAL_PG_BIN"], "pg_ctl"), "-D", root / "data", "-m", "fast", "-t", "30", "-w", "stop")


if __name__ == "__main__":
    if sys.argv[1] == "collect":
        sys.exit(collect(sys.argv[2]))
    {"build": build, "setup": setup, "stop": stop}[sys.argv[1]]()
