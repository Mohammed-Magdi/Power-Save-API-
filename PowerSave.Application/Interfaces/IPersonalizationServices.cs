using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerSave.Application.Common.Models;
using PowerSave.Application.DTOs;
using PowerSave.Domain.Enums;

namespace PowerSave.Application.Interfaces;

public interface IPersonalizationService
{
    Task<Result<PersonalizationStatusDto>> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<PersonalizationSavedAnswersDto>> GetSavedAnswersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<PersonalizationStatusDto>> ResumeOnboardingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<PersonalizationStatusDto>> SkipStageAsync(Guid userId, PersonalizationStage stage, CancellationToken cancellationToken = default);
    Task<Result<PersonalizationStatusDto>> CompletePersonalizationAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IHouseholdProfileService
{
    Task<Result<HouseholdProfileDto>> SaveHouseholdProfileAsync(Guid userId, HouseholdProfileDto dto, CancellationToken cancellationToken = default);
    Task<Result<HouseholdProfileDto>> GetHouseholdProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<LifestyleProfileDto>> SaveLifestyleProfileAsync(Guid userId, LifestyleProfileDto dto, CancellationToken cancellationToken = default);
    Task<Result<LifestyleProfileDto>> GetLifestyleProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IApplianceService
{
    Task<Result<IReadOnlyList<ApplianceCategoryResponseDto>>> GetCategoriesWithAppliancesAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ApplianceCatalogResponseDto>>> GetAppliancesByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserApplianceResponseDto>>> GetUserAppliancesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserApplianceResponseDto>>> SaveApplianceSelectionsAsync(Guid userId, SaveAppliancesRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<UserApplianceResponseDto>> AddCustomApplianceAsync(Guid userId, ApplianceSelectionItemDto customAppliance, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveUserApplianceAsync(Guid userId, Guid userApplianceId, CancellationToken cancellationToken = default);
    Task<Result<bool>> RemoveAllUserAppliancesAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IApplianceModelService
{
    Task<Result<IReadOnlyList<ApplianceCatalogResponseDto>>> GetModelSensitiveAppliancesAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ApplianceModelResponseDto>>> GetModelsForApplianceAsync(Guid applianceCatalogId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ApplianceModelResponseDto>>> SearchModelsAsync(string query, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserApplianceResponseDto>>> SaveModelSelectionsAsync(Guid userId, SaveApplianceModelsRequestDto request, CancellationToken cancellationToken = default);
}

public interface IMeterSetupService
{
    Task<Result<MeterSetupDto>> SaveMeterSetupAsync(Guid userId, MeterSetupDto dto, CancellationToken cancellationToken = default);
    Task<Result<MeterSetupDto>> GetMeterSetupAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ISavingTargetService
{
    Task<Result<SavingTargetDto>> SaveSavingTargetAsync(Guid userId, SavingTargetDto dto, CancellationToken cancellationToken = default);
    Task<Result<SavingTargetDto>> GetSavingTargetAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IOperatingHoursCalculator
{
    string Version { get; }
    double CalculateSuggestedHours(
        HouseholdSize? householdSize,
        DaytimePresence? daytimePresence,
        TimeSpan? wakeUpTime,
        TimeSpan? bedTime,
        double catalogDefaultDailyHours,
        ApplianceCategoryType? categoryType,
        bool isModelSensitive);
}
