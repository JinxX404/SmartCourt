namespace SmartCourt.Features.Auth.ResetPassword;

public interface IResetPasswordService
{
    Task ResetPasswordAsync(string email, string? token, string newPassword, CancellationToken cancellationToken);
}
