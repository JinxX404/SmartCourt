namespace SmartCourt.Features.Auth.Shared;

public sealed class AuthEmailOptions
{
    public const string SectionName = "AuthEmail";

    public string PublicBaseUrl { get; set; } = string.Empty;
}
