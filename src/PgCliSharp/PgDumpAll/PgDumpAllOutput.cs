using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents the SQL-script destination for pg_dumpall without forcing the selected encoding through a .NET text decoder.</para>
/// <para>JA: 選択したエンコーディングを .NET のテキストデコーダーへ強制せず、pg_dumpall の SQL スクリプト出力先を表します。</para>
/// </summary>
public sealed class PgDumpAllOutput
{
    private PgDumpAllOutput(
        PgDumpAllOutputKind kind,
        Stream? standardOutput,
        string? path)
    {
        Kind = kind;
        StandardOutput = standardOutput;
        Path = path;
    }

    /// <summary><para>EN: Gets the output kind.</para><para>JA: 出力先の種類を取得します。</para></summary>
    public PgDumpAllOutputKind Kind { get; }

    /// <summary><para>EN: Gets the caller-owned stdout stream when applicable.</para><para>JA: 該当する場合、呼び出し側所有の標準出力ストリームを取得します。</para></summary>
    public Stream? StandardOutput { get; }

    /// <summary><para>EN: Gets the output file path when applicable.</para><para>JA: 該当する場合、出力ファイルパスを取得します。</para></summary>
    public string? Path { get; }

    /// <summary><para>EN: Creates a byte-preserving stdout destination for the SQL script.</para><para>JA: SQL スクリプトをバイト列のまま保持する標準出力先を作成します。</para></summary>
    /// <param name="destination"><para>EN: Writable caller-owned stream.</para><para>JA: 書き込み可能で呼び出し側所有のストリームです。</para></param>
    /// <returns><para>EN: The output destination.</para><para>JA: 出力先です。</para></returns>
    public static PgDumpAllOutput ToStream(Stream destination)
    {
#if NETSTANDARD2_0
        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }
#else
        ArgumentNullException.ThrowIfNull(destination);
#endif
        if (!destination.CanWrite)
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.OutputStreamMustBeWritable),
                nameof(destination));
        }

        return new PgDumpAllOutput(
            PgDumpAllOutputKind.StandardOutput,
            destination,
            null);
    }

    /// <summary><para>EN: Creates an output file written directly by pg_dumpall.</para><para>JA: pg_dumpall 自身が直接書き込む出力ファイルを作成します。</para></summary>
    /// <param name="path"><para>EN: Output file path.</para><para>JA: 出力ファイルパスです。</para></param>
    /// <returns><para>EN: The output destination.</para><para>JA: 出力先です。</para></returns>
    public static PgDumpAllOutput ToFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.OutputPathRequired),
                nameof(path));
        }

        return new PgDumpAllOutput(PgDumpAllOutputKind.File, null, path);
    }
}
