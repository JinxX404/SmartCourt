using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Clients.DTOs;

public class CompleteClientProfileRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? NationalNumber { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
}
