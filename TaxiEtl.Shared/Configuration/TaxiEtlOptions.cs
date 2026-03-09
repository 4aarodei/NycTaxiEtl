namespace TaxiEtl.Shared.Configuration;

public sealed class TaxiEtlOptions
{
    public const string SectionName = "TaxiEtl";
    public const int DefaultBatchSize = 50_000;
    public const int DefaultProgressLogInterval = 100_000;
    public const int DefaultBulkCopyTimeoutSeconds = 600;

    public string InputCsvPath { get; set; } = string.Empty;

    public string DuplicatesCsvPath { get; set; } = "duplicates.csv";

    public int BatchSize { get; set; } = DefaultBatchSize;

    public int ProgressLogInterval { get; set; } = DefaultProgressLogInterval;

    public int BulkCopyTimeoutSeconds { get; set; } = DefaultBulkCopyTimeoutSeconds;

    public string SourceTimeZoneId { get; set; } = "Eastern Standard Time";

    public string DestinationTableName { get; set; } = "dbo.TaxiRides";
}