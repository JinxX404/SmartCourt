using System;
using System.IO;
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
    IFileStorageService fileStorageService,
    IDocumentParsingProvider documentParsingProvider,
    ILogger<CaseAnalysisService> logger) : ICaseAnalysisService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IChatModelProvider _chatModelProvider = chatModelProvider;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IDocumentParsingProvider _documentParsingProvider = documentParsingProvider;
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
            You are an expert Egyptian Law AI classifier specializing in legal jurisdiction, court admission levels, and legal categorization under Egyptian Law.
            Analyze the submitted case title, description, and attached document contents to classify the case into exact categories.

            You MUST return ONLY a valid JSON object with the following schema:
            {
              "specialization": "FamilyLaw | CivilLaw | CommercialLaw | AdministrativeAndStateCouncilLaw | CriminalLaw | LaborLaw",
              "requiredLawyerLevel": "GeneralRegistration | PrimaryCourt | AppealCourt | CassationCourt",
              "complexity": "Routine | Standard | Advanced | Exceptional"
            }

            DETAILED CONCEPTUAL CLASSIFICATION GUIDELINES FOR SPECIALIZATIONS:

            1. "LaborLaw" (منازعات العمل والمستحقات العمالية):
               - RELATIONSHIP CONTEXT: An individual employee, worker, engineer, manager, or laborer vs. an employer, business, factory, software company, or corporate organization.
               - SITUATIONAL TRIGGERS:
                 * The case describes a person working for an employer (whether with a written or verbal contract, full-time or part-time).
                 * The conflict involves job dismissal, wrongful termination (الفصل التعسفي), verbal firing, account suspension/lockout by HR.
                 * The individual claims unpaid monthly salaries, overdue wages, annual leave compensation, end-of-service gratuity, severance pay, or work-related injury/social insurance benefits under Egyptian Labor Law No. 12 of 2003.
               - ABSOLUTE BOUNDARY RULE: Whenever an individual is demanding rights or compensation arising from their job/service for a company or employer, classify as "LaborLaw", REGARDLESS of whether the employer is a commercial enterprise or corporation.

            2. "CommercialLaw" (القانون التجاري والشركات):
               - RELATIONSHIP CONTEXT: Merchant vs. Merchant, Company vs. Company (B2B), Partner vs. Partner, Shareholder vs. Corporate Board, or Business vs. Commercial Paper Holder.
               - SITUATIONAL TRIGGERS:
                 * Two commercial companies or business partners disagreeing over commercial trade agreements, supply contracts, distribution rights, franchise deals, or agency commissions under Commercial Code No. 17 of 1999.
                 * Corporate internal disputes involving company formation, shareholder voting, partner expulsion, corporate restructuring, liquidation, or commercial registry.
                 * Commercial negotiable instruments (cheques, bills of exchange, promissory notes) issued between business entities or trade bankruptcy/insolvency.
               - ABSOLUTE BOUNDARY RULE: "CommercialLaw" is strictly for commercial activities between trading entities or corporate partners. NEVER select CommercialLaw for an individual employee suing their employer.

            3. "CriminalLaw" (القانون الجنائي والجنايات والجنح):
               - RELATIONSHIP CONTEXT: State Prosecutor / Victim vs. Accused Offender.
               - SITUATIONAL TRIGGERS:
                 * The case involves acts defined as crimes, felonies (جنايات), misdemeanors (جنح), or contraventions punishable by imprisonment or penal fines under the Egyptian Penal Code.
                 * Offenses including fraud (النصب), breach of trust (خيانة الأمانة), theft, forgery of official/private documents (التزوير), cybercrime (الجرائم الإلكترونية / السب والقذف عبر الإنترنت), bribery, embezzlement of public funds, physical assault, or drugs.

            4. "AdministrativeAndStateCouncilLaw" (القضاء الإداري ومجلس الدولة):
               - RELATIONSHIP CONTEXT: Citizen or Private Entity vs. Public Government Authority / Sovereign Ministry / State Department (الجهة الإدارية).
               - SITUATIONAL TRIGGERS:
                 * Challenging an official government administrative decision, executive order, license revocation, or decree before the State Council (مجلس الدولة).
                 * Disputes involving public sector civil service government employees (كادر الموظفين الحكوميين), or public state tenders, auctions, and government procurement contracts.

            5. "FamilyLaw" (الأحوال الشخصية والأسرة):
               - RELATIONSHIP CONTEXT: Spouses, Ex-spouses, Family Members, Heirs, Guardians.
               - SITUATIONAL TRIGGERS:
                 * Disputes involving marital status, divorce, khula, marriage contract validity, alimony/maintenance (نفقات الزوجية والأولاد), child custody/visitation (الحضانة والرؤية), estate inheritance among heirs (إعلام الوراثة والتركات), or legal guardianship over minors.

            6. "CivilLaw" (القانون المدني):
               - RELATIONSHIP CONTEXT: Private Individual vs. Private Individual or Entity in a General Civil Relationship.
               - SITUATIONAL TRIGGERS:
                 * Disputes involving real estate ownership, land registration (الشهر العقاري), property boundary lines, or court validation of contracts (دعاوى صحة ونفاذ / تثبيت الملكية).
                 * Residential or commercial real estate leasing contracts governed by the Civil Code, eviction, civil tort liability/damages (التعويض عن الضرر المدني), or personal private loans between individuals where no specialized code (like Labor or Commercial Law) applies.

            Enum value reference:
            - requiredLawyerLevel:
              - "GeneralRegistration": قضايا بسيطة لا تتطلب درجة قيد عالية (جدول عام)
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
                var storagePath = doc.StoredFile?.FileUrl;

                docInfoBuilder.AppendLine($"--- Document: {name} ({type}) ---");

                if (!string.IsNullOrWhiteSpace(storagePath))
                {
                    try
                    {
                        var fileBytes = await _fileStorageService.DownloadAsync(storagePath, cancellationToken);
                        if (fileBytes.Length > 0)
                        {
                            using var stream = new MemoryStream(fileBytes);
                            var extractedText = await _documentParsingProvider.ExtractTextAsync(stream, name, cancellationToken);

                            if (!string.IsNullOrWhiteSpace(extractedText))
                            {
                                const int maxDocChars = 4000;
                                var truncatedText = extractedText.Length > maxDocChars
                                    ? string.Concat(extractedText.AsSpan(0, maxDocChars), "\n[... Content truncated due to length ...]")
                                    : extractedText;

                                docInfoBuilder.AppendLine("Extracted Content:");
                                docInfoBuilder.AppendLine(truncatedText);
                            }
                            else
                            {
                                docInfoBuilder.AppendLine("[No text content could be extracted from this document.]");
                            }
                        }
                        else
                        {
                            docInfoBuilder.AppendLine("[Document file is empty.]");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to download or parse content for document {FileName} (StoragePath: {StoragePath}) in CaseId {CaseId}", name, storagePath, caseId);
                        docInfoBuilder.AppendLine($"[Unable to extract text content: {ex.Message}]");
                    }
                }
                else
                {
                    docInfoBuilder.AppendLine("[No storage path available for this document.]");
                }

                docInfoBuilder.AppendLine();
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
