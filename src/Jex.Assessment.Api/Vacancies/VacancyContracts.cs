using System.ComponentModel.DataAnnotations;

namespace Jex.Assessment.Api.Vacancies;

public sealed record CreateBackofficeVacancyRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(4_000)] string Description,
    VacancyStatus Status = VacancyStatus.Draft);

public sealed record UpdateVacancyRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(4_000)] string Description,
    VacancyStatus Status);

public sealed record CreatePublicVacancyRequest(
    [Required, MaxLength(200)] string Title);

public sealed record VacancySummaryResponse(
    Guid Id,
    Guid CompanyId,
    string Title,
    string? Description,
    VacancyStatus Status,
    DateTime CreatedAtUtc);

public sealed record PublicVacancyResponse(
    Guid Id,
    string Title,
    string Description);
