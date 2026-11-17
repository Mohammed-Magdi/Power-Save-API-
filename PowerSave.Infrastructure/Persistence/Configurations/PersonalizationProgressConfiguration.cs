using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerSave.Domain.Entities;

namespace PowerSave.Infrastructure.Persistence.Configurations;

public class PersonalizationProgressConfiguration : IEntityTypeConfiguration<PersonalizationProgress>
{
    public void Configure(EntityTypeBuilder<PersonalizationProgress> builder)
    {
        builder.ToTable("PersonalizationProgresses");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CompletedStagesCsv)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.SkippedStagesCsv)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
