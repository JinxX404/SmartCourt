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
using SmartCourt.Common.Models;
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

    public async Task<FinalizeResultDto> ProcessMatchingAndPersistAsync(Guid caseId, PagedRequest? pagedRequest = null, CancellationToken cancellationToken = default)
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
            var exp = explanations.GetValueOrDefault(c.Candidate.LawyerId);
            if (string.IsNullOrWhiteSpace(exp))
            {
                exp = BuildTailoredFallbackExplanation(c, caseEntity.Governorate);
            }

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

        pagedRequest ??= new PagedRequest();
        var pageNumber = pagedRequest.PageNumber < 1 ? 1 : pagedRequest.PageNumber;
        var pageSize = pagedRequest.PageSize < 1 ? 10 : pagedRequest.PageSize;

        var totalRecords = scoredCandidates.Count;
        var totalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize);

        var pagedCandidates = scoredCandidates
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new FinalizeResultDto
        {
            CaseId = caseId,
            TotalEligibleLawyers = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            Recommendations = pagedCandidates.Select(c => new CaseRecommendationDto
            {
                LawyerId = c.Candidate.LawyerId,
                LawyerName = c.Candidate.LawyerName,
                TotalScore = c.TotalScore,
                LocationScore = c.LocationScore,
                ExperienceScore = c.ExperienceScore,
                RatingScore = c.RatingScore,
                ResponseTimeScore = c.ResponseTimeScore,
                Explanation = explanations.GetValueOrDefault(c.Candidate.LawyerId) ?? BuildTailoredFallbackExplanation(c, caseEntity.Governorate),
                Rank = c.Rank
            }).ToList()
        };
    }

    public async Task<PagedResponse<FinalizeResultDto>> GetRecommendationsAsync(Guid caseId, Guid currentUserId, PagedRequest? pagedRequest = null, CancellationToken cancellationToken = default)
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

        pagedRequest ??= new PagedRequest();
        var pageNumber = pagedRequest.PageNumber < 1 ? 1 : pagedRequest.PageNumber;
        var pageSize = pagedRequest.PageSize < 1 ? 10 : pagedRequest.PageSize;

        var totalRecords = await _dbContext.CaseRecommendations
            .Where(cr => cr.CaseId == caseId)
            .CountAsync(cancellationToken);

        var totalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)pageSize);

        var recs = await _dbContext.CaseRecommendations
            .Include(cr => cr.LawyerProfile)
            .ThenInclude(lp => lp.User)
            .AsNoTracking()
            .Where(cr => cr.CaseId == caseId)
            .OrderBy(cr => cr.Rank)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dto = new FinalizeResultDto
        {
            CaseId = caseId,
            TotalEligibleLawyers = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
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

        return PagedResponse<FinalizeResultDto>.OkPaged(dto, pageNumber, pageSize, totalRecords, totalPages);
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
            Your task is to generate a UNIQUE, INDIVIDUALIZED, and HIGHLY TAILORED recommendation explanation for EACH lawyer candidate matched to the client's case.

            CRITICAL DIRECTIVES FOR TAILORED EXPLANATIONS:
            - Write a distinct, natural, and informative Arabic explanation (1-2 sentences) tailored specifically to each lawyer's background, strengths, and candidate details.
            - Focus on each lawyer's individual standout traits:
              * If the lawyer is located in the case governorate, mention their physical presence and local court familiarity.
              * If the lawyer has high experience years or handles high case volume, emphasize their specialized track record.
              * If the lawyer has high client ratings, mention client satisfaction and service quality.
              * If the lawyer has fast response times, mention prompt communication and responsiveness.
            - DO NOT use generic boilerplate sentences that repeat identical phrasing for every candidate.
            - DO NOT mention any internal algorithm numbers, raw score calculations, matrix weights, parameters, or ranks (such as Total Score, Location Score, 0.85, #1, etc.).

            Return ONLY a valid JSON object mapping each lawyer's ID to their custom tailored Arabic explanation.
            Format:
            {
              "lawyer_guid_here": "سبب الترشيح المخصص والمميز لكل محامي بالعربية...",
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
            var cleanedJson = CleanJsonResponse(response.Content);
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
                explanations[c.Candidate.LawyerId] = BuildTailoredFallbackExplanation(c, caseEntity.Governorate);
            }
        }

        return explanations;
    }

    private static string BuildTailoredFallbackExplanation(ScoredLawyerCandidate candidate, string? caseGovernorate)
    {
        var c = candidate.Candidate;
        var traits = new List<string>();

        if (candidate.LocationScore >= 1.0 && !string.IsNullOrWhiteSpace(c.Governorate))
        {
            traits.Add($"تواجده وممارسته المباشرة بالمحاكم في محافظة {c.Governorate}");
        }
        else if (candidate.LocationScore >= 0.5 && !string.IsNullOrWhiteSpace(c.Governorate))
        {
            traits.Add($"موقعه الجغرافي القريب بمحافظة {c.Governorate}");
        }

        if (c.SpecializationYearsOfExperience > 0 && c.SpecializationCasesHandled > 0)
        {
            traits.Add($"خبرته العملية الممتدة لـ {c.SpecializationYearsOfExperience} سنوات وتعامله مع {c.SpecializationCasesHandled} قضية متخصصة");
        }
        else if (c.SpecializationYearsOfExperience > 0)
        {
            traits.Add($"خبرته في مجال التخصص البالغة {c.SpecializationYearsOfExperience} سنوات");
        }
        else if (c.SpecializationCasesHandled > 0)
        {
            traits.Add($"سجله في نظر {c.SpecializationCasesHandled} قضية متخصصة");
        }

        if (c.AverageRating >= 4.5m)
        {
            traits.Add($"تقييمه الممتاز من العملاء ({c.AverageRating:0.0} من 5)");
        }
        else if (c.AverageRating > 0m)
        {
            traits.Add($"تقييمه الإيجابي من العملاء ({c.AverageRating:0.0} من 5)");
        }

        if (c.AverageResponseTimeHours > 0 && c.AverageResponseTimeHours <= 12)
        {
            traits.Add("سرعة الاستجابة والتواصل الفعال");
        }

        if (traits.Count == 0)
        {
            return $"تم ترشيح المحامي {c.LawyerName} لتناسب درجات قيده وخبرته مع متطلبات قضيتك.";
        }

        if (traits.Count == 1)
        {
            return $"تم ترشيح المحامي {c.LawyerName} لـ{traits[0]}.";
        }

        var joinedTraits = string.Join("، و", traits);
        return $"تم ترشيح المحامي {c.LawyerName} بناءً على {joinedTraits}.";
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
        cleaned = cleaned.Trim();

        var firstBrace = cleaned.IndexOf('{');
        var lastBrace = cleaned.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return cleaned;
    }
}
