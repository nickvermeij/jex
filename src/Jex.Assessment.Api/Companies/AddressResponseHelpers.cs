namespace Jex.Assessment.Api.Companies;

public class AddressResponseHelpers
{
    

    internal static AddressResponse ToResponse(Address address) => new(
        address.Street,
        address.HouseNumber,
        address.PostalCode,
        address.City,
        address.CountryCode,
        address.Latitude,
        address.Longitude);
}