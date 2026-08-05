namespace SmartCourt.Features.Auth.ResendVerification;

public interface IResendVerificationService
{
    Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);
}
