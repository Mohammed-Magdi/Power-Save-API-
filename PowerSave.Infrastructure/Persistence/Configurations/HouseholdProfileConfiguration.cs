using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class HouseholdProfileConfiguration : IEntityTypeConfiguration<HouseholdProfile>
{
    public void Configure(EntityTypeBuilder<HouseholdProfile> builder)
    {
        builder.ToTable("HouseholdProfiles");

        builder.HasKey(h => h.Id);

        builder.HasIndex(h => h.UserId)
            .IsUnique();

        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
