using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Shared;
using SmartCourt.Features.Consultations.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Consultations;

public sealed class ConsultationValidationTests
{
    [Theory]
    [InlineData(250, 37.5, 212.5)]
    [InlineData(999, 149.85, 849.15)]
    [InlineData(1999, 299.85, 1699.15)]
    public void SettlementUsesCurrentFifteenPercentPlatformFee(
        decimal gross,
        decimal expectedFee,
        decimal expectedNet)
    {
        var result = ConsultationPolicy.CalculateSettlement(gross);
        Assert.Equal(expectedFee, result.Fee);
        Assert.Equal(expectedNet, result.Net);
    }

    [Fact]
    public async Task SlotValidatorRejectsNonUtcAndDuplicateSlots()
    {
        var local = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Local);
        var request = new CreateConsultationSlotsRequest([new(local), new(local)]);
        var result = await new CreateConsultationSlotsRequestValidator().ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, item => item.ErrorMessage.Contains("UTC"));
        Assert.Contains(result.Errors, item => item.ErrorMessage.Contains("unique"));
    }

    [Theory]
    [InlineData("http://meet.example.com/room")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("not-a-url")]
    public async Task MarkPerformedValidatorRejectsUnsafeMeetingUrls(string meetingUrl)
    {
        var result = await new MarkConsultationPerformedRequestValidator()
            .ValidateAsync(new MarkConsultationPerformedRequest(meetingUrl));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, item => item.ErrorMessage.Contains("HTTPS"));
    }
}
