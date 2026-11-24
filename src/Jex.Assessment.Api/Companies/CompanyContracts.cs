using System.ComponentModel.DataAnnotations;
using Jex.Assessment.Api.Vacancies;

namespace Jex.Assessment.Api.Companies;

public sealed record AddressRequest(
    [Required, MaxLength(200)] string Street,
    [Required, MaxLength(20)] string HouseNumber,
    [Required, MaxLength(20)] string PostalCode,
    [Required, MaxLength(100)] string City,
    [Required, RegularExpression("^[A-Za-z]{2}$")] string CountryCode,
    [Range(-90, 90)] double? Latitude = null,
    [Range(-180, 180)] double? Longitude = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Latitude.HasValue != Longitude.HasValue)
        {
            yield return new ValidationResult(
                "Latitude and longitude must either both be supplied or both be omitted.",
                [nameof(Latitude), nameof(Longitude)]);
        }
    }
}

public sealed record AddressResponse(
    string Street,
    string HouseNumber,
    string PostalCode,
    string City,
    string CountryCode,
    double? Latitude,
    double? Longitude);

public sealed record CreateCompanyRequest(
    [Required, MaxLength(200)] string Name,
    [Required] AddressRequest Address);

public sealed record UpdateCompanyRequest(
    [Required, MaxLength(200)] string Name,
    [Required] AddressRequest Address);

public sealed record CompanySummaryResponse(Guid Id, string Name, AddressResponse Address);

public sealed record CompanyDetailResponse(
    Guid Id,
    string Name,
    AddressResponse Address,
    IReadOnlyCollection<VacancySummaryResponse> Vacancies);

public sealed record PublicCompanyResponse(
    Guid Id,
    string Name,
    AddressResponse Address,
    IReadOnlyCollection<PublicVacancyResponse> Vacancies);

public sealed record NearbyCompanyResponse(
    Guid Id,
    string Name,
    AddressResponse Address,
    double DistanceKm,
    IReadOnlyCollection<PublicVacancyResponse> Vacancies);
