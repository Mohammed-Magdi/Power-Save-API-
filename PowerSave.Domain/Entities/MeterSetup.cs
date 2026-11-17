using System;
using PowerSave.Domain.Enums;

namespace PowerSave.Domain.Entities;

public class MeterSetup
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public MeterType MeterType { get; set; }
    public int BillPaymentDay { get; set; } = 1;
    public DateTime ConsumptionStartDate { get; set; }

    public decimal? MonthlyBillAmount { get; set; }
    public decimal? MonthlyCardBudget { get; set; }

    public bool IsSkipped { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
