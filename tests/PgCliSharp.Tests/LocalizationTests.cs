using System.Globalization;
using System.Xml.Linq;
using PgCliSharp.Internal.Localization;

namespace PgCliSharp.Tests;

public sealed class LocalizationTests
{
    [Fact]
    public void EveryRequiredMessageKey_HasEnglishAndJapaneseResources()
    {
        var english = CultureInfo.GetCultureInfo("en-US");
        var japanese = CultureInfo.GetCultureInfo("ja-JP");

        foreach (string key in MessageKeys.All)
        {
            string en = MessageProvider.GetString(key, english);
            string ja = MessageProvider.GetString(key, japanese);

            Assert.NotEqual(key, en);
            Assert.NotEqual(key, ja);
            Assert.NotEqual(en, ja);
        }
    }

    [Fact]
    public void ResourceFiles_HaveIdenticalKeysAndCompositeFormatPlaceholders()
    {
        Dictionary<string, string> english = ReadResource("Messages.resx");
        Dictionary<string, string> japanese = ReadResource("Messages.ja.resx");

        Assert.Equal(
            english.Keys.OrderBy(key => key, StringComparer.Ordinal),
            japanese.Keys.OrderBy(key => key, StringComparer.Ordinal));

        foreach (string key in english.Keys)
        {
            Assert.Equal(
                GetCompositeFormatPlaceholderIndexes(english[key]),
                GetCompositeFormatPlaceholderIndexes(japanese[key]));
        }
    }

    [Fact]
    public void CompositeFormatParser_HandlesEscapedBracesAndRejectsMalformedInput()
    {
        Assert.Equal(
            new[] { 1, 2 },
            GetCompositeFormatPlaceholderIndexes(
                "literal {{0}} then {2:N0} and {1,-8} and {2}"));

        Assert.Throws<FormatException>(
            () => GetCompositeFormatPlaceholderIndexes("missing close {0"));
        Assert.Throws<FormatException>(
            () => GetCompositeFormatPlaceholderIndexes("unexpected } close"));
    }

    [Fact]
    public void UnknownCulture_FallsBackToNeutralEnglishResource()
    {
        string english = MessageProvider.GetString(
            MessageKeys.ProcessExitedWithError,
            CultureInfo.GetCultureInfo("en-US"));
        string fallback = MessageProvider.GetString(
            MessageKeys.ProcessExitedWithError,
            CultureInfo.GetCultureInfo("fr-FR"));

        Assert.Equal(english, fallback);
    }

    [Fact]
    public void VersionMismatchException_LocalizesMessageButKeepsStructuredData()
    {
        CultureInfo original = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var english = new PgExecutableVersionMismatchException(
                "/tools/pg_dump",
                PostgreSqlMajorVersion.V18,
                17);

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            var japanese = new PgExecutableVersionMismatchException(
                "/tools/pg_dump",
                PostgreSqlMajorVersion.V18,
                17);

            Assert.NotEqual(english.Message, japanese.Message);
            Assert.Equal(english.ExecutablePath, japanese.ExecutablePath);
            Assert.Equal(english.ExpectedVersion, japanese.ExpectedVersion);
            Assert.Equal(english.ActualMajorVersion, japanese.ActualMajorVersion);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    private static Dictionary<string, string> ReadResource(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "resx", fileName);
        XDocument document = XDocument.Load(path);

        return document.Root!
            .Elements("data")
            .Where(element => element.Attribute("name") is not null)
            .ToDictionary(
                element => element.Attribute("name")!.Value,
                element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);
    }

    private static int[] GetCompositeFormatPlaceholderIndexes(string format)
    {
        var indexes = new SortedSet<int>();

        for (int index = 0; index < format.Length; index++)
        {
            char current = format[index];

            if (current == '{')
            {
                if (index + 1 < format.Length && format[index + 1] == '{')
                {
                    index++;
                    continue;
                }

                int itemStart = index + 1;
                int close = format.IndexOf('}', itemStart);
                if (close < 0)
                {
                    throw new FormatException("Composite format item is not closed.");
                }

                string item = format.Substring(itemStart, close - itemStart);
                int delimiter = item.IndexOfAny(new[] { ',', ':' });
                string indexText = (delimiter < 0 ? item : item.Substring(0, delimiter)).Trim();

                if (!int.TryParse(
                        indexText,
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out int placeholderIndex))
                {
                    throw new FormatException("Composite format item has an invalid index.");
                }

                indexes.Add(placeholderIndex);
                index = close;
                continue;
            }

            if (current == '}')
            {
                if (index + 1 < format.Length && format[index + 1] == '}')
                {
                    index++;
                    continue;
                }

                throw new FormatException("Composite format contains an unmatched closing brace.");
            }
        }

        return indexes.ToArray();
    }
}
