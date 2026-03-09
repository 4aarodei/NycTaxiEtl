using TaxiEtl.Domain.Entities;

namespace TaxiEtl.Application.Abstractions;

public interface IDeduplicationService
{
    bool IsDuplicate(TaxiRide ride);
}