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
using SmartCourt.Features.CaseReview.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Features.CaseReview;

public class CaseReviewService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IChatModelProvider chatModelProvider,
    ILogger<CaseReviewService> logger) : ICaseReviewService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IChatModelProvider _chatModelProvider = chatModelProvider;
    private readonly ILogger<CaseReviewService> _logger = logger;

    public async Task<CaseReviewReportDto> CreateReviewReportAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var caseEntity = await _dbContext.Cases
            .Include(c => c.Documents)
                .ThenInclude(d => d.StoredFile)
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        if (caseEntity.ClientId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بمراجعة هذه القضية.");
        }

        // Validate transition (must be in Submitted status to move to Reviewed)
        CaseStatusTransitionGuard.EnsureCanTransition(caseEntity.Status, CaseStatus.Reviewed);

        // 1. Generate AI Review Feedback
        var reviewPoints = await RequestAiReviewPointsAsync(caseEntity, cancellationToken);

        // 2. Mark previous reports as not latest
        var previousReports = await _dbContext.CaseReviewReports
            .Where(r => r.CaseId == caseId && r.IsLatest)
            .ToListAsync(cancellationToken);

        foreach (var report in previousReports)
        {
            report.IsLatest = false;
        }

        // 3. Create new CaseReviewReport
        var newReport = new CaseReviewReport
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            IsLatest = true,
            ReviewPoints = reviewPoints ?? new List<ReviewPoint>()
        };

        _dbContext.CaseReviewReports.Add(newReport);

        // 4. Update Case status
        caseEntity.Status = CaseStatus.Reviewed;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(newReport);
    }

    public async Task<List<CaseReviewReportDto>> GetReviewReportsAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var caseEntity = await _dbContext.Cases
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        if (caseEntity.ClientId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول لمراجعات هذه القضية.");
        }

        var reports = await _dbContext.CaseReviewReports
            .AsNoTracking()
            .Where(r => r.CaseId == caseId)
            .OrderByDescending(r => r.CreatedAt)
            .Include(r => r.ReviewPoints)
            .ToListAsync(cancellationToken);

        return reports.Select(MapToDto).ToList();
    }

    public async Task<CaseReviewReportDto> GetLatestReviewReportAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var caseEntity = await _dbContext.Cases
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        if (caseEntity.ClientId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول لمراجعة هذه القضية.");
        }

        var latestReport = await _dbContext.CaseReviewReports
            .AsNoTracking()
            .Where(r => r.CaseId == caseId && r.IsLatest)
            .Include(r => r.ReviewPoints)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestReport == null)
        {
            throw new NotFoundException("لا يوجد تقرير مراجعة لهذه القضية.");
        }

        return MapToDto(latestReport);
    }

    private async Task<List<ReviewPoint>> RequestAiReviewPointsAsync(CaseEntity caseEntity, CancellationToken cancellationToken)
    {
        var systemPrompt = """
            You are an expert Egyptian Law AI Legal Assistant.
            Analyze the submitted legal case details and documents to provide structured, highly detailed, comprehensive improvement feedback for the client based strictly on Egyptian Law.

            Provide an in-depth analysis covering:
            1. Strengths (نقاط القوة القانونية): Explain what makes the client's side in the legal case stronger than the other party's position under Egyptian Law (evidentiary superiority, contractual terms, written proof, statutory compliance).
            2. Weaknesses (نقاط الضعف والمخاطر): Explain what gives the opposing party an advantage over the client in this dispute under Egyptian Law (gaps in formal notice via court bailiffs, unverified dates, lack of official receipts or registration, procedural defenses).
            3. Missing Information (المعلومات والنقاط المفقودة): Identify missing essential factual details, exact dates, financial breakdown (principal, interest, damages), or timeline events required for Egyptian court filings.
            4. Missing Documents (المستندات والوثائق المفقودة): Specify exact, non-generic document names required for this specific case domain under Egyptian Law (e.g., National ID copy, signed written contract, official bailiff notices/إنذارات رسمية على يد محضر, bank transfer receipts/إيصالات سداد, commercial registry, etc.).
            5. Suggestions (مقترحات صياغة وهيكلة الدعوى): Practical, actionable steps to refine and restructure the case facts, timeline, and evidence to make the case file much more solid BEFORE passing it to the next stage.

            ABSOLUTE STRICT RULE FOR SUGGESTIONS:
            Do NOT ever suggest consulting, hiring, or taking guidance/advice from a lawyer. All suggestions must focus strictly on how the client can structure the case description, quantify claims, and gather required evidence directly.

            You MUST output ONLY a valid JSON array of objects following this exact schema:
            [
              {
                "type": "Strength | Weakness | Suggestion | MissingCaseInfo | MissingCaseDoc",
                "description": "Comprehensive, highly detailed legal explanation in Arabic."
              }
            ]

            Allowed values for 'type':
            - "Strength": Strong legal aspects giving the client an advantage over the opposing party.
            - "Weakness": Weak points or risks giving the opposing party an advantage.
            - "Suggestion": Practical recommendations to refine the structure and content of the case.
            - "MissingCaseInfo": Missing factual or legal details required for court filings.
            - "MissingCaseDoc": Specific missing documents or evidence needed for Egyptian Law procedures.

            Output ONLY raw JSON. Do not include extra text or markdown formatting outside the JSON array.
            """;

        var docInfoBuilder = new StringBuilder();
        if (caseEntity.Documents != null && caseEntity.Documents.Count > 0)
        {
            foreach (var doc in caseEntity.Documents)
            {
                if (doc == null) continue;
                var name = doc.StoredFile?.OriginalFileName ?? "Document";
                var type = doc.StoredFile?.ContentType ?? "application/octet-stream";
                docInfoBuilder.AppendLine($"- Document: {name} ({type})");
            }
        }
        else
        {
            docInfoBuilder.AppendLine("No documents attached.");
        }

        var userPrompt = $"""
            Case Title: {caseEntity.Title}
            Case Description: {caseEntity.Description}
            Governorate: {caseEntity.Governorate ?? "Not specified"}
            City: {caseEntity.City ?? "Not specified"}
            Attached Documents:
            {docInfoBuilder}
            """;

        var aiResponse = await _chatModelProvider.GenerateAsync(systemPrompt, userPrompt, cancellationToken);

        return ParseReviewPoints(aiResponse);
    }

    private static List<ReviewPoint> ParseReviewPoints(string? aiOutput)
    {
        if (string.IsNullOrWhiteSpace(aiOutput))
        {
            return new List<ReviewPoint>
            {
                new ReviewPoint
                {
                    Id = Guid.NewGuid(),
                    Description = "تمت مراجعة القضية بنجاح.",
                    Type = ReviewPointType.Suggestion
                }
            };
        }

        var cleanedJson = aiOutput.Trim();
        if (cleanedJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleanedJson = cleanedJson[7..];
        }
        else if (cleanedJson.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleanedJson = cleanedJson[3..];
        }

        if (cleanedJson.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleanedJson = cleanedJson[..^3];
        }

        cleanedJson = cleanedJson.Trim();

        var points = new List<ReviewPoint>();

        try
        {
            using var jsonDoc = JsonDocument.Parse(cleanedJson);
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in jsonDoc.RootElement.EnumerateArray())
                {
                    var typeStr = element.TryGetProperty("type", out var tProp) && tProp.ValueKind == JsonValueKind.String ? tProp.GetString() : null;
                    var descStr = element.TryGetProperty("description", out var dProp) && dProp.ValueKind == JsonValueKind.String ? dProp.GetString() : null;

                    if (string.IsNullOrWhiteSpace(descStr)) continue;

                    points.Add(new ReviewPoint
                    {
                        Id = Guid.NewGuid(),
                        Description = descStr.Trim(),
                        Type = MapType(typeStr)
                    });
                }
            }
        }
        catch
        {
            points.Add(new ReviewPoint
            {
                Id = Guid.NewGuid(),
                Description = string.IsNullOrWhiteSpace(cleanedJson) ? "تمت مراجعة القضية بنجاح." : cleanedJson,
                Type = ReviewPointType.Suggestion
            });
        }

        if (points.Count == 0)
        {
            points.Add(new ReviewPoint
            {
                Id = Guid.NewGuid(),
                Description = "تمت مراجعة القضية بنجاح.",
                Type = ReviewPointType.Suggestion
            });
        }

        return points;
    }

    private static ReviewPointType MapType(string? typeStr)
    {
        if (string.IsNullOrWhiteSpace(typeStr)) return ReviewPointType.Suggestion;

        return typeStr.Trim().ToLowerInvariant() switch
        {
            "strength" => ReviewPointType.Strength,
            "weakness" => ReviewPointType.Weakness,
            "suggestion" => ReviewPointType.Suggestion,
            "missingcaseinfo" or "missing_case_info" or "missinginfo" => ReviewPointType.MissingCaseInfo,
            "missingcasedoc" or "missing_case_doc" or "missingdoc" or "missingdocument" => ReviewPointType.MissingCaseDoc,
            _ => ReviewPointType.Suggestion
        };
    }

    private static CaseReviewReportDto MapToDto(CaseReviewReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var reviewPointDtos = new List<ReviewPointDto>();
        if (report.ReviewPoints != null)
        {
            foreach (var p in report.ReviewPoints)
            {
                if (p == null) continue;
                reviewPointDtos.Add(new ReviewPointDto
                {
                    Id = p.Id,
                    Description = p.Description ?? string.Empty,
                    Type = p.Type.ToString()
                });
            }
        }

        return new CaseReviewReportDto
        {
            Id = report.Id,
            CaseId = report.CaseId,
            IsLatest = report.IsLatest,
            CreatedAt = report.CreatedAt,
            ReviewPoints = reviewPointDtos
        };
    }
}
