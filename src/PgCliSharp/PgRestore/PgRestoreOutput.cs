using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents pg_restore direct-database execution or generated SQL/list output.</para>
/// <para>JA: pg_restore のデータベース直接実行、または生成 SQL・一覧の出力先を表します。</para>
/// </summary>
public sealed class PgRestoreOutput
{
    private PgRestoreOutput(
        PgRestoreOutputKind kind,
        string? database,
        Stream? standardOutput,
        string? path)
    {
        Kind = kind;
        Database = database;
        StandardOutput = standardOutput;
        Path = path;
    }

    /// <summary><para>EN: Gets the output kind.</para><para>JA: 出力先の種類を取得します。</para></summary>
    public PgRestoreOutputKind Kind { get; }

    /// <summary><para>EN: Gets the database name or libpq connection string for direct restore.</para><para>JA: データベース直接復元で使用するデータベース名または libpq 接続文字列を取得します。</para></summary>
    public string? Database { get; }

    /// <summary><para>EN: Gets the caller-owned standard-output destination stream.</para><para>JA: 呼び出し側所有の標準出力先ストリームを取得します。</para></summary>
    public Stream? StandardOutput { get; }

    /// <summary><para>EN: Gets the output file path when pg_restore writes through --file.</para><para>JA: pg_restore が --file で書き込む場合の出力ファイルパスを取得します。</para></summary>
    public string? Path { get; }

    /// <summary><para>EN: Creates a direct database restore destination.</para><para>JA: データベースへの直接復元先を作成します。</para></summary>
    /// <param name="database"><para>EN: Database name or libpq connection string.</para><para>JA: データベース名または libpq 接続文字列です。</para></param>
    /// <returns><para>EN: The direct restore destination.</para><para>JA: 直接復元先です。</para></returns>
    public static PgRestoreOutput ToDatabase(string database)
    {
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.DatabaseNameRequired),
                nameof(database));
        }

        return new PgRestoreOutput(PgRestoreOutputKind.Database, database, null, null);
    }

    /// <summary><para>EN: Creates generated SQL or list output streamed through stdout. PgCliSharp emits --file=- explicitly for stable PostgreSQL 10-18 behavior.</para><para>JA: 生成 SQL または一覧を標準出力へストリーミングする出力先を作成します。PgCliSharp は PostgreSQL 10〜18 で一貫した動作にするため --file=- を明示的に出力します。</para></summary>
    /// <param name="destination"><para>EN: Writable caller-owned stream.</para><para>JA: 書き込み可能で呼び出し側所有のストリームです。</para></param>
    /// <returns><para>EN: The stream destination.</para><para>JA: ストリーム出力先です。</para></returns>
    public static PgRestoreOutput ToStream(Stream destination)
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

        return new PgRestoreOutput(PgRestoreOutputKind.StandardOutput, null, destination, null);
    }

    /// <summary><para>EN: Creates a generated SQL or list file destination written by pg_restore.</para><para>JA: pg_restore 自身が書き込む生成 SQL または一覧のファイル出力先を作成します。</para></summary>
    /// <param name="path"><para>EN: Output file path.</para><para>JA: 出力ファイルのパスです。</para></param>
    /// <returns><para>EN: The file destination.</para><para>JA: ファイル出力先です。</para></returns>
    public static PgRestoreOutput ToFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                MessageProvider.GetString(MessageKeys.OutputPathRequired),
                nameof(path));
        }

        return new PgRestoreOutput(PgRestoreOutputKind.File, null, null, path);
    }
}
