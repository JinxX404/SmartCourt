using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Events;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Auth.ChangePassword;

public class ChangePasswordService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IAuthHelperService authHelperService,
    ICurrentUserService currentUserService,
    IOutboxWriter outboxWriter) : IChangePasswordService
{
    public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException("المستخدم غير معروف");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (!await userManager.CheckPasswordAsync(user, currentPassword))
            {
                throw new ValidationException("CurrentPassword", "كلمة المرور الحالية غير صحيحة.");
            }

            if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
            {
                throw new ValidationException(
                    "NewPassword",
                    "يجب أن تختلف كلمة المرور الجديدة عن كلمة المرور الحالية.");
            }

            var changeResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            EnsurePasswordChangeSucceeded(changeResult);

            authHelperService.RevokeAllActiveRefreshTokens(user);

            await AuthOutbox.EnqueuePasswordChangedAsync(
                outboxWriter,
                user.Id,
                Guid.NewGuid(),
                cancellationToken);

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

    private static void EnsurePasswordChangeSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var failures = new List<KeyValuePair<string, string[]>>();
        var passwordMismatchError = result.Errors.FirstOrDefault(error => error.Code == "PasswordMismatch");

        if (passwordMismatchError is not null)
        {
            failures.Add(new KeyValuePair<string, string[]>(
                "CurrentPassword",
                ["كلمة المرور الحالية غير صحيحة."]));
        }

        var otherErrors = result.Errors
            .Where(error => error.Code != "PasswordMismatch")
            .Select(error => error.Description)
            .ToArray();

        if (otherErrors.Length > 0)
        {
            failures.Add(new KeyValuePair<string, string[]>("NewPassword", otherErrors));
        }

        throw new ValidationException(failures);
    }
}
