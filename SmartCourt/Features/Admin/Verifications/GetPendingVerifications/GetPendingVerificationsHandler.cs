using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Verifications.GetPendingVerifications.DTOs;
using SmartCourt.Features.Admin.Verifications.Shared;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.GetPendingVerifications;

public sealed class GetPendingVerificationsHandler(
    ApplicationDbContext context,
    IValidator<GetPendingVerificationsQuery> validator)
    : IRequestHandler<GetPendingVerificationsQuery, PagedResponse<IReadOnlyList<PendingVerificationListItemDto>>>
{
    public async Task<PagedResponse<IReadOnlyList<PendingVerificationListItemDto>>> Handle(
        GetPendingVerificationsQuery request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new PagedResponse<IReadOnlyList<PendingVerificationListItemDto>>
            {
                Success = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Errors = validationResult.Errors.Select(error => error.ErrorMessage).ToList()
            };
        }

        var query = context.Users
            .AsNoTracking()
            .Where(VerificationQueueFilter.HasCurrentDocumentWithStatus(request.Status));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(user => user.FullName.Contains(search) || user.Email!.Contains(search));
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(user => new PendingVerificationListItemDto
            {
                LawyerId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                PendingDocumentCount = user.VerificationDocuments.Count(document =>
                    document.IsCurrent && document.Status == VerificationDocumentStatus.Pending),
                VerifiedDocumentCount = user.VerificationDocuments.Count(document =>
                    document.IsCurrent && document.Status == VerificationDocumentStatus.Verified),
                RejectedDocumentCount = user.VerificationDocuments.Count(document =>
                    document.IsCurrent &&
                    (document.Status == VerificationDocumentStatus.Rejected ||
                     document.Status == VerificationDocumentStatus.Expired)),
                Role = context.UserRoles.Where(ur => ur.UserId == user.Id)
                    .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);
        return PagedResponse<IReadOnlyList<PendingVerificationListItemDto>>.OkPaged(
            items, request.PageNumber, request.PageSize, totalRecords, totalPages);
    }
}
