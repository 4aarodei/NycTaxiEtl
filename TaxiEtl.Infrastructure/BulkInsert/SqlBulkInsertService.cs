using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TaxiEtl.Application.Abstractions;
using TaxiEtl.Domain.Entities;
using TaxiEtl.Shared.Configuration;

namespace TaxiEtl.Infrastructure.BulkInsert;

public sealed class SqlBulkInsertService : IBulkInsertService
{
    private readonly TaxiEtlOptions _options;

    public SqlBulkInsertService(IOptions<TaxiEtlOptions> options)
    {
        _options = options.Value;
    }

    public async Task BulkInsertAsync(
        IReadOnlyList<TaxiRide> batch,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (batch.Count == 0)
        {
            return;
        }

        var table = CreateDataTable();

        foreach (var ride in batch)
        {
            table.Rows.Add(
                ride.PickupDatetimeUtc,
                ride.DropoffDatetimeUtc,
                ride.PassengerCount,
                ride.TripDistance,
                ride.StoreAndFwdFlag,
                ride.PULocationID,
                ride.DOLocationID,
                ride.FareAmount,
                ride.TipAmount);
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        
        using var bulkCopy = new SqlBulkCopy(
            connection,
            SqlBulkCopyOptions.TableLock,
            externalTransaction: null);

        bulkCopy.DestinationTableName = _options.DestinationTableName;
        bulkCopy.BatchSize = batch.Count;
        bulkCopy.BulkCopyTimeout = _options.BulkCopyTimeoutSeconds;
        bulkCopy.EnableStreaming = true;

        bulkCopy.ColumnMappings.Add("PickupDatetimeUtc", "PickupDatetimeUtc");
        bulkCopy.ColumnMappings.Add("DropoffDatetimeUtc", "DropoffDatetimeUtc");
        bulkCopy.ColumnMappings.Add("PassengerCount", "PassengerCount");
        bulkCopy.ColumnMappings.Add("TripDistance", "TripDistance");
        bulkCopy.ColumnMappings.Add("StoreAndFwdFlag", "StoreAndFwdFlag");
        bulkCopy.ColumnMappings.Add("PULocationID", "PULocationID");
        bulkCopy.ColumnMappings.Add("DOLocationID", "DOLocationID");
        bulkCopy.ColumnMappings.Add("FareAmount", "FareAmount");
        bulkCopy.ColumnMappings.Add("TipAmount", "TipAmount");

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
    }

    private static DataTable CreateDataTable()
    {
        var table = new DataTable();

        table.Columns.Add("PickupDatetimeUtc", typeof(DateTime));
        table.Columns.Add("DropoffDatetimeUtc", typeof(DateTime));
        table.Columns.Add("PassengerCount", typeof(int));
        table.Columns.Add("TripDistance", typeof(decimal));
        table.Columns.Add("StoreAndFwdFlag", typeof(string));
        table.Columns.Add("PULocationID", typeof(int));
        table.Columns.Add("DOLocationID", typeof(int));
        table.Columns.Add("FareAmount", typeof(decimal));
        table.Columns.Add("TipAmount", typeof(decimal));

        return table;
    }
}