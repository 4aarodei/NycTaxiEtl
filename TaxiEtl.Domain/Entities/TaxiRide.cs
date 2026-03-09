namespace TaxiEtl.Domain.Entities;

public sealed class TaxiRide
{
    public DateTime PickupDatetimeUtc { get; init; }

    public DateTime DropoffDatetimeUtc { get; init; }

    public int PassengerCount { get; init; }

    public decimal TripDistance { get; init; }

    public string StoreAndFwdFlag { get; init; } = default!;

    public int PULocationID { get; init; }

    public int DOLocationID { get; init; }

    public decimal FareAmount { get; init; }

    public decimal TipAmount { get; init; }
}