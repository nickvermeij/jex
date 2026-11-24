using Jex.Assessment.Api.Companies;
using Jex.Assessment.Api.Vacancies;
using Microsoft.EntityFrameworkCore;

namespace Jex.Assessment.Api.Infrastructure;

public static class DevelopmentDataSeeder
{
    private static readonly Guid JexId = Guid.Parse("01900000-0000-7000-8000-000000000001");
    private static readonly Guid AmsterdamCompanyId = Guid.Parse("01900000-0000-7000-8000-000000000002");
    private static readonly Guid UtrechtCompanyId = Guid.Parse("01900000-0000-7000-8000-000000000003");

    public static async Task SeedAsync(
        AppDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var companies = CreateCompanies();
        var companyIds = companies.Select(company => company.Id).ToArray();
        var existingCompanyIds = await dbContext.Companies
            .Where(company => companyIds.Contains(company.Id))
            .Select(company => company.Id)
            .ToHashSetAsync(cancellationToken);

        var newCompanies = companies
            .Where(company => !existingCompanyIds.Contains(company.Id))
            .ToArray();

        if (newCompanies.Length > 0)
        {
            dbContext.Companies.AddRange(newCompanies);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var vacancies = CreateVacancies();
        var vacancyIds = vacancies.Select(vacancy => vacancy.Id).ToArray();
        var existingVacancyIds = await dbContext.Vacancies
            .Where(vacancy => vacancyIds.Contains(vacancy.Id))
            .Select(vacancy => vacancy.Id)
            .ToHashSetAsync(cancellationToken);

        var newVacancies = vacancies
            .Where(vacancy => !existingVacancyIds.Contains(vacancy.Id))
            .ToArray();

        if (newVacancies.Length > 0)
        {
            dbContext.Vacancies.AddRange(newVacancies);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Development seed complete: {CompanyCount} companies and {VacancyCount} vacancies added.",
            newCompanies.Length,
            newVacancies.Length);
    }

    private static Company[] CreateCompanies() =>
    [
        new Company
        {
            Id = JexId,
            Name = "JEX",
            Address = new Address
            {
                Street = "Nassaukade",
                HouseNumber = "5",
                PostalCode = "3071 JL",
                City = "Rotterdam",
                CountryCode = "NL",
                Latitude = 51.9143,
                Longitude = 4.5034
            }
        },
        new Company
        {
            Id = AmsterdamCompanyId,
            Name = "Canal Tech",
            Address = new Address
            {
                Street = "Keizersgracht",
                HouseNumber = "123",
                PostalCode = "1015 CJ",
                City = "Amsterdam",
                CountryCode = "NL",
                Latitude = 52.3740,
                Longitude = 4.8897
            }
        },
        new Company
        {
            Id = UtrechtCompanyId,
            Name = "Dom Digital",
            Address = new Address
            {
                Street = "Domplein",
                HouseNumber = "9",
                PostalCode = "3512 JC",
                City = "Utrecht",
                CountryCode = "NL",
                Latitude = 52.0907,
                Longitude = 5.1214
            }
        }
    ];

    private static Vacancy[] CreateVacancies() =>
    [
        new Vacancy
        {
            Id = Guid.Parse("01900000-0000-7000-8000-000000000101"),
            CompanyId = JexId,
            Title = "Senior .NET Developer",
            Description = "Bouw schaalbare services en werk mee aan technische keuzes.",
            Status = VacancyStatus.Active,
            CreatedAtUtc = new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc)
        },
        new Vacancy
        {
            Id = Guid.Parse("01900000-0000-7000-8000-000000000102"),
            CompanyId = JexId,
            Title = "Platform Engineer",
            Description = null,
            Status = VacancyStatus.Draft,
            CreatedAtUtc = new DateTime(2026, 1, 11, 9, 0, 0, DateTimeKind.Utc)
        },
        new Vacancy
        {
            Id = Guid.Parse("01900000-0000-7000-8000-000000000103"),
            CompanyId = AmsterdamCompanyId,
            Title = "Backend Developer",
            Description = "Werk aan API's voor ons recruitmentplatform.",
            Status = VacancyStatus.Active,
            CreatedAtUtc = new DateTime(2026, 1, 12, 9, 0, 0, DateTimeKind.Utc)
        },
        new Vacancy
        {
            Id = Guid.Parse("01900000-0000-7000-8000-000000000104"),
            CompanyId = UtrechtCompanyId,
            Title = "Software Engineer",
            Description = "Deze vacature is inmiddels gesloten.",
            Status = VacancyStatus.Closed,
            CreatedAtUtc = new DateTime(2026, 1, 13, 9, 0, 0, DateTimeKind.Utc)
        }
    ];
}
