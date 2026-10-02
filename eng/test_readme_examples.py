"""Synthetic tooling tests; actual compilation is independently required in CI."""
import contextlib
import io
from pathlib import Path
import tempfile
import unittest
from unittest import mock

import compile_readme_examples as compiler


class ReadmeCompilerTests(unittest.TestCase):
    def test_extracts_every_block_and_only_invokes_build(self):
        def inspect_build(command, **kwargs):
            self.assertEqual(command[:2], ["dotnet", "build"])
            project = Path(command[2])
            code = (project.parent / "Examples.cs").read_text(encoding="utf-8")
            self.assertEqual(code.count("static async Task Example"), 12)
            self.assertIn("WriteAsync(commands, 0, commands.Length)", code)
            self.assertIn("net8.0;net10.0", project.read_text(encoding="utf-8"))
            self.assertTrue(kwargs["check"])
        with mock.patch.object(compiler.subprocess, "run", side_effect=inspect_build) as build, contextlib.redirect_stdout(io.StringIO()):
            compiler.main()
        build.assert_called_once()

    def test_missing_csharp_blocks_fails_without_build(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "eng").mkdir()
            (root / "README.md").write_text("No snippets / 例なし", encoding="utf-8")
            with mock.patch.object(compiler, "__file__", str(root / "eng" / "compile-readme-examples.py")), mock.patch.object(compiler.subprocess, "run") as build:
                with self.assertRaisesRegex(ValueError, "Missing C# examples"):
                    compiler.main()
                build.assert_not_called()


if __name__ == "__main__":
    unittest.main()
