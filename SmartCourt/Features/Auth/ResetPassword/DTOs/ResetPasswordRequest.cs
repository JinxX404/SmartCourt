namespace SmartCourt.Features.Auth.ResetPassword.DTOs;

public record ResetPasswordRequest(
    string Email,
    string? Token,
    string NewPassword,
    string ConfirmNewPassword
);
