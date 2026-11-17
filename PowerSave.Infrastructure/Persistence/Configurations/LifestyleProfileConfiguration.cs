using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class LifestyleProfileConfiguration : IEntityTypeConfiguration<LifestyleProfile>
{
    public void Configure(EntityTypeBuilder<LifestyleProfile> builder)
    {
        builder.ToTable("LifestyleProfiles");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.DailyRoutine)
            .HasMaxLength(200);

        builder.Property(l => l.HomeTimeSlotsCsv)
            .HasMaxLength(200);

        builder.Property(l => l.WeekendRoutine)
            .HasMaxLength(200);

        builder.HasIndex(l => l.UserId)
            .IsUnique();

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
