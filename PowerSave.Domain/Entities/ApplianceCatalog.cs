using System;
using System.Collections.Generic;

namespace PowerSave.Domain.Entities;

public class ApplianceCatalog
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    public bool IsModelSensitive { get; set; }
    public double DefaultWattage { get; set; }
    public double DefaultDailyHours { get; set; }
    public int MaxAllowedQuantity { get; set; } = 20;

    public ApplianceCategory Category { get; set; } = null!;
    public ICollection<ApplianceModel> Models { get; set; } = new List<ApplianceModel>();
    public ICollection<UserAppliance> UserAppliances { get; set; } = new List<UserAppliance>();
}
