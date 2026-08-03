using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Case.BusinessRules;
using SmartCourt.Features.Matching.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Features.Matching;

public class MatchingService(
    ApplicationDbContext dbContext,
    IChatModelProvider chatModelProvider,
    ILogger<MatchingService> logger) : IMatchingService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IChatModelProvider _chatModelProvider = chatModelProvider;
    private readonly ILogger<MatchingService> _logger = logger;

    public async Task<List<ScoredLawyerCandidate>> FindAndScoreMatchesAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var caseEntity = await _dbContext.Cases
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        var caseProfile = await _dbContext.CaseProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.CaseId == caseId, cancellationToken);

        if (caseProfile == null)
        {
            throw new NotFoundException("لم يتم تحفيز/تحليل ملف القضية بعد.");
        }

        // Layer 1: Deterministic Eligibility Filtering
        var eligibleLawyerProfiles = await _dbContext.LawyerProfiles
            .Include(lp => lp.User)
            .Include(lp => lp.Specializations)
            .AsNoTracking()
            .Where(lp => lp.User.Status == SmartCourt.Features.Auth.Enums.UserStatus.Active && lp.User.EmailConfirmed && lp.IsAvailable)
            .Where(lp => lp.Level >= caseProfile.RequiredLawyerLevelId)
            .Where(lp => lp.Specializations.Any(s => s.Specialization == caseProfile.Specialization))
            .ToListAsync(cancellationToken);

        if (eligibleLawyerProfiles.Count == 0)
        {
            return [];
        }

        var candidates = eligibleLawyerProfiles.Select(lp =>
        {
            var matchedSpec = lp.Specializations.First(s => s.Specialization == caseProfile.Specialization);
            return new LawyerCandidate
            {
                LawyerId = lp.UserId,
                LawyerName = lp.User.FullName,
                Governorate = lp.User.Governorate,
                Level = lp.Level,
                IsAvailable = lp.IsAvailable,
                AverageRating = lp.AverageRating,
                AverageResponseTimeHours = lp.AverageResponseTimeHours,
                SpecializationYearsOfExperience = matchedSpec.YearsOfExperience,
                SpecializationCasesHandled = matchedSpec.CasesHandled
            };
        }).ToList();

        // Layer 2: Qualification Weighted Scoring
        var strategy = MatchingStrategy.GetStrategy(caseProfile.Complexity);
        return MatchingEngine.RankCandidates(candidates, caseEntity.Governorate, strategy);
    }

    public async Task<FinalizeResultDto> ProcessMatchingAndPersistAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var caseEntity = await _dbContext.Cases
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        var caseProfile = await _dbContext.CaseProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.CaseId == caseId, cancellationToken);

        if (caseProfile == null)
        {
            throw new NotFoundException("لم يتم تحفيز/تحليل ملف القضية بعد.");
        }

        var scoredCandidates = await FindAndScoreMatchesAsync(caseId, cancellationToken);
        var strategy = MatchingStrategy.GetStrategy(caseProfile.Complexity);

        // Batch explanation generation
        var explanations = await GenerateExplanationsAsync(caseEntity, scoredCandidates, strategy, cancellationToken);

        // Remove old recommendations for this case if any exist
        var existingRecs = await _dbContext.CaseRecommendations
            .Where(cr => cr.CaseId == caseId)
            .ToListAsync(cancellationToken);

        if (existingRecs.Count > 0)
        {
            _dbContext.CaseRecommendations.RemoveRange(existingRecs);
        }

        var newRecommendations = new List<CaseRecommendation>();
        foreach (var c in scoredCandidates)
        {
            var exp = explanations.GetValueOrDefault(c.Candidate.LawyerId)
                ?? $"تم ترشيح المحامي {c.Candidate.LawyerName} بحصوله على الترتيب #{c.Rank} بنتيجة إجمالية {c.TotalScore}.";

            newRecommendations.Add(new CaseRecommendation
            {
                Id = Guid.NewGuid(),
                CaseId = caseId,
                LawyerId = c.Candidate.LawyerId,
                TotalScore = (decimal)c.TotalScore,
                LocationScore = (decimal)c.LocationScore,
                ExperienceScore = (decimal)c.ExperienceScore,
                RatingScore = (decimal)c.RatingScore,
                ResponseTimeScore = (decimal)c.ResponseTimeScore,
                Explanation = exp,
                Rank = c.Rank
            });
        }

        if (newRecommendations.Count > 0)
        {
            _dbContext.CaseRecommendations.AddRange(newRecommendations);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FinalizeResultDto
        {
            CaseId = caseId,
            TotalEligibleLawyers = scoredCandidates.Count,
            Recommendations = scoredCandidates.Select(c => new CaseRecommendationDto
            {
                LawyerId = c.Candidate.LawyerId,
                LawyerName = c.Candidate.LawyerName,
                TotalScore = c.TotalScore,
                LocationScore = c.LocationScore,
                ExperienceScore = c.ExperienceScore,
                RatingScore = c.RatingScore,
                ResponseTimeScore = c.ResponseTimeScore,
                Explanation = explanations.GetValueOrDefault(c.Candidate.LawyerId) ?? string.Empty,
                Rank = c.Rank
            }).ToList()
        };
    }

    public async Task<FinalizeResultDto> GetRecommendationsAsync(Guid caseId, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var caseEntity = await _dbContext.Cases
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        if (caseEntity.ClientId != currentUserId)
        {
            throw new ForbiddenAccessException("ليس لديك صلاحية لعرض توصيات هذه القضية.");
        }

        if (caseEntity.Status != CaseStatus.Matched)
        {
            throw new BusinessException("Recommendations are not available. The case has not been matched yet.");
        }

        var recs = await _dbContext.CaseRecommendations
            .Include(cr => cr.LawyerProfile)
            .ThenInclude(lp => lp.User)
            .AsNoTracking()
            .Where(cr => cr.CaseId == caseId)
            .OrderBy(cr => cr.Rank)
            .ToListAsync(cancellationToken);

        return new FinalizeResultDto
        {
            CaseId = caseId,
            TotalEligibleLawyers = recs.Count,
            Recommendations = recs.Select(r => new CaseRecommendationDto
            {
                LawyerId = r.LawyerId,
                LawyerName = r.LawyerProfile?.User?.FullName ?? string.Empty,
                TotalScore = (double)r.TotalScore,
                LocationScore = (double)r.LocationScore,
                ExperienceScore = (double)r.ExperienceScore,
                RatingScore = (double)r.RatingScore,
                ResponseTimeScore = (double)r.ResponseTimeScore,
                Explanation = r.Explanation,
                Rank = r.Rank
            }).ToList()
        };
    }

    private async Task<Dictionary<Guid, string>> GenerateExplanationsAsync(
        CaseEntity caseEntity,
        List<ScoredLawyerCandidate> scoredCandidates,
        MatchingStrategy strategy,
        CancellationToken cancellationToken)
    {
        var explanations = new Dictionary<Guid, string>();
        if (scoredCandidates.Count == 0) return explanations;

        var systemPrompt = """
            You are an expert Egyptian Law AI legal advisor.
            Explain why each lawyer was matched to the client's case based on their qualifications and scores.

            Return ONLY a valid JSON object mapping each lawyer's ID to a clear, concise Arabic explanation (1-2 sentences).
            Format:
            {
              "lawyer_guid_here": "سبب الترشيح والتميز بالعربية...",
              ...
            }
            """;

        var candidatesSummary = new StringBuilder();
        foreach (var candidate in scoredCandidates)
        {
            candidatesSummary.AppendLine($"""
                - Lawyer ID: {candidate.Candidate.LawyerId}
                  Name: {candidate.Candidate.LawyerName}
                  Rank: #{candidate.Rank}
                  Total Score: {candidate.TotalScore}
                  Location Score: {candidate.LocationScore} (Gov: {candidate.Candidate.Governorate ?? "Not specified"})
                  Experience Score: {candidate.ExperienceScore} (Years: {candidate.Candidate.SpecializationYearsOfExperience}, Cases: {candidate.Candidate.SpecializationCasesHandled})
                  Rating Score: {candidate.RatingScore} (Avg Rating: {candidate.Candidate.AverageRating})
                  Response Time Score: {candidate.ResponseTimeScore} (Avg Hours: {candidate.Candidate.AverageResponseTimeHours})
                """);
        }

        var userPrompt = $"""
            Case Details:
            Title: {caseEntity.Title}
            Description: {caseEntity.Description}
            Governorate: {caseEntity.Governorate ?? "Not specified"}

            Strategy Weights:
            LocationWeight: {strategy.LocationWeight}, ExperienceWeight: {strategy.ExperienceWeight}, RatingWeight: {strategy.RatingWeight}, ResponseTimeWeight: {strategy.ResponseTimeWeight}

            Scored Lawyer Candidates:
            {candidatesSummary}
            """;

        try
        {
            var response = await _chatModelProvider.GenerateAsync(systemPrompt, userPrompt, cancellationToken);
            var cleanedJson = CleanJsonResponse(response);
            using var doc = JsonDocument.Parse(cleanedJson);
            foreach (var element in doc.RootElement.EnumerateObject())
            {
                if (Guid.TryParse(element.Name, out var lawyerId))
                {
                    explanations[lawyerId] = element.Value.GetString() ?? string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI Explanation generation failed or was unparseable. Falling back to default explanations.");
        }

        foreach (var c in scoredCandidates)
        {
            if (!explanations.TryGetValue(c.Candidate.LawyerId, out var exp) || string.IsNullOrWhiteSpace(exp))
            {
                explanations[c.Candidate.LawyerId] = $"تم ترشيح المحامي {c.Candidate.LawyerName} بحصوله على الترتيب #{c.Rank} بنتيجة إجمالية {c.TotalScore} بناءً على ملاءمة التخصص والموقع الجغرافي وخبرته العملية.";
            }
        }

        return explanations;
    }

    private static string CleanJsonResponse(string raw)
    {
        var cleaned = raw.Trim();
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[7..];
        }
        else if (cleaned.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[3..];
        }
        if (cleaned.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[..^3];
        }
        return cleaned.Trim();
    }
}
