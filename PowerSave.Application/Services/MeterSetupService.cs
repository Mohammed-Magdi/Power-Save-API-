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

public class MeterSetupService : IMeterSetupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MeterSetupService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<MeterSetupDto>> SaveMeterSetupAsync(Guid userId, MeterSetupDto dto, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;

        if (dto.BillPaymentDay < 1 || dto.BillPaymentDay > 31)
        {
            return Result<MeterSetupDto>.Failure(AuthErrorCode.InvalidPaymentDay, 400);
        }

        if (dto.ConsumptionStartDate.Date > now.Date)
        {
            return Result<MeterSetupDto>.Failure(AuthErrorCode.FutureConsumptionStartDate, 400);
        }

        if (dto.MonthlyBillAmount.HasValue && dto.MonthlyBillAmount.Value < 0)
        {
            return Result<MeterSetupDto>.Failure(AuthErrorCode.InvalidBillAmount, 400);
        }

        if (dto.MonthlyCardBudget.HasValue && dto.MonthlyCardBudget.Value < 0)
        {
            return Result<MeterSetupDto>.Failure(AuthErrorCode.InvalidCardBudget, 400);
        }

        var repo = _unitOfWork.Repository<MeterSetup>();
        var existing = await repo.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);

        if (existing == null)
        {
            existing = new MeterSetup
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MeterType = dto.MeterType,
                BillPaymentDay = dto.BillPaymentDay,
                ConsumptionStartDate = dto.ConsumptionStartDate == default ? now.Date : dto.ConsumptionStartDate.Date,
                MonthlyBillAmount = dto.MonthlyBillAmount,
                MonthlyCardBudget = dto.MonthlyCardBudget,
                IsSkipped = false,
                CreatedAtUtc = now,
                LastUpdatedAtUtc = now
            };
            await repo.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.MeterType = dto.MeterType;
            existing.BillPaymentDay = dto.BillPaymentDay;
            if (dto.ConsumptionStartDate != default)
            {
                existing.ConsumptionStartDate = dto.ConsumptionStartDate.Date;
            }
            existing.MonthlyBillAmount = dto.MonthlyBillAmount ?? existing.MonthlyBillAmount;
            existing.MonthlyCardBudget = dto.MonthlyCardBudget ?? existing.MonthlyCardBudget;
            existing.IsSkipped = false;
            existing.LastUpdatedAtUtc = now;
            repo.Update(existing);
        }

        await MarkStageCompletedAsync(userId, PersonalizationStage.MeterAndTarget, PersonalizationStage.Completed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MeterSetupDto>.Success(new MeterSetupDto
        {
            MeterType = existing.MeterType,
            BillPaymentDay = existing.BillPaymentDay,
            ConsumptionStartDate = existing.ConsumptionStartDate,
            MonthlyBillAmount = existing.MonthlyBillAmount,
            MonthlyCardBudget = existing.MonthlyCardBudget,
            IsSkipped = existing.IsSkipped
        }, 200, "تم حفظ إعدادات العداد بنجاح.");
    }

    public async Task<Result<MeterSetupDto>> GetMeterSetupAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<MeterSetup>();
        var existing = await repo.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
        if (existing == null)
        {
            return Result<MeterSetupDto>.Success(new MeterSetupDto
            {
                MeterType = MeterType.Regular,
                BillPaymentDay = 1,
                ConsumptionStartDate = _dateTimeProvider.UtcNow.Date,
                IsSkipped = false
            });
        }

        return Result<MeterSetupDto>.Success(new MeterSetupDto
        {
            MeterType = existing.MeterType,
            BillPaymentDay = existing.BillPaymentDay,
            ConsumptionStartDate = existing.ConsumptionStartDate,
            MonthlyBillAmount = existing.MonthlyBillAmount,
            MonthlyCardBudget = existing.MonthlyCardBudget,
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
