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
    private const string InvalidConfirmationMessage =
        "رابط تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.";

    public async Task ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId)
            || !Guid.TryParse(userId, out var parsedUserId)
            || string.IsNullOrWhiteSpace(token))
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

        if (user.EmailConfirmed)
        {
            throw new BusinessException("الحساب مفعل مسبقاً. يرجى التوجه لصفحة تسجيل الدخول.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var confirmationResult = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!confirmationResult.Succeeded)
            {
                throw new BusinessException(InvalidConfirmationMessage);
            }


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

}
