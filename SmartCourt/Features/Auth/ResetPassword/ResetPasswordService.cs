using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Extensions;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Persistence;
using System.Text;

namespace SmartCourt.Features.Auth.ResetPassword;

public class ResetPasswordService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IAuthHelperService authHelperService) : IResetPasswordService
{
    private const int MaximumEncodedTokenLength = 2048;
    private const string InvalidResetMessage = "رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.";

    public async Task ResetPasswordAsync(
        string email,
        string? token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !user.IsAccessEligible())
        {
            throw new BusinessException(InvalidResetMessage);
        }

        if (string.IsNullOrWhiteSpace(token) || token.Length > MaximumEncodedTokenLength)
        {
            throw new BusinessException(InvalidResetMessage);
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            throw new BusinessException(InvalidResetMessage);
        }

        await dbContext.Entry(user)
            .Collection(applicationUser => applicationUser.RefreshTokens)
            .LoadAsync(cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var resetResult = await userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (!resetResult.Succeeded)
            {
                if (resetResult.Errors.Any(error => error.Code == "InvalidToken"))
                {
                    throw new BusinessException(InvalidResetMessage);
                }

                throw new ValidationException(
                    "NewPassword",
                    string.Join(" ", resetResult.Errors.Select(error => error.Description)));
            }

            authHelperService.RevokeAllActiveRefreshTokens(user);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new BusinessException(
                    string.Join(" ", updateResult.Errors.Select(error => error.Description)));
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
