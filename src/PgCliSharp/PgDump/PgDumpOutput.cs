using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents the destination for a pg_dump payload without assuming that stdout is text.</para>
/// <para>JA: 標準出力をテキストと仮定せずに pg_dump ペイロードの出力先を表します。</para>
/// </summary>
public sealed class PgDumpOutput
{
    private PgDumpOutput(PgDumpOutputKind kind, Stream? stream, string? path)
    {
        Kind = kind;
        StandardOutput = stream;
        Path = path;
    }

    /// <summary><para>EN: Gets the output destination kind.</para><para>JA: 出力先の種類を取得します。</para></summary>
    public PgDumpOutputKind Kind { get; }

    /// <summary>
    /// <para>EN: Gets the caller-owned stdout destination stream when <see cref="Kind"/> is <see cref="PgDumpOutputKind.StandardOutput"/>.</para>
    /// <para>JA: <see cref="Kind"/> が <see cref="PgDumpOutputKind.StandardOutput"/> の場合、呼び出し側が所有する標準出力先ストリームを取得します。</para>
    /// </summary>
    public Stream? StandardOutput { get; }

    /// <summary>
    /// <para>EN: Gets the file or directory path when pg_dump writes the destination through --file.</para>
    /// <para>JA: pg_dump が --file で出力する場合のファイルまたはディレクトリパスを取得します。</para>
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// <para>EN: Creates a binary-safe stdout destination. PgCliSharp does not decode or buffer the payload.</para>
    /// <para>JA: バイナリセーフな標準出力先を作成します。PgCliSharp はペイロードをデコードまたは全量バッファリングしません。</para>
    /// </summary>
    /// <param name="destination"><para>EN: Writable caller-owned destination stream.</para><para>JA: 書き込み可能で呼び出し側が所有する出力先ストリームです。</para></param>
    /// <returns><para>EN: The output destination.</para><para>JA: 出力先です。</para></returns>
    public static PgDumpOutput ToStream(Stream destination)
    {
        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (!destination.CanWrite)
        {
            throw new ArgumentException("The pg_dump output stream must be writable.", nameof(destination));
        }

        return new PgDumpOutput(PgDumpOutputKind.StandardOutput, destination, null);
    }

    /// <summary>
    /// <para>EN: Creates a file destination written directly by pg_dump through --file.</para>
    /// <para>JA: --file を通じて pg_dump 自身が直接書き込むファイル出力先を作成します。</para>
    /// </summary>
    /// <param name="path"><para>EN: Output file path.</para><para>JA: 出力ファイルのパスです。</para></param>
    /// <returns><para>EN: The output destination.</para><para>JA: 出力先です。</para></returns>
    public static PgDumpOutput ToFile(string path)
    {
        return new PgDumpOutput(PgDumpOutputKind.File, null, ValidatePath(path));
    }

    /// <summary>
    /// <para>EN: Creates a directory-format destination written directly by pg_dump through --file.</para>
    /// <para>JA: --file を通じて pg_dump 自身が直接書き込むディレクトリ形式の出力先を作成します。</para>
    /// </summary>
    /// <param name="path"><para>EN: Target directory path.</para><para>JA: 出力先ディレクトリのパスです。</para></param>
    /// <returns><para>EN: The output destination.</para><para>JA: 出力先です。</para></returns>
    public static PgDumpOutput ToDirectory(string path)
    {
        return new PgDumpOutput(PgDumpOutputKind.Directory, null, ValidatePath(path));
    }

    private static string ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.OutputPathRequired),
                nameof(path));
        }

        return path;
    }
}
