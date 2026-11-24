using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Jex.Assessment.Api.Infrastructure;
using Jex.Assessment.Api.Vacancies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jex.Assessment.Api.Companies;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/companies")]
public sealed class CompaniesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<PublicCompanyResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PublicCompanyResponse>>> GetWithActiveVacancies(
        CancellationToken cancellationToken)
    {
        var companies = await dbContext.Companies
            .AsNoTracking()
            .Where(company => company.Vacancies.Any(vacancy => vacancy.Status == VacancyStatus.Active))
            .OrderBy(company => company.Name)
            .Select(company => new PublicCompanyResponse(
                company.Id,
                company.Name,
                AddressResponseHelpers.ToResponse(company.Address),
                company.Vacancies
                    .Where(vacancy => vacancy.Status == VacancyStatus.Active)
                    .OrderByDescending(vacancy => vacancy.CreatedAtUtc)
                    .Select(vacancy => new PublicVacancyResponse(
                        vacancy.Id,
                        vacancy.Title,
                        vacancy.Description!))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Ok(companies);
    }

    [HttpGet("nearby")]
    [ProducesResponseType<IReadOnlyCollection<NearbyCompanyResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<NearbyCompanyResponse>>> GetNearby(
        [FromQuery, Range(-90, 90)] double latitude,
        [FromQuery, Range(-180, 180)] double longitude,
        [FromQuery, Range(0.1, 500)] double radiusKm = 25,
        CancellationToken cancellationToken = default)
    {
        // SQLite has no spatial index/provider. A latitude bound limits the candidate set in SQL;
        // exact Haversine distance is then calculated in memory. For larger datasets a DB with spatial features should be used.
        const double kilometresPerLatitudeDegree = 111.32;
        var latitudeDelta = radiusKm / kilometresPerLatitudeDegree;

        var candidates = await dbContext.Companies
            .AsNoTracking()
            .Where(company => company.Address.Latitude.HasValue && company.Address.Longitude.HasValue)
            .Where(company => company.Address.Latitude >= latitude - latitudeDelta &&
                              company.Address.Latitude <= latitude + latitudeDelta)
            .Where(company => company.Vacancies.Any(vacancy => vacancy.Status == VacancyStatus.Active))
            .Select(company => new NearbyCandidate(
                company.Id,
                company.Name,
                AddressResponseHelpers.ToResponse(company.Address),
                company.Address.Latitude!.Value,
                company.Address.Longitude!.Value,
                company.Vacancies
                    .Where(vacancy => vacancy.Status == VacancyStatus.Active)
                    .OrderByDescending(vacancy => vacancy.CreatedAtUtc)
                    .Select(vacancy => new PublicVacancyResponse(
                        vacancy.Id,
                        vacancy.Title,
                        vacancy.Description!))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var companies = candidates
            .Select(company => new
            {
                Company = company,
                DistanceKm = Haversine.DistanceInKilometres(
                    latitude, longitude, company.Latitude, company.Longitude)
            })
            .Where(result => result.DistanceKm <= radiusKm)
            .OrderBy(result => result.DistanceKm)
            .Select(result => new NearbyCompanyResponse(
                result.Company.Id,
                result.Company.Name,
                result.Company.Address,
                Math.Round(result.DistanceKm, 2),
                result.Company.Vacancies))
            .ToList();

        return Ok(companies);
    }

    private sealed record NearbyCandidate(
        Guid Id,
        string Name,
        AddressResponse Address,
        double Latitude,
        double Longitude,
        IReadOnlyCollection<PublicVacancyResponse> Vacancies);
}
