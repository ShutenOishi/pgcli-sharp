using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Identifies how a PostgreSQL 17+ pg_dump filter definition is supplied.</para>
/// <para>JA: PostgreSQL 17 以降の pg_dump フィルター定義の供給方法を表します。</para>
/// </summary>
public enum PgDumpFilterSourceKind
{
    /// <summary><para>EN: Read filter rules from a named file.</para><para>JA: 指定したファイルからフィルタールールを読み込みます。</para></summary>
    File,

    /// <summary><para>EN: Read filter rules from standard input by emitting --filter=-.</para><para>JA: --filter=- を指定し、標準入力からフィルタールールを読み込みます。</para></summary>
    StandardInput,
}

/// <summary>
/// <para>EN: Represents a pg_dump filter source introduced in PostgreSQL 17.</para>
/// <para>JA: PostgreSQL 17 で導入された pg_dump フィルターの入力元を表します。</para>
/// </summary>
public sealed class PgDumpFilterSource
{
    private PgDumpFilterSource(PgDumpFilterSourceKind kind, string? path, Stream? input)
    {
        Kind = kind;
        Path = path;
        Input = input;
    }

    /// <summary><para>EN: Gets the filter source kind.</para><para>JA: フィルター入力元の種類を取得します。</para></summary>
    public PgDumpFilterSourceKind Kind { get; }

    /// <summary><para>EN: Gets the filter file path for a file source.</para><para>JA: ファイル入力元の場合のフィルターファイルパスを取得します。</para></summary>
    public string? Path { get; }

    /// <summary>
    /// <para>EN: Gets the caller-owned input stream for a standard-input source.</para>
    /// <para>JA: 標準入力ソースの場合、呼び出し側が所有する入力ストリームを取得します。</para>
    /// </summary>
    public Stream? Input { get; }

    /// <summary>
    /// <para>EN: Creates a filter source backed by a file path.</para>
    /// <para>JA: ファイルパスを使用するフィルター入力元を作成します。</para>
    /// </summary>
    /// <param name="path"><para>EN: Filter file path.</para><para>JA: フィルターファイルのパスです。</para></param>
    /// <returns><para>EN: The filter source.</para><para>JA: フィルター入力元です。</para></returns>
    public static PgDumpFilterSource FromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.FilterPathRequired),
                nameof(path));
        }

        return new PgDumpFilterSource(PgDumpFilterSourceKind.File, path, null);
    }

    /// <summary>
    /// <para>EN: Creates a filter source that pg_dump reads from standard input by using --filter=-.</para>
    /// <para>JA: --filter=- を使用し、pg_dump が標準入力から読み込むフィルター入力元を作成します。</para>
    /// </summary>
    /// <param name="input"><para>EN: Readable caller-owned stream containing filter rules.</para><para>JA: フィルタールールを含む、読み取り可能で呼び出し側が所有するストリームです。</para></param>
    /// <returns><para>EN: The filter source.</para><para>JA: フィルター入力元です。</para></returns>
    public static PgDumpFilterSource FromStandardInput(Stream input)
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

        return new PgDumpFilterSource(PgDumpFilterSourceKind.StandardInput, null, input);
    }
}
