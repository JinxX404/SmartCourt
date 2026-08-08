using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals;
using SmartCourt.Features.Proposals.AcceptProposal;
using SmartCourt.Features.Proposals.CreateProposal;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.GetProposal;
using SmartCourt.Features.Proposals.GetProposals;
using SmartCourt.Features.Proposals.RejectProposal;
using Xunit;

namespace SmartCourt.Tests.Features.Proposals;

public sealed class ProposalsControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedAndMapsRequestToCommand()
    {
        var mediator = new RecordingMediator();
        var controller = new ProposalsController(mediator);
        var request = new CreateProposalRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "We would like to propose representation.");

        var action = await controller.CreateAsync(request, CancellationToken.None);

        var result = Assert.IsType<ObjectResult>(Convert(action));
        var response = Assert.IsType<ApiResponse<ProposalDetailDto>>(result.Value);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.True(response.Success);
        Assert.Same(mediator.Detail, response.Data);
        Assert.Equal(request.LegalCaseId, mediator.CreateCommand!.LegalCaseId);
        Assert.Equal(request.LawyerUserId, mediator.CreateCommand!.LawyerUserId);
        Assert.Equal(request.Message, mediator.CreateCommand!.Message);
    }

    [Fact]
    public async Task ReadsAndMutates_ReturnWrappedResponses()
    {
        var mediator = new RecordingMediator();
        var controller = new ProposalsController(mediator);
        var proposalId = Guid.NewGuid();
        var query = new GetProposalsQuery(
            ProposalInboxDirection.Received,
            null,
            "search");

        var listAction = await controller.ListAsync(query, CancellationToken.None);
        var getAction = await controller.GetAsync(proposalId, CancellationToken.None);
        var acceptAction = await controller.AcceptAsync(proposalId, CancellationToken.None);
        var rejectAction = await controller.RejectAsync(
            proposalId,
            new RejectProposalRequest("Not the right fit"),
            CancellationToken.None);

        AssertWrappedOk(listAction, mediator.Page);
        AssertWrappedOk(getAction, mediator.Detail);
        AssertWrappedOk(acceptAction, mediator.Detail);
        AssertWrappedOk(rejectAction, mediator.Detail);
        Assert.Same(query, mediator.ListQuery);
        Assert.Equal(proposalId, mediator.GetQuery!.ProposalId);
        Assert.Equal(proposalId, mediator.AcceptCommand!.ProposalId);
        Assert.Equal(proposalId, mediator.RejectCommand!.ProposalId);
        Assert.Equal("Not the right fit", mediator.RejectCommand!.Reason);
    }

    [Fact]
    public void Endpoints_DefineExpectedRoutesAndRoles()
    {
        AssertEndpoint(
            nameof(ProposalsController.CreateAsync),
            typeof(HttpPostAttribute),
            null,
            "Client");
        AssertEndpoint(
            nameof(ProposalsController.ListAsync),
            typeof(HttpGetAttribute),
            null,
            "Client,Lawyer");
        AssertEndpoint(
            nameof(ProposalsController.GetAsync),
            typeof(HttpGetAttribute),
            "{proposalId:guid}",
            "Client,Lawyer");
        AssertEndpoint(
            nameof(ProposalsController.AcceptAsync),
            typeof(HttpPostAttribute),
            "{proposalId:guid}/accept",
            "Lawyer");
        AssertEndpoint(
            nameof(ProposalsController.RejectAsync),
            typeof(HttpPostAttribute),
            "{proposalId:guid}/reject",
            "Lawyer");
    }

    private static void AssertWrappedOk<T>(
        ActionResult<ApiResponse<T>> action,
        T expected)
    {
        var result = Assert.IsType<ObjectResult>(Convert(action));
        var response = Assert.IsType<ApiResponse<T>>(result.Value);
        Assert.True(response.Success);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(expected, response.Data);
    }

    private static IActionResult Convert<T>(ActionResult<ApiResponse<T>> action)
    {
        return ((IConvertToActionResult)action).Convert();
    }

    private static void AssertEndpoint(
        string methodName,
        Type httpAttributeType,
        string? route,
        string roles)
    {
        var method = typeof(ProposalsController).GetMethod(methodName);
        Assert.NotNull(method);
        var httpAttribute = Assert.Single(
            method.GetCustomAttributes(httpAttributeType, inherit: true));
        Assert.Equal(
            route,
            Assert.IsAssignableFrom<HttpMethodAttribute>(httpAttribute).Template);
        var authorize = Assert.Single(
            method.GetCustomAttributes<AuthorizeAttribute>(inherit: true));
        Assert.Equal(roles, authorize.Roles);
    }

    private sealed class RecordingMediator : IMediator
    {
        private readonly ProposalDetailDto _detail = CreateDetail();
        private readonly ProposalPageDto _page = new([], 1, 10, 0, false);

        public ProposalDetailDto Detail => _detail;
        public ProposalPageDto Page => _page;
        public ApiResponse<ProposalDetailDto> CreatedDetail { get; }
        public ApiResponse<ProposalDetailDto> DetailResponse { get; }
        public ApiResponse<ProposalPageDto> PageResponse { get; }
        public CreateProposalCommand? CreateCommand { get; private set; }
        public GetProposalsQuery? ListQuery { get; private set; }
        public GetProposalQuery? GetQuery { get; private set; }
        public AcceptProposalCommand? AcceptCommand { get; private set; }
        public RejectProposalCommand? RejectCommand { get; private set; }

        public RecordingMediator()
        {
            CreatedDetail = ApiResponse<ProposalDetailDto>.Created(_detail);
            DetailResponse = ApiResponse<ProposalDetailDto>.Ok(_detail);
            PageResponse = ApiResponse<ProposalPageDto>.Ok(_page);
        }

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case CreateProposalCommand command:
                    CreateCommand = command;
                    return Task.FromResult((TResponse)(object)CreatedDetail);
                case GetProposalsQuery query:
                    ListQuery = query;
                    return Task.FromResult((TResponse)(object)PageResponse);
                case GetProposalQuery query:
                    GetQuery = query;
                    return Task.FromResult((TResponse)(object)DetailResponse);
                case AcceptProposalCommand command:
                    AcceptCommand = command;
                    return Task.FromResult((TResponse)(object)DetailResponse);
                case RejectProposalCommand command:
                    RejectCommand = command;
                    return Task.FromResult((TResponse)(object)DetailResponse);
                default:
                    throw new NotSupportedException(request.GetType().Name);
            }
        }

        public Task<object?> Send(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;

        private static ProposalDetailDto CreateDetail()
        {
            return new ProposalDetailDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Case title",
                Guid.NewGuid(),
                "Client",
                Guid.NewGuid(),
                "Lawyer",
                "Message",
                ProposalStatus.Pending.ToString(),
                null,
                new DateTime(2026, 7, 30, 9, 0, 0, DateTimeKind.Utc),
                null,
                new DateTime(2026, 7, 30, 9, 0, 0, DateTimeKind.Utc));
        }

    }
}

