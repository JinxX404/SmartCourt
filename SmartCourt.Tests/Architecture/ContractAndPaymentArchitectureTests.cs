using Xunit;

namespace SmartCourt.Tests.Architecture;

public sealed class ContractAndPaymentArchitectureTests
{
    [Fact]
    public void Repository_ObeysContractsAndPaymentsArchitectureRules()
    {
        var repositoryRoot = FindRepositoryRoot();
        var files = ContractAndPaymentArchitectureRules.LoadRepositorySources(
            repositoryRoot);

        var violations = ContractAndPaymentArchitectureRules.Inspect(files);

        Assert.True(
            violations.Count == 0,
            string.Join(
                Environment.NewLine,
                violations.Select(violation =>
                    $"{violation.Rule} {violation.Path}: {violation.Message}")));
    }

    [Fact]
    public void MediatRRule_DetectsFeatureReference()
    {
        AssertRuleDetected(
            "CAP001",
            "SmartCourt/Features/Contracts/ContractService.cs",
            """
            using MediatR;
            public sealed class CreateContract : IRequest<Guid>;
            """);
    }

    [Fact]
    public void ServiceBoundaryRule_DetectsControllerReference()
    {
        AssertRuleDetected(
            "CAP002",
            "SmartCourt/Features/Contracts/ContractService.cs",
            """
            public sealed class ContractService(OtherController controller)
            {
            }
            """);
    }

    [Fact]
    public void DataAnnotationRule_DetectsDtoOrEntityAttribute()
    {
        AssertRuleDetected(
            "CAP003",
            "SmartCourt/Features/Contracts/DTOs/CreateContractRequest.cs",
            """
            using System.ComponentModel.DataAnnotations;
            public sealed record CreateContractRequest([Required] string Title);
            """);
    }

    [Fact]
    public void AutoMapperRule_DetectsFeatureReference()
    {
        AssertRuleDetected(
            "CAP004",
            "SmartCourt/Features/Contracts/ContractService.cs",
            """
            using AutoMapper;
            public sealed class ContractService(IMapper mapper);
            """);
    }

    [Fact]
    public void ExternalSdkRule_DetectsSdkOutsideProviderImplementation()
    {
        AssertRuleDetected(
            "CAP005",
            "SmartCourt/Features/Payments/PaymentEscrowService.cs",
            """
            using Stripe;
            public sealed class PaymentEscrowService(PaymentIntentService payments);
            """);
    }

    [Fact]
    public void ControllerResponseRule_DetectsUnwrappedAction()
    {
        AssertRuleDetected(
            "CAP006",
            "SmartCourt/Features/Contracts/ContractsController.cs",
            """
            public sealed class ContractsController
            {
                [HttpGet]
                public async Task<ActionResult<ContractDetailDto>> GetAsync()
                {
                    return new ContractDetailDto();
                }
            }
            """);
    }

    [Fact]
    public void AsyncRule_DetectsEveryBlockingPattern()
    {
        var source = """
            public sealed class ContractService
            {
                public void Run(Task operation)
                {
                    var value = GetValueAsync().Result;
                    operation.Wait();
                    GetValueAsync().GetAwaiter().GetResult();
                }
            }
            """;

        var violations = Inspect(
            "SmartCourt/Features/Contracts/ContractService.cs",
            source);

        Assert.Contains(violations, violation => violation.Rule == "CAP007");
    }

    [Fact]
    public void ExternalSdkRule_AllowsSdkInsideProviderImplementation()
    {
        var violations = Inspect(
            "SmartCourt/Providers/Payments/StripePaymentProvider.cs",
            """
            using Stripe;
            public sealed class StripePaymentProvider(PaymentIntentService payments);
            """);

        Assert.DoesNotContain(violations, violation => violation.Rule == "CAP005");
    }

    private static void AssertRuleDetected(
        string rule,
        string path,
        string source)
    {
        var violations = Inspect(path, source);

        Assert.Contains(violations, violation => violation.Rule == rule);
    }

    private static IReadOnlyList<ArchitectureViolation> Inspect(
        string path,
        string source)
    {
        return ContractAndPaymentArchitectureRules.Inspect(
            [new ArchitectureSourceFile(path, source)]);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SmartCourt.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the SmartCourt repository root.");
    }
}
