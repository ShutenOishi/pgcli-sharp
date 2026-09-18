using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents the archive input consumed by pg_restore.</para>
/// <para>JA: pg_restore が読み込むアーカイブ入力を表します。</para>
/// </summary>
public sealed class PgRestoreInput
{
    private PgRestoreInput(PgRestoreInputKind kind, string? path, Stream? standardInput)
    {
        Kind = kind;
        Path = path;
        StandardInput = standardInput;
    }

    /// <summary><para>EN: Gets the archive input kind.</para><para>JA: アーカイブ入力の種類を取得します。</para></summary>
    public PgRestoreInputKind Kind { get; }

    /// <summary><para>EN: Gets the archive file or directory path when applicable.</para><para>JA: 該当する場合、アーカイブファイルまたはディレクトリのパスを取得します。</para></summary>
    public string? Path { get; }

    /// <summary><para>EN: Gets the caller-owned archive stream when standard input is used.</para><para>JA: 標準入力を使用する場合、呼び出し側所有のアーカイブストリームを取得します。</para></summary>
    public Stream? StandardInput { get; }

    /// <summary><para>EN: Creates a file-backed archive input.</para><para>JA: ファイルを使用するアーカイブ入力を作成します。</para></summary>
    /// <param name="path"><para>EN: Archive file path.</para><para>JA: アーカイブファイルのパスです。</para></param>
    /// <returns><para>EN: The archive input.</para><para>JA: アーカイブ入力です。</para></returns>
    public static PgRestoreInput FromFile(string path) =>
        new PgRestoreInput(PgRestoreInputKind.File, ValidatePath(path), null);

    /// <summary><para>EN: Creates a directory-format archive input.</para><para>JA: ディレクトリ形式アーカイブ入力を作成します。</para></summary>
    /// <param name="path"><para>EN: Archive directory path.</para><para>JA: アーカイブディレクトリのパスです。</para></param>
    /// <returns><para>EN: The archive input.</para><para>JA: アーカイブ入力です。</para></returns>
    public static PgRestoreInput FromDirectory(string path) =>
        new PgRestoreInput(PgRestoreInputKind.Directory, ValidatePath(path), null);

    /// <summary><para>EN: Creates archive input streamed through pg_restore standard input.</para><para>JA: pg_restore の標準入力を通じてストリーミングするアーカイブ入力を作成します。</para></summary>
    /// <param name="input"><para>EN: Readable caller-owned archive stream.</para><para>JA: 読み取り可能で呼び出し側所有のアーカイブストリームです。</para></param>
    /// <returns><para>EN: The archive input.</para><para>JA: アーカイブ入力です。</para></returns>
    public static PgRestoreInput FromStandardInput(Stream input)
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
                MessageProvider.GetString(MessageKeys.InputStreamMustBeReadable),
                nameof(input));
        }

        return new PgRestoreInput(PgRestoreInputKind.StandardInput, null, input);
    }

    private static string ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.InputPathRequired),
                nameof(path));
        }

        return path;
    }
}
