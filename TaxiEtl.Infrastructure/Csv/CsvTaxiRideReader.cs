using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TaxiEtl.Application.Abstractions;
using TaxiEtl.Application.Models;

namespace TaxiEtl.Infrastructure.Csv;

public sealed class CsvTaxiRideReader : ICsvTaxiRideReader
{
    public async IAsyncEnumerable<RawTaxiRideCsvRow> ReadAsync(
        string filePath,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 1024,
            options: FileOptions.SequentialScan);

        using var reader = new StreamReader(stream);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var csv = new CsvReader(reader, config);

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            RawTaxiRideCsvRow? row = null;

            try
            {
                row = new RawTaxiRideCsvRow
                {
                    PickupDatetime = GetField(csv, "tpep_pickup_datetime"),
                    DropoffDatetime = GetField(csv, "tpep_dropoff_datetime"),
                    PassengerCount = GetField(csv, "passenger_count"),
                    TripDistance = GetField(csv, "trip_distance"),
                    StoreAndFwdFlag = GetField(csv, "store_and_fwd_flag"),
                    PULocationID = GetField(csv, "PULocationID"),
                    DOLocationID = GetField(csv, "DOLocationID"),
                    FareAmount = GetField(csv, "fare_amount"),
                    TipAmount = GetField(csv, "tip_amount")
                };
            }
            catch
            {
                continue;
            }

            if (row is not null)
            {
                yield return row;
            }
        }
    }

    private static string GetField(CsvReader csv, string fieldName)
    {
        return csv.TryGetField(fieldName, out string? value)
            ? value ?? string.Empty
            : string.Empty;
    }
}