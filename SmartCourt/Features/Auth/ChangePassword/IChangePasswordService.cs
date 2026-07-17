namespace SmartCourt.Features.Auth.ChangePassword;

public interface IChangePasswordService
{
    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
