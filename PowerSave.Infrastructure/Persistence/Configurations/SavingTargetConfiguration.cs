using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class SavingTargetConfiguration : IEntityTypeConfiguration<SavingTarget>
{
    public void Configure(EntityTypeBuilder<SavingTarget> builder)
    {
        builder.ToTable("SavingTargets");

        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.UserId)
            .IsUnique();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
