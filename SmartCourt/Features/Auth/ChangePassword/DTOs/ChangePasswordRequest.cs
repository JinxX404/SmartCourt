namespace SmartCourt.Features.Auth.ChangePassword.DTOs;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);
