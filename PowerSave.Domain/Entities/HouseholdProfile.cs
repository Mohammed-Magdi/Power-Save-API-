using System;
using PowerSave.Domain.Enums;

namespace PowerSave.Domain.Entities;

public class HouseholdProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public HouseholdSize? HouseholdSize { get; set; }
    public HomeType? HomeType { get; set; }
    public HomeArea? HomeArea { get; set; }
    public int? NumberOfRooms { get; set; }
    public DaytimePresence? DaytimePresence { get; set; }

    public bool IsSkipped { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
