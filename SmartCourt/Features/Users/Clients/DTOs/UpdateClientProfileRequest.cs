using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Clients.DTOs;

public class UpdateClientProfileRequest
{
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? NationalNumber { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
}
