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

public class SavingTargetService : ISavingTargetService
{
    private static readonly int[] AllowedPercentages = { 5, 10, 15, 20 };
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SavingTargetService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SavingTargetDto>> SaveSavingTargetAsync(Guid userId, SavingTargetDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.SavingPercentage.HasValue && !AllowedPercentages.Contains(dto.SavingPercentage.Value))
        {
            return Result<SavingTargetDto>.Failure(AuthErrorCode.InvalidSavingPercentage, 400);
        }

        var repo = _unitOfWork.Repository<SavingTarget>();
        var existing = await repo.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        var now = _dateTimeProvider.UtcNow;

        if (existing == null)
        {
            existing = new SavingTarget
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SavingPercentage = dto.SavingPercentage,
                IsSkipped = dto.IsSkipped || !dto.SavingPercentage.HasValue,
                ConfiguredAtUtc = now,
                LastUpdatedAtUtc = now
            };
            await repo.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.SavingPercentage = dto.SavingPercentage;
            existing.IsSkipped = dto.IsSkipped || !dto.SavingPercentage.HasValue;
            existing.LastUpdatedAtUtc = now;
            repo.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SavingTargetDto>.Success(new SavingTargetDto
        {
            SavingPercentage = existing.SavingPercentage,
            IsSkipped = existing.IsSkipped
        }, 200, "تم حفظ هدف التوفير بنجاح (للإشعارات فقط).");
    }

    public async Task<Result<SavingTargetDto>> GetSavingTargetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<SavingTarget>();
        var existing = await repo.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (existing == null)
        {
            return Result<SavingTargetDto>.Success(new SavingTargetDto { IsSkipped = false });
        }

        return Result<SavingTargetDto>.Success(new SavingTargetDto
        {
            SavingPercentage = existing.SavingPercentage,
            IsSkipped = existing.IsSkipped
        });
    }
}
