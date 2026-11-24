namespace Jex.Assessment.Api.Companies;

public sealed class Address
{
    public required string Street { get; init; }
    public required string HouseNumber { get; init; }
    public required string PostalCode { get; init; }
    public required string City { get; init; }
    public required string CountryCode { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}
