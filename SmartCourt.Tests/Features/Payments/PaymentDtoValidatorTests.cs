using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class PaymentDtoValidatorTests
{
    [Fact]
    public void ValidFundingAndWithdrawalRequestsPass()
    {
        var funding = new FundMilestoneRequest("mock-card-success");
        var withdrawal = new CreateWithdrawalRequest(
            1_000.25m,
            "mock-bank-account");

        Assert.True(
            new FundMilestoneRequestValidator().Validate(funding).IsValid);
        Assert.True(
            new CreateWithdrawalRequestValidator()
                .Validate(withdrawal)
                .IsValid);
    }

    [Fact]
    public void FundingAndWithdrawalRequestsRejectInvalidValues()
    {
        var fundingResult = new FundMilestoneRequestValidator().Validate(
            new FundMilestoneRequest(" "));
        var withdrawalResult =
            new CreateWithdrawalRequestValidator().Validate(
                new CreateWithdrawalRequest(10.001m, " "));

        Assert.Contains(
            fundingResult.Errors,
            error => error.PropertyName
                == nameof(FundMilestoneRequest.PaymentMethodReference));
        Assert.Contains(
            withdrawalResult.Errors,
            error => error.PropertyName
                == nameof(CreateWithdrawalRequest.Amount));
        Assert.Contains(
            withdrawalResult.Errors,
            error => error.PropertyName
                == nameof(CreateWithdrawalRequest.DestinationReference));
    }

    [Fact]
    public void WebhookValidatorRequiresTrustedShapeAndEgp()
    {
        var result = new PaymentWebhookRequestValidator().Validate(
            new PaymentWebhookRequest(
                string.Empty,
                Guid.Empty,
                string.Empty,
                (PaymentTransactionStatus)99,
                0.001m,
                "USD",
                DateTime.SpecifyKind(
                    DateTime.Now,
                    DateTimeKind.Local),
                null));

        Assert.Contains(
            result.Errors,
            error => error.PropertyName
                == nameof(PaymentWebhookRequest.EventId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName
                == nameof(PaymentWebhookRequest.PaymentTransactionId));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName
                == nameof(PaymentWebhookRequest.Currency));
        Assert.NotEmpty(result.Errors);
        Assert.All(
            result.Errors,
            error => Assert.Matches(
                "[\\u0600-\\u06FF]",
                error.ErrorMessage));
    }

    [Fact]
    public void ResponseDtosDoNotExposeSensitivePaymentReferences()
    {
        var responseTypes = new[]
        {
            typeof(PaymentDto),
            typeof(PaymentAttemptDto),
            typeof(EscrowLedgerEntryDto),
            typeof(PaymentHistoryDto),
            typeof(WalletDto),
            typeof(PaymentActionResultDto)
        };
        var forbiddenNames = new[]
        {
            "PaymentMethodReference",
            "DestinationReference",
            "ProviderTransactionId",
            "IdempotencyKey"
        };

        foreach (var responseType in responseTypes)
        {
            foreach (var forbiddenName in forbiddenNames)
            {
                Assert.Null(responseType.GetProperty(forbiddenName));
            }
        }
    }
}
