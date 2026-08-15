using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Validators;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class PaymentsControllerTests
{
    [Fact]
    public async Task Endpoints_ReturnWrappedResponsesAndForwardInputs()
    {
        var service = new RecordingPaymentApi();
        var controller = CreateController(service);
        var milestoneId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var fundRequest =
            new FundMilestoneRequest("mock-success-card");

        var fund = await controller.FundAsync(
            milestoneId,
            fundRequest,
            "fund-key",
            CancellationToken.None);
        var contractPayments =
            await controller.GetContractPaymentsAsync(
                contractId,
                CancellationToken.None);
        var milestonePayment =
            await controller.GetMilestonePaymentAsync(
                milestoneId,
                CancellationToken.None);
        var retry = await controller.RetryAsync(
            transactionId,
            new RetryPaymentRequest("pm_retry"),
            "retry-key",
            CancellationToken.None);
        var session = await controller.CreatePaymentSessionAsync(
            milestoneId,
            new CreateMilestonePaymentSessionRequest("ctoken_demo_success"),
            "session-key",
            CancellationToken.None);
        var retrySession = await controller.RetryPaymentSessionAsync(
            transactionId,
            new RetryPaymentSessionRequest("ctoken_demo_retry"),
            "retry-session-key",
            CancellationToken.None);

        AssertWrappedOk(fund, service.FundingOperation);
        AssertWrappedOk(
            contractPayments,
            service.PaymentHistory);
        AssertWrappedOk(milestonePayment, service.Payment);
        AssertWrappedOk(retry, service.FundingOperation);
        AssertWrappedOk(session, service.FundingOperation);
        AssertWrappedOk(retrySession, service.FundingOperation);
        Assert.Equal(milestoneId, service.FundMilestoneId);
        Assert.Same(fundRequest, service.FundRequest);
        Assert.Equal("session-key", service.FundIdempotencyKey);
        Assert.Equal(contractId, service.ContractPaymentsId);
        Assert.Equal(
            milestoneId,
            service.MilestonePaymentId);
        Assert.Equal(transactionId, service.RetryTransactionId);
        Assert.Equal("pm_retry", service.RetryPaymentMethodReference);
        Assert.Equal("retry-session-key", service.RetryIdempotencyKey);
        Assert.Equal("ctoken_demo_success", service.ConfirmationTokenReference);
        Assert.Equal("ctoken_demo_retry", service.RetryConfirmationTokenReference);
    }

    [Fact]
    public async Task Retry_InvalidIdempotencyHeaderFailsBeforeServiceCall()
    {
        var service = new RecordingPaymentApi();
        var controller = CreateController(service);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            controller.RetryAsync(
                Guid.NewGuid(),
                new RetryPaymentRequest("pm_retry"),
                null,
                CancellationToken.None));

        Assert.Contains("Idempotency-Key", exception.Message);
        Assert.Null(service.RetryTransactionId);
    }

    [Fact]
    public async Task Webhook_ReadsExactBodyValidatesAndReturnsWrappedResponse()
    {
        var service = new RecordingPaymentApi();
        var controller = CreateController(
            service,
            new PaymentProviderOptions
            {
                WebhookAllowedIpRanges = ["203.0.113.0/24"]
            });
        controller.HttpContext.Connection.RemoteIpAddress =
            System.Net.IPAddress.Parse("203.0.113.10");
        var request = new PaymentWebhookRequest(
            "event-1",
            Guid.NewGuid(),
            "provider-transaction-1",
            PaymentTransactionStatus.Completed,
            100m,
            "EGP",
            new DateTime(
                2026,
                8,
                15,
                10,
                0,
                0,
                DateTimeKind.Utc),
            null);
        var rawBody = JsonSerializer.Serialize(
            request,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        controller.ControllerContext.HttpContext.Request.Body =
            new MemoryStream(Encoding.UTF8.GetBytes(rawBody));

        var action = await controller.HandleWebhookAsync(
            request.EventId,
            "1786788000",
            "v1=signature",
            CancellationToken.None);

        AssertWrappedOk(action, service.WebhookResult);
        Assert.Equal(rawBody, service.WebhookRawBody);
        Assert.Equal(request.EventId, service.WebhookEventId);
        Assert.Equal(request, service.WebhookRequest);
    }

    [Fact]
    public async Task Webhook_RejectsDeclaredOversizedBodyBeforeReading()
    {
        var service = new RecordingPaymentApi();
        var controller = CreateController(
            service,
            new PaymentProviderOptions
            {
                WebhookMaximumBodySizeBytes = 32
            });
        controller.Request.ContentLength = 33;
        controller.Request.Body = new MemoryStream([1]);

        await Assert.ThrowsAsync<PayloadTooLargeException>(() =>
            controller.HandleWebhookAsync(
                "event-1",
                "1786788000",
                "v1=signature",
                CancellationToken.None));

        Assert.Null(service.WebhookRequest);
        Assert.Equal(0, controller.Request.Body.Position);
    }

    [Fact]
    public async Task Webhook_RejectsChunkedBodyWhenBoundedReadExceedsLimit()
    {
        var service = new RecordingPaymentApi();
        var controller = CreateController(
            service,
            new PaymentProviderOptions
            {
                WebhookMaximumBodySizeBytes = 32
            });
        controller.Request.Body = new MemoryStream(new byte[33]);

        await Assert.ThrowsAsync<PayloadTooLargeException>(() =>
            controller.HandleWebhookAsync(
                "event-1",
                "1786788000",
                "v1=signature",
                CancellationToken.None));

        Assert.Null(service.WebhookRequest);
    }

    [Fact]
    public async Task Webhook_RejectsSourceOutsideConfiguredProviderRanges()
    {
        var service = new RecordingPaymentApi();
        var controller = CreateController(
            service,
            new PaymentProviderOptions
            {
                WebhookAllowedIpRanges = ["203.0.113.0/24"]
            });
        controller.HttpContext.Connection.RemoteIpAddress =
            System.Net.IPAddress.Parse("198.51.100.10");
        controller.Request.Body = new MemoryStream([1]);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            controller.HandleWebhookAsync(
                "event-1",
                "1786788000",
                "v1=signature",
                CancellationToken.None));

        Assert.Null(service.WebhookRequest);
        Assert.Equal(0, controller.Request.Body.Position);
    }

    [Fact]
    public void Endpoints_DefineExpectedRoutesAndRoleBoundaries()
    {
        AssertEndpoint(
            nameof(PaymentsController.FundAsync),
            "milestones/{milestoneId:guid}/fund",
            "Client");
        AssertEndpoint(
            nameof(PaymentsController.GetContractPaymentsAsync),
            "contracts/{contractId:guid}/payments",
            "Client,Lawyer,FinanceAdministrator,SuperAdministrator");
        AssertEndpoint(
            nameof(PaymentsController.GetMilestonePaymentAsync),
            "milestones/{milestoneId:guid}/payment",
            "Client,Lawyer,FinanceAdministrator,SuperAdministrator");
        AssertEndpoint(
            nameof(PaymentsController.RetryAsync),
            "payments/{paymentTransactionId:guid}/retry",
            "FinanceAdministrator,SuperAdministrator");
        AssertEndpoint(
            nameof(PaymentsController.CreatePaymentSessionAsync),
            "milestones/{milestoneId:guid}/payment-session",
            "Client");
        AssertEndpoint(
            nameof(PaymentsController.RetryPaymentSessionAsync),
            "payments/{paymentTransactionId:guid}/retry-session",
            "Client");

        var webhook = typeof(PaymentsController).GetMethod(
            nameof(PaymentsController.HandleWebhookAsync));
        Assert.NotNull(webhook);
        var route = Assert.Single(
            webhook.GetCustomAttributes<HttpPostAttribute>());
        Assert.Equal("payments/webhook", route.Template);
        Assert.Single(
            webhook.GetCustomAttributes<AllowAnonymousAttribute>());
    }

    private static PaymentsController CreateController(
        RecordingPaymentApi service,
        PaymentProviderOptions? providerOptions = null)
    {
        var controller = new PaymentsController(
            service,
            service,
            service,
            new RetryPaymentRequestValidator(),
            new CreateMilestonePaymentSessionRequestValidator(),
            new RetryPaymentSessionRequestValidator(),
            new PaymentWebhookRequestValidator(),
            Options.Create(
                providerOptions ?? new PaymentProviderOptions()));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    private static void AssertEndpoint(
        string methodName,
        string route,
        string roles)
    {
        var method = typeof(PaymentsController).GetMethod(methodName);
        Assert.NotNull(method);
        var httpAttribute = Assert.Single(
            method.GetCustomAttributes<HttpMethodAttribute>());
        Assert.Equal(route, httpAttribute.Template);
        var authorize = Assert.Single(
            method.GetCustomAttributes<AuthorizeAttribute>());
        Assert.NotNull(authorize);
        Assert.Equal(roles, authorize.Roles);
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

    private sealed class RecordingPaymentApi
        : IPaymentEscrowService, IPaymentQueryService, IPaymentWebhookService
    {
        public RecordingPaymentApi()
        {
            FundingOperation = new FundingOperationDto(
                Guid.NewGuid(),
                Payment.MilestoneId,
                "Succeeded",
                null,
                null,
                null,
                Payment,
                DateTimeOffset.UtcNow);
        }

        public PaymentDto Payment { get; } = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            5m,
            95m,
            "EGP",
            EscrowHoldStatus.Funded,
            null,
            null);

        public FundingOperationDto FundingOperation { get; }

        public PaymentHistoryDto PaymentHistory { get; } = new(
            [],
            [],
            []);

        public PaymentActionResultDto WebhookResult { get; } = new(
            Guid.NewGuid(),
            "Completed",
            new DateTime(
                2026,
                8,
                15,
                10,
                0,
                0,
                DateTimeKind.Utc));

        public Guid? FundMilestoneId { get; private set; }
        public FundMilestoneRequest? FundRequest { get; private set; }
        public string? FundIdempotencyKey { get; private set; }
        public Guid? ContractPaymentsId { get; private set; }
        public Guid? MilestonePaymentId { get; private set; }
        public Guid? RetryTransactionId { get; private set; }
        public string? RetryPaymentMethodReference { get; private set; }
        public string? RetryIdempotencyKey { get; private set; }
        public string? ConfirmationTokenReference { get; private set; }
        public string? RetryConfirmationTokenReference { get; private set; }
        public PaymentWebhookRequest? WebhookRequest { get; private set; }
        public string? WebhookEventId { get; private set; }
        public string? WebhookRawBody { get; private set; }

        public Task<FundingOperationDto> FundAsync(
            Guid milestoneId,
            FundMilestoneRequest request,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            FundMilestoneId = milestoneId;
            FundRequest = request;
            FundIdempotencyKey = idempotencyKey;
            return Task.FromResult(FundingOperation);
        }

        public Task<FundingOperationDto> FundWithConfirmationTokenAsync(
            Guid milestoneId,
            string confirmationTokenReference,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            FundMilestoneId = milestoneId;
            ConfirmationTokenReference = confirmationTokenReference;
            FundIdempotencyKey = idempotencyKey;
            return Task.FromResult(FundingOperation);
        }

        public Task<PaymentHistoryDto> GetContractPaymentsAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            ContractPaymentsId = contractId;
            return Task.FromResult(PaymentHistory);
        }

        public Task<PaymentDto> GetMilestonePaymentAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
        {
            MilestonePaymentId = milestoneId;
            return Task.FromResult(Payment);
        }

        public Task<FundingOperationDto> RetryAsync(
            Guid paymentTransactionId,
            string paymentMethodReference,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            RetryTransactionId = paymentTransactionId;
            RetryPaymentMethodReference = paymentMethodReference;
            RetryIdempotencyKey = idempotencyKey;
            return Task.FromResult(FundingOperation);
        }

        public Task<FundingOperationDto> RetryWithConfirmationTokenAsync(
            Guid paymentTransactionId,
            string confirmationTokenReference,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            RetryTransactionId = paymentTransactionId;
            RetryConfirmationTokenReference = confirmationTokenReference;
            RetryIdempotencyKey = idempotencyKey;
            return Task.FromResult(FundingOperation);
        }

        public Task<PaymentDto> CompleteFundingAsync(
            Milestone milestone,
            Guid lawyerUserId,
            PaymentTransaction paymentTransaction,
            ProviderResult providerResult,
            Guid? reservationId,
            Guid? actorUserId,
            Guid correlationId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PaymentActionResultDto> FinalizeFailedExternalResultAsync(
            Milestone milestone,
            PaymentTransaction paymentTransaction,
            string? providerTransactionId,
            Guid? reservationId,
            Guid correlationId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Guid?> FindProcessingFundingReservationIdAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
            => Task.FromResult<Guid?>(null);

        public Task<PaymentActionResultDto> HandleWebhookAsync(
            PaymentWebhookRequest request,
            string? eventIdHeader,
            string? timestampHeader,
            string? signatureHeader,
            string rawBody,
            CancellationToken cancellationToken)
        {
            WebhookRequest = request;
            WebhookEventId = eventIdHeader;
            WebhookRawBody = rawBody;
            return Task.FromResult(WebhookResult);
        }

        public Task<JobExecutionResult>
            ReconcileProviderTransactionAsync(
                Guid paymentTransactionId,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                JobExecutionResult.NoOp("NotUsed"));
        }
    }
}
