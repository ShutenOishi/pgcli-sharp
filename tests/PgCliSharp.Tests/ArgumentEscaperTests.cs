using PgCliSharp.Internal.Execution;

namespace PgCliSharp.Tests;

public sealed class ArgumentEscaperTests
{
    [Theory]
    [InlineData("simple", "simple")]
    [InlineData("", """")]
    [InlineData("hello world", ""hello world"")]
    [InlineData("a"b", ""a\"b"")]
    public void Escape_ProducesProcessStartInfoCompatibleToken(string value, string expected)
    {
        Assert.Equal(expected, ArgumentEscaper.Escape(value));
    }

    [Fact]
    public void Escape_QuotedValueWithTrailingBackslash_DoublesTrailingBackslash()
    {
        string escaped = ArgumentEscaper.Escape(@"C:\Program Files\PostgreSQL\");

        Assert.Equal(""C:\Program Files\PostgreSQL\\"", escaped);
    }

    [Fact]
    public void JoinForProcessStartInfo_PreservesArgumentBoundaries()
    {
        string joined = ArgumentEscaper.JoinForProcessStartInfo(
            new[] { "--file", @"C:\backup files\dump.bin", "--verbose" });

        Assert.Equal("--file "C:\backup files\dump.bin" --verbose", joined);
    }
}
