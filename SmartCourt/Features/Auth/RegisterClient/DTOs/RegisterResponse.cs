namespace SmartCourt.Features.Auth.RegisterClient.DTOs;

public record RegisterResponse(
    string UserId,
    string Email,
    string FullName,
    string Role
);
