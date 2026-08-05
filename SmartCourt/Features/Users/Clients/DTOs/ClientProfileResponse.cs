using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Clients.DTOs;

public class ClientProfileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = string.Empty;
}
