namespace SmartCourt.Features.Auth.Login.DTOs;

public record UserDto(
    string Id,
    string Email,
    string FullName,
    string Role,
    string Status
);
