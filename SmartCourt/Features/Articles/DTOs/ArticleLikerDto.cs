namespace SmartCourt.Features.Articles.DTOs;

public record ArticleLikerDto(
    Guid Id,
    string FullName,
    string? ProfilePictureUrl
);
