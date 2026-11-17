using System;

namespace PowerSave.Domain.Entities;

public class UserAppliance
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public Guid? ApplianceCatalogId { get; set; }
    public string? CustomName { get; set; }

    public Guid? ApplianceModelId { get; set; }
    public bool IsModelUnknown { get; set; }
    public bool IsModelSkipped { get; set; }

    public double Wattage { get; set; }
    public bool IsWattageEstimated { get; set; }
    public double DailyOperatingHours { get; set; }

    public string? RoomLocation { get; set; }
    public int SequenceNumber { get; set; } = 1;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ApplianceCatalog? ApplianceCatalog { get; set; }
    public ApplianceModel? ApplianceModel { get; set; }
}
