namespace SmartCourt.Features.Auth.ConfirmEmail;

public interface IConfirmEmailService
{
    Task ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default);
}
