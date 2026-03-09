using System.Globalization;
using TaxiEtl.Application.Abstractions;
using TaxiEtl.Application.Models;
using TaxiEtl.Domain.Entities;
using TaxiEtl.Shared.Configuration;

namespace TaxiEtl.Infrastructure.Csv;

public sealed class TaxiRideTransformer : ITaxiRideTransformer
{
    private readonly TimeZoneInfo _sourceTimeZone;

    public TaxiRideTransformer(TaxiEtlOptions options)
    {
        _sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.SourceTimeZoneId);
    }

    public bool TryTransform(
        RawTaxiRideCsvRow rawRow,
        out TaxiRide? ride,
        out string? error)
    {
        ride = null;
        error = null;

        try
        {
            var pickupRaw = rawRow.PickupDatetime.Trim();
            var dropoffRaw = rawRow.DropoffDatetime.Trim();
            var passengerCountRaw = rawRow.PassengerCount.Trim();
            var tripDistanceRaw = rawRow.TripDistance.Trim();
            var storeAndFwdFlagRaw = rawRow.StoreAndFwdFlag.Trim();
            var puLocationIdRaw = rawRow.PULocationID.Trim();
            var doLocationIdRaw = rawRow.DOLocationID.Trim();
            var fareAmountRaw = rawRow.FareAmount.Trim();
            var tipAmountRaw = rawRow.TipAmount.Trim();

            if (!TryParseEasternToUtc(pickupRaw, out var pickupUtc))
            {
                error = $"Invalid pickup datetime: '{pickupRaw}'";
                return false;
            }

            if (!TryParseEasternToUtc(dropoffRaw, out var dropoffUtc))
            {
                error = $"Invalid dropoff datetime: '{dropoffRaw}'";
                return false;
            }

            if (!int.TryParse(passengerCountRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var passengerCount))
            {
                error = $"Invalid passenger count: '{passengerCountRaw}'";
                return false;
            }

            if (!decimal.TryParse(tripDistanceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var tripDistance))
            {
                error = $"Invalid trip distance: '{tripDistanceRaw}'";
                return false;
            }

            if (!int.TryParse(puLocationIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var puLocationId))
            {
                error = $"Invalid PULocationID: '{puLocationIdRaw}'";
                return false;
            }

            if (!int.TryParse(doLocationIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var doLocationId))
            {
                error = $"Invalid DOLocationID: '{doLocationIdRaw}'";
                return false;
            }

            if (!decimal.TryParse(fareAmountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var fareAmount))
            {
                error = $"Invalid fare amount: '{fareAmountRaw}'";
                return false;
            }

            if (!decimal.TryParse(tipAmountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var tipAmount))
            {
                error = $"Invalid tip amount: '{tipAmountRaw}'";
                return false;
            }

            if (passengerCount <= 0)
            {
                error = "Passenger count must be greater than zero.";
                return false;
            }

            if (tripDistance < 0 || fareAmount < 0 || tipAmount < 0)
            {
                error = "Trip distance, fare amount, and tip amount must be non-negative.";
                return false;
            }

            var normalizedFlag = NormalizeStoreAndFwdFlag(storeAndFwdFlagRaw);

            if (normalizedFlag is null)
            {
                error = $"Invalid store_and_fwd_flag: '{storeAndFwdFlagRaw}'";
                return false;
            }

            ride = new TaxiRide
            {
                PickupDatetimeUtc = pickupUtc,
                DropoffDatetimeUtc = dropoffUtc,
                PassengerCount = passengerCount,
                TripDistance = tripDistance,
                StoreAndFwdFlag = normalizedFlag,
                PULocationID = puLocationId,
                DOLocationID = doLocationId,
                FareAmount = fareAmount,
                TipAmount = tipAmount
            };

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private bool TryParseEasternToUtc(string value, out DateTime utcDateTime)
    {
        utcDateTime = default;

        if (!DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var localDateTime))
        {
            return false;
        }

        localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, _sourceTimeZone);

        return true;
    }

    private static string? NormalizeStoreAndFwdFlag(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "Y" => "Yes",
            "N" => "No",
            _ => null
        };
    }
}