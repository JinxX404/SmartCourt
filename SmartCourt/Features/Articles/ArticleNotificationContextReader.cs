using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;

namespace SmartCourt.Features.Articles;

internal sealed class ArticleNotificationContextReader : IArticleNotificationContextReader
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ArticleNotificationContextReader(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyCollection<Guid>> GetAdminUserIdsAsync(CancellationToken cancellationToken)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        return admins.Select(a => a.Id).ToList();
    }
}
