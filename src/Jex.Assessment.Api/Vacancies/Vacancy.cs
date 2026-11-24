using Jex.Assessment.Api.Companies;

namespace Jex.Assessment.Api.Vacancies;

public sealed class Vacancy
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid CompanyId { get; init; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public VacancyStatus Status { get; set; } = VacancyStatus.Draft;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public Company Company { get; init; } = null!;
}
