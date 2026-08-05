using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class AdminWalletAdjustmentValidatorTests
{
    [Fact]
    public async Task Validator_RequiresNonZeroPreciseDeltaAndReason()
    {
        var validator = new AdminWalletAdjustmentRequestValidator();

        var invalid = await validator.ValidateAsync(
            new AdminWalletAdjustmentRequest(
                Guid.Empty,
                0m,
                0m,
                "قصير"));
        var imprecise = await validator.ValidateAsync(
            new AdminWalletAdjustmentRequest(
                Guid.NewGuid(),
                0.001m,
                0m,
                "سبب إداري واضح ومفصل لتصحيح الرصيد المالي."));

        Assert.False(invalid.IsValid);
        Assert.False(imprecise.IsValid);
        Assert.Contains(
            imprecise.Errors,
            error => error.PropertyName
                == nameof(AdminWalletAdjustmentRequest.PendingBalanceDelta));
    }
}
