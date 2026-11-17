using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class UserApplianceConfiguration : IEntityTypeConfiguration<UserAppliance>
{
    public void Configure(EntityTypeBuilder<UserAppliance> builder)
    {
        builder.ToTable("UserAppliances");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.CustomName)
            .HasMaxLength(200);

        builder.Property(u => u.RoomLocation)
            .HasMaxLength(150);

        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => u.ApplianceCatalogId);
        builder.HasIndex(u => u.ApplianceModelId);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.ApplianceCatalog)
            .WithMany(c => c.UserAppliances)
            .HasForeignKey(u => u.ApplianceCatalogId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(u => u.ApplianceModel)
            .WithMany(m => m.UserAppliances)
            .HasForeignKey(u => u.ApplianceModelId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
