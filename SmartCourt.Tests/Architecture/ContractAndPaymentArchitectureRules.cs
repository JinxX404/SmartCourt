using System.Text;
using System.Text.RegularExpressions;

namespace SmartCourt.Tests.Architecture;

internal static partial class ContractAndPaymentArchitectureRules
{
    private static readonly string[] TargetDirectories =
    [
        "SmartCourt/Features/Contracts",
        "SmartCourt/Features/Milestones",
        "SmartCourt/Features/Payments",
        "SmartCourt/Features/Disputes",
        "SmartCourt/Features/Cases/Integration",
        "SmartCourt/Features/Proposals/Integration",
        "SmartCourt/Features/Chat/Integration",
        "SmartCourt/Features/Files/Integration",
        "SmartCourt/Features/Notifications/Integration",
        "SmartCourt/Features/Users/Integration",
        "SmartCourt/Infrastructure/Providers/Payments",
        "SmartCourt/Infrastructure/Providers/Jobs",
        "SmartCourt/Infrastructure/Providers/Events",
        "SmartCourt/Providers/Payments",
        "SmartCourt/Providers/Jobs",
        "SmartCourt.Tests/Features/Contracts",
        "SmartCourt.Tests/Features/Milestones",
        "SmartCourt.Tests/Features/Payments",
        "SmartCourt.Tests/Features/Disputes",
        "SmartCourt.Tests/Integration/ContractsAndPayments",
        "SmartCourt.Tests/TestDoubles/ContractAndPayment"
    ];

    private static readonly HashSet<string> TargetEntityFileNames =
    [
        "Contract.cs",
        "Milestone.cs",
        "MilestoneChangeRequest.cs",
        "MilestoneSubmission.cs",
        "MilestoneSubmissionAttachment.cs",
        "ContractAttachment.cs",
        "EscrowAccount.cs",
        "EscrowHold.cs",
        "EscrowLedgerEntry.cs",
        "PaymentTransaction.cs",
        "LawyerWallet.cs",
        "WithdrawalRequest.cs",
        "Dispute.cs",
        "DisputeResolution.cs",
        "DisputeEvidence.cs",
        "LawyerPenalty.cs",
        "ContractStateHistory.cs",
        "MilestoneStateHistory.cs",
        "IdempotencyRecord.cs",
        "OutboxMessage.cs"
    ];

    public static IReadOnlyList<ArchitectureViolation> Inspect(
        IEnumerable<ArchitectureSourceFile> files)
    {
        var violations = new List<ArchitectureViolation>();

        foreach (var file in files)
        {
            var path = NormalizePath(file.Path);
            var code = RemoveCommentsAndLiterals(file.Source);

            AddMediatRViolation(path, code, violations);
            AddServiceControllerViolation(path, code, violations);
            AddDataAnnotationViolation(path, code, violations);
            AddAutoMapperViolation(path, code, violations);
            AddExternalSdkViolation(path, code, violations);
            AddControllerResponseViolations(path, code, violations);
            AddBlockingAsyncViolation(path, code, violations);
        }

        return violations;
    }

    public static IReadOnlyList<ArchitectureSourceFile> LoadRepositorySources(
        string repositoryRoot)
    {
        var files = new List<ArchitectureSourceFile>();

        foreach (var relativeDirectory in TargetDirectories)
        {
            var directory = Path.Combine(
                repositoryRoot,
                relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(directory))
            {
                continue;
            }

            files.AddRange(Directory
                .EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Select(path => LoadSource(repositoryRoot, path)));
        }

        var entityDirectory = Path.Combine(repositoryRoot, "SmartCourt", "Entities");
        if (Directory.Exists(entityDirectory))
        {
            files.AddRange(Directory
                .EnumerateFiles(entityDirectory, "*.cs", SearchOption.TopDirectoryOnly)
                .Where(path => TargetEntityFileNames.Contains(Path.GetFileName(path)))
                .Select(path => LoadSource(repositoryRoot, path)));
        }

        return files
            .DistinctBy(file => NormalizePath(file.Path), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static ArchitectureSourceFile LoadSource(
        string repositoryRoot,
        string path)
    {
        return new ArchitectureSourceFile(
            Path.GetRelativePath(repositoryRoot, path),
            File.ReadAllText(path));
    }

    private static void AddMediatRViolation(
        string path,
        string code,
        ICollection<ArchitectureViolation> violations)
    {
        if (MediatRPattern().IsMatch(code))
        {
            violations.Add(new ArchitectureViolation(
                "CAP001",
                path,
                "Contracts and Payments code must not reference MediatR or CQRS request types."));
        }
    }

    private static void AddServiceControllerViolation(
        string path,
        string code,
        ICollection<ArchitectureViolation> violations)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var isService = fileName.EndsWith("Service", StringComparison.Ordinal)
            || ServiceDeclarationPattern().IsMatch(code);

        if (isService && ControllerTypePattern().IsMatch(code))
        {
            violations.Add(new ArchitectureViolation(
                "CAP002",
                path,
                "Feature services must communicate through service interfaces, not controllers."));
        }
    }

    private static void AddDataAnnotationViolation(
        string path,
        string code,
        ICollection<ArchitectureViolation> violations)
    {
        if (!IsDtoOrEntity(path) || !DataAnnotationPattern().IsMatch(code))
        {
            return;
        }

        violations.Add(new ArchitectureViolation(
            "CAP003",
            path,
            "Contracts and Payments DTOs and entities must not use Data Annotations."));
    }

    private static void AddAutoMapperViolation(
        string path,
        string code,
        ICollection<ArchitectureViolation> violations)
    {
        if (AutoMapperPattern().IsMatch(code))
        {
            violations.Add(new ArchitectureViolation(
                "CAP004",
                path,
                "Contracts and Payments code must map manually and must not reference AutoMapper."));
        }
    }

    private static void AddExternalSdkViolation(
        string path,
        string code,
        ICollection<ArchitectureViolation> violations)
    {
        if (IsProviderImplementation(path) || !ExternalSdkPattern().IsMatch(code))
        {
            return;
        }

        violations.Add(new ArchitectureViolation(
            "CAP005",
            path,
            "External SDK types may appear only in provider implementations."));
    }

    private static void AddControllerResponseViolations(
        string path,
        string code,
        ICollection<ArchitectureViolation> violations)
    {
        if (!Path.GetFileName(path).EndsWith("Controller.cs", StringComparison.Ordinal))
        {
            return;
        }

        var searchFrom = 0;
        while (true)
        {
            var httpAttribute = code.IndexOf("[Http", searchFrom, StringComparison.Ordinal);
            if (httpAttribute < 0)
            {
                return;
            }

            var publicMethod = code.IndexOf("public ", httpAttribute, StringComparison.Ordinal);
            var nextHttpAttribute = code.IndexOf(
                "[Http",
                httpAttribute + 1,
                StringComparison.Ordinal);
            if (publicMethod < 0
                || nextHttpAttribute >= 0 && nextHttpAttribute < publicMethod)
            {
                searchFrom = httpAttribute + 5;
                continue;
            }

            var parametersStart = code.IndexOf('(', publicMethod);
            if (parametersStart < 0)
            {
                return;
            }

            var signature = code[publicMethod..parametersStart];
            if (!signature.Contains("ApiResponse", StringComparison.Ordinal))
            {
                violations.Add(new ArchitectureViolation(
                    "CAP006",
                    path,
                    "Every controller action must declare an ApiResponse or ApiResponse<T> result."));
            }

            searchFrom = parametersStart + 1;
        }
    }

    private static void AddBlockingAsyncViolation(
        string path,
        string code,
        ICollection<ArchitectureViolation> violations)
    {
        if (BlockingAsyncPattern().IsMatch(code))
        {
            violations.Add(new ArchitectureViolation(
                "CAP007",
                path,
                "Contracts and Payments code must not synchronously block asynchronous work."));
        }
    }

    private static bool IsDtoOrEntity(string path)
    {
        if (path.Contains("/DTOs/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/Entities/", StringComparison.OrdinalIgnoreCase)
                && IsContractsAndPaymentsFeaturePath(path))
        {
            return true;
        }

        return (path.StartsWith("SmartCourt/Entities/", StringComparison.OrdinalIgnoreCase)
                || path.Contains("/SmartCourt/Entities/", StringComparison.OrdinalIgnoreCase))
            && TargetEntityFileNames.Contains(Path.GetFileName(path));
    }

    private static bool IsContractsAndPaymentsFeaturePath(string path)
    {
        return path.Contains("/Features/Contracts/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/Features/Milestones/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/Features/Payments/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/Features/Disputes/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProviderImplementation(string path)
    {
        return path.StartsWith("SmartCourt/Providers/", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/SmartCourt/Providers/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string RemoveCommentsAndLiterals(string source)
    {
        var result = new StringBuilder(source.Length);
        var state = LexicalState.Code;

        for (var index = 0; index < source.Length; index++)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';

            switch (state)
            {
                case LexicalState.Code when current == '/' && next == '/':
                    result.Append("  ");
                    index++;
                    state = LexicalState.LineComment;
                    break;
                case LexicalState.Code when current == '/' && next == '*':
                    result.Append("  ");
                    index++;
                    state = LexicalState.BlockComment;
                    break;
                case LexicalState.Code when current == '@' && next == '"':
                    result.Append("  ");
                    index++;
                    state = LexicalState.VerbatimString;
                    break;
                case LexicalState.Code when current == '"':
                    result.Append(' ');
                    state = LexicalState.String;
                    break;
                case LexicalState.Code when current == '\'':
                    result.Append(' ');
                    state = LexicalState.Character;
                    break;
                case LexicalState.LineComment when current == '\r' || current == '\n':
                    result.Append(current);
                    state = LexicalState.Code;
                    break;
                case LexicalState.BlockComment when current == '*' && next == '/':
                    result.Append("  ");
                    index++;
                    state = LexicalState.Code;
                    break;
                case LexicalState.String when current == '\\':
                    result.Append("  ");
                    index++;
                    break;
                case LexicalState.String when current == '"':
                    result.Append(' ');
                    state = LexicalState.Code;
                    break;
                case LexicalState.Character when current == '\\':
                    result.Append("  ");
                    index++;
                    break;
                case LexicalState.Character when current == '\'':
                    result.Append(' ');
                    state = LexicalState.Code;
                    break;
                case LexicalState.VerbatimString when current == '"' && next == '"':
                    result.Append("  ");
                    index++;
                    break;
                case LexicalState.VerbatimString when current == '"':
                    result.Append(' ');
                    state = LexicalState.Code;
                    break;
                case LexicalState.Code:
                    result.Append(current);
                    break;
                default:
                    result.Append(current is '\r' or '\n' ? current : ' ');
                    break;
            }
        }

        return result.ToString();
    }

    [GeneratedRegex(
        @"\busing\s+MediatR\b|\bIRequest(?:Handler)?\s*<|\bIMediator\b|\bISender\b|\bIPublisher\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex MediatRPattern();

    [GeneratedRegex(
        @"\b(?:class|interface)\s+I?\w*Service\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex ServiceDeclarationPattern();

    [GeneratedRegex(@"\b\w+Controller\b", RegexOptions.CultureInvariant)]
    private static partial Regex ControllerTypePattern();

    [GeneratedRegex(
        @"\bSystem\.ComponentModel\.DataAnnotations\b|\[(?:Required|Key|Column|ForeignKey|NotMapped|Timestamp|ConcurrencyCheck|DatabaseGenerated|MaxLength|MinLength|Range|StringLength|Precision|Unicode|DeleteBehavior)\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex DataAnnotationPattern();

    [GeneratedRegex(
        @"\busing\s+AutoMapper\b|\bIMapper\b|\bCreateMap\s*<|\bProfile\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex AutoMapperPattern();

    [GeneratedRegex(
        @"\b(?:Stripe|Paymob|PayPal|Adyen|Braintree|CheckoutDotCom|Square|Hangfire|Supabase|Twilio|Qdrant|MailKit|UglyToad)\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex ExternalSdkPattern();

    [GeneratedRegex(
        @"\.\s*Result\b|\.\s*Wait\s*\(|\.\s*GetAwaiter\s*\(\s*\)\s*\.\s*GetResult\s*\(",
        RegexOptions.CultureInvariant)]
    private static partial Regex BlockingAsyncPattern();

    private enum LexicalState
    {
        Code,
        LineComment,
        BlockComment,
        String,
        VerbatimString,
        Character
    }
}

internal sealed record ArchitectureSourceFile(string Path, string Source);

internal sealed record ArchitectureViolation(
    string Rule,
    string Path,
    string Message);
