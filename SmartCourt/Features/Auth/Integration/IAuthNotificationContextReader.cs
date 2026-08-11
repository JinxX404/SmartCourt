namespace SmartCourt.Features.Auth.Integration;

public interface IAuthNotificationContextReader
{
    Task<AuthAccountNotificationContext> GetAccountAsync(
        Guid userId,
        CancellationToken cancellationToken);
}

public sealed record AuthAccountNotificationContext(Guid UserId);
