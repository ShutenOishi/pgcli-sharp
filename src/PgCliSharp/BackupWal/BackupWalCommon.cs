using PgCliSharp.Internal.Localization;

namespace PgCliSharp;

/// <summary>
/// <para>EN: Represents a PostgreSQL write-ahead-log location in X/Y hexadecimal form.</para>
/// <para>JA: X/Y の16進形式で PostgreSQL の WAL 位置を表します。</para>
/// </summary>
public sealed class PgLogSequenceNumber
{
    /// <summary><para>EN: Creates a validated WAL location.</para><para>JA: 検証済みの WAL 位置を作成します。</para></summary>
    /// <param name="value"><para>EN: Location such as 0/16B6C50.</para><para>JA: 0/16B6C50 のような位置です。</para></param>
    public PgLogSequenceNumber(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException(
                MessageProvider.Format(MessageKeys.InvalidOptionValue, "LSN", value ?? "<null>"),
                nameof(value));
        }

        Value = value!.ToUpperInvariant();
    }

    /// <summary><para>EN: Gets the normalized hexadecimal location.</para><para>JA: 正規化された16進位置を取得します。</para></summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;

    private static bool IsValid(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] parts = value.Split('/');
        return parts.Length == 2 && parts.All(IsHex);
    }

    private static bool IsHex(string part)
    {
        if (part.Length == 0)
        {
            return false;
        }

        foreach (char c in part)
        {
            bool hex = (c >= '0' && c <= '9') ||
                       (c >= 'A' && c <= 'F') ||
                       (c >= 'a' && c <= 'f');
            if (!hex)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// <para>EN: Maps a PostgreSQL tablespace path to a destination path.</para>
/// <para>JA: PostgreSQL の tablespace パスを出力先パスへマッピングします。</para>
/// </summary>
public sealed class PgTablespaceMapping
{
    /// <summary><para>EN: Creates a tablespace mapping.</para><para>JA: tablespace mapping を作成します。</para></summary>
    public PgTablespaceMapping(string sourceDirectory, string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(sourceDirectory))
        {
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.InputPathRequired), nameof(sourceDirectory));
        }

        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            throw new ArgumentException(MessageProvider.GetString(MessageKeys.OutputPathRequired), nameof(targetDirectory));
        }

        SourceDirectory = sourceDirectory;
        TargetDirectory = targetDirectory;
    }

    /// <summary><para>EN: Gets the source tablespace directory.</para><para>JA: 元の tablespace ディレクトリを取得します。</para></summary>
    public string SourceDirectory { get; }

    /// <summary><para>EN: Gets the mapped destination directory.</para><para>JA: マッピング先ディレクトリを取得します。</para></summary>
    public string TargetDirectory { get; }

    /// <inheritdoc />
    public override string ToString() => Escape(SourceDirectory) + "=" + Escape(TargetDirectory);

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace("=", "\\=");
}

/// <summary>
/// <para>EN: Specifies a backup-manifest checksum algorithm.</para>
/// <para>JA: backup manifest のチェックサムアルゴリズムを指定します。</para>
/// </summary>
public enum PgBackupManifestChecksum
{
    /// <summary><para>EN: Do not checksum files.</para><para>JA: ファイルのチェックサムを作成しません。</para></summary>
    None,
    /// <summary><para>EN: CRC32C.</para><para>JA: CRC32C です。</para></summary>
    Crc32C,
    /// <summary><para>EN: SHA-224.</para><para>JA: SHA-224 です。</para></summary>
    Sha224,
    /// <summary><para>EN: SHA-256.</para><para>JA: SHA-256 です。</para></summary>
    Sha256,
    /// <summary><para>EN: SHA-384.</para><para>JA: SHA-384 です。</para></summary>
    Sha384,
    /// <summary><para>EN: SHA-512.</para><para>JA: SHA-512 です。</para></summary>
    Sha512,
}

/// <summary>
/// <para>EN: Specifies a filesystem synchronization method for backup tools.</para>
/// <para>JA: バックアップツールのファイルシステム同期方式を指定します。</para>
/// </summary>
public enum PgBackupSyncMethod
{
    /// <summary><para>EN: Synchronize files individually with fsync.</para><para>JA: fsync で個別ファイルを同期します。</para></summary>
    Fsync,
    /// <summary><para>EN: Synchronize through syncfs where supported.</para><para>JA: 対応環境で syncfs により同期します。</para></summary>
    Syncfs,
}

/// <summary>
/// <para>EN: Base execution metadata shared by Phase 4 command results.</para>
/// <para>JA: Phase 4 コマンド結果で共有する実行メタデータです。</para>
/// </summary>
public abstract class PgBackupWalResult
{
    internal PgBackupWalResult(int exitCode, TimeSpan duration, Version executableVersion, string rawExecutableVersion, string standardError)
    {
        ExitCode = exitCode;
        Duration = duration;
        ExecutableVersion = executableVersion;
        RawExecutableVersion = rawExecutableVersion;
        StandardError = standardError;
    }

    /// <summary><para>EN: Gets the process exit code.</para><para>JA: プロセス終了コードを取得します。</para></summary>
    public int ExitCode { get; }
    /// <summary><para>EN: Gets execution duration.</para><para>JA: 実行時間を取得します。</para></summary>
    public TimeSpan Duration { get; }
    /// <summary><para>EN: Gets the parsed executable version.</para><para>JA: 解析された実行ファイルバージョンを取得します。</para></summary>
    public Version ExecutableVersion { get; }
    /// <summary><para>EN: Gets the raw numeric version text.</para><para>JA: 数値バージョン文字列を取得します。</para></summary>
    public string RawExecutableVersion { get; }
    /// <summary><para>EN: Gets original PostgreSQL stderr.</para><para>JA: PostgreSQL の元の標準エラーを取得します。</para></summary>
    public string StandardError { get; }
}
