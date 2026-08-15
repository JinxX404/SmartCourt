using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Bookings;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Consultations;

public sealed class ConsultationFeatureIntegrationTests
    : IClassFixture<SmartCourtWebApplicationFactory>
{
    private readonly SmartCourtWebApplicationFactory _factory;

    public ConsultationFeatureIntegrationTests(SmartCourtWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ClientCanDiscoverBookPayCancelAndReuseConsultationSlot()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        await _factory.SeedUserAsync(lawyerId, $"lawyer-{lawyerId:N}@test.local", "Lawyer", "Mona Adel");
        await _factory.SeedUserAsync(clientId, $"client-{clientId:N}@test.local", "Client", "Omar Hassan");
        await SeedLawyerProfileAsync(lawyerId);

        using var lawyerClient = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        using var client = _factory.CreateAuthenticatedClient(clientId, "Client");

        var settingsResponse = await lawyerClient.PutAsJsonAsync(
            "/api/consultations/lawyer/settings",
            new UpdateConsultationSettingsRequest(true, 0, 60, 15, "Africa/Cairo"));
        Assert.Equal(HttpStatusCode.OK, settingsResponse.StatusCode);

        var offeringResponse = await lawyerClient.PostAsJsonAsync(
            "/api/consultations/lawyer/offerings",
            new CreateConsultationOfferingRequest(
                ConsultationMode.InOffice,
                Specialization.RealEstateAndPropertyRegistration,
                "Property contract review",
                "Review ownership papers and explain the property registration steps.",
                45,
                250m,
                "Nasr City office, Cairo",
                ["45-minute consultation", "Initial document review"],
                true));
        Assert.Equal(HttpStatusCode.Created, offeringResponse.StatusCode);
        var offering = (await offeringResponse.Content.ReadFromJsonAsync<ApiResponse<ConsultationOfferingDto>>())!.Data!;
        Assert.Equal(12.50m, decimal.Round(offering.Price * 0.05m, 2));

        var startAt = DateTime.UtcNow.AddHours(30);
        startAt = new DateTime(startAt.Year, startAt.Month, startAt.Day, startAt.Hour, 0, 0, DateTimeKind.Utc);
        var slotsResponse = await lawyerClient.PostAsJsonAsync(
            $"/api/consultations/lawyer/offerings/{offering.Id}/slots",
            new CreateConsultationSlotsRequest([new(startAt)]));
        Assert.Equal(HttpStatusCode.Created, slotsResponse.StatusCode);
        var slot = (await slotsResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ConsultationSlotDto>>>())!.Data!.Single();

        var publicResponse = await _factory.CreateClient().GetAsync("/api/consultations/lawyers?page=1&pageSize=5");
        Assert.True(
            publicResponse.StatusCode == HttpStatusCode.OK,
            $"Expected consultation discovery to succeed but received {publicResponse.StatusCode}: {await publicResponse.Content.ReadAsStringAsync()}");
        var publicPage = (await publicResponse.Content.ReadFromJsonAsync<ApiResponse<ConsultationPageDto<ConsultationLawyerDto>>>())!.Data!;
        var publicOffering = Assert.Single(Assert.Single(publicPage.Items).Offerings);
        Assert.Null(publicOffering.OfficeLocation);

        var booking = await CreateBookingAsync(client, offering.Id, slot.Id);
        Assert.Equal(ConsultationBookingStatus.AwaitingPayment, booking.Status);
        Assert.Null(booking.OfficeLocation);
        Assert.Equal(250m, booking.GrossAmount);
        Assert.Equal(12.50m, booking.PlatformFeeAmount);
        Assert.Equal(237.50m, booking.LawyerNetAmount);

        var paymentKey = Guid.NewGuid().ToString();
        using var paymentRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/consultations/bookings/{booking.Id}/payment-session")
        {
            Content = JsonContent.Create(new CreateConsultationPaymentSessionRequest("mock-success-consultation"))
        };
        paymentRequest.Headers.Add("Idempotency-Key", paymentKey);
        var paymentResponse = await client.SendAsync(paymentRequest);
        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);
        var firstPayment = (await paymentResponse.Content
            .ReadFromJsonAsync<ApiResponse<ConsultationPaymentDto>>())!.Data!;

        using var replayRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/consultations/bookings/{booking.Id}/payment-session")
        {
            Content = JsonContent.Create(new CreateConsultationPaymentSessionRequest("mock-success-consultation"))
        };
        replayRequest.Headers.Add("Idempotency-Key", paymentKey);
        var replayResponse = await client.SendAsync(replayRequest);
        Assert.Equal(HttpStatusCode.OK, replayResponse.StatusCode);
        var replayedPayment = (await replayResponse.Content
            .ReadFromJsonAsync<ApiResponse<ConsultationPaymentDto>>())!.Data!;
        Assert.Equal(firstPayment.TransactionId, replayedPayment.TransactionId);

        var paidBookingResponse = await client.GetAsync($"/api/consultations/bookings/{booking.Id}");
        var paidBooking = (await paidBookingResponse.Content.ReadFromJsonAsync<ApiResponse<ConsultationBookingDto>>())!.Data!;
        Assert.Equal(ConsultationBookingStatus.Confirmed, paidBooking.Status);
        Assert.Equal("Nasr City office, Cairo", paidBooking.OfficeLocation);

        var cancellation = await client.PostAsJsonAsync(
            $"/api/consultations/bookings/{booking.Id}/cancel",
            new CancelConsultationBookingRequest("I no longer need this consultation."));
        Assert.Equal(HttpStatusCode.OK, cancellation.StatusCode);
        var cancelled = (await cancellation.Content.ReadFromJsonAsync<ApiResponse<ConsultationBookingDto>>())!.Data!;
        Assert.Equal(ConsultationBookingStatus.Refunded, cancelled.Status);

        var secondBooking = await CreateBookingAsync(client, offering.Id, slot.Id);
        Assert.Equal(ConsultationBookingStatus.AwaitingPayment, secondBooking.Status);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var wallet = await db.LawyerWallets.AsNoTracking().SingleAsync(item => item.LawyerUserId == lawyerId);
        Assert.Equal(0m, wallet.PendingBalance);
        Assert.Equal(0m, wallet.AvailableBalance);
        Assert.False((await db.LawyerProfiles.AsNoTracking().SingleAsync(item => item.UserId == lawyerId)).IsAvailable);
    }

    [Fact]
    public async Task NonOwnerCannotReadConsultationBooking()
    {
        var strangerId = Guid.NewGuid();
        await _factory.SeedUserAsync(strangerId, $"stranger-{strangerId:N}@test.local", "Client");
        using var stranger = _factory.CreateAuthenticatedClient(strangerId, "Client");
        var response = await stranger.GetAsync($"/api/consultations/bookings/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompletedConsultationEntersHoldThenReleasesNetAmountToWallet()
    {
        var lawyerId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        await _factory.SeedUserAsync(lawyerId, $"release-lawyer-{lawyerId:N}@test.local", "Lawyer", "Karim Fathy");
        await _factory.SeedUserAsync(clientId, $"release-client-{clientId:N}@test.local", "Client", "Salma Nabil");
        await SeedLawyerProfileAsync(lawyerId);
        using var lawyer = _factory.CreateAuthenticatedClient(lawyerId, "Lawyer");
        using var client = _factory.CreateAuthenticatedClient(clientId, "Client");

        Assert.Equal(HttpStatusCode.OK, (await lawyer.PutAsJsonAsync(
            "/api/consultations/lawyer/settings",
            new UpdateConsultationSettingsRequest(true, 0, 60, 0, "Africa/Cairo"))).StatusCode);
        var offeringHttp = await lawyer.PostAsJsonAsync(
            "/api/consultations/lawyer/offerings",
            new CreateConsultationOfferingRequest(
                ConsultationMode.Phone,
                Specialization.RealEstateAndPropertyRegistration,
                "Detailed property advice",
                "A detailed call covering ownership, contract terms, and registration risks.",
                45, 1_000m, null,
                ["45-minute call", "Contract risk explanation"], true));
        var offering = (await offeringHttp.Content.ReadFromJsonAsync<ApiResponse<ConsultationOfferingDto>>())!.Data!;
        var start = DateTime.UtcNow.AddDays(2);
        start = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, DateTimeKind.Utc);
        var slotsHttp = await lawyer.PostAsJsonAsync(
            $"/api/consultations/lawyer/offerings/{offering.Id}/slots",
            new CreateConsultationSlotsRequest([new(start)]));
        var slot = (await slotsHttp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ConsultationSlotDto>>>())!.Data!.Single();
        var booking = await CreateBookingAsync(client, offering.Id, slot.Id);

        using (var paymentRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/consultations/bookings/{booking.Id}/payment-session")
        {
            Content = JsonContent.Create(new CreateConsultationPaymentSessionRequest("mock-success-consultation-release"))
        })
        {
            paymentRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
            Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(paymentRequest)).StatusCode);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var stored = await db.ConsultationBookings.SingleAsync(item => item.Id == booking.Id);
            stored.StartAtUtc = DateTime.UtcNow.AddHours(-1);
            stored.EndAtUtc = DateTime.UtcNow.AddMinutes(-10);
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.OK, (await lawyer.PostAsJsonAsync(
            $"/api/consultations/bookings/{booking.Id}/mark-performed",
            new MarkConsultationPerformedRequest())).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync(
            $"/api/consultations/bookings/{booking.Id}/confirm-completion", null)).StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hold = await db.ConsultationEscrowHolds.SingleAsync(item => item.BookingId == booking.Id);
            Assert.Equal(EscrowHoldStatus.Funded, hold.Status);
            Assert.NotNull(hold.HoldExpiresAtUtc);
            hold.HoldExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
            var jobs = scope.ServiceProvider.GetRequiredService<IConsultationJobService>();
            await jobs.ReleaseAsync(booking.Id, CancellationToken.None);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hold = await db.ConsultationEscrowHolds.AsNoTracking().SingleAsync(item => item.BookingId == booking.Id);
            var wallet = await db.LawyerWallets.AsNoTracking().SingleAsync(item => item.LawyerUserId == lawyerId);
            Assert.Equal(EscrowHoldStatus.Released, hold.Status);
            Assert.Equal(0m, wallet.PendingBalance);
            Assert.Equal(950m, wallet.AvailableBalance);
        }
    }

    private async Task SeedLawyerProfileAsync(Guid lawyerId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.LawyerProfiles.Add(new LawyerProfile
        {
            UserId = lawyerId,
            Bio = "Real-estate lawyer with contract and registration experience.",
            IsAvailable = false
        });
        db.LawyerSpecializations.Add(new LawyerSpecialization
        {
            Id = Guid.NewGuid(),
            LawyerProfileUserId = lawyerId,
            Specialization = Specialization.RealEstateAndPropertyRegistration,
            YearsOfExperience = 8,
            CasesHandled = 120
        });
        await db.SaveChangesAsync();
    }

    private static async Task<ConsultationBookingDto> CreateBookingAsync(
        HttpClient client,
        Guid offeringId,
        Guid slotId)
    {
        var response = await client.PostAsJsonAsync(
            "/api/consultations/bookings",
            new CreateConsultationBookingRequest(
                offeringId,
                slotId,
                "Apartment purchase review",
                "I need the ownership chain and preliminary sale contract reviewed before paying the deposit."));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<ConsultationBookingDto>>())!.Data!;
    }
}
