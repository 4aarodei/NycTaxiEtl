namespace TaxiEtl.Application.Models;

public sealed class ImportStatistics
{
    public long ProcessedRows { get; private set; }

    public long InsertedRows { get; private set; }

    public long DuplicateRows { get; private set; }

    public long InvalidRows { get; private set; }

    public void IncrementProcessed() => ProcessedRows++;

    public void IncrementInserted(long count = 1) => InsertedRows += count;

    public void IncrementDuplicates(long count = 1) => DuplicateRows += count;

    public void IncrementInvalid(long count = 1) => InvalidRows += count;
}