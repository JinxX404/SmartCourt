using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Validators;
using SmartCourt.Infrastructure.Providers.Jobs;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class WalletsApiTests
{
    [Fact]
    public async Task Endpoints_ReturnWrappedResponses()
    {
        var service = new RecordingWalletBoundary();
        var controller = new WalletsController(
            service,
            new CreateWithdrawalRequestValidator());
        var request = new CreateWithdrawalRequest(
            100m,
            "bank-account-token");

        var walletAction = await controller.GetAsync(
            CancellationToken.None);
        var withdrawalAction = await controller.WithdrawAsync(
            request,
            "withdrawal-key",
            CancellationToken.None);
        var historyAction = await controller.GetWithdrawalsAsync(
            CancellationToken.None);

        AssertWrappedOk(walletAction, service.Wallet);
        AssertWrappedOk(withdrawalAction, service.ActionResult);
        AssertWrappedOk(historyAction, service.Withdrawals);
        Assert.Same(request, service.Request);
        Assert.Equal("withdrawal-key", service.IdempotencyKey);
    }

    [Fact]
    public async Task InvalidWithdrawal_FailsBeforeServiceCall()
    {
        var service = new RecordingWalletBoundary();
        var controller = new WalletsController(
            service,
            new CreateWithdrawalRequestValidator());

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            controller.WithdrawAsync(
                new CreateWithdrawalRequest(0m, " "),
                "withdrawal-key",
                CancellationToken.None));

        Assert.Matches("[\\u0600-\\u06FF]", exception.Message);
        Assert.Equal(0, service.WithdrawCalls);
    }

    [Fact]
    public void Endpoints_UseLawyerOnlyWalletRoutes()
    {
        var controllerAuthorization =
            Assert.Single(
                typeof(WalletsController)
                    .GetCustomAttributes(
                        typeof(AuthorizeAttribute),
                        inherit: true)
                    .Cast<AuthorizeAttribute>());
        Assert.Equal("Lawyer", controllerAuthorization.Roles);
        AssertRoute(nameof(WalletsController.GetAsync), "GET", null);
        AssertRoute(
            nameof(WalletsController.WithdrawAsync),
            "POST",
            "withdrawals");
        AssertRoute(
            nameof(WalletsController.GetWithdrawalsAsync),
            "GET",
            "withdrawals");
    }

    private static void AssertWrappedOk<T>(
        ActionResult<ApiResponse<T>> action,
        T expected)
    {
        var result = Assert.IsType<OkObjectResult>(
            ((IConvertToActionResult)action).Convert());
        var response = Assert.IsType<ApiResponse<T>>(result.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(expected, response.Data);
    }

    private static void AssertRoute(
        string methodName,
        string httpMethod,
        string? template)
    {
        var method = typeof(WalletsController).GetMethod(methodName);
        Assert.NotNull(method);
        var route = Assert.Single(
            method.GetCustomAttributes(
                    typeof(HttpMethodAttribute),
                    inherit: true)
                .Cast<HttpMethodAttribute>());
        Assert.Contains(httpMethod, route.HttpMethods);
        Assert.Equal(template, route.Template);
    }

    private sealed class RecordingWalletBoundary : IWalletService
    {
        public WalletDto Wallet { get; } = new(
            Guid.NewGuid(),
            "EGP",
            50m,
            500m,
            1_000m);

        public PaymentActionResultDto ActionResult { get; } = new(
            Guid.NewGuid(),
            "Completed",
            DateTime.UtcNow);

        public IReadOnlyList<WithdrawalDto> Withdrawals { get; } =
        [
            new WithdrawalDto(
                Guid.NewGuid(),
                100m,
                "EGP",
                SmartCourt.Features.Payments.Enums.WithdrawalStatus.Completed,
                "paid",
                null,
                false,
                DateTime.UtcNow,
                DateTime.UtcNow)
        ];

        public CreateWithdrawalRequest? Request { get; private set; }
        public string? IdempotencyKey { get; private set; }
        public int WithdrawCalls { get; private set; }

        public Task<WalletDto> GetAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Wallet);
        }

        public Task<IReadOnlyList<WithdrawalDto>> GetWithdrawalsAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(Withdrawals);

        public Task<PaymentActionResultDto> WithdrawAsync(
            CreateWithdrawalRequest request,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WithdrawCalls++;
            Request = request;
            IdempotencyKey = idempotencyKey;
            return Task.FromResult(ActionResult);
        }

        public Task<JobExecutionResult> ReconcilePendingWithdrawalsAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                JobExecutionResult.NoOp(
                    "NoPendingWithdrawals"));
        }
    }
}
