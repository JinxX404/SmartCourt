namespace SmartCourt.Features.Users.Clients.DTOs;

public class UpdateClientProfileRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; }
}
