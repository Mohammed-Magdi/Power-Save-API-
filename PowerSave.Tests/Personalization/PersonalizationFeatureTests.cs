using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PowerSave.Application.Common.Interfaces;
using PowerSave.Application.Common.Localization;
using PowerSave.Application.DTOs;
using PowerSave.Application.Interfaces;
using PowerSave.Application.Services;
using PowerSave.Domain.Entities;
using PowerSave.Domain.Enums;
using PowerSave.Infrastructure.Persistence;
using PowerSave.Tests.Helpers;
using Xunit;

namespace PowerSave.Tests.Personalization;

public class PersonalizationFeatureTests
{
    private readonly PowerSaveDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TestTimeProvider _timeProvider;
    private readonly IOperatingHoursCalculator _hoursCalculator;

    public PersonalizationFeatureTests()
    {
        var options = new DbContextOptionsBuilder<PowerSaveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PowerSaveDbContext(options);
        _unitOfWork = new UnitOfWork(_dbContext);

        _timeProvider = new TestTimeProvider { UtcNow = new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc) };
        _hoursCalculator = new OperatingHoursCalculator();
    }

    [Fact]
    public async Task PersonalizationService_GetStatus_WhenNewUser_InitializesHouseholdStage()
    {
        var service = new PersonalizationService(_unitOfWork, _timeProvider);
        var userId = Guid.NewGuid();

        var result = await service.GetStatusAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(PersonalizationStage.Household, result.Data.CurrentStage);
        Assert.Equal(StageStatus.NotStarted, result.Data.CurrentStageStatus);
        Assert.False(result.Data.IsCompleted);
    }

    [Fact]
    public async Task PersonalizationService_SkipStage_MarksStageSkippedAndAdvances()
    {
        var service = new PersonalizationService(_unitOfWork, _timeProvider);
        var userId = Guid.NewGuid();

        var skipResult = await service.SkipStageAsync(userId, PersonalizationStage.Household);

        Assert.True(skipResult.IsSuccess);
        Assert.Equal(PersonalizationStage.Lifestyle, skipResult.Data!.CurrentStage);
        Assert.Contains(PersonalizationStage.Household, skipResult.Data.SkippedStages);

        var household = await _unitOfWork.Repository<HouseholdProfile>().FirstOrDefaultAsync(h => h.UserId == userId);
        Assert.NotNull(household);
        Assert.True(household.IsSkipped);
        Assert.Null(household.HouseholdSize);
    }

    [Fact]
    public async Task HouseholdService_SaveValidProfile_UpdatesAndAdvancesStage()
    {
        var householdService = new HouseholdProfileService(_unitOfWork, _timeProvider);
        var personalizationService = new PersonalizationService(_unitOfWork, _timeProvider);
        var userId = Guid.NewGuid();

        var dto = new HouseholdProfileDto
        {
            HouseholdSize = HouseholdSize.ThreeToFour,
            HomeType = HomeType.Apartment,
            HomeArea = HomeArea.Between70And120,
            NumberOfRooms = 3,
            DaytimePresence = DaytimePresence.TwoToThree
        };

        var saveResult = await householdService.SaveHouseholdProfileAsync(userId, dto);
        Assert.True(saveResult.IsSuccess);

        var statusResult = await personalizationService.GetStatusAsync(userId);
        Assert.Contains(PersonalizationStage.Household, statusResult.Data!.CompletedStages);
        Assert.Equal(PersonalizationStage.Lifestyle, statusResult.Data.CurrentStage);
    }

    [Fact]
    public async Task ApplianceService_SaveSelections_QuantityConvertsToIndependentRecords()
    {
        var catalogId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        await _unitOfWork.Repository<ApplianceCategory>().AddAsync(new ApplianceCategory
        {
            Id = categoryId,
            CategoryType = ApplianceCategoryType.CoolingAndHeating,
            NameAr = "تبريد",
            NameEn = "Cooling"
        });

        await _unitOfWork.Repository<ApplianceCatalog>().AddAsync(new ApplianceCatalog
        {
            Id = catalogId,
            CategoryId = categoryId,
            NameAr = "تكييف 1.5 حصان",
            NameEn = "AC 1.5 HP",
            DefaultWattage = 1200,
            DefaultDailyHours = 8,
            IsModelSensitive = true
        });

        await _unitOfWork.SaveChangesAsync();

        var applianceService = new ApplianceService(_unitOfWork, _hoursCalculator, _timeProvider);
        var userId = Guid.NewGuid();

        var request = new SaveAppliancesRequestDto
        {
            Selections = new List<ApplianceSelectionItemDto>
            {
                new ApplianceSelectionItemDto
                {
                    ApplianceCatalogId = catalogId,
                    Quantity = 3,
                    RoomLocation = "غرفة المعيشة"
                }
            }
        };

        var saveResult = await applianceService.SaveApplianceSelectionsAsync(userId, request);

        Assert.True(saveResult.IsSuccess);
        Assert.Equal(3, saveResult.Data!.Count);

        var dbRecords = await _unitOfWork.Repository<UserAppliance>().FindAsync(u => u.UserId == userId);
        Assert.Equal(3, dbRecords.Count);
        Assert.True(dbRecords.All(r => r.ApplianceCatalogId == catalogId));
        Assert.True(dbRecords.All(r => r.IsWattageEstimated));
        Assert.Equal(3, dbRecords.Select(r => r.Id).Distinct().Count());
    }

    [Fact]
    public async Task ApplianceModelService_ApplyModel_UpdatesIndividualApplianceWattage()
    {
        var catalogId = Guid.NewGuid();
        var modelId = Guid.NewGuid();

        await _unitOfWork.Repository<ApplianceCatalog>().AddAsync(new ApplianceCatalog
        {
            Id = catalogId,
            CategoryId = Guid.NewGuid(),
            NameAr = "تكييف 1.5 حصان",
            NameEn = "AC 1.5 HP",
            DefaultWattage = 1200
        });

        await _unitOfWork.Repository<ApplianceModel>().AddAsync(new ApplianceModel
        {
            Id = modelId,
            ApplianceCatalogId = catalogId,
            Brand = "Sharp",
            ModelNameOrNumber = "Inverter XP12",
            Wattage = 950
        });

        var userApplianceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await _unitOfWork.Repository<UserAppliance>().AddAsync(new UserAppliance
        {
            Id = userApplianceId,
            UserId = userId,
            ApplianceCatalogId = catalogId,
            Wattage = 1200,
            IsWattageEstimated = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdatedAtUtc = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        var modelService = new ApplianceModelService(_unitOfWork, _timeProvider);

        var request = new SaveApplianceModelsRequestDto
        {
            ModelSelections = new List<ApplianceModelSelectionItemDto>
            {
                new ApplianceModelSelectionItemDto
                {
                    UserApplianceId = userApplianceId,
                    ApplianceModelId = modelId
                }
            }
        };

        var result = await modelService.SaveModelSelectionsAsync(userId, request);

        Assert.True(result.IsSuccess);
        var updated = await _unitOfWork.Repository<UserAppliance>().GetByIdAsync(userApplianceId);
        Assert.NotNull(updated);
        Assert.Equal(modelId, updated.ApplianceModelId);
        Assert.Equal(950, updated.Wattage);
        Assert.False(updated.IsWattageEstimated);
    }

    [Fact]
    public async Task MeterSetup_DefaultBillPaymentDay_IsDay1WhenSkipped()
    {
        var personalizationService = new PersonalizationService(_unitOfWork, _timeProvider);
        var userId = Guid.NewGuid();

        var skipResult = await personalizationService.SkipStageAsync(userId, PersonalizationStage.MeterAndTarget);
        Assert.True(skipResult.IsSuccess);

        var meter = await _unitOfWork.Repository<MeterSetup>().FirstOrDefaultAsync(m => m.UserId == userId);
        Assert.NotNull(meter);
        Assert.Equal(1, meter.BillPaymentDay);
        Assert.True(meter.IsSkipped);
        Assert.Null(meter.MonthlyBillAmount);
    }

    [Fact]
    public async Task SavingTarget_InvalidPercentage_ReturnsFailure()
    {
        var service = new SavingTargetService(_unitOfWork, _timeProvider);
        var userId = Guid.NewGuid();

        var result = await service.SaveSavingTargetAsync(userId, new SavingTargetDto
        {
            SavingPercentage = 30 // غير معتمد (المعتمد 5, 10, 15, 20)
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrorCode.InvalidSavingPercentage, result.ErrorCode);
    }
}
