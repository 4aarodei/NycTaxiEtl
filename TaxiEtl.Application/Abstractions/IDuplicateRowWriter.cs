using TaxiEtl.Application.Models;

namespace TaxiEtl.Application.Abstractions;

public interface IDuplicateRowWriter : IAsyncDisposable
{
    Task WriteAsync(
        RawTaxiRideCsvRow rawRow,
        CancellationToken cancellationToken = default);
}