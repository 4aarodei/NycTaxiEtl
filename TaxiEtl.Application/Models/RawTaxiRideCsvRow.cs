namespace TaxiEtl.Application.Models;

public sealed class RawTaxiRideCsvRow
{
    public string PickupDatetime { get; set; } = string.Empty;

    public string DropoffDatetime { get; set; } = string.Empty;

    public string PassengerCount { get; set; } = string.Empty;

    public string TripDistance { get; set; } = string.Empty;

    public string StoreAndFwdFlag { get; set; } = string.Empty;

    public string PULocationID { get; set; } = string.Empty;

    public string DOLocationID { get; set; } = string.Empty;

    public string FareAmount { get; set; } = string.Empty;

    public string TipAmount { get; set; } = string.Empty;
}