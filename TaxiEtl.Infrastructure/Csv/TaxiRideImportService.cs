using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using TaxiEtl.Application.Abstractions;
using TaxiEtl.Application.Models;
using TaxiEtl.Domain.Entities;
using TaxiEtl.Shared.Configuration;

namespace TaxiEtl.Infrastructure.Csv;

public sealed class TaxiRideImportService : ITaxiRideImportService
{
    private readonly ICsvTaxiRideReader _csvReader;
    private readonly ITaxiRideTransformer _transformer;
    private readonly IDeduplicationService _deduplicationService;
    private readonly IBulkInsertService _bulkInsertService;
    private readonly ILogger<TaxiRideImportService> _logger;
    private readonly TaxiEtlOptions _options;

    public TaxiRideImportService(
        ICsvTaxiRideReader csvReader,
        ITaxiRideTransformer transformer,
        IDeduplicationService deduplicationService,
        IBulkInsertService bulkInsertService,
        IOptions<TaxiEtlOptions> options,
        ILogger<TaxiRideImportService> logger)
    {
        _csvReader = csvReader;
        _transformer = transformer;
        _deduplicationService = deduplicationService;
        _bulkInsertService = bulkInsertService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<int> RunAsync(
        string inputFilePath,
        string duplicatesFilePath,
        string connectionString,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var statistics = new ImportStatistics();

        // Bounded channel: макс. 2 батчі у буфері, щоб не з'їсти пам'ять
        var channel = Channel.CreateBounded<List<TaxiRide>>(new BoundedChannelOptions(2)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        // Producer: читання CSV + трансформація + дедуплікація
        var producerTask = Task.Run(async () =>
        {
            try
            {
                var batch = new List<TaxiRide>(batchSize);
                await using var duplicateWriter = new DuplicateRowWriter(duplicatesFilePath);

                await foreach (var rawRow in _csvReader.ReadAsync(inputFilePath, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    statistics.IncrementProcessed();

                    var transformed = _transformer.TryTransform(rawRow, out var ride, out _);

                    if (!transformed || ride is null)
                    {
                        statistics.IncrementInvalid();
                        LogProgress(statistics);
                        continue;
                    }

                    if (_deduplicationService.IsDuplicate(ride))
                    {
                        statistics.IncrementDuplicates();
                        await duplicateWriter.WriteAsync(rawRow, cancellationToken);
                        LogProgress(statistics);
                        continue;
                    }

                    batch.Add(ride);

                    if (batch.Count >= batchSize)
                    {
                        await channel.Writer.WriteAsync(batch, cancellationToken);
                        batch = new List<TaxiRide>(batchSize);
                    }

                    LogProgress(statistics);
                }

                if (batch.Count > 0)
                {
                    await channel.Writer.WriteAsync(batch, cancellationToken);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // Consumer: bulk insert батчів у БД
        await foreach (var batch in channel.Reader.ReadAllAsync(cancellationToken))
        {
            await _bulkInsertService.BulkInsertAsync(batch, connectionString, cancellationToken);
            statistics.IncrementInserted(batch.Count);
        }

        await producerTask;

        _logger.LogInformation(
            "Import finished. Processed: {ProcessedRows}, Inserted: {InsertedRows}, Duplicates: {DuplicateRows}, Invalid: {InvalidRows}",
            statistics.ProcessedRows,
            statistics.InsertedRows,
            statistics.DuplicateRows,
            statistics.InvalidRows);

        return (int)statistics.InsertedRows;
    }

    private void LogProgress(ImportStatistics statistics)
    {
        if (statistics.ProcessedRows % _options.ProgressLogInterval != 0)
        {
            return;
        }

        _logger.LogInformation(
            "Processed {ProcessedRows}. Inserted: {InsertedRows}, Duplicates: {DuplicateRows}, Invalid: {InvalidRows}",
            statistics.ProcessedRows,
            statistics.InsertedRows,
            statistics.DuplicateRows,
            statistics.InvalidRows);
    }
}