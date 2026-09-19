using System.Globalization;
using PgCliSharp.Internal.DatabaseMaintenance;

namespace PgCliSharp.Internal.PgAmcheck;

internal static class PgAmcheckOptionAvailabilityCatalog
{
    internal static readonly MaintenanceOptionAvailabilityInfo CheckUnique = MaintenanceAvailability.Since("--checkunique", PostgreSqlMajorVersion.V17);
    internal static readonly IReadOnlyList<MaintenanceOptionAvailabilityInfo> All = new[] { CheckUnique };
}

internal static class PgAmcheckArgumentBuilder
{
    internal static IReadOnlyList<string> Build(PgAmcheckOptions options)
    {
        var args = new List<string>();
        MaintenanceArgument.AddConnection(args, options.Host, options.Port, options.Username, options.PasswordPrompt);
        MaintenanceArgument.AddValue(args, "--maintenance-db", options.MaintenanceDatabase);
        MaintenanceArgument.AddFlag(args, "--all", options.AllDatabases);
        MaintenanceArgument.AddRepeatable(args, "--database", options.DatabasePatterns);
        MaintenanceArgument.AddRepeatable(args, "--exclude-database", options.ExcludedDatabasePatterns);
        MaintenanceArgument.AddFlag(args, "--echo", options.Echo);
        MaintenanceArgument.AddRepeatable(args, "--index", options.IndexPatterns);
        MaintenanceArgument.AddRepeatable(args, "--exclude-index", options.ExcludedIndexPatterns);
        MaintenanceArgument.AddValue(args, "--jobs", options.Jobs);
        MaintenanceArgument.AddFlag(args, "--progress", options.Progress);
        MaintenanceArgument.AddRepeatable(args, "--relation", options.RelationPatterns);
        MaintenanceArgument.AddRepeatable(args, "--exclude-relation", options.ExcludedRelationPatterns);
        MaintenanceArgument.AddRepeatable(args, "--schema", options.SchemaPatterns);
        MaintenanceArgument.AddRepeatable(args, "--exclude-schema", options.ExcludedSchemaPatterns);
        MaintenanceArgument.AddRepeatable(args, "--table", options.TablePatterns);
        MaintenanceArgument.AddRepeatable(args, "--exclude-table", options.ExcludedTablePatterns);
        MaintenanceArgument.AddFlag(args, "--verbose", options.Verbose);
        MaintenanceArgument.AddFlag(args, "--no-dependent-indexes", options.NoDependentIndexes);
        MaintenanceArgument.AddFlag(args, "--no-dependent-toast", options.NoDependentToast);
        MaintenanceArgument.AddFlag(args, "--exclude-toast-pointers", options.ExcludeToastPointers);
        MaintenanceArgument.AddFlag(args, "--on-error-stop", options.OnErrorStop);

        if (options.Skip.HasValue)
            MaintenanceArgument.AddValue(args, "--skip", options.Skip.Value switch
            {
                PgAmcheckSkipMode.None => "none",
                PgAmcheckSkipMode.AllVisible => "all-visible",
                PgAmcheckSkipMode.AllFrozen => "all-frozen",
                _ => throw new ArgumentOutOfRangeException(nameof(options)),
            });

        MaintenanceArgument.AddValue(args, "--startblock", options.StartBlock);
        MaintenanceArgument.AddValue(args, "--endblock", options.EndBlock);
        MaintenanceArgument.AddFlag(args, "--rootdescend", options.RootDescend);
        MaintenanceArgument.AddFlag(args, "--no-strict-names", !options.StrictNames);
        MaintenanceArgument.AddFlag(args, "--heapallindexed", options.HeapAllIndexed);
        MaintenanceArgument.AddFlag(args, "--parent-check", options.ParentCheck);

        if (options.InstallMissing is not null)
        {
            args.Add(options.InstallMissing.Schema is null
                ? "--install-missing"
                : "--install-missing=" + options.InstallMissing.Schema);
        }

        MaintenanceArgument.AddFlag(args, "--checkunique", options.CheckUnique);

        if (options.DatabaseName is not null) args.Add(options.DatabaseName);
        return args;
    }
}

internal static class PgAmcheckValidator
{
    internal static void Validate(PgAmcheckOptions options, PostgreSqlMajorVersion version)
    {
        MaintenanceArgument.ValidatePort(options.Port, version);
        MaintenanceArgument.ValidatePositive(options.Jobs, "--jobs", version);
        if (options.CheckUnique) MaintenanceAvailability.Ensure(PgAmcheckOptionAvailabilityCatalog.CheckUnique, version);

        if (options.StartBlock == uint.MaxValue)
            throw new PgInvalidOptionValueException(version, "--startblock", options.StartBlock);
        if (options.EndBlock == uint.MaxValue)
            throw new PgInvalidOptionValueException(version, "--endblock", options.EndBlock);
        if (options.StartBlock.HasValue && options.EndBlock.HasValue && options.EndBlock.Value < options.StartBlock.Value)
            throw new PgInvalidOptionCombinationException(version, "--endblock", "--startblock");

        if (options.AllDatabases && options.DatabaseName is not null)
            throw new PgInvalidOptionCombinationException(version, "--all", "database-name");

        bool hasDatabasePatterns = options.DatabasePatterns.Count > 0 || options.ExcludedDatabasePatterns.Count > 0;
        if (options.DatabaseName is not null && hasDatabasePatterns)
            throw new PgInvalidOptionCombinationException(version, "database-name", "--database/--exclude-database");

        ValidatePatterns(options.DatabasePatterns, "--database", version);
        ValidatePatterns(options.ExcludedDatabasePatterns, "--exclude-database", version);
        ValidatePatterns(options.IndexPatterns, "--index", version);
        ValidatePatterns(options.ExcludedIndexPatterns, "--exclude-index", version);
        ValidatePatterns(options.RelationPatterns, "--relation", version);
        ValidatePatterns(options.ExcludedRelationPatterns, "--exclude-relation", version);
        ValidatePatterns(options.SchemaPatterns, "--schema", version);
        ValidatePatterns(options.ExcludedSchemaPatterns, "--exclude-schema", version);
        ValidatePatterns(options.TablePatterns, "--table", version);
        ValidatePatterns(options.ExcludedTablePatterns, "--exclude-table", version);

        if (options.DatabaseName is not null && string.IsNullOrWhiteSpace(options.DatabaseName))
            throw new PgInvalidOptionValueException(version, "database-name", options.DatabaseName);
    }

    private static void ValidatePatterns(IEnumerable<string> values, string option, PostgreSqlMajorVersion version) =>
        MaintenanceArgument.ValidateNonEmpty(values, option, version);
}
