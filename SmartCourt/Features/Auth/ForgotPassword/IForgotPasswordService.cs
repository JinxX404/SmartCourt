namespace SmartCourt.Features.Auth.ForgotPassword;

public interface IForgotPasswordService
{
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
}
