namespace SmartCourt.Features.Auth;

public class JwtOptions
{
    public string? Secret { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int ExpiresInMinutes { get; set; } = 60;
}
