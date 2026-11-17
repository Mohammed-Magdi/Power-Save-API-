using System;
using System.Collections.Generic;
using PowerSave.Domain.Enums;

namespace PowerSave.Application.DTOs;

public class PersonalizationStatusDto
{
    public PersonalizationStage CurrentStage { get; set; }
    public StageStatus CurrentStageStatus { get; set; }
    public List<PersonalizationStage> CompletedStages { get; set; } = new();
    public List<PersonalizationStage> SkippedStages { get; set; } = new();
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
}

public class SkipStageRequestDto
{
    public PersonalizationStage Stage { get; set; }
}

public class HouseholdProfileDto
{
    public HouseholdSize? HouseholdSize { get; set; }
    public HomeType? HomeType { get; set; }
    public HomeArea? HomeArea { get; set; }
    public int? NumberOfRooms { get; set; }
    public DaytimePresence? DaytimePresence { get; set; }
    public bool IsSkipped { get; set; }
}

public class LifestyleProfileDto
{
    public string? DailyRoutine { get; set; }
    public List<string>? HomeTimeSlots { get; set; }
    public TimeSpan? WakeUpTime { get; set; }
    public TimeSpan? BedTime { get; set; }
    public string? WeekendRoutine { get; set; }
    public bool IsSkipped { get; set; }
}

public class ApplianceSelectionItemDto
{
    public Guid? ApplianceCatalogId { get; set; }
    public string? CustomName { get; set; }
    public int Quantity { get; set; } = 1;
    public double? CustomWattage { get; set; }
    public string? RoomLocation { get; set; }
}

public class SaveAppliancesRequestDto
{
    public List<ApplianceSelectionItemDto> Selections { get; set; } = new();
}

public class ApplianceModelSelectionItemDto
{
    public Guid UserApplianceId { get; set; }
    public Guid? ApplianceModelId { get; set; }
    public bool IsModelUnknown { get; set; }
    public bool IsModelSkipped { get; set; }
    public double? CustomWattage { get; set; }
}

public class SaveApplianceModelsRequestDto
{
    public List<ApplianceModelSelectionItemDto> ModelSelections { get; set; } = new();
}

public class MeterSetupDto
{
    public MeterType MeterType { get; set; }
    public int BillPaymentDay { get; set; } = 1;
    public DateTime ConsumptionStartDate { get; set; }
    public decimal? MonthlyBillAmount { get; set; }
    public decimal? MonthlyCardBudget { get; set; }
    public bool IsSkipped { get; set; }
}

public class SavingTargetDto
{
    public int? SavingPercentage { get; set; }
    public bool IsSkipped { get; set; }
}

public class PersonalizationSavedAnswersDto
{
    public PersonalizationStatusDto Status { get; set; } = null!;
    public HouseholdProfileDto? Household { get; set; }
    public LifestyleProfileDto? Lifestyle { get; set; }
    public List<UserApplianceResponseDto> Appliances { get; set; } = new();
    public MeterSetupDto? Meter { get; set; }
    public SavingTargetDto? SavingTarget { get; set; }
}

public class ApplianceCategoryResponseDto
{
    public Guid Id { get; set; }
    public ApplianceCategoryType CategoryType { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
    public List<ApplianceCatalogResponseDto> Appliances { get; set; } = new();
}

public class ApplianceCatalogResponseDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsModelSensitive { get; set; }
    public double DefaultWattage { get; set; }
    public double DefaultDailyHours { get; set; }
    public int MaxAllowedQuantity { get; set; }
}

public class ApplianceModelResponseDto
{
    public Guid Id { get; set; }
    public Guid ApplianceCatalogId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string ModelNameOrNumber { get; set; } = string.Empty;
    public string? CapacityOrSize { get; set; }
    public string? EnergyRating { get; set; }
    public double Wattage { get; set; }
}

public class UserApplianceResponseDto
{
    public Guid Id { get; set; }
    public Guid? ApplianceCatalogId { get; set; }
    public string ApplianceName { get; set; } = string.Empty;
    public string? CustomName { get; set; }
    public Guid? ApplianceModelId { get; set; }
    public string? ModelBrand { get; set; }
    public string? ModelNameOrNumber { get; set; }
    public bool IsModelUnknown { get; set; }
    public bool IsModelSkipped { get; set; }
    public double Wattage { get; set; }
    public bool IsWattageEstimated { get; set; }
    public double DailyOperatingHours { get; set; }
    public string? RoomLocation { get; set; }
    public int SequenceNumber { get; set; }
}
