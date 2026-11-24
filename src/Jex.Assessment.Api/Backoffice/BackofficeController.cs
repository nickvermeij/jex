using Asp.Versioning;
using Jex.Assessment.Api.Companies;
using Jex.Assessment.Api.Infrastructure;
using Jex.Assessment.Api.Vacancies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jex.Assessment.Api.Backoffice;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/backoffice/companies")]
public sealed class BackofficeController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    
    [ProducesResponseType<IReadOnlyCollection<CompanySummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CompanySummaryResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var companies = await dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .Select(company => new CompanySummaryResponse(
                company.Id,
                company.Name,
                AddressResponseHelpers.ToResponse(company.Address)))
            .ToListAsync(cancellationToken);

        return Ok(companies);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CompanyDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyDetailResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies
            .AsNoTracking()
            .Where(company => company.Id == id)
            .Select(company => new CompanyDetailResponse(
                company.Id,
                company.Name,
                AddressResponseHelpers.ToResponse(company.Address),
                company.Vacancies
                    .OrderByDescending(vacancy => vacancy.CreatedAtUtc)
                    .Select(vacancy => new VacancySummaryResponse(
                        vacancy.Id,
                        vacancy.CompanyId,
                        vacancy.Title,
                        vacancy.Description,
                        vacancy.Status,
                        vacancy.CreatedAtUtc))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    [ProducesResponseType<CompanySummaryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanySummaryResponse>> Create(
        CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        if (!HasContent(request.Name, request.Address))
        {
            ModelState.AddModelError(nameof(request.Name), "Name and address fields cannot contain only whitespace.");
            return ValidationProblem(ModelState);
        }

        var company = new Company
        {
            Name = request.Name.Trim(),
            Address = ToEntity(request.Address)
        };

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CompanySummaryResponse(company.Id, company.Name, AddressResponseHelpers.ToResponse(company.Address));
        return CreatedAtAction(nameof(GetById), new { version = "1", id = company.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies.FindAsync([id], cancellationToken);
        if (company is null)
        {
            return NotFound();
        }

        if (!HasContent(request.Name, request.Address))
        {
            ModelState.AddModelError(nameof(request.Name), "Name and address fields cannot contain only whitespace.");
            return ValidationProblem(ModelState);
        }

        company.Name = request.Name.Trim();
        company.Address = ToEntity(request.Address);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await dbContext.Companies
            .Where(company => company.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted == 0 ? NotFound() : NoContent();
    }

    [HttpPost("vacancies")]
    [ProducesResponseType<VacancySummaryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacancySummaryResponse>> Create(
        Guid companyId,
        CreateBackofficeVacancyRequest request,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Companies.AnyAsync(company => company.Id == companyId, cancellationToken))
        {
            return NotFound();
        }

        var title = request.Title.Trim();
        var description = request.Description.Trim();
        if (title.Length == 0 || description.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Title), "Title and description cannot contain only whitespace.");
            return ValidationProblem(ModelState);
        }

        var vacancy = new Vacancy
        {
            CompanyId = companyId,
            Title = title,
            Description = description,
            Status = request.Status
        };

        dbContext.Vacancies.Add(vacancy);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToVacancySummaryResponse(vacancy);
        return CreatedAtAction(nameof(GetById), new { version = "1", id = vacancy.Id }, response);
    }

    private static VacancySummaryResponse ToVacancySummaryResponse(Vacancy vacancy) =>
        new(vacancy.Id, vacancy.CompanyId, vacancy.Title, vacancy.Description,
            vacancy.Status, vacancy.CreatedAtUtc);

    private static bool HasContent(string name, AddressRequest address) =>
        !string.IsNullOrWhiteSpace(name) &&
        !string.IsNullOrWhiteSpace(address.Street) &&
        !string.IsNullOrWhiteSpace(address.HouseNumber) &&
        !string.IsNullOrWhiteSpace(address.PostalCode) &&
        !string.IsNullOrWhiteSpace(address.City) &&
        !string.IsNullOrWhiteSpace(address.CountryCode);

    private static Address ToEntity(AddressRequest address) => new()
    {
        Street = address.Street.Trim(),
        HouseNumber = address.HouseNumber.Trim(),
        PostalCode = address.PostalCode.Trim().ToUpperInvariant(),
        City = address.City.Trim(),
        CountryCode = address.CountryCode.Trim().ToUpperInvariant(),
        Latitude = address.Latitude,
        Longitude = address.Longitude
    };
    
}
