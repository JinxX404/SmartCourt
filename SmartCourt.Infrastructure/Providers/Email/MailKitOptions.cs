namespace SmartCourt.Infrastructure.Providers.Email;

public class MailKitOptions
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}
