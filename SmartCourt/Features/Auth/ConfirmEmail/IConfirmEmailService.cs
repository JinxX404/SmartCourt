namespace SmartCourt.Features.Auth.ConfirmEmail;

public interface IConfirmEmailService
{
    Task ConfirmEmailAsync(string? userId, string? token, CancellationToken cancellationToken = default);
    Task ConfirmEmailChangeAsync(string userId, string newEmail, string token, CancellationToken cancellationToken = default);
}
