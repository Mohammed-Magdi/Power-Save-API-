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

public class HouseholdProfileService : IHouseholdProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public HouseholdProfileService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<HouseholdProfileDto>> SaveHouseholdProfileAsync(Guid userId, HouseholdProfileDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.NumberOfRooms.HasValue && (dto.NumberOfRooms.Value < 1 || dto.NumberOfRooms.Value > 20))
        {
            return Result<HouseholdProfileDto>.Failure(AuthErrorCode.InvalidNumberOfRooms, 400);
        }

        var repo = _unitOfWork.Repository<HouseholdProfile>();
        var existing = await repo.FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        if (existing == null)
        {
            existing = new HouseholdProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                HouseholdSize = dto.HouseholdSize,
                HomeType = dto.HomeType,
                HomeArea = dto.HomeArea,
                NumberOfRooms = dto.NumberOfRooms,
                DaytimePresence = dto.DaytimePresence,
                IsSkipped = false,
                CreatedAtUtc = now,
                LastUpdatedAtUtc = now
            };
            await repo.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.HouseholdSize = dto.HouseholdSize ?? existing.HouseholdSize;
            existing.HomeType = dto.HomeType ?? existing.HomeType;
            existing.HomeArea = dto.HomeArea ?? existing.HomeArea;
            existing.NumberOfRooms = dto.NumberOfRooms ?? existing.NumberOfRooms;
            existing.DaytimePresence = dto.DaytimePresence ?? existing.DaytimePresence;
            existing.IsSkipped = false;
            existing.LastUpdatedAtUtc = now;
            repo.Update(existing);
        }

        await MarkStageCompletedAsync(userId, PersonalizationStage.Household, PersonalizationStage.Lifestyle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<HouseholdProfileDto>.Success(new HouseholdProfileDto
        {
            HouseholdSize = existing.HouseholdSize,
            HomeType = existing.HomeType,
            HomeArea = existing.HomeArea,
            NumberOfRooms = existing.NumberOfRooms,
            DaytimePresence = existing.DaytimePresence,
            IsSkipped = existing.IsSkipped
        }, 200, "تم حفظ بيانات المنزل بنجاح.");
    }

    public async Task<Result<HouseholdProfileDto>> GetHouseholdProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<HouseholdProfile>();
        var existing = await repo.FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
        if (existing == null)
        {
            return Result<HouseholdProfileDto>.Success(new HouseholdProfileDto { IsSkipped = false });
        }

        return Result<HouseholdProfileDto>.Success(new HouseholdProfileDto
        {
            HouseholdSize = existing.HouseholdSize,
            HomeType = existing.HomeType,
            HomeArea = existing.HomeArea,
            NumberOfRooms = existing.NumberOfRooms,
            DaytimePresence = existing.DaytimePresence,
            IsSkipped = existing.IsSkipped
        });
    }

    public async Task<Result<LifestyleProfileDto>> SaveLifestyleProfileAsync(Guid userId, LifestyleProfileDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<LifestyleProfile>();
        var existing = await repo.FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        string? slotsCsv = dto.HomeTimeSlots != null && dto.HomeTimeSlots.Count > 0
            ? string.Join(",", dto.HomeTimeSlots.Distinct())
            : null;

        if (existing == null)
        {
            existing = new LifestyleProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DailyRoutine = dto.DailyRoutine,
                HomeTimeSlotsCsv = slotsCsv,
                WakeUpTime = dto.WakeUpTime,
                BedTime = dto.BedTime,
                WeekendRoutine = dto.WeekendRoutine,
                IsSkipped = false,
                CreatedAtUtc = now,
                LastUpdatedAtUtc = now
            };
            await repo.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.DailyRoutine = dto.DailyRoutine ?? existing.DailyRoutine;
            existing.HomeTimeSlotsCsv = slotsCsv ?? existing.HomeTimeSlotsCsv;
            existing.WakeUpTime = dto.WakeUpTime ?? existing.WakeUpTime;
            existing.BedTime = dto.BedTime ?? existing.BedTime;
            existing.WeekendRoutine = dto.WeekendRoutine ?? existing.WeekendRoutine;
            existing.IsSkipped = false;
            existing.LastUpdatedAtUtc = now;
            repo.Update(existing);
        }

        await MarkStageCompletedAsync(userId, PersonalizationStage.Lifestyle, PersonalizationStage.Appliances, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LifestyleProfileDto>.Success(new LifestyleProfileDto
        {
            DailyRoutine = existing.DailyRoutine,
            HomeTimeSlots = string.IsNullOrWhiteSpace(existing.HomeTimeSlotsCsv) ? new List<string>() : existing.HomeTimeSlotsCsv.Split(',').ToList(),
            WakeUpTime = existing.WakeUpTime,
            BedTime = existing.BedTime,
            WeekendRoutine = existing.WeekendRoutine,
            IsSkipped = existing.IsSkipped
        }, 200, "تم حفظ بيانات نمط الحياة بنجاح.");
    }

    public async Task<Result<LifestyleProfileDto>> GetLifestyleProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<LifestyleProfile>();
        var existing = await repo.FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);
        if (existing == null)
        {
            return Result<LifestyleProfileDto>.Success(new LifestyleProfileDto { IsSkipped = false, HomeTimeSlots = new List<string>() });
        }

        return Result<LifestyleProfileDto>.Success(new LifestyleProfileDto
        {
            DailyRoutine = existing.DailyRoutine,
            HomeTimeSlots = string.IsNullOrWhiteSpace(existing.HomeTimeSlotsCsv) ? new List<string>() : existing.HomeTimeSlotsCsv.Split(',').ToList(),
            WakeUpTime = existing.WakeUpTime,
            BedTime = existing.BedTime,
            WeekendRoutine = existing.WeekendRoutine,
            IsSkipped = existing.IsSkipped
        });
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
