using System;
using System.Collections.Generic;
using System.Linq;
using SmartCourt.Features.Case.BusinessRules;

namespace SmartCourt.Features.Matching;

public static class MatchingEngine
{
    public static List<ScoredLawyerCandidate> RankCandidates(
        IEnumerable<LawyerCandidate> candidates,
        string? caseGovernorate,
        MatchingStrategy strategy)
    {
        var pool = candidates.ToList();
        if (pool.Count == 0) return [];

        if (pool.Count == 1)
        {
            var single = pool[0];
            var locScore = GovernorateRegions.CalculateLocationScore(caseGovernorate, single.Governorate);
            var ratScore = single.AverageRating <= 0 ? 0.5 : Math.Min(1.0, (double)single.AverageRating / 5.0);
            var expScore = 1.0;
            var respScore = single.AverageResponseTimeHours <= 0 ? 0.5 : 1.0;

            var total = (strategy.LocationWeight * locScore)
                      + (strategy.ExperienceWeight * expScore)
                      + (strategy.RatingWeight * ratScore)
                      + (strategy.ResponseTimeWeight * respScore);

            return [
                new ScoredLawyerCandidate
                {
                    Candidate = single,
                    TotalScore = Math.Round(total, 4),
                    LocationScore = Math.Round(locScore, 4),
                    ExperienceScore = Math.Round(expScore, 4),
                    RatingScore = Math.Round(ratScore, 4),
                    ResponseTimeScore = Math.Round(respScore, 4),
                    Rank = 1
                }
            ];
        }

        // Min-Max bounds for Experience Years
        var minYears = pool.Min(c => c.SpecializationYearsOfExperience);
        var maxYears = pool.Max(c => c.SpecializationYearsOfExperience);

        // Min-Max bounds for Experience Cases
        var minCases = pool.Min(c => c.SpecializationCasesHandled);
        var maxCases = pool.Max(c => c.SpecializationCasesHandled);

        // Min-Max bounds for Response Time
        var validRespTimes = pool.Where(c => c.AverageResponseTimeHours > 0)
            .Select(c => (double)c.AverageResponseTimeHours)
            .ToList();

        double minResp = validRespTimes.Count > 0 ? validRespTimes.Min() : 0;
        double maxResp = validRespTimes.Count > 0 ? validRespTimes.Max() : 0;

        var scored = new List<ScoredLawyerCandidate>();

        foreach (var c in pool)
        {
            // Location Score
            var locScore = GovernorateRegions.CalculateLocationScore(caseGovernorate, c.Governorate);

            // Experience Score
            double normYears = maxYears == minYears ? 1.0 : (double)(c.SpecializationYearsOfExperience - minYears) / (maxYears - minYears);
            double normCases = maxCases == minCases ? 1.0 : (double)(c.SpecializationCasesHandled - minCases) / (maxCases - minCases);
            double expScore = 0.85 * normYears + 0.15 * normCases;

            // Rating Score
            double ratScore = c.AverageRating <= 0 ? 0.5 : Math.Min(1.0, (double)c.AverageRating / 5.0);

            // Response Time Score (Inverted: lower response time is better)
            double respScore;
            if (c.AverageResponseTimeHours <= 0)
            {
                respScore = 0.5;
            }
            else if (maxResp == minResp)
            {
                respScore = 1.0;
            }
            else
            {
                respScore = 1.0 - (((double)c.AverageResponseTimeHours - minResp) / (maxResp - minResp));
            }

            var total = (strategy.LocationWeight * locScore)
                      + (strategy.ExperienceWeight * expScore)
                      + (strategy.RatingWeight * ratScore)
                      + (strategy.ResponseTimeWeight * respScore);

            scored.Add(new ScoredLawyerCandidate
            {
                Candidate = c,
                TotalScore = Math.Round(total, 4),
                LocationScore = Math.Round(locScore, 4),
                ExperienceScore = Math.Round(expScore, 4),
                RatingScore = Math.Round(ratScore, 4),
                ResponseTimeScore = Math.Round(respScore, 4)
            });
        }

        var sorted = scored.OrderByDescending(s => s.TotalScore).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Rank = i + 1;
        }

        return sorted;
    }
}
