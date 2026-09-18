using System.Globalization;
using System.Resources;

namespace PgCliSharp.Internal.Localization;

internal static class MessageProvider
{
    private static readonly ResourceManager ResourceManager =
        new ResourceManager("PgCliSharp.Resources.Messages", typeof(MessageProvider).Assembly);

    internal static string GetString(string key, CultureInfo? culture = null)
    {
        return ResourceManager.GetString(key, culture ?? CultureInfo.CurrentUICulture) ?? key;
    }

    internal static string Format(string key, params object[] arguments)
    {
        CultureInfo culture = CultureInfo.CurrentUICulture;
        return string.Format(culture, GetString(key, culture), arguments);
    }
}
