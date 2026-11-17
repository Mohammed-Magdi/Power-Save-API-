using System;
using System.Collections.Generic;

namespace PowerSave.Domain.Entities;

public class ApplianceModel
{
    public Guid Id { get; set; }
    public Guid ApplianceCatalogId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string ModelNameOrNumber { get; set; } = string.Empty;
    public string? CapacityOrSize { get; set; }
    public string? EnergyRating { get; set; }
    public double Wattage { get; set; }

    public ApplianceCatalog ApplianceCatalog { get; set; } = null!;
    public ICollection<UserAppliance> UserAppliances { get; set; } = new List<UserAppliance>();
}
