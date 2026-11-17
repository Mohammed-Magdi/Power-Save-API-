using System;

namespace PowerSave.Domain.Entities;

public class PersonalizationProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public int CurrentStage { get; set; }
    public int StageStatus { get; set; }

    public string CompletedStagesCsv { get; set; } = string.Empty;
    public string SkippedStagesCsv { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
