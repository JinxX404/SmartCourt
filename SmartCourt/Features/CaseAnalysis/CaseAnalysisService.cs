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
              "specialization": "FamilyLaw | CivilLaw | CommercialLaw | AdministrativeAndStateCouncilLaw | CriminalLaw | LaborLaw | ConstitutionalLaw | TaxLaw | CustomsLaw | CorporateLaw | Contracts | IntellectualProperty | Arbitration | BankingAndFinance | Investment | RealEstateAndPropertyRegistration | Execution | Insurance | Environment | InformationTechnologyAndTelecommunications | Cybercrimes",
              "requiredLawyerLevel": "GeneralRegistration | PrimaryCourt | AppealCourt | CassationCourt",
              "complexity": "Routine | Standard | Advanced | Exceptional"
            }

            DETAILED CONCEPTUAL CLASSIFICATION GUIDELINES FOR SPECIALIZATIONS:

            1. "FamilyLaw" (الأحوال الشخصية والأسرة):
               - Disputes involving marital status, divorce, khula, marriage contract validity, alimony/maintenance (نفقات الزوجية والأولاد), child custody/visitation (الحضانة والرؤية), estate inheritance among heirs (إعلام الوراثة والتركات), or legal guardianship over minors.

            2. "CivilLaw" (القانون المدني):
               - Disputes involving general civil contracts, civil tort liability/damages (التعويض عن الضرر المدني), personal loans between individuals, residential leasing contracts, or general civil rights where no specialized code applies.

            3. "CommercialLaw" (القانون التجاري والأوراق التجارية):
               - Disputes between merchants/businesses (B2B), trade agreements, distribution deals, agency commissions, negotiable instruments (cheques, bills of exchange, promissory notes) issued between business entities, or bankruptcy/insolvency under Commercial Code No. 17 of 1999.

            4. "AdministrativeAndStateCouncilLaw" (القضاء الإداري ومجلس الدولة):
               - Challenging official government administrative decisions, executive orders, license revocations, sovereign ministry decrees before the State Council (مجلس الدولة), public sector civil service employee disputes, or government procurement contracts/tenders.

            5. "CriminalLaw" (القانون الجنائي والجنايات والجنح):
               - Cases involving offenses defined under the Egyptian Penal Code: fraud (النصب), breach of trust (خيانة الأمانة), theft, forgery of official/private documents (التزوير), physical assault, embezzlement of public funds, or felonies/misdemeanors punishable by imprisonment or penal fines.

            6. "LaborLaw" (قانون العمل ومنازعات العمال):
               - RELATIONSHIP CONTEXT: An individual employee/worker vs. an employer/company.
               - Disputes regarding wrongful termination (الفصل التعسفي), unpaid wages/salaries, end-of-service gratuity, annual leave balance payout, or social insurance benefits under Egyptian Labor Law No. 12 of 2003.

            7. "ConstitutionalLaw" (القانون الدستوري):
               - Challenges regarding unconstitutionality of laws, decrees, or regulations before the Supreme Constitutional Court (المحكمة الدستورية العليا).

            8. "TaxLaw" (قانون الضرائب):
               - Disputes with the Egyptian Tax Authority regarding income tax, VAT (ضريبة القيمة المضافة), stamp tax, tax audits, or appeals before Tax Dispute Resolution Committees.

            9. "CustomsLaw" (قانون الجمارك):
               - Disputes involving customs clearance, tariff valuations, customs smuggling (التهريب الجمركي), or fine disputes with the Customs Authority under Egyptian Customs Law.

            10. "CorporateLaw" (قانون الشركات):
                - Internal corporate matters: company incorporation, shareholder voting/rights, board of directors disputes, partner expulsion, corporate restructuring, mergers, acquisitions, or liquidation under Companies Law No. 159 of 1981.

            11. "Contracts" (العقود والالتزامات):
                - Cases focused primarily on drafting, interpreting, enforcing, or invalidating specialized private or commercial contracts, breach of contract clauses, liquidated damages, or contract rescission.

            12. "IntellectualProperty" (الملكية الفكرية):
                - Disputes involving trademark registration/infringement (العلامات التجارية), patents, copyright (حقوق المؤلف), industrial designs, or trade secret theft under Intellectual Property Law No. 82 of 2002.

            13. "Arbitration" (التحكيم والوسائل البديلة):
                - Domestic or international arbitration proceedings, annulment lawsuits of arbitral awards (دعاوى إبطال أحكام التحكيم), or enforcement of arbitral awards under Egyptian Arbitration Law No. 27 of 1994.

            14. "BankingAndFinance" (القانون المصرفي والتمويل):
                - Disputes involving bank loans, letter of credit (خطابات الضمان), mortgages, Central Bank of Egypt regulations, or consumer/mortgage finance institutions.

            15. "Investment" (قانون الاستثمار):
                - Disputes involving investment incentives, free zones, or investor disputes with the General Authority for Investment and Free Zones (GAFI) under Investment Law No. 72 of 2017.

            16. "RealEstateAndPropertyRegistration" (الشهر العقاري والتسجيل):
                - Disputes over real estate property ownership, land boundary lines, real estate registry (الشهر العقاري), contract validity and enforcement lawsuits (دعاوى صحة ونفاذ), or real estate title registration.

            17. "Execution" (منازعات وإشكالات التنفيذ):
                - Disputes regarding enforcement of court rulings, execution stays (إشكال في التنفيذ), precautionary attachment (الحجز التحفظي), or forced auctions (البيع الجبري).

            18. "Insurance" (قانون التأمين):
                - Disputes with insurance companies regarding life, property, auto accident compensation, or maritime/cargo insurance claims.

            19. "Environment" (قانون البيئة):
                - Disputes involving environmental pollution, natural reserves violations, or industrial environmental compliance under Environmental Law No. 4 of 1994.

            20. "InformationTechnologyAndTelecommunications" (تكنولوجيا المعلومات والاتصالات):
                - Disputes regarding telecommunication licenses, electronic signatures, software licensing, or National Telecommunications Regulatory Authority (NTRA) compliance.

            21. "Cybercrimes" (الجرائم الإلكترونية والسيبرانية):
                - Criminal offenses committed online/digitally: cyber-blackmail (الابتزاز الإلكتروني), online defamation/libel, hacking/unauthorized access, identity theft, or data privacy breaches under Anti-Cybercrime Law No. 175 of 2018.

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
            var aiResponseModel = await _chatModelProvider.GenerateAsync(systemPrompt, userPrompt, cancellationToken);
            aiResponse = aiResponseModel.Content;
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

        var firstBrace = cleanedJson.IndexOf('{');
        var lastBrace = cleanedJson.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            cleanedJson = cleanedJson.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

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
