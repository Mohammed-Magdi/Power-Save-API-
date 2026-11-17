using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerSave.Application.Common.Interfaces;
using PowerSave.Application.Common.Localization;
using PowerSave.Application.Common.Models;
using PowerSave.Application.DTOs;
using PowerSave.Application.Interfaces;
using PowerSave.Domain.Entities;
using PowerSave.Domain.Enums;

namespace PowerSave.Application.Services;

public class PersonalizationService : IPersonalizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PersonalizationService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<PersonalizationStatusDto>> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var progressRepo = _unitOfWork.Repository<PersonalizationProgress>();
        var progress = await progressRepo.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (progress == null)
        {
            var now = _dateTimeProvider.UtcNow;
            progress = new PersonalizationProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CurrentStage = (int)PersonalizationStage.Household,
                StageStatus = (int)StageStatus.NotStarted,
                CompletedStagesCsv = string.Empty,
                SkippedStagesCsv = string.Empty,
                IsCompleted = false,
                LastUpdatedAtUtc = now
            };

            await progressRepo.AddAsync(progress, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<PersonalizationStatusDto>.Success(MapToStatusDto(progress));
    }

    public async Task<Result<PersonalizationSavedAnswersDto>> GetSavedAnswersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var statusResult = await GetStatusAsync(userId, cancellationToken);
        if (!statusResult.IsSuccess)
        {
            return Result<PersonalizationSavedAnswersDto>.Failure(statusResult.ErrorCode, statusResult.StatusCode, statusResult.Message);
        }

        var household = await _unitOfWork.Repository<HouseholdProfile>().FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
        var lifestyle = await _unitOfWork.Repository<LifestyleProfile>().FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);
        var userAppliances = await _unitOfWork.Repository<UserAppliance>().FindAsync(u => u.UserId == userId, cancellationToken);
        var meter = await _unitOfWork.Repository<MeterSetup>().FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
        var savingTarget = await _unitOfWork.Repository<SavingTarget>().FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        var catalogRepo = _unitOfWork.Repository<ApplianceCatalog>();
        var modelRepo = _unitOfWork.Repository<ApplianceModel>();

        var applianceDtos = new List<UserApplianceResponseDto>();
        foreach (var ua in userAppliances.OrderBy(a => a.SequenceNumber))
        {
            string name = ua.CustomName ?? "جهاز";
            if (ua.ApplianceCatalogId.HasValue)
            {
                var cat = await catalogRepo.GetByIdAsync(ua.ApplianceCatalogId.Value, cancellationToken);
                if (cat != null) name = cat.NameAr;
            }

            string? modelBrand = null;
            string? modelNumber = null;
            if (ua.ApplianceModelId.HasValue)
            {
                var mod = await modelRepo.GetByIdAsync(ua.ApplianceModelId.Value, cancellationToken);
                if (mod != null)
                {
                    modelBrand = mod.Brand;
                    modelNumber = mod.ModelNameOrNumber;
                }
            }

            applianceDtos.Add(new UserApplianceResponseDto
            {
                Id = ua.Id,
                ApplianceCatalogId = ua.ApplianceCatalogId,
                ApplianceName = name,
                CustomName = ua.CustomName,
                ApplianceModelId = ua.ApplianceModelId,
                ModelBrand = modelBrand,
                ModelNameOrNumber = modelNumber,
                IsModelUnknown = ua.IsModelUnknown,
                IsModelSkipped = ua.IsModelSkipped,
                Wattage = ua.Wattage,
                IsWattageEstimated = ua.IsWattageEstimated,
                DailyOperatingHours = ua.DailyOperatingHours,
                RoomLocation = ua.RoomLocation,
                SequenceNumber = ua.SequenceNumber
            });
        }

        var result = new PersonalizationSavedAnswersDto
        {
            Status = statusResult.Data!,
            Household = household == null ? null : new HouseholdProfileDto
            {
                HouseholdSize = household.HouseholdSize,
                HomeType = household.HomeType,
                HomeArea = household.HomeArea,
                NumberOfRooms = household.NumberOfRooms,
                DaytimePresence = household.DaytimePresence,
                IsSkipped = household.IsSkipped
            },
            Lifestyle = lifestyle == null ? null : new LifestyleProfileDto
            {
                DailyRoutine = lifestyle.DailyRoutine,
                HomeTimeSlots = string.IsNullOrWhiteSpace(lifestyle.HomeTimeSlotsCsv) ? new List<string>() : lifestyle.HomeTimeSlotsCsv.Split(',').ToList(),
                WakeUpTime = lifestyle.WakeUpTime,
                BedTime = lifestyle.BedTime,
                WeekendRoutine = lifestyle.WeekendRoutine,
                IsSkipped = lifestyle.IsSkipped
            },
            Appliances = applianceDtos,
            Meter = meter == null ? null : new MeterSetupDto
            {
                MeterType = meter.MeterType,
                BillPaymentDay = meter.BillPaymentDay,
                ConsumptionStartDate = meter.ConsumptionStartDate,
                MonthlyBillAmount = meter.MonthlyBillAmount,
                MonthlyCardBudget = meter.MonthlyCardBudget,
                IsSkipped = meter.IsSkipped
            },
            SavingTarget = savingTarget == null ? null : new SavingTargetDto
            {
                SavingPercentage = savingTarget.SavingPercentage,
                IsSkipped = savingTarget.IsSkipped
            }
        };

        return Result<PersonalizationSavedAnswersDto>.Success(result);
    }

    public async Task<Result<PersonalizationStatusDto>> ResumeOnboardingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetStatusAsync(userId, cancellationToken);
    }

    public async Task<Result<PersonalizationStatusDto>> SkipStageAsync(Guid userId, PersonalizationStage stage, CancellationToken cancellationToken = default)
    {
        var progressRepo = _unitOfWork.Repository<PersonalizationProgress>();
        var progress = await progressRepo.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        var nextStage = stage < PersonalizationStage.Completed ? (PersonalizationStage)((int)stage + 1) : PersonalizationStage.Completed;

        if (progress == null)
        {
            progress = new PersonalizationProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CurrentStage = (int)nextStage,
                StageStatus = nextStage == PersonalizationStage.Completed ? (int)StageStatus.Completed : (int)StageStatus.NotStarted,
                CompletedStagesCsv = string.Empty,
                SkippedStagesCsv = ((int)stage).ToString(),
                IsCompleted = nextStage == PersonalizationStage.Completed,
                LastUpdatedAtUtc = now
            };
            await progressRepo.AddAsync(progress, cancellationToken);
        }
        else
        {
            var skippedList = ParseCsv(progress.SkippedStagesCsv);
            if (!skippedList.Contains((int)stage))
            {
                skippedList.Add((int)stage);
                progress.SkippedStagesCsv = string.Join(",", skippedList);
            }

            if (stage < PersonalizationStage.Completed)
            {
                progress.CurrentStage = (int)stage + 1;
                progress.StageStatus = (int)StageStatus.NotStarted;
            }
            else
            {
                progress.StageStatus = (int)StageStatus.Completed;
                progress.IsCompleted = true;
            }

            progress.LastUpdatedAtUtc = now;
            progressRepo.Update(progress);
        }

        switch (stage)
        {
            case PersonalizationStage.Household:
                var hRepo = _unitOfWork.Repository<HouseholdProfile>();
                var hProfile = await hRepo.FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
                if (hProfile == null)
                {
                    await hRepo.AddAsync(new HouseholdProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        IsSkipped = true,
                        CreatedAtUtc = now,
                        LastUpdatedAtUtc = now
                    }, cancellationToken);
                }
                else
                {
                    hProfile.IsSkipped = true;
                    hProfile.LastUpdatedAtUtc = now;
                    hRepo.Update(hProfile);
                }
                break;

            case PersonalizationStage.Lifestyle:
                var lRepo = _unitOfWork.Repository<LifestyleProfile>();
                var lProfile = await lRepo.FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);
                if (lProfile == null)
                {
                    await lRepo.AddAsync(new LifestyleProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        IsSkipped = true,
                        CreatedAtUtc = now,
                        LastUpdatedAtUtc = now
                    }, cancellationToken);
                }
                else
                {
                    lProfile.IsSkipped = true;
                    lProfile.LastUpdatedAtUtc = now;
                    lRepo.Update(lProfile);
                }
                break;

            case PersonalizationStage.MeterAndTarget:
                var mRepo = _unitOfWork.Repository<MeterSetup>();
                var mSetup = await mRepo.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
                if (mSetup == null)
                {
                    await mRepo.AddAsync(new MeterSetup
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        MeterType = MeterType.Regular,
                        BillPaymentDay = 1,
                        ConsumptionStartDate = now.Date,
                        IsSkipped = true,
                        CreatedAtUtc = now,
                        LastUpdatedAtUtc = now
                    }, cancellationToken);
                }
                else
                {
                    mSetup.IsSkipped = true;
                    mSetup.LastUpdatedAtUtc = now;
                    mRepo.Update(mSetup);
                }

                var sRepo = _unitOfWork.Repository<SavingTarget>();
                var sTarget = await sRepo.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
                if (sTarget == null)
                {
                    await sRepo.AddAsync(new SavingTarget
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        SavingPercentage = null,
                        IsSkipped = true,
                        ConfiguredAtUtc = now,
                        LastUpdatedAtUtc = now
                    }, cancellationToken);
                }
                else
                {
                    sTarget.IsSkipped = true;
                    sTarget.LastUpdatedAtUtc = now;
                    sRepo.Update(sTarget);
                }
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PersonalizationStatusDto>.Success(MapToStatusDto(progress), 200, "تم تخطي المرحلة بنجاح.");
    }

    public async Task<Result<PersonalizationStatusDto>> CompletePersonalizationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var progressRepo = _unitOfWork.Repository<PersonalizationProgress>();
        var progress = await progressRepo.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        if (progress == null)
        {
            progress = new PersonalizationProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CurrentStage = (int)PersonalizationStage.Completed,
                StageStatus = (int)StageStatus.Completed,
                IsCompleted = true,
                CompletedAtUtc = now,
                LastUpdatedAtUtc = now
            };
            await progressRepo.AddAsync(progress, cancellationToken);
        }
        else
        {
            progress.CurrentStage = (int)PersonalizationStage.Completed;
            progress.StageStatus = (int)StageStatus.Completed;
            progress.IsCompleted = true;
            progress.CompletedAtUtc = now;
            progress.LastUpdatedAtUtc = now;
            progressRepo.Update(progress);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PersonalizationStatusDto>.Success(MapToStatusDto(progress), 200, "تم إكمال التخصيص وإعداد الحساب بنجاح.");
    }

    private static PersonalizationStatusDto MapToStatusDto(PersonalizationProgress progress)
    {
        return new PersonalizationStatusDto
        {
            CurrentStage = (PersonalizationStage)progress.CurrentStage,
            CurrentStageStatus = (StageStatus)progress.StageStatus,
            CompletedStages = ParseCsv(progress.CompletedStagesCsv).Select(s => (PersonalizationStage)s).ToList(),
            SkippedStages = ParseCsv(progress.SkippedStagesCsv).Select(s => (PersonalizationStage)s).ToList(),
            IsCompleted = progress.IsCompleted,
            CompletedAtUtc = progress.CompletedAtUtc,
            LastUpdatedAtUtc = progress.LastUpdatedAtUtc
        };
    }

    private static List<int> ParseCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new List<int>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                  .Where(v => v.HasValue)
                  .Select(v => v!.Value)
                  .ToList();
    }
}
