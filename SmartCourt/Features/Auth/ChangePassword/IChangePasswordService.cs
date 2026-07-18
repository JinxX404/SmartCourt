namespace SmartCourt.Features.Auth.ChangePassword;

public interface IChangePasswordService
{
    Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken);
}
