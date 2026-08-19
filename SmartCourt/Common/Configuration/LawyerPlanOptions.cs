using System.Collections.Generic;

namespace SmartCourt.Common.Configuration;

public sealed class LawyerPlanOptions
{
    public const string SectionName = "LawyerPlans";
    public List<LawyerPlanDefinition> Plans { get; set; } = [];
}

public sealed class LawyerPlanDefinition
{
    public string PlanType { get; set; } = string.Empty;
    public int DailyTokenLimit { get; set; }
    public int DailyCreditLimit { get; set; }
    public decimal MonthlyPriceEgp { get; set; }
}
