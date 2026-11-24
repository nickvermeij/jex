using Asp.Versioning;
using Jex.Assessment.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jex.Assessment.Api.Vacancies;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/vacancies")]
public sealed class VacanciesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<VacancySummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacancySummaryResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var vacancy = await dbContext.Vacancies
            .AsNoTracking()
            .Where(vacancy => vacancy.Id == id)
            .Select(vacancy => new VacancySummaryResponse(
                vacancy.Id,
                vacancy.CompanyId,
                vacancy.Title,
                vacancy.Description,
                vacancy.Status,
                vacancy.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return vacancy is null ? NotFound() : Ok(vacancy);
    }
    
    [HttpPost("{companyId:guid}/vacancies")]
    [ProducesResponseType<VacancySummaryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacancySummaryResponse>> CreateVacancy(
        Guid companyId,
        CreatePublicVacancyRequest request,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Companies.AnyAsync(company => company.Id == companyId, cancellationToken))
        {
            return NotFound();
        }

        var title = request.Title.Trim();
        if (title.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Title), "Title cannot contain only whitespace.");
            return ValidationProblem(ModelState);
        }

        var vacancy = new Vacancy
        {
            CompanyId = companyId,
            Title = title,
            Description = null,
            Status = VacancyStatus.Draft
        };

        dbContext.Vacancies.Add(vacancy);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new VacancySummaryResponse(
            vacancy.Id,
            vacancy.CompanyId,
            vacancy.Title,
            vacancy.Description,
            vacancy.Status,
            vacancy.CreatedAtUtc);

        return CreatedAtAction(
            nameof(VacanciesController.GetById),
            "Vacancies",
            new { version = "1", id = vacancy.Id },
            response);
    }

    [HttpGet("~/api/v{version:apiVersion}/companies/{companyId:guid}/vacancies")]
    [ProducesResponseType<IReadOnlyCollection<VacancySummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<VacancySummaryResponse>>> GetForCompany(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Companies.AnyAsync(company => company.Id == companyId, cancellationToken))
        {
            return NotFound();
        }

        var vacancies = await dbContext.Vacancies
            .AsNoTracking()
            .Where(vacancy => vacancy.CompanyId == companyId)
            .OrderByDescending(vacancy => vacancy.CreatedAtUtc)
            .Select(vacancy => new VacancySummaryResponse(
                vacancy.Id,
                vacancy.CompanyId,
                vacancy.Title,
                vacancy.Description,
                vacancy.Status,
                vacancy.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(vacancies);
    }

    

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateVacancyRequest request,
        CancellationToken cancellationToken)
    {
        var vacancy = await dbContext.Vacancies.FindAsync([id], cancellationToken);
        if (vacancy is null)
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

        vacancy.Title = title;
        vacancy.Description = description;
        vacancy.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await dbContext.Vacancies
            .Where(vacancy => vacancy.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted == 0 ? NotFound() : NoContent();
    }
}
