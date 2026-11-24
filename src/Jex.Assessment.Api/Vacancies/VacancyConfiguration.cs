using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jex.Assessment.Api.Vacancies;

public sealed class VacancyConfiguration : IEntityTypeConfiguration<Vacancy>
{
    public void Configure(EntityTypeBuilder<Vacancy> builder)
    {
        builder.HasKey(vacancy => vacancy.Id);
        builder.Property(vacancy => vacancy.Title).HasMaxLength(200).IsRequired();
        builder.Property(vacancy => vacancy.Description).HasMaxLength(4_000);
        builder.Property(vacancy => vacancy.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(vacancy => new { vacancy.CompanyId, vacancy.Status });
    }
}
