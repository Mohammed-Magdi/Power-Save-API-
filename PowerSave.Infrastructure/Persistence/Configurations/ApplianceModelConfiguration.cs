using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class ApplianceModelConfiguration : IEntityTypeConfiguration<ApplianceModel>
{
    public void Configure(EntityTypeBuilder<ApplianceModel> builder)
    {
        builder.ToTable("ApplianceModels");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Brand)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(m => m.ModelNameOrNumber)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(m => m.CapacityOrSize)
            .HasMaxLength(100);

        builder.Property(m => m.EnergyRating)
            .HasMaxLength(50);

        builder.HasIndex(m => m.ApplianceCatalogId);
        builder.HasIndex(m => m.Brand);

        builder.HasOne(m => m.ApplianceCatalog)
            .WithMany(a => a.Models)
            .HasForeignKey(m => m.ApplianceCatalogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
