using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class ApplianceCatalogConfiguration : IEntityTypeConfiguration<ApplianceCatalog>
{
    public void Configure(EntityTypeBuilder<ApplianceCatalog> builder)
    {
        builder.ToTable("ApplianceCatalogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.NameAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.NameEn)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.IconUrl)
            .HasMaxLength(500);

        builder.HasIndex(a => a.CategoryId);

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Appliances)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
