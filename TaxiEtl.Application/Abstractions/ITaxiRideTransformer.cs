using TaxiEtl.Application.Models;
using TaxiEtl.Domain.Entities;

namespace TaxiEtl.Application.Abstractions;

public interface ITaxiRideTransformer
{
    bool TryTransform(
        RawTaxiRideCsvRow rawRow,
        out TaxiRide? ride,
        out string? error);
}