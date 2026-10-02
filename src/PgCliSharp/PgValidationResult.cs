using System.Collections.ObjectModel;

namespace PgCliSharp;

/// <summary><para>EN: Stable, culture-independent categories for offline option validation.</para><para>JA: オフラインのオプション検証で使用する、カルチャに依存しない安定した分類です。</para></summary>
public enum PgValidationErrorCode
{
    /// <summary><para>EN: A typed option value is invalid.</para><para>JA: 型付きオプションの値が無効です。</para></summary>
    InvalidValue,
    /// <summary><para>EN: Options cannot be combined as requested.</para><para>JA: 指定したオプションを組み合わせられません。</para></summary>
    InvalidCombination,
    /// <summary><para>EN: An option is unavailable for the supplied version.</para><para>JA: 指定したバージョンではオプションを利用できません。</para></summary>
    UnsupportedOption,
}

/// <summary><para>EN: The first offline validation failure, with localized text and structured option/property names.</para><para>JA: 最初のオフライン検証エラーです。翻訳済みメッセージと構造化されたオプション名・プロパティ名を保持します。</para></summary>
public sealed class PgValidationError
{
    internal PgValidationError(PgValidationErrorCode code, string message, IEnumerable<string> optionNames, IEnumerable<string> propertyNames)
    {
        Code = code;
        Message = message;
        OptionNames = new ReadOnlyCollection<string>(optionNames.ToArray());
        PropertyNames = new ReadOnlyCollection<string>(propertyNames.ToArray());
    }
    /// <summary><para>EN: Gets the stable error category.</para><para>JA: 安定したエラー分類を取得します。</para></summary>
    public PgValidationErrorCode Code { get; }
    /// <summary><para>EN: Gets the localized validation message captured at validation time.</para><para>JA: 検証時に取得した翻訳済み検証メッセージを取得します。</para></summary>
    public string Message { get; }
    /// <summary><para>EN: Gets the involved CLI option names or validator labels.</para><para>JA: 関連する CLI オプション名または検証用ラベルを取得します。</para></summary>
    public IReadOnlyList<string> OptionNames { get; }
    /// <summary><para>EN: Gets known Options or input/output property names. Unmapped labels have no invented binding.</para><para>JA: 判明している Options または入出力のプロパティ名を取得します。未対応のラベルに推測した対応付けは行いません。</para></summary>
    public IReadOnlyList<string> PropertyNames { get; }
}

/// <summary><para>EN: Offline validation outcome; validity does not verify the executable, files, server or stream contents.</para><para>JA: オフライン検証結果です。有効であっても実行ファイル・ファイル・サーバー・ストリーム内容の確認は行いません。</para></summary>
public sealed class PgValidationResult
{
    internal PgValidationResult(PgValidationError? error, bool requiresExecutableVersionCheck)
    {
        Errors = new ReadOnlyCollection<PgValidationError>(error is null ? Array.Empty<PgValidationError>() : new[] { error });
        RequiresExecutableVersionCheck = requiresExecutableVersionCheck;
    }
    /// <summary><para>EN: Gets whether the offline checks passed. Deferred patch checks are reported separately.</para><para>JA: オフライン検証に合格したか取得します。保留されたパッチ版検証は別途報告します。</para></summary>
    public bool IsValid => Errors.Count == 0;
    /// <summary><para>EN: Gets zero errors or the first failure; this is not an exhaustive error list.</para><para>JA: エラーなし、または最初のエラーを取得します。全エラーを網羅する一覧ではありません。</para></summary>
    public IReadOnlyList<PgValidationError> Errors { get; }
    /// <summary><para>EN: Gets whether a requested patch-sensitive option still needs an exact executable version. Execution always probes the actual executable independently.</para><para>JA: 要求したパッチ版依存オプションに実行ファイルの正確なバージョンが必要か取得します。実行時は実際の実行ファイルを独立して確認します。</para></summary>
    public bool RequiresExecutableVersionCheck { get; }
}
