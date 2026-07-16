namespace SmartCourt.Features.Auth;

public record RegisterResponse(
    string UserId,
    string Email,
    string FullName,
    string Role
);
