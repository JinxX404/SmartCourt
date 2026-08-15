using Microsoft.EntityFrameworkCore;

namespace SmartCourt.Common.Entities;

[Owned]
public class RefreshToken
{
    public string HashedToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresOn { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedOn { get; set; }
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresOn;
    public bool IsActive => RevokedOn is null && !IsExpired;
}
