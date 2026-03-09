using TaxiEtl.Domain.Entities;

namespace TaxiEtl.Application.Abstractions;

public interface IBulkInsertService
{
    Task BulkInsertAsync(
        IReadOnlyList<TaxiRide> batch,
        string connectionString,
        CancellationToken cancellationToken = default);
}