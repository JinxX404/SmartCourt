namespace SmartCourt.Common.RateLimiting;

public interface IAccountKeyRateLimiter
{
    void CheckForgotPassword(string email);
    void CheckResendVerification(string email);
    void CheckResetPassword(string email, string token);
    void CheckConfirmEmail(string userId);
}
