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

public class ApplianceModelService : IApplianceModelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplianceModelService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<ApplianceCatalogResponseDto>>> GetModelSensitiveAppliancesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<ApplianceCatalog>()
            .FindAsync(a => a.IsModelSensitive, cancellationToken);

        var result = items.Select(a => new ApplianceCatalogResponseDto
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

    public async Task<Result<IReadOnlyList<ApplianceModelResponseDto>>> GetModelsForApplianceAsync(Guid applianceCatalogId, CancellationToken cancellationToken = default)
    {
        var models = await _unitOfWork.Repository<ApplianceModel>()
            .FindAsync(m => m.ApplianceCatalogId == applianceCatalogId, cancellationToken);

        var result = models.Select(m => new ApplianceModelResponseDto
        {
            Id = m.Id,
            ApplianceCatalogId = m.ApplianceCatalogId,
            Brand = m.Brand,
            ModelNameOrNumber = m.ModelNameOrNumber,
            CapacityOrSize = m.CapacityOrSize,
            EnergyRating = m.EnergyRating,
            Wattage = m.Wattage
        }).ToList();

        return Result<IReadOnlyList<ApplianceModelResponseDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ApplianceModelResponseDto>>> SearchModelsAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result<IReadOnlyList<ApplianceModelResponseDto>>.Success(new List<ApplianceModelResponseDto>());
        }

        string q = query.Trim().ToLower();
        var models = await _unitOfWork.Repository<ApplianceModel>()
            .FindAsync(m => m.Brand.ToLower().Contains(q) || m.ModelNameOrNumber.ToLower().Contains(q), cancellationToken);

        var result = models.Select(m => new ApplianceModelResponseDto
        {
            Id = m.Id,
            ApplianceCatalogId = m.ApplianceCatalogId,
            Brand = m.Brand,
            ModelNameOrNumber = m.ModelNameOrNumber,
            CapacityOrSize = m.CapacityOrSize,
            EnergyRating = m.EnergyRating,
            Wattage = m.Wattage
        }).ToList();

        return Result<IReadOnlyList<ApplianceModelResponseDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<UserApplianceResponseDto>>> SaveModelSelectionsAsync(
        Guid userId,
        SaveApplianceModelsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var userApplianceRepo = _unitOfWork.Repository<UserAppliance>();
        var modelRepo = _unitOfWork.Repository<ApplianceModel>();
        var catalogRepo = _unitOfWork.Repository<ApplianceCatalog>();

        var userAppliances = await userApplianceRepo.FindAsync(u => u.UserId == userId, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        foreach (var selection in request.ModelSelections)
        {
            var target = userAppliances.FirstOrDefault(u => u.Id == selection.UserApplianceId);
            if (target == null)
            {
                continue;
            }

            if (selection.IsModelSkipped)
            {
                target.IsModelSkipped = true;
                target.IsModelUnknown = false;
                target.ApplianceModelId = null;
                target.IsWattageEstimated = true;
            }
            else if (selection.IsModelUnknown)
            {
                target.IsModelUnknown = true;
                target.IsModelSkipped = false;
                target.ApplianceModelId = null;
                target.IsWattageEstimated = true;

                if (target.ApplianceCatalogId.HasValue)
                {
                    var catalog = await catalogRepo.GetByIdAsync(target.ApplianceCatalogId.Value, cancellationToken);
                    if (catalog != null)
                    {
                        target.Wattage = catalog.DefaultWattage;
                    }
                }
            }
            else if (selection.ApplianceModelId.HasValue)
            {
                var model = await modelRepo.GetByIdAsync(selection.ApplianceModelId.Value, cancellationToken);
                if (model == null)
                {
                    return Result<IReadOnlyList<UserApplianceResponseDto>>.Failure(AuthErrorCode.ApplianceModelNotFound, 404);
                }

                target.ApplianceModelId = model.Id;
                target.Wattage = model.Wattage;
                target.IsWattageEstimated = false;
                target.IsModelUnknown = false;
                target.IsModelSkipped = false;
            }
            else if (selection.CustomWattage.HasValue && selection.CustomWattage > 0)
            {
                target.Wattage = selection.CustomWattage.Value;
                target.IsWattageEstimated = false;
            }

            target.LastUpdatedAtUtc = now;
            userApplianceRepo.Update(target);
        }

        await MarkStageCompletedAsync(userId, PersonalizationStage.ApplianceModels, PersonalizationStage.MeterAndTarget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        return Result<IReadOnlyList<UserApplianceResponseDto>>.Success(list, 200, "تم حفظ موديلات الأجهزة بنجاح.");
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
