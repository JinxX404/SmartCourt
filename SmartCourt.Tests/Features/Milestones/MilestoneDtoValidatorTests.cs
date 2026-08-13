using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Milestones.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestoneDtoValidatorTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(2026, 7, 28, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddMilestone_ValidRequestPasses()
    {
        var request = new AddMilestoneRequest(
            "Initial filing",
            "Prepare and file the claim.",
            1,
            5_000.25m,
            14,
            CurrentTime.AddDays(14).UtcDateTime);

        var result = new AddMilestoneRequestValidator(
            new FixedTimeProvider(CurrentTime)).Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void AddExpense_RequiresNullDeliverablesAndDuration()
    {
        var valid = new AddMilestoneRequest(
            "Court fee",
            null,
            null,
            1,
            500m,
            null,
            CurrentTime.AddDays(2).UtcDateTime,
            MilestoneType.Expense);
        var invalid = valid with
        {
            Deliverables = ["Receipt"],
            DurationDays = 1
        };

        Assert.True(CreateAddValidator().Validate(valid).IsValid);
        var result = CreateAddValidator().Validate(invalid);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(AddMilestoneRequest.Deliverables));
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(AddMilestoneRequest.DurationDays));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10.001)]
    public void AddMilestone_InvalidAmountFails(decimal amount)
    {
        var request = ValidAddRequest() with { Amount = amount };

        var result = CreateAddValidator().Validate(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.Amount));
    }

    [Fact]
    public void AddMilestone_InvalidOrderDurationAndDueDateFail()
    {
        var request = ValidAddRequest() with
        {
            OrderNumber = 0,
            DurationDays = 366,
            DueDate = CurrentTime.AddMinutes(-1).UtcDateTime
        };

        var result = CreateAddValidator().Validate(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.OrderNumber));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.DurationDays));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.DueDate));
    }

    [Fact]
    public void SubmitMilestone_RequiresNotesAndValidUniqueFileIds()
    {
        var fileId = Guid.NewGuid();
        var request = new SubmitMilestoneRequest(
            string.Empty,
            [fileId, fileId, Guid.Empty]);

        var result = new SubmitMilestoneRequestValidator().Validate(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.Notes));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.StoredFileIds));
    }

    [Fact]
    public void CreateChangeRequest_RequiresAnActualValidChange()
    {
        var validator = new CreateMilestoneChangeRequestValidator(
            new FixedTimeProvider(CurrentTime));
        var emptyRequest = new CreateMilestoneChangeRequest(
            null,
            null,
            null,
            "Waiting for court papers.");
        var invalidRequest = emptyRequest with
        {
            ProposedDurationDays = 0,
            ProposedDueDate = CurrentTime.AddDays(-1).UtcDateTime
        };

        var emptyResult = validator.Validate(emptyRequest);
        var invalidResult = validator.Validate(invalidRequest);

        Assert.Contains(
            emptyResult.Errors,
            error => error.PropertyName == string.Empty);
        Assert.Contains(
            invalidResult.Errors,
            error =>
                error.PropertyName
                == nameof(invalidRequest.ProposedDurationDays));
        Assert.Contains(
            invalidResult.Errors,
            error =>
                error.PropertyName
                == nameof(invalidRequest.ProposedDueDate));
    }

    [Fact]
    public void RequestDtos_DoNotExposeServerOwnedOrFundedAmountFields()
    {
        var requestTypes = new[]
        {
            typeof(AddMilestoneRequest),
            typeof(UpdateMilestoneRequest),
            typeof(SubmitMilestoneRequest),
            typeof(RequestMilestoneChangesRequest),
            typeof(CreateMilestoneChangeRequest),
            typeof(RejectChangeRequest)
        };
        var forbiddenProperties = new[]
        {
            "FundingStatus",
            "FundedAt",
            "EscrowHoldId",
            "AutoAcceptEligibleAt",
            "PlatformFee",
            "AcceptanceSource",
            "CreatedAt",
            "UpdatedAt"
        };

        foreach (var requestType in requestTypes)
        {
            foreach (var property in forbiddenProperties)
            {
                Assert.Null(requestType.GetProperty(property));
            }
        }

        Assert.Null(
            typeof(CreateMilestoneChangeRequest).GetProperty("Amount"));
    }

    [Fact]
    public void AllValidationMessagesAreArabic()
    {
        var result = CreateAddValidator().Validate(
            new AddMilestoneRequest(
                string.Empty,
                " ",
                0,
                0,
                0,
                CurrentTime.AddDays(-1).UtcDateTime));

        Assert.NotEmpty(result.Errors);
        Assert.All(
            result.Errors,
            error => Assert.Matches(
                "[\\u0600-\\u06FF]",
                error.ErrorMessage));
    }

    private static AddMilestoneRequest ValidAddRequest()
    {
        return new AddMilestoneRequest(
            "Initial filing",
            null,
            1,
            1_000m,
            30,
            CurrentTime.AddDays(30).UtcDateTime);
    }

    private static AddMilestoneRequestValidator CreateAddValidator()
        => new(new FixedTimeProvider(CurrentTime));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
