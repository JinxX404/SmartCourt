using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Users.Clients;
using SmartCourt.Features.Users.Clients.DTOs;
using SmartCourt.Features.Users.Lawyers;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Features.Users.Shared.DTOs;
using Xunit;

namespace SmartCourt.Tests.Features.Users;

public sealed class MutationResponseTests
{
    [Fact]
    public async Task ClientUpdate_ReturnsMessageOnlyApiResponse()
    {
        var result = await new ClientsController(new TestClientService()).UpdateAsync(
            new UpdateClientProfileRequest(),
            CancellationToken.None);

        AssertMessageOnlyResponse(result);
    }

    [Fact]
    public async Task ClientDelete_ReturnsMessageOnlyApiResponse()
    {
        var result = await new ClientsController(new TestClientService()).DeleteAsync(
            new DeleteAccountRequest("password"),
            CancellationToken.None);

        AssertMessageOnlyResponse(result);
    }

    [Fact]
    public async Task LawyerUpdate_ReturnsMessageOnlyApiResponse()
    {
        var result = await new LawyersController(new TestLawyerService()).UpdateProfile(
            new UpdateLawyerProfileRequest(),
            CancellationToken.None);

        AssertMessageOnlyResponse(result);
    }

    [Fact]
    public async Task LawyerDelete_ReturnsMessageOnlyApiResponse()
    {
        var result = await new LawyersController(new TestLawyerService()).DeleteProfile(
            new DeleteAccountRequest("password"),
            CancellationToken.None);

        AssertMessageOnlyResponse(result);
    }

    private static void AssertMessageOnlyResponse(IActionResult actionResult)
    {
        var response = Assert.IsType<OkObjectResult>(actionResult).Value;
        Assert.IsType<ApiResponse>(response);
        Assert.IsNotType<ApiResponse<string>>(response);
        Assert.True(((ApiResponse)response).Success);
        Assert.False(string.IsNullOrWhiteSpace(((ApiResponse)response).Message));
    }

    private sealed class TestClientService : IClientService
    {
        public Task<ClientProfileResponse> GetProfileAsync(CancellationToken cancellationToken)
            => Task.FromResult(new ClientProfileResponse());

        public Task CompleteProfileAsync(CompleteClientProfileRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task UpdateProfileAsync(UpdateClientProfileRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task DeleteProfileAsync(DeleteAccountRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class TestLawyerService : ILawyerService
    {
        public Task<LawyerProfileResponse> GetProfileAsync(CancellationToken cancellationToken)
            => Task.FromResult(new LawyerProfileResponse());

        public Task CompleteProfileAsync(CompleteLawyerProfileRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<PublicLawyerProfileResponse> GetPublicProfileAsync(Guid lawyerId, CancellationToken cancellationToken)
            => Task.FromResult(new PublicLawyerProfileResponse());

        public Task UpdateProfileAsync(UpdateLawyerProfileRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task DeleteProfileAsync(DeleteAccountRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
