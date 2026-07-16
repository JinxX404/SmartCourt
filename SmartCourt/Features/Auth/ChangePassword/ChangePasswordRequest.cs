namespace SmartCourt.Features.Auth.ChangePassword;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);
