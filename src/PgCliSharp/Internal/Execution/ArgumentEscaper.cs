using System.Text;

namespace PgCliSharp.Internal.Execution;

internal static class ArgumentEscaper
{
    internal static string JoinForProcessStartInfo(IEnumerable<string> arguments)
    {
        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        var builder = new StringBuilder();
        bool first = true;

        foreach (string argument in arguments)
        {
            if (!first)
            {
                builder.Append(' ');
            }

            builder.Append(Escape(argument));
            first = false;
        }

        return builder.ToString();
    }

    internal static string Escape(string argument)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(nameof(argument));
        }

        if (argument.Length == 0)
        {
            return """";
        }

        bool requiresQuotes = false;
        for (int i = 0; i < argument.Length; i++)
        {
            if (char.IsWhiteSpace(argument[i]) || argument[i] == '"')
            {
                requiresQuotes = true;
                break;
            }
        }

        if (!requiresQuotes)
        {
            return argument;
        }

        var builder = new StringBuilder(argument.Length + 2);
        builder.Append('"');

        int backslashCount = 0;
        foreach (char character in argument)
        {
            if (character == '\\')
            {
                backslashCount++;
                continue;
            }

            if (character == '"')
            {
                builder.Append('\\', (backslashCount * 2) + 1);
                builder.Append('"');
                backslashCount = 0;
                continue;
            }

            builder.Append('\\', backslashCount);
            builder.Append(character);
            backslashCount = 0;
        }

        builder.Append('\\', backslashCount * 2);
        builder.Append('"');
        return builder.ToString();
    }
}
