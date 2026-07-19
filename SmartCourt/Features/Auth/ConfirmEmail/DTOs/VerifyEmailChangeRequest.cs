namespace SmartCourt.Features.Auth.ConfirmEmail.DTOs;

public class VerifyEmailChangeRequest
{
    public string UserId { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
