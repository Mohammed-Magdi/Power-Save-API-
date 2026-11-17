using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PowerSave.Domain.Entities;

namespace PowerSave.Application.Interfaces;

public interface IPowerSaveDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<UserConsent> UserConsents { get; }
    DbSet<PhoneVerificationOtp> PhoneVerificationOtps { get; }
    DbSet<UserOtpRateLimit> UserOtpRateLimits { get; }

    DbSet<PersonalizationProgress> PersonalizationProgresses { get; }
    DbSet<HouseholdProfile> HouseholdProfiles { get; }
    DbSet<LifestyleProfile> LifestyleProfiles { get; }
    DbSet<ApplianceCategory> ApplianceCategories { get; }
    DbSet<ApplianceCatalog> ApplianceCatalogs { get; }
    DbSet<ApplianceModel> ApplianceModels { get; }
    DbSet<UserAppliance> UserAppliances { get; }
    DbSet<MeterSetup> MeterSetups { get; }
    DbSet<SavingTarget> SavingTargets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

