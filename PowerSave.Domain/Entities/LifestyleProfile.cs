using System;

namespace PowerSave.Domain.Entities;

public class LifestyleProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string? DailyRoutine { get; set; }
    public string? HomeTimeSlotsCsv { get; set; }

    public TimeSpan? WakeUpTime { get; set; }
    public TimeSpan? BedTime { get; set; }

    public string? WeekendRoutine { get; set; }

    public bool IsSkipped { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
