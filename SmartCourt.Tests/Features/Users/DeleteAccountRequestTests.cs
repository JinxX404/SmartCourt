using Microsoft.AspNetCore.Mvc;
using SmartCourt.Features.Users.Clients;
using SmartCourt.Features.Users.Lawyers;
using SmartCourt.Features.Users.Shared.DTOs;
using SmartCourt.Features.Users.Shared.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Users;

public sealed class DeleteAccountRequestTests
{
    [Fact]
    public void Validator_RejectsEmptyCurrentPassword()
    {
        var request = new DeleteAccountRequest(string.Empty);

        var result = new DeleteAccountRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.CurrentPassword));
    }

    [Fact]
    public void Validator_AcceptsCurrentPassword()
    {
        var request = new DeleteAccountRequest("CurrentPassword123");

        var result = new DeleteAccountRequestValidator().Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(typeof(ClientsController), nameof(ClientsController.DeleteAsync))]
    [InlineData(typeof(LawyersController), nameof(LawyersController.DeleteProfile))]
    public void DeleteEndpoint_RequiresDeleteAccountRequest(Type controllerType, string methodName)
    {
        var method = controllerType.GetMethod(methodName);

        Assert.NotNull(method);
        var requestParameter = Assert.Single(method.GetParameters(), parameter =>
            parameter.ParameterType == typeof(DeleteAccountRequest));
        Assert.NotNull(requestParameter.GetCustomAttributes(typeof(FromBodyAttribute), inherit: true).SingleOrDefault());
    }
}
