namespace TaxiEtl.Domain.Entities;

public sealed class TaxiRide
{
    public long Id { get; init; }

    public DateTime PickupDatetimeUtc { get; init; }

    public DateTime DropoffDatetimeUtc { get; init; }

    public byte PassengerCount { get; init; }

    public decimal TripDistance { get; init; }

    public string StoreAndFwdFlag { get; init; } = default!;

    public short PULocationID { get; init; }

    public short DOLocationID { get; init; }

    public decimal FareAmount { get; init; }

    public decimal TipAmount { get; init; }
}