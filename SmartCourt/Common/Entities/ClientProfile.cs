using SmartCourt.Entities;

namespace SmartCourt.Common.Entities;

public class ClientProfile
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
