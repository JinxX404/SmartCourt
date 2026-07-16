namespace SmartCourt.Features.Auth;

public class ClientProfile
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
