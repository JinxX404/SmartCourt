namespace SmartCourt.Features.Auth.RegisterClient.DTOs;

public record RegisterClientRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string NationalNumber
);
