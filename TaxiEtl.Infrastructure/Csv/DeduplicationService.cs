using TaxiEtl.Application.Abstractions;
using TaxiEtl.Domain.Entities;

namespace TaxiEtl.Infrastructure.Csv;

public sealed class DeduplicationService : IDeduplicationService
{
    private readonly HashSet<(DateTime Pickup, DateTime Dropoff, byte Passengers)> _seenKeys = [];

    public bool IsDuplicate(TaxiRide ride)
    {
        var key = (
            ride.PickupDatetimeUtc,
            ride.DropoffDatetimeUtc,
            ride.PassengerCount);

        return !_seenKeys.Add(key);
    }
}
