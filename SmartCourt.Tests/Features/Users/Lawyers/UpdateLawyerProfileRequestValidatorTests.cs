using SmartCourt.Common.Enums;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Features.Users.Lawyers.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Users.Lawyers;

public sealed class UpdateLawyerProfileRequestValidatorTests
{
    [Fact]
    public void Validator_RejectsUndefinedLevel()
    {
        var request = CreateRequest();
        request.Level = (LawyerLevel)127;

        var result = new UpdateLawyerProfileRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Level));
    }



    private static UpdateLawyerProfileRequest CreateRequest()
    {
        return new UpdateLawyerProfileRequest
        {
            PhoneNumber = "+201012345678",
            Level = LawyerLevel.GeneralRegistration,
            Address = "Cairo"
        };
    }
}
