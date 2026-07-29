using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Validators;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Enums;
using System.Reflection;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestonesControllerTests
{
    private const string ValidIfMatch = "\"AQIDBA==\"";

    [Fact]
    public async Task ReadsAndCreates_ReturnWrappedResponses()
    {
        var service = new RecordingMilestoneStub();
        var controller = CreateController(service);
        var contractId = Guid.NewGuid();

        var addAction = await controller.AddAsync(
            contractId,
            new AddMilestoneRequest(
                "إيداع الدعوى",
                "إعداد وإيداع المستندات.",
                1,
                5_000m,
                14,
                null),
            CancellationToken.None);
        var listAction = await controller.ListAsync(
            contractId,
            CancellationToken.None);
        var changeRequestAction = await controller.CreateChangeRequestAsync(
            Guid.NewGuid(),
            new CreateMilestoneChangeRequest(
                "وصف محدث",
                21,
                null,
                "تحتاج المرحلة إلى وقت إضافي."),
            ValidIfMatch,
            CancellationToken.None);
        var changeRequestResult = ConvertAction(changeRequestAction);

        var addResult = Assert.IsType<CreatedAtActionResult>(
            ConvertAction(addAction));
        AssertWrapped(addResult.Value, service.Milestone);
        Assert.Equal(StatusCodes.Status201Created, addResult.StatusCode);
        AssertWrappedOk(listAction, service.Milestones);
        Assert.Equal(
            StatusCodes.Status201Created,
            Assert.IsType<ObjectResult>(changeRequestResult).StatusCode);
        AssertWrapped(
            Assert.IsType<ObjectResult>(changeRequestResult).Value,
            service.ActionResult);
        Assert.Equal(ValidIfMatch, service.LastIfMatch);
    }

    [Fact]
    public async Task Mutations_ForwardValidatedIfMatchAndWrapResponses()
    {
        var service = new RecordingMilestoneStub();
        var controller = CreateController(service);
        var contractId = Guid.NewGuid();
        var milestoneId = Guid.NewGuid();
        var changeRequestId = Guid.NewGuid();

        var update = await controller.UpdateAsync(
            contractId,
            milestoneId,
            new UpdateMilestoneRequest(
                "تعديل",
                "وصف معدل.",
                15,
                null),
            ValidIfMatch,
            CancellationToken.None);
        var approve = await controller.ApproveAsync(
            milestoneId,
            ValidIfMatch,
            CancellationToken.None);
        var ready = await controller.MarkReadyForFundingAsync(
            milestoneId,
            ValidIfMatch,
            CancellationToken.None);
        var approveRequest = await controller.ApproveChangeRequestAsync(
            changeRequestId,
            ValidIfMatch,
            CancellationToken.None);
        var rejectRequest = await controller.RejectChangeRequestAsync(
            changeRequestId,
            new RejectChangeRequest("السبب"),
            ValidIfMatch,
            CancellationToken.None);
        var cancelRequest = await controller.CancelChangeRequestAsync(
            changeRequestId,
            ValidIfMatch,
            CancellationToken.None);

        AssertWrappedOk(update, service.Milestone);
        AssertWrappedOk(approve, service.ActionResult);
        AssertWrappedOk(ready, service.ActionResult);
        AssertWrappedOk(approveRequest, service.ActionResult);
        AssertWrappedOk(rejectRequest, service.ActionResult);
        AssertWrappedOk(cancelRequest, service.ActionResult);
        Assert.Equal(6, service.IfMatchCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("W/\"AQIDBA==\"")]
    public async Task InvalidIfMatch_FailsBeforeServiceCall(
        string? ifMatch)
    {
        var service = new RecordingMilestoneStub();
        var controller = CreateController(service);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            controller.ApproveAsync(
                Guid.NewGuid(),
                ifMatch,
                CancellationToken.None));

        Assert.Contains("If-Match", exception.Message);
        Assert.Equal(0, service.ApproveCallCount);
    }

    [Fact]
    public void Endpoints_DefineExpectedRoutesAndRoles()
    {
        AssertEndpoint(
            nameof(MilestonesController.AddAsync),
            typeof(HttpPostAttribute),
            "contracts/{contractId:guid}/milestones",
            "Client,Lawyer");
        AssertEndpoint(
            nameof(MilestonesController.ListAsync),
            typeof(HttpGetAttribute),
            "contracts/{contractId:guid}/milestones",
            "Client,Lawyer,Moderator,SuperAdministrator");
        AssertEndpoint(
            nameof(MilestonesController.UpdateAsync),
            typeof(HttpPutAttribute),
            "contracts/{contractId:guid}/milestones/{milestoneId:guid}",
            "Client,Lawyer");
        AssertEndpoint(
            nameof(MilestonesController.MarkReadyForFundingAsync),
            typeof(HttpPostAttribute),
            "milestones/{milestoneId:guid}/ready-for-funding",
            "Lawyer");
        AssertEndpoint(
            nameof(MilestonesController.RejectChangeRequestAsync),
            typeof(HttpPostAttribute),
            "change-requests/{changeRequestId:guid}/reject",
            "Client,Lawyer");
    }

    private static MilestonesController CreateController(
        RecordingMilestoneStub service)
    {
        return new MilestonesController(
            service,
            new IfMatchRequestValidator());
    }

    private static void AssertWrappedOk<T>(
        ActionResult<ApiResponse<T>> action,
        T expected)
    {
        var result = Assert.IsType<OkObjectResult>(
            ConvertAction(action));
        AssertWrapped(result.Value, expected);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    private static void AssertWrapped<T>(
        object? value,
        T expected)
    {
        var response = Assert.IsType<ApiResponse<T>>(value);
        Assert.True(response.Success);
        Assert.Equal(expected, response.Data);
    }

    private static IActionResult ConvertAction<T>(
        ActionResult<ApiResponse<T>> action)
    {
        return ((IConvertToActionResult)action).Convert();
    }

    private static void AssertEndpoint(
        string methodName,
        Type httpAttributeType,
        string route,
        string roles)
    {
        var method = typeof(MilestonesController).GetMethod(methodName);
        Assert.NotNull(method);
        var http = Assert.Single(
            method.GetCustomAttributes(httpAttributeType, inherit: true));
        Assert.Equal(
            route,
            Assert.IsAssignableFrom<HttpMethodAttribute>(http).Template);
        var authorize = Assert.Single(
            method.GetCustomAttributes<AuthorizeAttribute>(inherit: true));
        Assert.Equal(roles, authorize.Roles);
    }

    private sealed class RecordingMilestoneStub : IMilestoneService
    {
        public MilestoneDto Milestone { get; } =
            new(
                Guid.NewGuid(),
                1,
                "مرحلة",
                null,
                1_000m,
                14,
                null,
                MilestoneStatus.Draft,
                MilestoneFundingStatus.Unfunded,
                null,
                null,
                null,
                null,
                null,
                null);

        public IReadOnlyList<MilestoneDto> Milestones { get; }
            = [];

        public MilestoneActionResultDto ActionResult { get; } =
            new(
                Guid.NewGuid(),
                MilestoneStatus.Draft.ToString(),
                new DateTime(2026, 7, 29, 8, 0, 0, DateTimeKind.Utc));

        public string? LastIfMatch { get; private set; }
        public int IfMatchCallCount { get; private set; }
        public int ApproveCallCount { get; private set; }

        public Task<MilestoneDto> AddAsync(
            Guid contractId,
            AddMilestoneRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(Milestone);

        public Task<IReadOnlyList<MilestoneDto>> ListAsync(
            Guid contractId,
            CancellationToken cancellationToken)
            => Task.FromResult(Milestones);

        public Task<MilestoneDto> UpdateDraftAsync(
            Guid contractId,
            Guid milestoneId,
            UpdateMilestoneRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            RecordIfMatch(ifMatch);
            return Task.FromResult(Milestone);
        }

        public Task<MilestoneActionResultDto> ApproveAsync(
            Guid milestoneId,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            ApproveCallCount++;
            RecordIfMatch(ifMatch);
            return Task.FromResult(ActionResult);
        }

        public Task<MilestoneActionResultDto> MarkReadyForFundingAsync(
            Guid milestoneId,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            RecordIfMatch(ifMatch);
            return Task.FromResult(ActionResult);
        }

        public Task<MilestoneActionResultDto> CreateChangeRequestAsync(
            Guid milestoneId,
            CreateMilestoneChangeRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            RecordIfMatch(ifMatch);
            return Task.FromResult(ActionResult);
        }

        public Task<MilestoneActionResultDto> ApproveChangeRequestAsync(
            Guid changeRequestId,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            RecordIfMatch(ifMatch);
            return Task.FromResult(ActionResult);
        }

        public Task<MilestoneActionResultDto> RejectChangeRequestAsync(
            Guid changeRequestId,
            RejectChangeRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            RecordIfMatch(ifMatch);
            return Task.FromResult(ActionResult);
        }

        public Task<MilestoneActionResultDto> CancelChangeRequestAsync(
            Guid changeRequestId,
            string ifMatch,
            CancellationToken cancellationToken)
        {
            RecordIfMatch(ifMatch);
            return Task.FromResult(ActionResult);
        }

        private void RecordIfMatch(string ifMatch)
        {
            LastIfMatch = ifMatch;
            IfMatchCallCount++;
        }
    }
}
