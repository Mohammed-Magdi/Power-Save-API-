using System;
using PowerSave.Application.Interfaces;
using PowerSave.Domain.Enums;

namespace PowerSave.Application.Services;

public class OperatingHoursCalculator : IOperatingHoursCalculator
{
    public string Version => "1.0.0-egypt-standard";

    public double CalculateSuggestedHours(
        HouseholdSize? householdSize,
        DaytimePresence? daytimePresence,
        TimeSpan? wakeUpTime,
        TimeSpan? bedTime,
        double catalogDefaultDailyHours,
        ApplianceCategoryType? categoryType,
        bool isModelSensitive)
    {
        if (catalogDefaultDailyHours <= 0)
        {
            return 1.0;
        }

        if (catalogDefaultDailyHours >= 24.0)
        {
            return 24.0;
        }

        double factor = 1.0;

        if (householdSize.HasValue)
        {
            factor += householdSize.Value switch
            {
                HouseholdSize.One => -0.15,
                HouseholdSize.Two => -0.05,
                HouseholdSize.ThreeToFour => 0.05,
                HouseholdSize.FiveToSix => 0.15,
                HouseholdSize.SevenOrMore => 0.25,
                _ => 0.0
            };
        }

        if (daytimePresence.HasValue)
        {
            factor += daytimePresence.Value switch
            {
                DaytimePresence.Nobody => -0.20,
                DaytimePresence.One => -0.05,
                DaytimePresence.TwoToThree => 0.05,
                DaytimePresence.Most => 0.20,
                DaytimePresence.Varies => 0.0,
                _ => 0.0
            };
        }

        if (wakeUpTime.HasValue && bedTime.HasValue)
        {
            double awakeHours;
            if (bedTime.Value >= wakeUpTime.Value)
            {
                awakeHours = (bedTime.Value - wakeUpTime.Value).TotalHours;
            }
            else
            {
                awakeHours = (TimeSpan.FromHours(24) - wakeUpTime.Value + bedTime.Value).TotalHours;
            }

            if (awakeHours > 16)
            {
                factor += 0.10;
            }
            else if (awakeHours < 12)
            {
                factor -= 0.10;
            }
        }

        double calculated = catalogDefaultDailyHours * Math.Max(0.5, Math.Min(factor, 1.8));

        if (calculated > 24.0)
        {
            calculated = 24.0;
        }

        return Math.Round(calculated, 1);
    }
}
