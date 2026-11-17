using System;
using System.Collections.Generic;
using PowerSave.Domain.Enums;

namespace PowerSave.Domain.Entities;

public class ApplianceCategory
{
    public Guid Id { get; set; }
    public ApplianceCategoryType CategoryType { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<ApplianceCatalog> Appliances { get; set; } = new List<ApplianceCatalog>();
}
