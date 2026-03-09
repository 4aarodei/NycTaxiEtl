using TaxiEtl.Application.Models;

namespace TaxiEtl.Application.Abstractions;

public interface ICsvTaxiRideReader
{
    IAsyncEnumerable<RawTaxiRideCsvRow> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}