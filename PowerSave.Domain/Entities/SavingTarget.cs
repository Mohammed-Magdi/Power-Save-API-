using System;

namespace PowerSave.Domain.Entities;

public class SavingTarget
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public int? SavingPercentage { get; set; }
    public bool IsSkipped { get; set; }

    public DateTime ConfiguredAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
