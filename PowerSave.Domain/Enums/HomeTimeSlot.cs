using System;

namespace PowerSave.Domain.Enums;

[Flags]
public enum HomeTimeSlot
{
    None = 0,
    Morning = 1,
    Afternoon = 2,
    Evening = 4,
    Night = 8,
    MostOfTheDay = 16,
    Varies = 32
}
