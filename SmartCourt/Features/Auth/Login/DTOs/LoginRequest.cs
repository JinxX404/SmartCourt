namespace SmartCourt.Features.Auth.Login.DTOs;

public record LoginRequest(
    string Email,
    string Password
);
