namespace SmartCourt.Interfaces;

public interface IAuthService
{
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string email, string token, string newPassword);
    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task ResendVerificationEmailAsync(string email);
}
