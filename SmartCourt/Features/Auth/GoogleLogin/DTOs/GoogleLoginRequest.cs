namespace SmartCourt.Features.Auth.GoogleLogin.DTOs;

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
    public string? Role { get; set; } // Optional: used when user is new and selects a role
}
