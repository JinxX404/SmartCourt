namespace SmartCourt.Features.Auth.ResetPassword;

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword
);
