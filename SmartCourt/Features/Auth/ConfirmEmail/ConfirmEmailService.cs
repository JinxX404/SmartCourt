using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Persistence;
using System.Text;

namespace SmartCourt.Features.Auth.ConfirmEmail;

public class ConfirmEmailService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) : IConfirmEmailService
{
    private const int MaximumUserIdLength = 64;
    private const int MaximumEncodedTokenLength = 2048;
    private const string InvalidConfirmationMessage =
        "رابط تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.";

    public async Task ConfirmEmailAsync(
        string? userId,
        string? token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)
            || userId.Length > MaximumUserIdLength
            || !Guid.TryParse(userId, out var parsedUserId)
            || string.IsNullOrWhiteSpace(token)
            || token.Length > MaximumEncodedTokenLength)
        {
            throw new BusinessException(InvalidConfirmationMessage);
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            throw new BusinessException(InvalidConfirmationMessage);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(parsedUserId.ToString());
        if (user is null)
        {
            throw new BusinessException(InvalidConfirmationMessage);
        }

        var roles = await userManager.GetRolesAsync(user);
        var expectedStatus = roles.Contains("Client")
            ? UserStatus.Active
            : roles.Contains("Lawyer")
                ? UserStatus.PendingReview
                : (UserStatus?)null;

        if (expectedStatus is null)
        {
            throw new BusinessException(InvalidConfirmationMessage);
        }

        if (user.EmailConfirmed && user.Status == expectedStatus)
        {
            return;
        }

        if (user.EmailConfirmed || user.Status != UserStatus.Unverified)
        {
            throw new BusinessException(InvalidConfirmationMessage);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var confirmationResult = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!confirmationResult.Succeeded)
            {
                throw new BusinessException(InvalidConfirmationMessage);
            }

            user.Status = expectedStatus.Value;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new BusinessException(InvalidConfirmationMessage);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ConfirmEmailChangeAsync(string userId, string newEmail, string token, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAndValidateAsync(userId);

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            throw new BusinessException("رمز تأكيد تغيير البريد الإلكتروني غير صالح أو مشوه.");
        }
        var result = await userManager.ChangeEmailAsync(user, newEmail, decodedToken);
        
        if (!result.Succeeded)
        {
            throw new BusinessException("رمز تغيير البريد الإلكتروني غير صالح أو منتهي الصلاحية.");
        }

        var setUserNameResult = await userManager.SetUserNameAsync(user, newEmail);
        if (!setUserNameResult.Succeeded)
        {
            throw new BusinessException(string.Join(" ", setUserNameResult.Errors.Select(e => e.Description)));
        }
    }

    private async Task<ApplicationUser> GetUserAndValidateAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("المستخدم غير موجود.");
        }

        return user;
    }
}
