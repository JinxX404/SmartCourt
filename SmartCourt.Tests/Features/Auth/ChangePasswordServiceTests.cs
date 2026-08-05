using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Extensions;
using System.Security.Claims;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class ChangePasswordServiceTests
{
    private const string CurrentPassword = PasswordServiceTestContext.CurrentPassword;
    private const string NewPassword = "NewPassword456!";

    [Fact]
    public async Task WrongCurrentPassword_ChangesNothing()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var originalSecurityStamp = user.SecurityStamp;

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            testContext.CreateChangePasswordService().ChangePasswordAsync(
                "WrongPassword123!",
                NewPassword,
                CancellationToken.None));

        Assert.Contains("CurrentPassword", exception.Errors.Keys);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.True(await testContext.UserManager.CheckPasswordAsync(storedUser, CurrentPassword));
        Assert.False(await testContext.UserManager.CheckPasswordAsync(storedUser, NewPassword));
        Assert.All(storedUser.RefreshTokens, token => Assert.True(token.IsActive));
    }

    [Fact]
    public async Task PasswordReuse_ChangesNothing()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var originalSecurityStamp = user.SecurityStamp;

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            testContext.CreateChangePasswordService().ChangePasswordAsync(
                CurrentPassword,
                CurrentPassword,
                CancellationToken.None));

        Assert.Contains("NewPassword", exception.Errors.Keys);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.True(await testContext.UserManager.CheckPasswordAsync(storedUser, CurrentPassword));
        Assert.All(storedUser.RefreshTokens, token => Assert.True(token.IsActive));
    }

    [Fact]
    public async Task Success_ChangesPasswordAndRevokesSessions()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var originalSecurityStamp = user.SecurityStamp;
        var originalPrincipal = CreatePrincipal(originalSecurityStamp!);

        await testContext.CreateChangePasswordService().ChangePasswordAsync(
            CurrentPassword,
            NewPassword,
            CancellationToken.None);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.NotEqual(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.False(storedUser.HasValidSecurityStamp(originalPrincipal));
        Assert.False(await testContext.UserManager.CheckPasswordAsync(storedUser, CurrentPassword));
        Assert.True(await testContext.UserManager.CheckPasswordAsync(storedUser, NewPassword));
        Assert.All(storedUser.RefreshTokens, token => Assert.False(token.IsActive));
    }

    [Fact]
    public async Task FinalUpdateFailure_RollsBackPasswordAndSessionChanges()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var originalSecurityStamp = user.SecurityStamp;
        testContext.UserManager.FailExplicitUpdate = true;

        await Assert.ThrowsAsync<BusinessException>(() =>
            testContext.CreateChangePasswordService().ChangePasswordAsync(
                CurrentPassword,
                NewPassword,
                CancellationToken.None));

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.True(await testContext.UserManager.CheckPasswordAsync(storedUser, CurrentPassword));
        Assert.False(await testContext.UserManager.CheckPasswordAsync(storedUser, NewPassword));
        Assert.All(storedUser.RefreshTokens, token => Assert.True(token.IsActive));
    }

    private static ClaimsPrincipal CreatePrincipal(string securityStamp)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ApplicationUserExtensions.SecurityStampClaimType, securityStamp)
        ]));
    }
}
