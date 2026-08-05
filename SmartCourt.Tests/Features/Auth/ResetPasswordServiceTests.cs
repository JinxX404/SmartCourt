using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Extensions;
using SmartCourt.Features.Auth.Enums;
using System.Security.Claims;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class ResetPasswordServiceTests
{
    private const string NewPassword = "NewPassword456!";
    private const string InvalidResetMessage =
        "رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.";

    [Fact]
    public async Task Success_UsesNormalizedEmailAndRevokesSessions()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var token = await testContext.GenerateEncodedResetTokenAsync(user);
        var originalSecurityStamp = user.SecurityStamp;
        var originalPrincipal = CreatePrincipal(originalSecurityStamp!);

        await testContext.CreateResetPasswordService().ResetPasswordAsync(
            user.Email!.ToUpperInvariant(),
            token,
            NewPassword,
            CancellationToken.None);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.NotEqual(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.False(storedUser.HasValidSecurityStamp(originalPrincipal));
        Assert.False(await testContext.UserManager.CheckPasswordAsync(
            storedUser,
            PasswordServiceTestContext.CurrentPassword));
        Assert.True(await testContext.UserManager.CheckPasswordAsync(storedUser, NewPassword));
        Assert.All(storedUser.RefreshTokens, refreshToken => Assert.False(refreshToken.IsActive));
    }

    [Fact]
    public async Task NonexistentAndMalformedRequests_ReturnSameGenericError()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var service = testContext.CreateResetPasswordService();

        var nonexistentError = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ResetPasswordAsync(
                "missing@example.com",
                "invalid-token",
                NewPassword,
                CancellationToken.None));
        var malformedError = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ResetPasswordAsync(
                user.Email!,
                "%%%",
                NewPassword,
                CancellationToken.None));
        var missingError = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ResetPasswordAsync(
                user.Email!,
                null,
                NewPassword,
                CancellationToken.None));
        var oversizedError = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ResetPasswordAsync(
                user.Email!,
                new string('a', 2049),
                NewPassword,
                CancellationToken.None));

        Assert.Equal(InvalidResetMessage, nonexistentError.Message);
        Assert.Equal(nonexistentError.Message, malformedError.Message);
        Assert.Equal(nonexistentError.Message, missingError.Message);
        Assert.Equal(nonexistentError.Message, oversizedError.Message);
    }

    [Theory]
    [InlineData(UserStatus.Unverified, false)]
    [InlineData(UserStatus.Unverified, true)]
    [InlineData(UserStatus.Suspended, true)]
    [InlineData(UserStatus.Rejected, true)]
    [InlineData(UserStatus.Deleted, true)]
    public async Task DisallowedAccount_ReturnsGenericError(
        UserStatus status,
        bool emailConfirmed)
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(status, emailConfirmed);
        var token = await testContext.GenerateEncodedResetTokenAsync(user);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            testContext.CreateResetPasswordService().ResetPasswordAsync(
                user.Email!,
                token,
                NewPassword,
                CancellationToken.None));

        Assert.Equal(InvalidResetMessage, exception.Message);
    }

    [Fact]
    public async Task ExpiredToken_ReturnsGenericError()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync(TimeSpan.Zero);
        var user = await testContext.CreateUserAsync();
        var token = await testContext.GenerateEncodedResetTokenAsync(user);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            testContext.CreateResetPasswordService().ResetPasswordAsync(
                user.Email!,
                token,
                NewPassword,
                CancellationToken.None));

        Assert.Equal(InvalidResetMessage, exception.Message);
    }

    [Fact]
    public async Task ReplayedToken_ReturnsGenericError()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var token = await testContext.GenerateEncodedResetTokenAsync(user);
        var service = testContext.CreateResetPasswordService();

        await service.ResetPasswordAsync(
            user.Email!,
            token,
            NewPassword,
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ResetPasswordAsync(
                user.Email!,
                token,
                "AnotherPassword789!",
                CancellationToken.None));

        Assert.Equal(InvalidResetMessage, exception.Message);
    }

    [Fact]
    public async Task InvalidNewPassword_ChangesNothing()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var token = await testContext.GenerateEncodedResetTokenAsync(user);
        var originalSecurityStamp = user.SecurityStamp;

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            testContext.CreateResetPasswordService().ResetPasswordAsync(
                user.Email!,
                token,
                "weak",
                CancellationToken.None));

        Assert.Contains("NewPassword", exception.Errors.Keys);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.True(await testContext.UserManager.CheckPasswordAsync(
            storedUser,
            PasswordServiceTestContext.CurrentPassword));
        Assert.All(storedUser.RefreshTokens, refreshToken => Assert.True(refreshToken.IsActive));
    }

    [Fact]
    public async Task FinalUpdateFailure_RollsBackPasswordAndSessionChanges()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var token = await testContext.GenerateEncodedResetTokenAsync(user);
        var originalSecurityStamp = user.SecurityStamp;
        testContext.UserManager.FailExplicitUpdate = true;

        await Assert.ThrowsAsync<BusinessException>(() =>
            testContext.CreateResetPasswordService().ResetPasswordAsync(
                user.Email!,
                token,
                NewPassword,
                CancellationToken.None));

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.True(await testContext.UserManager.CheckPasswordAsync(
            storedUser,
            PasswordServiceTestContext.CurrentPassword));
        Assert.False(await testContext.UserManager.CheckPasswordAsync(storedUser, NewPassword));
        Assert.All(storedUser.RefreshTokens, refreshToken => Assert.True(refreshToken.IsActive));
    }

    private static ClaimsPrincipal CreatePrincipal(string securityStamp)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ApplicationUserExtensions.SecurityStampClaimType, securityStamp)
        ]));
    }
}
