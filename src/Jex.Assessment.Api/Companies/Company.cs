using Jex.Assessment.Api.Vacancies;

namespace Jex.Assessment.Api.Companies;

public sealed class Company
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public required Address Address { get; set; }
    public ICollection<Vacancy> Vacancies { get; } = new List<Vacancy>();
}
