using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class MeterSetupConfiguration : IEntityTypeConfiguration<MeterSetup>
{
    public void Configure(EntityTypeBuilder<MeterSetup> builder)
    {
        builder.ToTable("MeterSetups");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MonthlyBillAmount)
            .HasPrecision(18, 2);

        builder.Property(m => m.MonthlyCardBudget)
            .HasPrecision(18, 2);

        builder.HasIndex(m => m.UserId)
            .IsUnique();

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
