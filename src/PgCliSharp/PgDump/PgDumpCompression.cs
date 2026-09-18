using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a typed pg_dump compression specification across PostgreSQL 10 through 18.</para>
/// <para>JA: PostgreSQL 10〜18 の pg_dump 圧縮指定を型付きで表します。</para>
/// </summary>
public sealed class PgDumpCompression
{
    private PgDumpCompression(
        PgDumpCompressionMethod? method,
        int? level,
        bool longMode,
        bool levelOnly)
    {
        Method = method;
        Level = level;
        LongMode = longMode;
        IsLevelOnly = levelOnly;
    }

    /// <summary>
    /// <para>EN: Gets the compression method for method-based PostgreSQL 16+ syntax.</para>
    /// <para>JA: PostgreSQL 16 以降の方式指定構文で使用する圧縮方式を取得します。</para>
    /// </summary>
    public PgDumpCompressionMethod? Method { get; }

    /// <summary>
    /// <para>EN: Gets the requested compression level, or null to use the method default.</para>
    /// <para>JA: 指定した圧縮レベルを取得します。null の場合は方式の既定値を使用します。</para>
    /// </summary>
    public int? Level { get; }

    /// <summary>
    /// <para>EN: Gets whether zstd long-distance matching is requested.</para>
    /// <para>JA: zstd の long-distance matching を要求するかどうかを取得します。</para>
    /// </summary>
    public bool LongMode { get; }

    /// <summary>
    /// <para>EN: Gets whether this value uses the numeric level-only syntax compatible with PostgreSQL 10+.</para>
    /// <para>JA: PostgreSQL 10 以降で互換性のある数値レベルのみの構文を使用するかどうかを取得します。</para>
    /// </summary>
    public bool IsLevelOnly { get; }

    /// <summary>
    /// <para>EN: Creates the numeric compression form accepted by PostgreSQL 10 through 18. Zero disables compression; 1 through 9 select gzip levels.</para>
    /// <para>JA: PostgreSQL 10〜18 で使用できる数値圧縮指定を作成します。0 は圧縮なし、1〜9 は gzip の圧縮レベルです。</para>
    /// </summary>
    /// <param name="level"><para>EN: A value from 0 through 9.</para><para>JA: 0〜9 の値です。</para></param>
    /// <returns><para>EN: The typed compression specification.</para><para>JA: 型付き圧縮指定です。</para></returns>
    public static PgDumpCompression FromLevel(int level)
    {
        if (level < 0 || level > 9)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                MessageProvider.Format(MessageKeys.InvalidCompressionLevel, level, "pg_dump numeric compression"));
        }

        return new PgDumpCompression(null, level, false, true);
    }

    /// <summary>
    /// <para>EN: Creates a PostgreSQL 16+ method-based compression specification.</para>
    /// <para>JA: PostgreSQL 16 以降の方式指定による圧縮指定を作成します。</para>
    /// </summary>
    /// <param name="method"><para>EN: Compression method.</para><para>JA: 圧縮方式です。</para></param>
    /// <param name="level"><para>EN: Optional method-specific compression level. gzip accepts -1 or 1 through 9; LZ4 accepts 0 through 12; zstd bounds are determined by the executable's linked zstd library.</para><para>JA: 任意の方式別圧縮レベルです。gzip は -1 または 1〜9、LZ4 は 0〜12 を受け付けます。zstd の範囲は実行ファイルがリンクしている zstd ライブラリに依存します。</para></param>
    /// <param name="longMode"><para>EN: Enables zstd long-distance mode. Valid only with zstd.</para><para>JA: zstd の long-distance モードを有効にします。zstd でのみ有効です。</para></param>
    /// <returns><para>EN: The typed compression specification.</para><para>JA: 型付き圧縮指定です。</para></returns>
    public static PgDumpCompression ForMethod(
        PgDumpCompressionMethod method,
        int? level = null,
        bool longMode = false)
    {
#if NETSTANDARD2_0
        bool methodDefined = Enum.IsDefined(typeof(PgDumpCompressionMethod), method);
#else
        bool methodDefined = Enum.IsDefined(method);
#endif
        if (!methodDefined)
        {
            throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }

        if (method == PgDumpCompressionMethod.None && (level.HasValue || longMode))
        {
            throw new ArgumentException(
                MessageProvider.Format(
                    MessageKeys.InvalidCompressionCombination,
                    "the none method cannot have a level or long mode"),
                nameof(method));
        }

        if (longMode && method != PgDumpCompressionMethod.Zstd)
        {
            throw new ArgumentException(
                MessageProvider.Format(
                    MessageKeys.InvalidCompressionCombination,
                    "long mode is supported only with zstd"),
                nameof(longMode));
        }

        if (level.HasValue)
        {
            if (method == PgDumpCompressionMethod.Gzip &&
                ((level.Value < 1 || level.Value > 9) && level.Value != -1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(level),
                    level,
                    MessageProvider.Format(MessageKeys.InvalidCompressionLevel, level.Value, "gzip"));
            }

            if (method == PgDumpCompressionMethod.Lz4 &&
                (level.Value < 0 || level.Value > 12))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(level),
                    level,
                    MessageProvider.Format(MessageKeys.InvalidCompressionLevel, level.Value, "lz4"));
            }
        }

        return new PgDumpCompression(method, level, longMode, false);
    }
}
