namespace SmartCourt.Features.Auth.PhoneVerification.DTOs;

public class ConfirmPhoneVerificationRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
