using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Offerings;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Consultations.Discovery;

public sealed record SearchConsultationLawyersQuery(ConsultationLawyerFilter Filter)
    : IRequest<ApiResponse<ConsultationPageDto<ConsultationLawyerDto>>>;

public sealed record GetConsultationLawyerQuery(Guid LawyerId)
    : IRequest<ApiResponse<ConsultationLawyerDto>>;

public sealed class ConsultationDiscoveryHandler(
    ApplicationDbContext dbContext,
    IPaymentProvider paymentProvider,
    IOptions<PaymentProviderOptions> paymentOptions,
    TimeProvider timeProvider,
    IFileStorageService fileStorageService,
    IValidator<ConsultationLawyerFilter> validator)
    : IRequestHandler<SearchConsultationLawyersQuery, ApiResponse<ConsultationPageDto<ConsultationLawyerDto>>>,
      IRequestHandler<GetConsultationLawyerQuery, ApiResponse<ConsultationLawyerDto>>
{
    public async Task<ApiResponse<ConsultationPageDto<ConsultationLawyerDto>>> Handle(
        SearchConsultationLawyersQuery query,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query.Filter, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<ConsultationPageDto<ConsultationLawyerDto>>.Fail(
                validation.Errors.Select(item => item.ErrorMessage).ToList());

        var now = timeProvider.GetUtcNow();
        var offerings = EligibleOfferings(now);
        if (query.Filter.Modes is { Length: > 0 })
            offerings = offerings.Where(item => query.Filter.Modes.Contains(item.Mode));
        if (query.Filter.Specializations is { Length: > 0 })
            offerings = offerings.Where(item => query.Filter.Specializations.Contains(item.Specialization));
        if (query.Filter.MinimumPrice.HasValue)
            offerings = offerings.Where(item => item.Price >= query.Filter.MinimumPrice.Value);
        if (query.Filter.MaximumPrice.HasValue)
            offerings = offerings.Where(item => item.Price <= query.Filter.MaximumPrice.Value);
        if (!string.IsNullOrWhiteSpace(query.Filter.Search))
        {
            var term = query.Filter.Search.Trim().ToLower();
            offerings = offerings.Where(item => item.Title.ToLower().Contains(term)
                || item.Description.ToLower().Contains(term)
                || dbContext.Users.Any(user => user.Id == item.LawyerId
                    && user.FullName.ToLower().Contains(term)));
        }
        if (query.Filter.AvailableFromUtc.HasValue || query.Filter.AvailableToUtc.HasValue)
        {
            var from = query.Filter.AvailableFromUtc ?? now;
            var to = query.Filter.AvailableToUtc ?? now.AddDays(30);
            offerings = offerings.Where(item => dbContext.ConsultationAvailabilitySlots.Any(slot =>
                slot.OfferingId == item.Id && slot.StartAtUtc >= from && slot.StartAtUtc <= to
                && (slot.Status == ConsultationSlotStatus.Available
                    || slot.Status == ConsultationSlotStatus.Reserved && slot.ReservedUntilUtc <= now)));
        }

        var lawyerQuery = offerings.Select(item => item.LawyerId).Distinct();
        var total = await lawyerQuery.CountAsync(cancellationToken);
        var lawyerIds = await lawyerQuery
            .OrderBy(id => id)
            .Skip((query.Filter.Page - 1) * query.Filter.PageSize)
            .Take(query.Filter.PageSize)
            .ToListAsync(cancellationToken);
        var items = await LoadLawyersAsync(lawyerIds, now, cancellationToken);
        var page = new ConsultationPageDto<ConsultationLawyerDto>(
            items, query.Filter.Page, query.Filter.PageSize, total,
            total == 0 ? 0 : (int)Math.Ceiling(total / (double)query.Filter.PageSize));
        return ApiResponse<ConsultationPageDto<ConsultationLawyerDto>>.Ok(page);
    }

    public async Task<ApiResponse<ConsultationLawyerDto>> Handle(
        GetConsultationLawyerQuery query,
        CancellationToken cancellationToken)
    {
        var items = await LoadLawyersAsync(
            [query.LawyerId], timeProvider.GetUtcNow(), cancellationToken);
        return items.Count == 0
            ? ApiResponse<ConsultationLawyerDto>.Fail("Consultation lawyer was not found.", 404)
            : ApiResponse<ConsultationLawyerDto>.Ok(items[0]);
    }

    private IQueryable<Domain.Entities.ConsultationOffering> EligibleOfferings(DateTimeOffset now)
    {
        var query = dbContext.ConsultationOfferings.AsNoTracking()
            .Where(item => item.IsActive
                && dbContext.LawyerConsultationSettings.Any(settings =>
                    settings.LawyerId == item.LawyerId && settings.IsEnabled)
                && dbContext.Users.Any(user => user.Id == item.LawyerId
                    && user.Status == UserStatus.Active && user.EmailConfirmed
                    && user.LawyerProfile != null)
                && dbContext.ConsultationAvailabilitySlots.Any(slot =>
                    slot.OfferingId == item.Id && slot.StartAtUtc > now
                    && (slot.Status == ConsultationSlotStatus.Available
                        || slot.Status == ConsultationSlotStatus.Reserved && slot.ReservedUntilUtc <= now)));

        if (paymentProvider is ILawyerPayoutAccountProvider)
        {
            var provider = paymentOptions.Value.ProviderCode;
            query = query.Where(item => dbContext.LawyerPayoutAccounts.Any(account =>
                account.LawyerUserId == item.LawyerId
                && account.ProviderCode == provider
                && account.Status == LawyerPayoutAccountStatus.Enabled
                && account.TransfersEnabled));
        }
        return query;
    }

    private async Task<IReadOnlyList<ConsultationLawyerDto>> LoadLawyersAsync(
        IReadOnlyCollection<Guid> lawyerIds,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (lawyerIds.Count == 0)
            return [];

        var users = await dbContext.Users.AsNoTracking()
            .Include(item => item.LawyerProfile)
            .Where(item => lawyerIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        var offerings = await EligibleOfferings(now)
            .Include(item => item.Inclusions)
            .Where(item => lawyerIds.Contains(item.LawyerId))
            .ToListAsync(cancellationToken);
        var offeringIds = offerings.Select(item => item.Id).ToList();
        var nextSlots = await dbContext.ConsultationAvailabilitySlots.AsNoTracking()
            .Where(slot => offeringIds.Contains(slot.OfferingId) && slot.StartAtUtc > now
                && (slot.Status == ConsultationSlotStatus.Available
                    || slot.Status == ConsultationSlotStatus.Reserved && slot.ReservedUntilUtc <= now))
            .GroupBy(slot => slot.OfferingId)
            .Select(group => new { OfferingId = group.Key, Next = group.Min(item => item.StartAtUtc) })
            .ToDictionaryAsync(item => item.OfferingId, item => (DateTimeOffset?)item.Next, cancellationToken);

        var dtos = await Task.WhenAll(users.Select(async user =>
        {
            var lawyerOfferings = offerings.Where(item => item.LawyerId == user.Id)
                .OrderBy(item => item.Price)
                .Select(item => ConsultationOfferingHandler.Map(
                    item, nextSlots.GetValueOrDefault(item.Id), includePrivateLocation: false))
                .ToList();
            var next = lawyerOfferings.Min(item => item.NextAvailableAtUtc);

            string? pictureUrl = null;
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                try { pictureUrl = await fileStorageService.GetDownloadUrlAsync(user.ProfilePictureUrl, cancellationToken); }
                catch { pictureUrl = null; }
            }

            return new ConsultationLawyerDto(
                user.Id, user.FullName, pictureUrl, user.Governorate, user.City,
                user.LawyerProfile?.AverageRating ?? 0m,
                true, lawyerOfferings.Count > 0, lawyerOfferings.Count > 0 ? null : "No available slots.",
                lawyerOfferings.Min(item => item.Price), "EGP", next, lawyerOfferings);
        }));

        return dtos
            .Where(item => item.Offerings.Count > 0)
            .OrderBy(item => Array.IndexOf(lawyerIds.ToArray(), item.LawyerId))
            .ToList();
    }
}
