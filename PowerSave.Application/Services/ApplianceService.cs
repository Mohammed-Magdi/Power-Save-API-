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

public class ApplianceService : IApplianceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOperatingHoursCalculator _hoursCalculator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplianceService(
        IUnitOfWork unitOfWork,
        IOperatingHoursCalculator hoursCalculator,
        IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _hoursCalculator = hoursCalculator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<ApplianceCategoryResponseDto>>> GetCategoriesWithAppliancesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Repository<ApplianceCategory>().GetAllAsync(cancellationToken);
        var catalogs = await _unitOfWork.Repository<ApplianceCatalog>().GetAllAsync(cancellationToken);

        var result = categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new ApplianceCategoryResponseDto
            {
                Id = c.Id,
                CategoryType = c.CategoryType,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                IconUrl = c.IconUrl,
                DisplayOrder = c.DisplayOrder,
                Appliances = catalogs
                    .Where(a => a.CategoryId == c.Id)
                    .Select(a => new ApplianceCatalogResponseDto
                    {
                        Id = a.Id,
                        CategoryId = a.CategoryId,
                        NameAr = a.NameAr,
                        NameEn = a.NameEn,
                        Description = a.Description,
                        IconUrl = a.IconUrl,
                        IsModelSensitive = a.IsModelSensitive,
                        DefaultWattage = a.DefaultWattage,
                        DefaultDailyHours = a.DefaultDailyHours,
                        MaxAllowedQuantity = a.MaxAllowedQuantity
                    }).ToList()
            }).ToList();

        return Result<IReadOnlyList<ApplianceCategoryResponseDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ApplianceCatalogResponseDto>>> GetAppliancesByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var catalogs = await _unitOfWork.Repository<ApplianceCatalog>().FindAsync(a => a.CategoryId == categoryId, cancellationToken);
        var result = catalogs.Select(a => new ApplianceCatalogResponseDto
        {
            Id = a.Id,
            CategoryId = a.CategoryId,
            NameAr = a.NameAr,
            NameEn = a.NameEn,
            Description = a.Description,
            IconUrl = a.IconUrl,
            IsModelSensitive = a.IsModelSensitive,
            DefaultWattage = a.DefaultWattage,
            DefaultDailyHours = a.DefaultDailyHours,
            MaxAllowedQuantity = a.MaxAllowedQuantity
        }).ToList();

        return Result<IReadOnlyList<ApplianceCatalogResponseDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<UserApplianceResponseDto>>> GetUserAppliancesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userAppliances = await _unitOfWork.Repository<UserAppliance>().FindAsync(u => u.UserId == userId, cancellationToken);
        var catalogRepo = _unitOfWork.Repository<ApplianceCatalog>();
        var modelRepo = _unitOfWork.Repository<ApplianceModel>();

        var list = new List<UserApplianceResponseDto>();
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

            list.Add(new UserApplianceResponseDto
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

        return Result<IReadOnlyList<UserApplianceResponseDto>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<UserApplianceResponseDto>>> SaveApplianceSelectionsAsync(
        Guid userId,
        SaveAppliancesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Selections == null || request.Selections.Count == 0)
        {
            return Result<IReadOnlyList<UserApplianceResponseDto>>.Failure(AuthErrorCode.InvalidApplianceQuantity, 400, "يجب تحديد جهاز واحد على الأقل أو تخطي المرحلة.");
        }

        var household = await _unitOfWork.Repository<HouseholdProfile>().FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
        var lifestyle = await _unitOfWork.Repository<LifestyleProfile>().FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);

        var catalogRepo = _unitOfWork.Repository<ApplianceCatalog>();
        var categoryRepo = _unitOfWork.Repository<ApplianceCategory>();
        var userApplianceRepo = _unitOfWork.Repository<UserAppliance>();

        var existingUserAppliances = await userApplianceRepo.FindAsync(u => u.UserId == userId, cancellationToken);
        userApplianceRepo.RemoveRange(existingUserAppliances);

        var newEntities = new List<UserAppliance>();
        var now = _dateTimeProvider.UtcNow;
        int globalSequence = 1;

        foreach (var item in request.Selections)
        {
            if (item.Quantity <= 0 || item.Quantity > 50)
            {
                return Result<IReadOnlyList<UserApplianceResponseDto>>.Failure(AuthErrorCode.InvalidApplianceQuantity, 400);
            }

            ApplianceCatalog? catalog = null;
            ApplianceCategory? category = null;

            if (item.ApplianceCatalogId.HasValue)
            {
                catalog = await catalogRepo.GetByIdAsync(item.ApplianceCatalogId.Value, cancellationToken);
                if (catalog == null)
                {
                    return Result<IReadOnlyList<UserApplianceResponseDto>>.Failure(AuthErrorCode.ApplianceNotFound, 404);
                }
                category = await categoryRepo.GetByIdAsync(catalog.CategoryId, cancellationToken);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(item.CustomName))
                {
                    return Result<IReadOnlyList<UserApplianceResponseDto>>.Failure(AuthErrorCode.EmptyCustomApplianceName, 400);
                }
            }

            double wattage = item.CustomWattage ?? (catalog?.DefaultWattage ?? 100);
            if (wattage <= 0)
            {
                return Result<IReadOnlyList<UserApplianceResponseDto>>.Failure(AuthErrorCode.InvalidWattage, 400);
            }

            double defaultDailyHours = catalog?.DefaultDailyHours ?? 4.0;
            double suggestedHours = _hoursCalculator.CalculateSuggestedHours(
                household?.HouseholdSize,
                household?.DaytimePresence,
                lifestyle?.WakeUpTime,
                lifestyle?.BedTime,
                defaultDailyHours,
                category?.CategoryType,
                catalog?.IsModelSensitive ?? false);

            for (int i = 1; i <= item.Quantity; i++)
            {
                var record = new UserAppliance
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ApplianceCatalogId = item.ApplianceCatalogId,
                    CustomName = item.CustomName,
                    ApplianceModelId = null,
                    IsModelUnknown = false,
                    IsModelSkipped = false,
                    Wattage = wattage,
                    IsWattageEstimated = true,
                    DailyOperatingHours = suggestedHours,
                    RoomLocation = item.RoomLocation,
                    SequenceNumber = globalSequence++,
                    CreatedAtUtc = now,
                    LastUpdatedAtUtc = now
                };
                newEntities.Add(record);
            }
        }

        await userApplianceRepo.AddRangeAsync(newEntities, cancellationToken);

        await MarkStageCompletedAsync(userId, PersonalizationStage.Appliances, PersonalizationStage.ApplianceModels, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetUserAppliancesAsync(userId, cancellationToken);
    }

    public async Task<Result<UserApplianceResponseDto>> AddCustomApplianceAsync(
        Guid userId,
        ApplianceSelectionItemDto customAppliance,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customAppliance.CustomName))
        {
            return Result<UserApplianceResponseDto>.Failure(AuthErrorCode.EmptyCustomApplianceName, 400);
        }

        double wattage = customAppliance.CustomWattage ?? 150;
        if (wattage <= 0)
        {
            return Result<UserApplianceResponseDto>.Failure(AuthErrorCode.InvalidWattage, 400);
        }

        var userApplianceRepo = _unitOfWork.Repository<UserAppliance>();
        int count = await userApplianceRepo.CountAsync(u => u.UserId == userId, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        var entity = new UserAppliance
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ApplianceCatalogId = null,
            CustomName = customAppliance.CustomName.Trim(),
            ApplianceModelId = null,
            IsModelUnknown = false,
            IsModelSkipped = false,
            Wattage = wattage,
            IsWattageEstimated = true,
            DailyOperatingHours = 4.0,
            RoomLocation = customAppliance.RoomLocation,
            SequenceNumber = count + 1,
            CreatedAtUtc = now,
            LastUpdatedAtUtc = now
        };

        await userApplianceRepo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserApplianceResponseDto>.Success(new UserApplianceResponseDto
        {
            Id = entity.Id,
            ApplianceCatalogId = null,
            ApplianceName = entity.CustomName,
            CustomName = entity.CustomName,
            Wattage = entity.Wattage,
            IsWattageEstimated = entity.IsWattageEstimated,
            DailyOperatingHours = entity.DailyOperatingHours,
            RoomLocation = entity.RoomLocation,
            SequenceNumber = entity.SequenceNumber
        }, 201, "تم إضافة الجهاز المخصص بنجاح.");
    }

    public async Task<Result<bool>> RemoveUserApplianceAsync(Guid userId, Guid userApplianceId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<UserAppliance>();
        var item = await repo.FirstOrDefaultAsync(u => u.Id == userApplianceId && u.UserId == userId, cancellationToken);
        if (item == null)
        {
            return Result<bool>.Failure(AuthErrorCode.ApplianceNotFound, 404, "الجهاز غير موجود في قائمة أجهزتك.");
        }

        repo.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, 200, "تم حذف الجهاز بنجاح.");
    }

    public async Task<Result<bool>> RemoveAllUserAppliancesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<UserAppliance>();
        var items = await repo.FindAsync(u => u.UserId == userId, cancellationToken);
        repo.RemoveRange(items);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, 200, "تم مسح جميع الأجهزة بنجاح.");
    }

    private async Task MarkStageCompletedAsync(Guid userId, PersonalizationStage completedStage, PersonalizationStage nextStage, CancellationToken cancellationToken)
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
                CurrentStage = (int)nextStage,
                StageStatus = (int)StageStatus.InProgress,
                CompletedStagesCsv = ((int)completedStage).ToString(),
                SkippedStagesCsv = string.Empty,
                IsCompleted = false,
                LastUpdatedAtUtc = now
            };
            await progressRepo.AddAsync(progress, cancellationToken);
        }
        else
        {
            var completedList = ParseCsv(progress.CompletedStagesCsv);
            if (!completedList.Contains((int)completedStage))
            {
                completedList.Add((int)completedStage);
                progress.CompletedStagesCsv = string.Join(",", completedList);
            }

            var skippedList = ParseCsv(progress.SkippedStagesCsv);
            skippedList.Remove((int)completedStage);
            progress.SkippedStagesCsv = string.Join(",", skippedList);

            if (progress.CurrentStage <= (int)completedStage)
            {
                progress.CurrentStage = (int)nextStage;
            }

            progress.StageStatus = (int)StageStatus.InProgress;
            progress.LastUpdatedAtUtc = now;
            progressRepo.Update(progress);
        }
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
