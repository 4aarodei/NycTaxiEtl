namespace TaxiEtl.Application.Abstractions;

public interface ITaxiRideImportService
{
    Task<int> RunAsync(
        string inputFilePath,
        string duplicatesFilePath,
        string connectionString,
        int batchSize,
        CancellationToken cancellationToken = default);
}