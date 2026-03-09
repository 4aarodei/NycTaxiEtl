using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TaxiEtl.Application.Abstractions;
using TaxiEtl.Application.Models;

namespace TaxiEtl.Infrastructure.Csv;

public sealed class DuplicateRowWriter : IDuplicateRowWriter
{
    private readonly StreamWriter _streamWriter;
    private readonly CsvWriter _csvWriter;
    private bool _headerWritten;

    public DuplicateRowWriter(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var stream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            bufferSize: 1024 * 64,
            options: FileOptions.SequentialScan);

        _streamWriter = new StreamWriter(stream);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture);
        _csvWriter = new CsvWriter(_streamWriter, config);
    }

    public async Task WriteAsync(RawTaxiRideCsvRow rawRow, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_headerWritten)
        {
            _csvWriter.WriteField("tpep_pickup_datetime");
            _csvWriter.WriteField("tpep_dropoff_datetime");
            _csvWriter.WriteField("passenger_count");
            _csvWriter.WriteField("trip_distance");
            _csvWriter.WriteField("store_and_fwd_flag");
            _csvWriter.WriteField("PULocationID");
            _csvWriter.WriteField("DOLocationID");
            _csvWriter.WriteField("fare_amount");
            _csvWriter.WriteField("tip_amount");
            await _csvWriter.NextRecordAsync();

            _headerWritten = true;
        }

        _csvWriter.WriteField(rawRow.PickupDatetime);
        _csvWriter.WriteField(rawRow.DropoffDatetime);
        _csvWriter.WriteField(rawRow.PassengerCount);
        _csvWriter.WriteField(rawRow.TripDistance);
        _csvWriter.WriteField(rawRow.StoreAndFwdFlag);
        _csvWriter.WriteField(rawRow.PULocationID);
        _csvWriter.WriteField(rawRow.DOLocationID);
        _csvWriter.WriteField(rawRow.FareAmount);
        _csvWriter.WriteField(rawRow.TipAmount);
        await _csvWriter.NextRecordAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _csvWriter.FlushAsync();
        await _streamWriter.FlushAsync();
        await _csvWriter.DisposeAsync();
        await _streamWriter.DisposeAsync();
    }
}