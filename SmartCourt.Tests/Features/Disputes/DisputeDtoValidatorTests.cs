using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Disputes.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Disputes;

public sealed class DisputeDtoValidatorTests
{
    [Fact]
    public async Task CreateValidator_RejectsInvalidArabicFacingRequest()
    {
        var validator = new CreateDisputeRequestValidator();
        var result = await validator.ValidateAsync(new CreateDisputeRequest(
            Guid.Empty,
            (DisputeCategory)999,
            "س",
            string.Empty,
            (DisputeRequestedOutcome)999,
            [Guid.Empty]));

        Assert.False(result.IsValid);
        Assert.All(result.Errors, error => Assert.Matches("[\u0600-\u06FF]", error.ErrorMessage));
    }

    [Fact]
    public async Task ResolveValidator_RequiresAmountsMatchingOutcomeShape()
    {
        var validator = new ResolveDisputeRequestValidator();
        var invalid = await validator.ValidateAsync(new ResolveDisputeRequest(
            DisputeResolutionType.FullRefund,
            1_000m,
            100m,
            "قرار موضح للنزاع"));
        var valid = await validator.ValidateAsync(new ResolveDisputeRequest(
            DisputeResolutionType.PartialSplit,
            500m,
            475m,
            "قرار موضح للنزاع"));

        Assert.False(invalid.IsValid);
        Assert.True(valid.IsValid);
    }

    [Fact]
    public async Task EvidenceValidator_RequiresTextOrFile()
    {
        var validator = new AddDisputeEvidenceRequestValidator();
        var result = await validator.ValidateAsync(
            new AddDisputeEvidenceRequest(null, []));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("دليل"));
    }
}
