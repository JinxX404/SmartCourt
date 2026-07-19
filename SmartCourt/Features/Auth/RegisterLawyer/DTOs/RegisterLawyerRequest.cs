using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.Auth.RegisterLawyer.DTOs;

public record RegisterLawyerRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string Phone,
    string Address,
    string Government,
    string City,
    string Gender,
    string NationalNumber,
    IFormFile NationalIdFront,
    IFormFile NationalIdBack,
    IFormFile SyndicateCard,
    IFormFile PersonalPhoto
);
