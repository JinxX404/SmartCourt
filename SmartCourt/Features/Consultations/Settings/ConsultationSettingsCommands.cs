using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Consultations.Settings;

public sealed record GetMyConsultationSettingsQuery
    : IRequest<ApiResponse<ConsultationSettingsDto>>;

public sealed record UpdateConsultationSettingsCommand(
    UpdateConsultationSettingsRequest Request)
    : IRequest<ApiResponse<ConsultationSettingsDto>>;

public sealed class ConsultationSettingsHandler(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IValidator<UpdateConsultationSettingsRequest> validator)
    : IRequestHandler<GetMyConsultationSettingsQuery, ApiResponse<ConsultationSettingsDto>>,
      IRequestHandler<UpdateConsultationSettingsCommand, ApiResponse<ConsultationSettingsDto>>
{
    public async Task<ApiResponse<ConsultationSettingsDto>> Handle(
        GetMyConsultationSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var settings = await dbContext.LawyerConsultationSettings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.LawyerId == lawyerId, cancellationToken);
        return ApiResponse<ConsultationSettingsDto>.Ok(
            settings is null
                ? new(lawyerId, false, 2, 60, 15, "Africa/Cairo")
                : Map(settings));
    }

    public async Task<ApiResponse<ConsultationSettingsDto>> Handle(
        UpdateConsultationSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(command.Request, cancellationToken);
        if (!result.IsValid)
        {
            return ApiResponse<ConsultationSettingsDto>.Fail(
                result.Errors.Select(item => item.ErrorMessage).ToList());
        }

        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var eligible = await dbContext.Users.AnyAsync(
            user => user.Id == lawyerId
                && user.Status == UserStatus.Active
                && user.EmailConfirmed
                && user.LawyerProfile != null,
            cancellationToken);
        if (!eligible)
        {
            return ApiResponse<ConsultationSettingsDto>.Fail(
                "Only an active, verified lawyer can configure consultations.", 403);
        }

        var now = timeProvider.GetUtcNow();
        var settings = await dbContext.LawyerConsultationSettings
            .SingleOrDefaultAsync(item => item.LawyerId == lawyerId, cancellationToken);
        if (settings is null)
        {
            settings = new LawyerConsultationSettings
            {
                LawyerId = lawyerId,
                CreatedAt = now
            };
            dbContext.LawyerConsultationSettings.Add(settings);
        }

        settings.IsEnabled = command.Request.IsEnabled;
        settings.MinimumBookingNoticeHours = command.Request.MinimumBookingNoticeHours;
        settings.MaximumAdvanceBookingDays = command.Request.MaximumAdvanceBookingDays;
        settings.BufferMinutes = command.Request.BufferMinutes;
        settings.TimeZoneId = command.Request.TimeZoneId.Trim();
        settings.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationSettingsDto>.Ok(Map(settings));
    }

    private static ConsultationSettingsDto Map(LawyerConsultationSettings item) => new(
        item.LawyerId,
        item.IsEnabled,
        item.MinimumBookingNoticeHours,
        item.MaximumAdvanceBookingDays,
        item.BufferMinutes,
        item.TimeZoneId);
}
