using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.CaseAnalysis;

public class CaseAnalysisService(
    ApplicationDbContext dbContext,
    IChatModelProvider chatModelProvider,
    ILogger<CaseAnalysisService> logger) : ICaseAnalysisService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IChatModelProvider _chatModelProvider = chatModelProvider;
    private readonly ILogger<CaseAnalysisService> _logger = logger;

    public async Task<CaseProfile> AnalyzeCaseAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var caseEntity = await _dbContext.Cases
            .Include(c => c.Documents)
            .ThenInclude(d => d.StoredFile)
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (caseEntity == null)
        {
            throw new NotFoundException("القضية غير موجودة.");
        }

        var systemPrompt = """
            You are an expert Egyptian Law AI classifier.
            Analyze the submitted case title, description, and attached documents to classify the case into exact categories according to Egyptian Law requirements.

            You MUST return ONLY a valid JSON object with the following schema:
            {
              "specialization": "FamilyLaw | CivilLaw | CommercialLaw | AdministrativeAndStateCouncilLaw | CriminalLaw | LaborLaw",
              "requiredLawyerLevel": "GeneralRegistration | PrimaryCourt | AppealCourt | CassationCourt",
              "complexity": "Routine | Standard | Advanced | Exceptional"
            }

            Enum value reference:
            - specialization:
              - "FamilyLaw": الأحوال الشخصية والأسرة
              - "CivilLaw": التعويضات، العقود المدنية، الملكية، الإيجارات
              - "CommercialLaw": الشركات، النزاعات التجارية، الأوراق التجارية
              - "AdministrativeAndStateCouncilLaw": القضاء الإداري ومجلس الدولة
              - "CriminalLaw": الجنايات والجنح والجرائم الجنائية
              - "LaborLaw": منازعات العمل والمستحقات العمالية

            - requiredLawyerLevel:
              - "GeneralRegistration": قضايا بسيطة لا تتطلب درجة قيد عالية
              - "PrimaryCourt": المحاكم الابتدائية
              - "AppealCourt": محاكم الاستئناف
              - "CassationCourt": محكمة النقض أو القضايا المعقدة للغاية

            - complexity:
              - "Routine": قضايا روتينية بسيطة
              - "Standard": قضايا قياسية معتادة
              - "Advanced": قضايا متقدمة تتطلب خبرة عالية
              - "Exceptional": قضايا استثنائية معقدة للغاية

            Output ONLY raw JSON. Do not include extra commentary or markdown outside the JSON object.
            """;

        var docInfoBuilder = new StringBuilder();
        if (caseEntity.Documents.Count > 0)
        {
            foreach (var doc in caseEntity.Documents)
            {
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
            Documents:
            {docInfoBuilder}
            """;

        string aiResponse;
        try
        {
            aiResponse = await _chatModelProvider.GenerateAsync(systemPrompt, userPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute IChatModelProvider call for CaseId {CaseId}", caseId);
            throw new BusinessException("AI analysis failed. Please try again.", ex);
        }

        var (specialization, lawyerLevel, complexity) = ParseClassification(aiResponse, caseId);

        var existingProfile = await _dbContext.CaseProfiles
            .FirstOrDefaultAsync(cp => cp.CaseId == caseId, cancellationToken);

        if (existingProfile != null)
        {
            existingProfile.Specialization = specialization;
            existingProfile.RequiredLawyerLevelId = lawyerLevel;
            existingProfile.Complexity = complexity;
        }
        else
        {
            existingProfile = new CaseProfile
            {
                Id = Guid.NewGuid(),
                CaseId = caseId,
                Specialization = specialization,
                RequiredLawyerLevelId = lawyerLevel,
                Complexity = complexity
            };
            _dbContext.CaseProfiles.Add(existingProfile);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return existingProfile;
    }

    private (Specialization Specialization, LawyerLevel Level, CaseComplexity Complexity) ParseClassification(string aiOutput, Guid caseId)
    {
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

        try
        {
            using var doc = JsonDocument.Parse(cleanedJson);
            var root = doc.RootElement;

            var specStr = root.TryGetProperty("specialization", out var sProp) ? sProp.GetString() : null;
            var levelStr = root.TryGetProperty("requiredLawyerLevel", out var lProp) ? lProp.GetString() : null;
            var compStr = root.TryGetProperty("complexity", out var cProp) ? cProp.GetString() : null;

            if (!Enum.TryParse<Specialization>(specStr, true, out var specialization))
            {
                throw new ArgumentException($"Invalid Specialization value: '{specStr}'");
            }

            if (!Enum.TryParse<LawyerLevel>(levelStr, true, out var level))
            {
                throw new ArgumentException($"Invalid LawyerLevel value: '{levelStr}'");
            }

            if (!Enum.TryParse<CaseComplexity>(compStr, true, out var complexity))
            {
                throw new ArgumentException($"Invalid CaseComplexity value: '{compStr}'");
            }

            return (specialization, level, complexity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unparseable AI classification response for CaseId {CaseId}. Raw AI Output: {RawOutput}", caseId, aiOutput);
            throw new BusinessException("AI analysis failed. Please try again.", ex);
        }
    }
}
