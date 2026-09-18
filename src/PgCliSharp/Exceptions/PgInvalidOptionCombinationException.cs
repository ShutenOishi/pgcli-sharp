using System.Collections.ObjectModel;
using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a combination of typed options that is invalid for the selected PostgreSQL CLI major version.</para>
/// <para>JA: 選択した PostgreSQL CLI メジャーバージョンでは無効となる型付きオプションの組み合わせを表します。</para>
/// </summary>
public sealed class PgInvalidOptionCombinationException : PgCliSharpException
{
    internal PgInvalidOptionCombinationException(
        PostgreSqlMajorVersion selectedVersion,
        params string[] optionNames)
        : base(
            MessageProvider.Format(
                MessageKeys.InvalidOptionCombination,
                string.Join(", ", optionNames),
                (int)selectedVersion))
    {
        SelectedVersion = selectedVersion;
        OptionNames = new ReadOnlyCollection<string>((string[])optionNames.Clone());
    }

    /// <summary><para>EN: Gets the selected PostgreSQL CLI major version.</para><para>JA: 選択された PostgreSQL CLI メジャーバージョンを取得します。</para></summary>
    public PostgreSqlMajorVersion SelectedVersion { get; }

    /// <summary><para>EN: Gets the option names that participate in the invalid combination.</para><para>JA: 無効な組み合わせに含まれるオプション名を取得します。</para></summary>
    public IReadOnlyList<string> OptionNames { get; }
}
