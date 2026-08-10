using System.Text.Json;
using SmartCourt.Features.Users.Clients.DTOs;
using SmartCourt.Features.Users.Lawyers.DTOs;
using Xunit;

namespace SmartCourt.Tests.Features.Users;

public sealed class ProfileEmailContractTests
{
    [Theory]
    [InlineData(typeof(UpdateClientProfileRequest))]
    [InlineData(typeof(UpdateLawyerProfileRequest))]
    public void UpdateProfileRequest_DoesNotExposeEmail(Type requestType)
    {
        Assert.Null(requestType.GetProperty("Email"));
    }

    [Theory]
    [InlineData(typeof(ClientProfileResponse))]
    [InlineData(typeof(LawyerProfileResponse))]
    public void ProfileResponse_RetainsEmail(Type responseType)
    {
        Assert.NotNull(responseType.GetProperty("Email"));
    }

    [Fact]
    public void ClientUpdate_IgnoresUnknownEmailField()
    {
        const string json = """
            {
              "email": "attacker@example.com",
              "phoneNumber": "+201012345678",
              "dateOfBirth": "1990-01-01",
              "address": "Cairo"
            }
            """;

        var request = JsonSerializer.Deserialize<UpdateClientProfileRequest>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(request);
        Assert.Equal(new DateOnly(1990, 1, 1), request.DateOfBirth);
        Assert.Equal("Cairo", request.Address);
    }

    [Fact]
    public void LawyerUpdate_IgnoresUnknownEmailField()
    {
        var json = """
            {
              "email": "attacker@example.com",
              "phoneNumber": "+201012345678",
              "dateOfBirth": "1990-01-01",
              "level": 0,
              "bio": "Bio",
              "address": "Cairo"
            }
            """;

        var request = JsonSerializer.Deserialize<UpdateLawyerProfileRequest>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(request);

        Assert.Equal("Cairo", request.Address);
    }
}
