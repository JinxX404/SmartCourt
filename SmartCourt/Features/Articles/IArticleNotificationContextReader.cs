namespace SmartCourt.Features.Articles;

public interface IArticleNotificationContextReader
{
    Task<IReadOnlyCollection<Guid>> GetAdminUserIdsAsync(CancellationToken cancellationToken);
}
