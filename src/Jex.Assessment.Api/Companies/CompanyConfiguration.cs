using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jex.Assessment.Api.Companies;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(company => company.Id);
        builder.Property(company => company.Name).HasMaxLength(200).IsRequired();

        builder.OwnsOne(company => company.Address, address =>
        {
            address.Property(value => value.Street)
                .HasColumnName("AddressStreet").HasMaxLength(200).IsRequired();
            address.Property(value => value.HouseNumber)
                .HasColumnName("AddressHouseNumber").HasMaxLength(20).IsRequired();
            address.Property(value => value.PostalCode)
                .HasColumnName("AddressPostalCode").HasMaxLength(20).IsRequired();
            address.Property(value => value.City)
                .HasColumnName("AddressCity").HasMaxLength(100).IsRequired();
            address.Property(value => value.CountryCode)
                .HasColumnName("AddressCountryCode").HasMaxLength(2).IsRequired();
            address.Property(value => value.Latitude).HasColumnName("Latitude");
            address.Property(value => value.Longitude).HasColumnName("Longitude");
            address.HasIndex(value => new { value.Latitude, value.Longitude });
        });
        builder.Navigation(company => company.Address).IsRequired();

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Companies_Latitude", "Latitude IS NULL OR (Latitude >= -90 AND Latitude <= 90)");
            table.HasCheckConstraint("CK_Companies_Longitude", "Longitude IS NULL OR (Longitude >= -180 AND Longitude <= 180)");
            table.HasCheckConstraint(
                "CK_Companies_CoordinatePair",
                "(Latitude IS NULL AND Longitude IS NULL) OR (Latitude IS NOT NULL AND Longitude IS NOT NULL)");
        });

        builder.HasMany(company => company.Vacancies)
            .WithOne(vacancy => vacancy.Company)
            .HasForeignKey(vacancy => vacancy.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
