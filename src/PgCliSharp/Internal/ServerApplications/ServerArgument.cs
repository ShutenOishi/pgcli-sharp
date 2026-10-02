using System.Globalization;
using System.Runtime.InteropServices;
using PgCliSharp.Internal.DatabaseMaintenance;
using PgCliSharp.ServerApplications;

namespace PgCliSharp.Internal.ServerApplications;

internal static class ServerArgument
{
    internal static bool Defined<T>(T value) where T : struct, Enum
    {
#if NETSTANDARD2_0
        return Enum.IsDefined(typeof(T), value);
#else
        return Enum.IsDefined(value);
#endif
    }
    internal static bool Contains(string value, char character)
    {
#if NETSTANDARD2_0
        return value.IndexOf(character) >= 0;
#else
        return value.Contains(character);
#endif
    }

    internal static void Value(ICollection<string> args, string option, object? value) => MaintenanceArgument.AddValue(args, option, value);
    internal static void Flag(ICollection<string> args, string option, bool value) => MaintenanceArgument.AddFlag(args, option, value);
    internal static void Invalid(PostgreSqlMajorVersion version, string option, object? value = null) =>
        throw new PgInvalidOptionValueException(version, option, value);
    internal static void Conflict(PostgreSqlMajorVersion version, string first, string second) =>
        throw new PgInvalidOptionCombinationException(version, first, second);
    internal static void Text(string? value, string option, PostgreSqlMajorVersion version, bool nonEmpty = false, bool redact = false)
    {
        if (value is not null && (Contains(value, '\0') || (nonEmpty && string.IsNullOrWhiteSpace(value))))
            Invalid(version, option, redact ? "<redacted>" : value);
    }
    internal static void Required(string? value, string option, PostgreSqlMajorVersion version)
    {
        if (string.IsNullOrWhiteSpace(value)) Invalid(version, option);
    }
    internal static void Directory(string? value, string environmentKey, IDictionary<string, string> environment, string option, PostgreSqlMajorVersion version)
    {
        string? effective = value;
        if (effective is null)
            effective = environment.TryGetValue(environmentKey, out string? overrideValue) ? overrideValue : Environment.GetEnvironmentVariable(environmentKey);
        Required(effective, option, version);
    }
    internal static void WalSize(int? size, PostgreSqlMajorVersion version)
    {
        if (size.HasValue && (size.Value < 1 || size.Value > 1024 || (size.Value & (size.Value - 1)) != 0))
            Invalid(version, "--wal-segsize", size);
    }
    internal static void Sync(PgFileSyncMethod? method, PostgreSqlMajorVersion version)
    {
        if (method == PgFileSyncMethod.Syncfs && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Invalid(version, "--sync-method", "syncfs");
    }
    internal static string SyncName(PgFileSyncMethod value) => value == PgFileSyncMethod.Fsync ? "fsync" : "syncfs";
    internal static string CharName(PgCharSignedness value) => value == PgCharSignedness.SignedValue ? "signed" : "unsigned";
    internal static string Authentication(PgInitDbAuthentication value) => value switch
    {
        PgInitDbAuthentication.ScramSha256 => "scram-sha-256",
        _ => value.ToString().ToLowerInvariant(),
    };
    internal static string Builtin(PgBuiltinLocale value) => value switch
    {
        PgBuiltinLocale.C => "C",
        PgBuiltinLocale.CUtf8 => "C.UTF-8",
        _ => "PG_UNICODE_FAST",
    };
}
