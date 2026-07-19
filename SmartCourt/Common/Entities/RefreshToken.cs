using Microsoft.EntityFrameworkCore;

namespace SmartCourt.Common.Entities;

[Owned]
public class RefreshToken
{
    public string HashedToken { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOn { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsActive => RevokedOn is null && !IsExpired;
}
