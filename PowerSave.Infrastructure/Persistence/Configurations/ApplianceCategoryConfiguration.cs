using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class ApplianceCategoryConfiguration : IEntityTypeConfiguration<ApplianceCategory>
{
    public void Configure(EntityTypeBuilder<ApplianceCategory> builder)
    {
        builder.ToTable("ApplianceCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NameAr)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.NameEn)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.IconUrl)
            .HasMaxLength(500);

        builder.HasIndex(c => c.CategoryType);
    }
}
