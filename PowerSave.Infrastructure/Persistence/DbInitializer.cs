using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PowerSave.Domain.Entities;
using PowerSave.Domain.Enums;

namespace PowerSave.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedPersonalizationCatalogAsync(PowerSaveDbContext context)
    {
        if (await context.ApplianceCategories.AnyAsync())
        {
            return;
        }

        var coolingCategory = new ApplianceCategory
        {
            Id = Guid.NewGuid(),
            CategoryType = ApplianceCategoryType.CoolingAndHeating,
            NameAr = "التبريد والتدفئة",
            NameEn = "Cooling & Heating",
            DisplayOrder = 1
        };

        var kitchenCategory = new ApplianceCategory
        {
            Id = Guid.NewGuid(),
            CategoryType = ApplianceCategoryType.Kitchen,
            NameAr = "المطبخ والأجهزة المنزلية",
            NameEn = "Kitchen Appliances",
            DisplayOrder = 2
        };

        var laundryCategory = new ApplianceCategory
        {
            Id = Guid.NewGuid(),
            CategoryType = ApplianceCategoryType.LaundryAndCleaning,
            NameAr = "الغسيل والتنظيف",
            NameEn = "Laundry & Cleaning",
            DisplayOrder = 3
        };

        var entertainmentCategory = new ApplianceCategory
        {
            Id = Guid.NewGuid(),
            CategoryType = ApplianceCategoryType.EntertainmentAndScreens,
            NameAr = "الترفيه والشاشات",
            NameEn = "Entertainment & Screens",
            DisplayOrder = 4
        };

        var waterCategory = new ApplianceCategory
        {
            Id = Guid.NewGuid(),
            CategoryType = ApplianceCategoryType.WaterAndHeating,
            NameAr = "تسخين المياه والمضخات",
            NameEn = "Water & Pumps",
            DisplayOrder = 5
        };

        var lightingCategory = new ApplianceCategory
        {
            Id = Guid.NewGuid(),
            CategoryType = ApplianceCategoryType.Lighting,
            NameAr = "الإضاءة",
            NameEn = "Lighting",
            DisplayOrder = 6
        };

        var otherCategory = new ApplianceCategory
        {
            Id = Guid.NewGuid(),
            CategoryType = ApplianceCategoryType.Other,
            NameAr = "أجهزة أخرى",
            NameEn = "Other Appliances",
            DisplayOrder = 7
        };

        context.ApplianceCategories.AddRange(
            coolingCategory, kitchenCategory, laundryCategory,
            entertainmentCategory, waterCategory, lightingCategory, otherCategory);

        var ac15 = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = coolingCategory.Id,
            NameAr = "تكييف 1.5 حصان",
            NameEn = "Air Conditioner 1.5 HP",
            IsModelSensitive = true,
            DefaultWattage = 1200,
            DefaultDailyHours = 8,
            MaxAllowedQuantity = 10
        };

        var ac225 = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = coolingCategory.Id,
            NameAr = "تكييف 2.25 حصان",
            NameEn = "Air Conditioner 2.25 HP",
            IsModelSensitive = true,
            DefaultWattage = 1800,
            DefaultDailyHours = 8,
            MaxAllowedQuantity = 10
        };

        var ac30 = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = coolingCategory.Id,
            NameAr = "تكييف 3 حصان",
            NameEn = "Air Conditioner 3 HP",
            IsModelSensitive = true,
            DefaultWattage = 2400,
            DefaultDailyHours = 8,
            MaxAllowedQuantity = 10
        };

        var fan = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = coolingCategory.Id,
            NameAr = "مروحة سقف / عمود",
            NameEn = "Ceiling / Stand Fan",
            IsModelSensitive = false,
            DefaultWattage = 75,
            DefaultDailyHours = 12,
            MaxAllowedQuantity = 15
        };

        var fridge = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = kitchenCategory.Id,
            NameAr = "ثلاجة",
            NameEn = "Refrigerator",
            IsModelSensitive = true,
            DefaultWattage = 200,
            DefaultDailyHours = 24,
            MaxAllowedQuantity = 5
        };

        var freezer = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = kitchenCategory.Id,
            NameAr = "ديب فريزر",
            NameEn = "Deep Freezer",
            IsModelSensitive = true,
            DefaultWattage = 250,
            DefaultDailyHours = 24,
            MaxAllowedQuantity = 5
        };

        var microwave = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = kitchenCategory.Id,
            NameAr = "ميكروويف",
            NameEn = "Microwave",
            IsModelSensitive = false,
            DefaultWattage = 1200,
            DefaultDailyHours = 0.5,
            MaxAllowedQuantity = 3
        };

        var kettle = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = kitchenCategory.Id,
            NameAr = "غلاية مياه (كاتل)",
            NameEn = "Electric Kettle",
            IsModelSensitive = false,
            DefaultWattage = 1800,
            DefaultDailyHours = 0.5,
            MaxAllowedQuantity = 3
        };

        var washingMachine = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = laundryCategory.Id,
            NameAr = "غسالة ملابس أوتوماتيك",
            NameEn = "Automatic Washing Machine",
            IsModelSensitive = true,
            DefaultWattage = 1000,
            DefaultDailyHours = 1.5,
            MaxAllowedQuantity = 3
        };

        var iron = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = laundryCategory.Id,
            NameAr = "مكواة ملابس",
            NameEn = "Clothes Iron",
            IsModelSensitive = false,
            DefaultWattage = 1500,
            DefaultDailyHours = 0.5,
            MaxAllowedQuantity = 3
        };

        var tv = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = entertainmentCategory.Id,
            NameAr = "شاشة تلفزيون (LED / Smart)",
            NameEn = "TV Screen (LED / Smart)",
            IsModelSensitive = true,
            DefaultWattage = 100,
            DefaultDailyHours = 6,
            MaxAllowedQuantity = 8
        };

        var waterHeater = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = waterCategory.Id,
            NameAr = "سخان مياه كهربائي",
            NameEn = "Electric Water Heater",
            IsModelSensitive = true,
            DefaultWattage = 1500,
            DefaultDailyHours = 4,
            MaxAllowedQuantity = 5
        };

        var ledLights = new ApplianceCatalog
        {
            Id = Guid.NewGuid(),
            CategoryId = lightingCategory.Id,
            NameAr = "لمبات ليد موفرة",
            NameEn = "LED Light Bulbs",
            IsModelSensitive = false,
            DefaultWattage = 12,
            DefaultDailyHours = 6,
            MaxAllowedQuantity = 50
        };

        context.ApplianceCatalogs.AddRange(
            ac15, ac225, ac30, fan, fridge, freezer, microwave, kettle,
            washingMachine, iron, tv, waterHeater, ledLights);

        var ac15Models = new List<ApplianceModel>
        {
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = ac15.Id, Brand = "Sharp", ModelNameOrNumber = "Inverter AH-XP12UHE", CapacityOrSize = "1.5 HP", EnergyRating = "A+++", Wattage = 950 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = ac15.Id, Brand = "Sharp", ModelNameOrNumber = "Standard AH-A12UCE", CapacityOrSize = "1.5 HP", EnergyRating = "B", Wattage = 1250 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = ac15.Id, Brand = "Carrier", ModelNameOrNumber = "Optimax Inverter", CapacityOrSize = "1.5 HP", EnergyRating = "A++", Wattage = 1000 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = ac15.Id, Brand = "Carrier", ModelNameOrNumber = "Classic 53QH", CapacityOrSize = "1.5 HP", EnergyRating = "C", Wattage = 1300 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = ac15.Id, Brand = "Tornado", ModelNameOrNumber = "TH-C12UEE", CapacityOrSize = "1.5 HP", EnergyRating = "B", Wattage = 1200 }
        };

        var ac225Models = new List<ApplianceModel>
        {
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = ac225.Id, Brand = "Sharp", ModelNameOrNumber = "Inverter AH-XP18UHE", CapacityOrSize = "2.25 HP", EnergyRating = "A+++", Wattage = 1500 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = ac225.Id, Brand = "Carrier", ModelNameOrNumber = "Optimax Inverter 2.25", CapacityOrSize = "2.25 HP", EnergyRating = "A++", Wattage = 1600 }
        };

        var fridgeModels = new List<ApplianceModel>
        {
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = fridge.Id, Brand = "Toshiba", ModelNameOrNumber = "GR-EF40P-H (16 Feet)", CapacityOrSize = "16 Feet", EnergyRating = "A", Wattage = 180 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = fridge.Id, Brand = "Sharp", ModelNameOrNumber = "SJ-GV58A (18 Feet Inverter)", CapacityOrSize = "18 Feet", EnergyRating = "A+", Wattage = 150 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = fridge.Id, Brand = "Kiriazi", ModelNameOrNumber = "E470 (16 Feet)", CapacityOrSize = "16 Feet", EnergyRating = "B", Wattage = 220 }
        };

        var tvModels = new List<ApplianceModel>
        {
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = tv.Id, Brand = "Samsung", ModelNameOrNumber = "Crystal UHD 50-inch", CapacityOrSize = "50 inch", EnergyRating = "A", Wattage = 85 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = tv.Id, Brand = "LG", ModelNameOrNumber = "OLED 55-inch C3", CapacityOrSize = "55 inch", EnergyRating = "A", Wattage = 95 },
            new ApplianceModel { Id = Guid.NewGuid(), ApplianceCatalogId = tv.Id, Brand = "Toshiba", ModelNameOrNumber = "Smart LED 43-inch", CapacityOrSize = "43 inch", EnergyRating = "A+", Wattage = 65 }
        };

        context.ApplianceModels.AddRange(ac15Models);
        context.ApplianceModels.AddRange(ac225Models);
        context.ApplianceModels.AddRange(fridgeModels);
        context.ApplianceModels.AddRange(tvModels);

        await context.SaveChangesAsync();
    }
}
