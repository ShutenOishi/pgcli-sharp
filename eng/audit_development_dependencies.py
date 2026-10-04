"""Audit new development assets independently of the frozen publication candidate."""
import json
from pathlib import Path
import sys
from audit_candidate import audit_dependencies

EXPECTED = {
    "System.Management/10.0.10", "System.CodeDom/10.0.10",
}


def audit(consumer, output):
    reports = {}
    for tfm in ("netstandard2.0", "net8.0", "net10.0"):
        records = audit_dependencies(Path(consumer) / tfm / "obj/project.assets.json")
        runtime = {record["package"] for record in records
                   if record["runtime_frameworks"] and not record["package"].startswith("PgCliSharp/")}
        assert runtime == (EXPECTED if tfm == "netstandard2.0" else set()), (tfm, runtime)
        reports[tfm] = records
    Path(output).write_text(json.dumps({"publication_candidate": False, "dependencies": reports},
                                      ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    audit(sys.argv[1], sys.argv[2])
