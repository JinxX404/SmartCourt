using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using SmartCourt.Common.Models;
using SmartCourt.Common.Validators;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts.Validators;
using Xunit;


namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractDtoValidatorTests
{
    [Fact]
    public void CreateRequest_EnforcesIdsAndTextBounds()
    {
        var validator = new CreateContractRequestValidator();
        var valid = new CreateContractRequest(
            Guid.NewGuid(),
            "A valid contract",
            new string('t', 20));

        Assert.True(validator.Validate(valid).IsValid);
        Assert.False(
            validator.Validate(
                new CreateContractRequest(
                    Guid.Empty,
                    new string('x', 2),
                    new string('t', 19))).IsValid);
        Assert.False(
            validator.Validate(
                new CreateContractRequest(
                    Guid.NewGuid(),
                    "Valid title",
                    new string('t', 20_001))).IsValid);
    }

    [Fact]
    public void UpdateAndTerminateRequests_EnforceBusinessTextBounds()
    {
        var updateValidator = new UpdateContractRequestValidator();
        Assert.True(
            updateValidator.Validate(
                new UpdateContractRequest(
                    "Updated title",
                    new string('t', 20))).IsValid);
        Assert.False(
            updateValidator.Validate(
                new UpdateContractRequest(
                    "No",
                    new string('t', 20))).IsValid);

        var terminateValidator = new TerminateContractRequestValidator();
        Assert.True(
            terminateValidator.Validate(
                new TerminateContractRequest("Reason")).IsValid);
        Assert.False(
            terminateValidator.Validate(
                new TerminateContractRequest(new string('r', 2_001))).IsValid);
    }

    [Fact]
    public void QueryValidators_EnforcePaginationAndDefinedEnum()
    {
        var validator = new ContractListQueryValidator();
        Assert.True(
            validator.Validate(
                new ContractListQuery(
                    ContractStatus.Active,
                    101,
                    100)).IsValid);
        Assert.False(
            validator.Validate(
                new ContractListQuery(
                    (ContractStatus)999,
                    1,
                    10)).IsValid);
        Assert.False(
            validator.Validate(
                new ContractListQuery(
                    null,
                    0,
                    10)).IsValid);
        Assert.False(
            validator.Validate(
                new ContractListQuery(
                    null,
                    1,
                    101)).IsValid);

        var historyValidator = new ContractStateHistoryQueryValidator();
        Assert.True(
            historyValidator.Validate(
                new ContractStateHistoryQuery(1, 100)).IsValid);
        Assert.False(
            historyValidator.Validate(
                new ContractStateHistoryQuery(1, 0)).IsValid);
    }

    [Theory]
    [InlineData("\"AQIDBA==\"", true)]
    [InlineData("\"AA==\"", true)]
    [InlineData("W/\"AQIDBA==\"", false)]
    [InlineData("*", false)]
    [InlineData("AQIDBA==", false)]
    [InlineData("\"not-base64\"", false)]
    public void IfMatchValidator_RequiresStrongQuotedBase64(
        string value,
        bool expectedValid)
    {
        var result = new IfMatchRequestValidator().Validate(
            new IfMatchRequest(value));

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void RequestDtos_DoNotExposeWritableStatusOrCalculatedTotal()
    {
        var requestProperties = typeof(CreateContractRequest)
            .GetProperties()
            .Concat(typeof(UpdateContractRequest).GetProperties())
            .Concat(typeof(TerminateContractRequest).GetProperties())
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain(nameof(ContractStatus), requestProperties);
        Assert.DoesNotContain("CurrentMilestoneTotal", requestProperties);
        Assert.DoesNotContain("TotalAmount", requestProperties);
        Assert.Empty(
            typeof(CreateContractRequest)
                .GetProperties()
                .SelectMany(property =>
                    property.GetCustomAttributes<ValidationAttribute>()));
    }

    [Fact]
    public void DetailDto_ContainsDerivedTotalAndPermittedActions()
    {
        var properties = typeof(ContractDetailDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains(nameof(ContractDetailDto.CurrentMilestoneTotal), properties);
        Assert.Contains(nameof(ContractDetailDto.PermittedActions), properties);
        Assert.DoesNotContain("TotalAmount", properties);
    }

    [Fact]
    public void ValidationErrors_AreWrittenInArabic()
    {
        var errors = new CreateContractRequestValidator()
            .Validate(
                new CreateContractRequest(
                    Guid.Empty,
                    string.Empty,
                    string.Empty))
            .Errors
            .Concat(
                new ContractListQueryValidator()
                    .Validate(
                        new ContractListQuery(
                            (ContractStatus)999,
                            0,
                            101))
                    .Errors)
            .Concat(
                new IfMatchRequestValidator()
                    .Validate(new IfMatchRequest("*"))
                    .Errors)
            .ToArray();

        Assert.NotEmpty(errors);
        Assert.All(
            errors,
            error => Assert.Matches(
                new Regex("[\\u0600-\\u06FF]"),
                error.ErrorMessage));
    }
}
