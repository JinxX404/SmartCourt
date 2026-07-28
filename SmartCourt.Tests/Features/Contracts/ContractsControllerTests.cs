using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts.Validators;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractsControllerTests
{
    private const string ValidIfMatch = "\"AQIDBA==\"";

    [Fact]
    public async Task Create_ReturnsCreatedWrappedResponse()
    {
        var service = new RecordingContractStub();
        var controller = CreateController(service);
        var request = new CreateContractRequest(
            Guid.NewGuid(),
            "Contract title",
            "Contract terms long enough for validation.");

        var action = await controller.CreateAsync(
            request,
            CancellationToken.None);

        var result = Assert.IsType<CreatedAtActionResult>(
            ConvertAction(action));
        var response =
            Assert.IsType<ApiResponse<ContractDetailDto>>(result.Value);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.Equal(nameof(ContractsController.GetAsync), result.ActionName);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status201Created, response.StatusCode);
        Assert.Same(service.ContractDetail, response.Data);
        Assert.Same(request, service.CreateRequest);
    }

    [Fact]
    public async Task Reads_ReturnOkWrappedResponses()
    {
        var service = new RecordingContractStub();
        var controller = CreateController(service);
        var contractId = Guid.NewGuid();
        var listQuery = new ContractListQuery();
        var historyQuery = new ContractStateHistoryQuery();

        var listAction = await controller.ListAsync(
            listQuery,
            CancellationToken.None);
        var getAction = await controller.GetAsync(
            contractId,
            CancellationToken.None);
        var historyAction = await controller.GetStateHistoryAsync(
            contractId,
            historyQuery,
            CancellationToken.None);

        AssertWrappedOk(listAction, service.ContractList);
        AssertWrappedOk(getAction, service.ContractDetail);
        AssertWrappedOk(historyAction, service.StateHistory);
        Assert.Same(listQuery, service.ListQuery);
        Assert.Equal(contractId, service.GetContractId);
        Assert.Same(historyQuery, service.HistoryQuery);
    }

    [Fact]
    public async Task Mutations_ForwardValidatedIfMatchAndReturnWrappedResponses()
    {
        var service = new RecordingContractStub();
        var controller = CreateController(service);
        var contractId = Guid.NewGuid();
        var updateRequest = new UpdateContractRequest(
            "Updated title",
            "Updated contract terms long enough for validation.");
        var terminateRequest = new TerminateContractRequest(
            "The parties agreed to end the contract.");

        var updateAction = await controller.UpdateAsync(
            contractId,
            updateRequest,
            ValidIfMatch,
            CancellationToken.None);
        var acceptAction = await controller.AcceptAsync(
            contractId,
            ValidIfMatch,
            CancellationToken.None);
        var terminateAction = await controller.TerminateAsync(
            contractId,
            terminateRequest,
            ValidIfMatch,
            CancellationToken.None);

        AssertWrappedOk(updateAction, service.ContractDetail);
        AssertWrappedOk(acceptAction, service.ActionResult);
        AssertWrappedOk(terminateAction, service.ContractDetail);
        Assert.Equal(ValidIfMatch, service.UpdateIfMatch);
        Assert.Equal(ValidIfMatch, service.AcceptIfMatch);
        Assert.Equal(ValidIfMatch, service.TerminateIfMatch);
        Assert.Same(updateRequest, service.UpdateRequest);
        Assert.Same(terminateRequest, service.TerminateRequest);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("W/\"AQIDBA==\"")]
    public async Task Mutation_InvalidIfMatchFailsBeforeServiceCall(
        string? ifMatch)
    {
        var service = new RecordingContractStub();
        var controller = CreateController(service);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            controller.AcceptAsync(
                Guid.NewGuid(),
                ifMatch,
                CancellationToken.None));

        Assert.Contains("If-Match", exception.Message);
        Assert.Equal(0, service.AcceptCallCount);
    }

    [Fact]
    public async Task ServiceException_IsNotCaughtByController()
    {
        var expected = new BusinessException(
            "لا يمكن تنفيذ الإجراء على العقد الحالي.");
        var controller = CreateController(
            new RecordingContractStub
            {
                ExceptionToThrow = expected
            });

        var actual = await Assert.ThrowsAsync<BusinessException>(() =>
            controller.GetAsync(
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.Same(expected, actual);
    }

    [Fact]
    public void Endpoints_DefineExpectedRoutesAndRoleBoundaries()
    {
        AssertEndpoint(
            nameof(ContractsController.CreateAsync),
            typeof(HttpPostAttribute),
            null,
            "Lawyer");
        AssertEndpoint(
            nameof(ContractsController.ListAsync),
            typeof(HttpGetAttribute),
            null,
            "Client,Lawyer");
        AssertEndpoint(
            nameof(ContractsController.GetAsync),
            typeof(HttpGetAttribute),
            "{contractId:guid}",
            "Client,Lawyer,Moderator,SuperAdministrator");
        AssertEndpoint(
            nameof(ContractsController.UpdateAsync),
            typeof(HttpPutAttribute),
            "{contractId:guid}",
            "Lawyer");
        AssertEndpoint(
            nameof(ContractsController.AcceptAsync),
            typeof(HttpPostAttribute),
            "{contractId:guid}/accept",
            "Client,Lawyer");
        AssertEndpoint(
            nameof(ContractsController.TerminateAsync),
            typeof(HttpPostAttribute),
            "{contractId:guid}/terminate",
            "Client,Lawyer");
        AssertEndpoint(
            nameof(ContractsController.GetStateHistoryAsync),
            typeof(HttpGetAttribute),
            "{contractId:guid}/state-history",
            "Client,Lawyer,Moderator,SuperAdministrator");
    }

    private static ContractsController CreateController(
        IContractService service)
    {
        return new ContractsController(
            service,
            new IfMatchRequestValidator());
    }

    private static void AssertWrappedOk<T>(
        ActionResult<ApiResponse<T>> action,
        T expected)
    {
        var result = Assert.IsType<OkObjectResult>(
            ConvertAction(action));
        var response = Assert.IsType<ApiResponse<T>>(result.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(expected, response.Data);
    }

    private static void AssertEndpoint(
        string methodName,
        Type httpAttributeType,
        string? route,
        string roles)
    {
        var method = typeof(ContractsController).GetMethod(methodName);
        Assert.NotNull(method);
        var httpAttribute = Assert.Single(
            method.GetCustomAttributes(httpAttributeType, inherit: true));
        var template = Assert.IsAssignableFrom<HttpMethodAttribute>(
            httpAttribute).Template;
        Assert.Equal(route, template);
        var authorize = Assert.Single(
            method.GetCustomAttributes<AuthorizeAttribute>(inherit: true));
        Assert.Equal(roles, authorize.Roles);
    }

    private static IActionResult ConvertAction<T>(
        ActionResult<ApiResponse<T>> action)
    {
        return ((IConvertToActionResult)action).Convert();
    }

    private sealed class RecordingContractStub : IContractService
    {
        public ContractDetailDto ContractDetail { get; } =
            CreateContractDetail();

        public PagedResult<ContractSummaryDto> ContractList { get; } =
            new([], 1, 10, 0, false);

        public PagedResult<ContractStateHistoryDto> StateHistory { get; } =
            new([], 1, 100, 0, false);

        public ContractActionResultDto ActionResult { get; } =
            new(
                Guid.NewGuid(),
                ContractStatus.Draft.ToString(),
                new DateTime(2026, 7, 28, 10, 0, 0, DateTimeKind.Utc));

        public Exception? ExceptionToThrow { get; init; }
        public CreateContractRequest? CreateRequest { get; private set; }
        public ContractListQuery? ListQuery { get; private set; }
        public Guid? GetContractId { get; private set; }
        public UpdateContractRequest? UpdateRequest { get; private set; }
        public string? UpdateIfMatch { get; private set; }
        public string? AcceptIfMatch { get; private set; }
        public int AcceptCallCount { get; private set; }
        public ContractStateHistoryQuery? HistoryQuery { get; private set; }
        public TerminateContractRequest? TerminateRequest { get; private set; }
        public string? TerminateIfMatch { get; private set; }

        public Task<ContractDetailDto> CreateAsync(
            CreateContractRequest request,
            CancellationToken cancellationToken)
        {
            CreateRequest = request;
            return Task.FromResult(ContractDetail);
        }

        public Task<PagedResult<ContractSummaryDto>> ListAsync(
            ContractListQuery query,
            CancellationToken cancellationToken)
        {
            ListQuery = query;
            return Task.FromResult(ContractList);
        }

        public Task<ContractDetailDto> GetAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            if (ExceptionToThrow is not null)
            {
                return Task.FromException<ContractDetailDto>(
                    ExceptionToThrow);
            }

            GetContractId = contractId;
            return Task.FromResult(ContractDetail);
        }

        public Task<ContractDetailDto> UpdateDraftAsync(
            Guid contractId,
            UpdateContractRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            UpdateRequest = request;
            UpdateIfMatch = ifMatch;
            return Task.FromResult(ContractDetail);
        }

        public Task<ContractActionResultDto> AcceptAsync(
            Guid contractId,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            AcceptCallCount++;
            AcceptIfMatch = ifMatch;
            return Task.FromResult(ActionResult);
        }

        public Task<ContractActionResultDto> EvaluateActivationAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ActionResult);
        }

        public Task<PagedResult<ContractStateHistoryDto>>
            GetStateHistoryAsync(
                Guid contractId,
                ContractStateHistoryQuery query,
                CancellationToken cancellationToken)
        {
            HistoryQuery = query;
            return Task.FromResult(StateHistory);
        }

        public Task<ContractActionResultDto> EvaluateCompletionAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ActionResult);
        }

        public Task<ContractDetailDto> TerminateAsync(
            Guid contractId,
            TerminateContractRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            TerminateRequest = request;
            TerminateIfMatch = ifMatch;
            return Task.FromResult(ContractDetail);
        }

        private static ContractDetailDto CreateContractDetail()
        {
            return new ContractDetailDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Contract",
                "Contract terms",
                "EGP",
                ContractStatus.Draft,
                null,
                null,
                null,
                null,
                null,
                0m,
                [],
                [],
                []);
        }
    }
}
