using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Identifies how a PostgreSQL 17+ pg_dumpall database-exclusion filter is supplied.</para>
/// <para>JA: PostgreSQL 17 以降の pg_dumpall データベース除外フィルターの供給方法を表します。</para>
/// </summary>
public enum PgDumpAllFilterSourceKind
{
    /// <summary><para>EN: Read filter rules from a named file.</para><para>JA: 指定したファイルからフィルタールールを読み込みます。</para></summary>
    File,

    /// <summary><para>EN: Read filter rules from standard input using --filter=-.</para><para>JA: --filter=- を使用して標準入力からフィルタールールを読み込みます。</para></summary>
    StandardInput,
}

/// <summary>
/// <para>EN: Represents a PostgreSQL 17+ pg_dumpall filter source. Upstream permits only exclude-database filter entries.</para>
/// <para>JA: PostgreSQL 17 以降の pg_dumpall フィルター入力元を表します。上流では database の exclude フィルターのみ使用できます。</para>
/// </summary>
public sealed class PgDumpAllFilterSource
{
    private PgDumpAllFilterSource(
        PgDumpAllFilterSourceKind kind,
        string? path,
        Stream? input)
    {
        Kind = kind;
        Path = path;
        Input = input;
    }

    /// <summary><para>EN: Gets the source kind.</para><para>JA: 入力元の種類を取得します。</para></summary>
    public PgDumpAllFilterSourceKind Kind { get; }

    /// <summary><para>EN: Gets the filter file path.</para><para>JA: フィルターファイルパスを取得します。</para></summary>
    public string? Path { get; }

    /// <summary><para>EN: Gets the caller-owned standard-input stream.</para><para>JA: 呼び出し側所有の標準入力ストリームを取得します。</para></summary>
    public Stream? Input { get; }

    /// <summary><para>EN: Creates a file-backed filter source.</para><para>JA: ファイルを使用するフィルター入力元を作成します。</para></summary>
    /// <param name="path"><para>EN: Filter file path.</para><para>JA: フィルターファイルパスです。</para></param>
    /// <returns><para>EN: The filter source.</para><para>JA: フィルター入力元です。</para></returns>
    public static PgDumpAllFilterSource FromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.FilterPathRequired),
                nameof(path));
        }

        return new PgDumpAllFilterSource(
            PgDumpAllFilterSourceKind.File,
            path,
            null);
    }

    /// <summary><para>EN: Creates a filter source read through standard input.</para><para>JA: 標準入力から読み込むフィルター入力元を作成します。</para></summary>
    /// <param name="input"><para>EN: Readable caller-owned filter stream.</para><para>JA: 読み取り可能で呼び出し側所有のフィルターストリームです。</para></param>
    /// <returns><para>EN: The filter source.</para><para>JA: フィルター入力元です。</para></returns>
    public static PgDumpAllFilterSource FromStandardInput(Stream input)
    {
#if NETSTANDARD2_0
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }
#else
        ArgumentNullException.ThrowIfNull(input);
#endif
        if (!input.CanRead)
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.FilterInputMustBeReadable),
                nameof(input));
        }

        return new PgDumpAllFilterSource(
            PgDumpAllFilterSourceKind.StandardInput,
            null,
            input);
    }
}
