using System.Globalization;
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
}
