namespace SmartCourt.Features.Auth;

public class LawyerProfile
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Bio { get; set; }
    public string? Address { get; set; }
}
