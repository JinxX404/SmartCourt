namespace SmartCourt.Features.Auth.ConfirmEmail.DTOs;

public class VerifyEmailRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
