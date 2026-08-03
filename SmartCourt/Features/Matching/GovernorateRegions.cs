using System;
using System.Collections.Generic;

namespace SmartCourt.Features.Matching;

public enum EgyptianRegion
{
    GreaterCairo,
    AlexandriaRegion,
    Delta,
    CanalZone,
    UpperEgyptNorth,
    UpperEgyptSouth,
    RedSeaSinaiNewValley
}

public static class GovernorateRegions
{
    private static readonly Dictionary<string, EgyptianRegion> RegionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Greater Cairo
        ["Cairo"] = EgyptianRegion.GreaterCairo, ["القاهرة"] = EgyptianRegion.GreaterCairo,
        ["Giza"] = EgyptianRegion.GreaterCairo, ["الجيزة"] = EgyptianRegion.GreaterCairo,
        ["Qalyubia"] = EgyptianRegion.GreaterCairo, ["القليوبية"] = EgyptianRegion.GreaterCairo, ["Qalyubiyya"] = EgyptianRegion.GreaterCairo,

        // Alexandria Region
        ["Alexandria"] = EgyptianRegion.AlexandriaRegion, ["الإسكندرية"] = EgyptianRegion.AlexandriaRegion, ["الاسكندرية"] = EgyptianRegion.AlexandriaRegion,
        ["Beheira"] = EgyptianRegion.AlexandriaRegion, ["البحيرة"] = EgyptianRegion.AlexandriaRegion,
        ["Matrouh"] = EgyptianRegion.AlexandriaRegion, ["مطروح"] = EgyptianRegion.AlexandriaRegion, ["Matruh"] = EgyptianRegion.AlexandriaRegion,

        // Delta
        ["Gharbia"] = EgyptianRegion.Delta, ["الغربية"] = EgyptianRegion.Delta,
        ["Monufia"] = EgyptianRegion.Delta, ["المنوفية"] = EgyptianRegion.Delta, ["Menofia"] = EgyptianRegion.Delta,
        ["Dakahlia"] = EgyptianRegion.Delta, ["الدقهلية"] = EgyptianRegion.Delta,
        ["Kafr El Sheikh"] = EgyptianRegion.Delta, ["كفر الشيخ"] = EgyptianRegion.Delta, ["Kafr El-Sheikh"] = EgyptianRegion.Delta,
        ["Damietta"] = EgyptianRegion.Delta, ["دمياط"] = EgyptianRegion.Delta,

        // Canal Zone
        ["Port Said"] = EgyptianRegion.CanalZone, ["بورسعيد"] = EgyptianRegion.CanalZone,
        ["Ismailia"] = EgyptianRegion.CanalZone, ["الإسماعيلية"] = EgyptianRegion.CanalZone, ["الاسماعيلية"] = EgyptianRegion.CanalZone,
        ["Suez"] = EgyptianRegion.CanalZone, ["السويس"] = EgyptianRegion.CanalZone,
        ["North Sinai"] = EgyptianRegion.CanalZone, ["شمال سيناء"] = EgyptianRegion.CanalZone,

        // Upper Egypt North
        ["Beni Suef"] = EgyptianRegion.UpperEgyptNorth, ["بني سويف"] = EgyptianRegion.UpperEgyptNorth,
        ["Fayoum"] = EgyptianRegion.UpperEgyptNorth, ["الفيوم"] = EgyptianRegion.UpperEgyptNorth, ["Faiyum"] = EgyptianRegion.UpperEgyptNorth,
        ["Minya"] = EgyptianRegion.UpperEgyptNorth, ["المنيا"] = EgyptianRegion.UpperEgyptNorth,

        // Upper Egypt South
        ["Asyut"] = EgyptianRegion.UpperEgyptSouth, ["أسيوط"] = EgyptianRegion.UpperEgyptSouth, ["اسيوط"] = EgyptianRegion.UpperEgyptSouth,
        ["Sohag"] = EgyptianRegion.UpperEgyptSouth, ["سوهاج"] = EgyptianRegion.UpperEgyptSouth,
        ["Qena"] = EgyptianRegion.UpperEgyptSouth, ["قنا"] = EgyptianRegion.UpperEgyptSouth,
        ["Luxor"] = EgyptianRegion.UpperEgyptSouth, ["الأقصر"] = EgyptianRegion.UpperEgyptSouth, ["الاقصر"] = EgyptianRegion.UpperEgyptSouth,
        ["Aswan"] = EgyptianRegion.UpperEgyptSouth, ["أسوان"] = EgyptianRegion.UpperEgyptSouth, ["اسوان"] = EgyptianRegion.UpperEgyptSouth,

        // Red Sea / Sinai / New Valley
        ["Red Sea"] = EgyptianRegion.RedSeaSinaiNewValley, ["البحر الأحمر"] = EgyptianRegion.RedSeaSinaiNewValley, ["البحر الاحمر"] = EgyptianRegion.RedSeaSinaiNewValley,
        ["South Sinai"] = EgyptianRegion.RedSeaSinaiNewValley, ["جنوب سيناء"] = EgyptianRegion.RedSeaSinaiNewValley,
        ["New Valley"] = EgyptianRegion.RedSeaSinaiNewValley, ["الوادي الجديد"] = EgyptianRegion.RedSeaSinaiNewValley
    };

    public static EgyptianRegion? GetRegion(string? governorate)
    {
        if (string.IsNullOrWhiteSpace(governorate)) return null;
        return RegionMap.TryGetValue(governorate.Trim(), out var region) ? region : null;
    }

    public static double CalculateLocationScore(string? caseGov, string? lawyerGov)
    {
        if (string.IsNullOrWhiteSpace(caseGov) || string.IsNullOrWhiteSpace(lawyerGov)) return 0.0;

        var cGov = caseGov.Trim();
        var lGov = lawyerGov.Trim();

        if (string.Equals(cGov, lGov, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        var cRegion = GetRegion(cGov);
        var lRegion = GetRegion(lGov);

        if (cRegion.HasValue && lRegion.HasValue && cRegion.Value == lRegion.Value)
            return 0.5;

        return 0.0;
    }
}
