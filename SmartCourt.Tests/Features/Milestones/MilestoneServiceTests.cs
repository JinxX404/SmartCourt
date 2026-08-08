using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;

using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestoneServiceTests
{
    private readonly DateTime _utcNow =
        new(2026, 8, 12, 9, 0, 0, DateTimeKind.Utc);
    private readonly Guid _contractId = Guid.NewGuid();
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _lawyerUserId = Guid.NewGuid();

    [Fact]
    public async Task AddAsync_RequiresTheNextSequentialOrder()
    {
        await using var context = CreateContext();
        var currentUser = new MutableCurrentUser(_clientUserId);
        var contracts = CreateContractStub(ContractStatus.Draft);
        var draftService = CreateDraftService(context, currentUser, contracts);

        var created = await draftService.AddAsync(
            _contractId,
            ValidAddRequest(1),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            draftService.AddAsync(
                _contractId,
                ValidAddRequest(3),
                CancellationToken.None));
        Assert.Equal(1, created.OrderNumber);
        Assert.Contains("2", exception.Message);
        Assert.Single(await context.Milestones.ToListAsync());
    }

    [Fact]
    public async Task UpdateDraftAsync_ResetsBothApprovals()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Draft, 1);
        milestone.AcceptedByClientAt = _utcNow.AddMinutes(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddMinutes(-1);
        await AddMilestonesAsync(context, milestone);
        var draftService = CreateDraftService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Draft));

        var result = await draftService.UpdateDraftAsync(
            _contractId,
            milestone.Id,
            new UpdateMilestoneRequest(
                "Updated filing",
                "Updated description",
                21,
                _utcNow.AddDays(21)),
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal("Updated filing", result.Title);
        Assert.Null(milestone.AcceptedByClientAt);
        Assert.Null(milestone.AcceptedByLawyerAt);
        Assert.Equal(MilestoneStatus.Draft, milestone.Status);
    }

    [Fact]
    public async Task ApproveAsync_TransitionsOnlyAfterBothParticipantsApprove()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Draft, 1);
        await AddMilestonesAsync(context, milestone);
        var currentUser = new MutableCurrentUser(_clientUserId);
        var contracts = CreateContractStub(ContractStatus.Draft);
        var service = CreateService(context, currentUser, contracts);

        var clientApproval = await service.ApproveAsync(
            milestone.Id,
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            MilestoneStatus.Draft.ToString(),
            clientApproval.Status);
        Assert.NotNull(milestone.AcceptedByClientAt);
        Assert.Null(milestone.AcceptedByLawyerAt);
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());

        currentUser.UserId = _lawyerUserId;
        var lawyerApproval = await service.ApproveAsync(
            milestone.Id,
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            MilestoneStatus.AwaitingFunding.ToString(),
            lawyerApproval.Status);
        Assert.Equal(MilestoneStatus.AwaitingFunding, milestone.Status);
        Assert.Single(await context.MilestoneStateHistories.ToListAsync());
        Assert.Equal(0, contracts.EvaluateActivationCallCount);
        var activationRequest = await context.OutboxMessages.SingleAsync();
        Assert.Equal(
            ContractPaymentEventTypes.ContractActivationRequested,
            activationRequest.EventType);
        Assert.Equal(_contractId, activationRequest.AggregateId);
        var payload = JsonSerializer.Deserialize<
            ContractActivationRequestedEventPayload>(
            activationRequest.Payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(payload);
        Assert.Equal(_contractId, payload.ContractId);
        Assert.Equal(_lawyerUserId, payload.RequestedByUserId);
    }

    [Fact]
    public async Task ApproveAsync_RejectsDuplicateAcceptance()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Draft, 1);
        milestone.AcceptedByClientAt = _utcNow.AddMinutes(-1);
        await AddMilestonesAsync(context, milestone);
        var service = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Draft));

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ApproveAsync(
                milestone.Id,
                ToETag(milestone.RowVersion),
                CancellationToken.None));

        Assert.Contains("مسبقًا", exception.Message);
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());
    }

    [Fact]
    public async Task MarkReadyForFundingAsync_WritesOneOutboxEvent()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(
            MilestoneStatus.AwaitingFunding,
            1);
        milestone.AcceptedByClientAt = _utcNow.AddMinutes(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddMinutes(-1);
        await AddMilestonesAsync(context, milestone);
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        var result = await service.MarkReadyForFundingAsync(
            milestone.Id,
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            MilestoneStatus.AwaitingFunding.ToString(),
            result.Status);
        Assert.Equal(_utcNow, milestone.ReadyForFundingAt);
        var outbox = await context.OutboxMessages.SingleAsync();
        Assert.Equal(
            ContractPaymentEventTypes.MilestoneReadyForFunding,
            outbox.EventType);
        Assert.Equal(milestone.Id, outbox.AggregateId);
    }

    [Fact]
    public async Task MarkReadyForFundingAsync_BlocksLaterUnsettledMilestone()
    {
        await using var context = CreateContext();
        var first = CreateMilestone(
            MilestoneStatus.AwaitingFunding,
            1);
        var second = CreateMilestone(
            MilestoneStatus.AwaitingFunding,
            2);
        await AddMilestonesAsync(context, first, second);
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.MarkReadyForFundingAsync(
                second.Id,
                ToETag(second.RowVersion),
                CancellationToken.None));

        Assert.Contains("المراحل السابقة", exception.Message);
        Assert.Empty(await context.OutboxMessages.ToListAsync());
    }

    [Fact]
    public async Task ListAsync_DerivesFundingAndPermittedActions()
    {
        await using var context = CreateContext();
        var draft = CreateMilestone(MilestoneStatus.Draft, 1);
        var funded = CreateMilestone(
            MilestoneStatus.FundedInProgress,
            2);
        funded.FundedAt = _utcNow.AddHours(-1);
        var hold = new EscrowHold(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _contractId,
            funded.Id,
            1_000m,
            50m,
            950m,
            Guid.NewGuid(),
            funded.FundedAt.Value,
            funded.FundedAt.Value);
        await AddMilestonesAsync(context, draft, funded);
        context.EscrowHolds.Add(hold);
        await context.SaveChangesAsync();
        var draftService = CreateDraftService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));

        var result = await draftService.ListAsync(
            _contractId,
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains("Update", result[0].PermittedActions);
        Assert.Contains("Approve", result[0].PermittedActions);
        Assert.Equal(
            MilestoneFundingStatus.Funded,
            result[1].FundingStatus);
        Assert.Equal(hold.Id, result[1].EscrowHoldId);
        Assert.Equal(950m, result[1].NetLawyerAmount);
    }

    [Fact]
    public async Task UpdateDraftAsync_RejectsNonDraftMilestone()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(
            MilestoneStatus.FundedInProgress,
            1);
        await AddMilestonesAsync(context, milestone);
        var draftService = CreateDraftService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Draft));

        await Assert.ThrowsAsync<BusinessException>(() =>
            draftService.UpdateDraftAsync(
                _contractId,
                milestone.Id,
                new UpdateMilestoneRequest(
                    "Updated filing",
                    null,
                    20,
                    null),
                ToETag(milestone.RowVersion),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateChangeRequestAsync_PersistsPendingRequestAndOutboxEvent()
    {
        await using var context = CreateContext();
        var milestone = CreateFundedMilestone();
        await AddMilestonesAsync(context, milestone);
        var changeRequestService = CreateChangeRequestService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));

        var result = await changeRequestService.CreateChangeRequestAsync(
            milestone.Id,
            new CreateMilestoneChangeRequest(
                "وصف محدث",
                21,
                _utcNow.AddDays(21),
                "تحتاج المرحلة إلى مدة إضافية."),
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        var request = await context.MilestoneChangeRequests.SingleAsync();
        Assert.Equal(request.Id, result.EntityId);
        Assert.Equal(ChangeRequestStatus.Pending, request.Status);
        Assert.Equal(_clientUserId, request.RequestedByUserId);
        Assert.Equal(
            ContractPaymentEventTypes.MilestoneChangeRequestCreated,
            (await context.OutboxMessages.SingleAsync()).EventType);
    }

    [Fact]
    public async Task CreateChangeRequestAsync_RejectsNonExtensionChanges()
    {
        await using var context = CreateContext();
        var milestone = CreateFundedMilestone();
        await AddMilestonesAsync(context, milestone);
        var changeRequestService = CreateChangeRequestService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            changeRequestService.CreateChangeRequestAsync(
                milestone.Id,
                new CreateMilestoneChangeRequest(
                    milestone.Description,
                    null,
                    null,
                    "لا يوجد تغيير فعلي."),
                ToETag(milestone.RowVersion),
                CancellationToken.None));

        Assert.Contains("تغييرًا فعليًا", exception.Message);
    }

    [Fact]
    public async Task ApproveChangeRequestAsync_OnlyOtherParticipantCanApproveAndPreservesFundingFacts()
    {
        await using var context = CreateContext();
        var milestone = CreateFundedMilestone();
        var originalAmount = milestone.Amount;
        var originalFundedAt = milestone.FundedAt;
        await AddMilestonesAsync(context, milestone);
        var currentUser = new MutableCurrentUser(_clientUserId);
        var contractStub = CreateContractStub(ContractStatus.Active);
        var changeRequestService = CreateChangeRequestService(
            context,
            currentUser,
            contractStub);
        var changeRequest = new MilestoneChangeRequest(
            Guid.NewGuid(),
            milestone.Id,
            _clientUserId,
            "وصف بعد التمديد",
            28,
            _utcNow.AddDays(28),
            "تحتاج المرحلة إلى مدة إضافية.",
            _utcNow)
        {
            RowVersion = [8, 8, 8, 8]
        };
        context.MilestoneChangeRequests.Add(changeRequest);
        await context.SaveChangesAsync();

        var requesterException = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            changeRequestService.ApproveChangeRequestAsync(
                changeRequest.Id,
                ToETag(changeRequest.RowVersion),
                CancellationToken.None));
        Assert.Contains("مقدم طلب", requesterException.Message);

        currentUser.UserId = _lawyerUserId;
        var result = await changeRequestService.ApproveChangeRequestAsync(
            changeRequest.Id,
            ToETag(changeRequest.RowVersion),
            CancellationToken.None);

        Assert.Equal(ChangeRequestStatus.Approved.ToString(), result.Status);
        Assert.Equal(ChangeRequestStatus.Approved, changeRequest.Status);
        Assert.Equal("وصف بعد التمديد", milestone.Description);
        Assert.Equal(28, milestone.DurationDays);
        Assert.Equal(_utcNow.AddDays(28), milestone.DueDate);
        Assert.Equal(originalAmount, milestone.Amount);
        Assert.Equal(originalFundedAt, milestone.FundedAt);
        Assert.Equal(MilestoneStatus.FundedInProgress, milestone.Status);
        Assert.Contains(
            ContractPaymentEventTypes.MilestoneChangeRequestApproved,
            (await context.OutboxMessages.ToListAsync()).Select(item => item.EventType));
    }

    [Fact]
    public async Task RejectChangeRequestAsync_StoresArabicDecisionReasonAndOutboxEvent()
    {
        await using var context = CreateContext();
        var milestone = CreateFundedMilestone();
        await AddMilestonesAsync(context, milestone);
        var currentUser = new MutableCurrentUser(_clientUserId);
        var contractStub = CreateContractStub(ContractStatus.Active);
        var changeRequestService = CreateChangeRequestService(
            context,
            currentUser,
            contractStub);
        var changeRequest = new MilestoneChangeRequest(
            Guid.NewGuid(),
            milestone.Id,
            _clientUserId,
            null,
            21,
            _utcNow.AddDays(21),
            "تحتاج المرحلة إلى مدة إضافية.",
            _utcNow)
        {
            RowVersion = [9, 9, 9, 9]
        };
        context.MilestoneChangeRequests.Add(changeRequest);
        await context.SaveChangesAsync();

        currentUser.UserId = _lawyerUserId;
        await changeRequestService.RejectChangeRequestAsync(
            changeRequest.Id,
            new RejectChangeRequest("لا توجد مستندات تبرر التمديد."),
            ToETag(changeRequest.RowVersion),
            CancellationToken.None);

        Assert.Equal(ChangeRequestStatus.Rejected, changeRequest.Status);
        Assert.Equal(
            "لا توجد مستندات تبرر التمديد.",
            changeRequest.DecisionReason);
        Assert.Contains(
            ContractPaymentEventTypes.MilestoneChangeRequestRejected,
            (await context.OutboxMessages.ToListAsync()).Select(item => item.EventType));
    }

    [Fact]
    public async Task SubmitAsync_ValidFundingCreatesImmutableVersionAndSchedulingEvent()
    {
        await using var context = CreateContext();
        var (milestone, hold, _) =
            await AddFundedChainAsync(context);
        var fileIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        var result = await service.SubmitAsync(
            milestone.Id,
            new SubmitMilestoneRequest(
                "تم إيداع مذكرة الدفاع والمرفقات.",
                fileIds),
            CancellationToken.None);

        Assert.Equal(MilestoneStatus.Submitted, result.Status);
        Assert.Equal(MilestoneStatus.Submitted, milestone.Status);
        Assert.Equal(_utcNow, milestone.SubmittedAt);
        Assert.Equal(
            _utcNow.AddDays(7),
            milestone.AutoAcceptEligibleAt);
        Assert.Equal(1, milestone.SubmissionVersion);
        var submission =
            await context.MilestoneSubmissions.SingleAsync();
        Assert.Equal(hold.Id, submission.EscrowHoldId);
        Assert.Equal(_lawyerUserId, submission.SubmittedByUserId);
        Assert.Equal(1, submission.Version);
        Assert.Equal(
            fileIds.OrderBy(id => id),
            (await context.MilestoneSubmissionAttachments
                .Select(attachment => attachment.StoredFileId)
                .ToListAsync())
            .OrderBy(id => id));
        var history =
            await context.MilestoneStateHistories.SingleAsync();
        Assert.Equal(
            MilestoneStatus.FundedInProgress,
            history.PreviousStatus);
        Assert.Equal(MilestoneStatus.Submitted, history.NewStatus);

        var message = await context.OutboxMessages.SingleAsync(
            item =>
                item.EventType
                    == ContractPaymentEventTypes.MilestoneSubmitted);
        var payload =
            JsonSerializer.Deserialize<MilestoneSubmissionEventPayload>(
                message.Payload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        Assert.NotNull(payload);
        Assert.Equal(milestone.Id, payload.MilestoneId);
        Assert.Equal(hold.Id, payload.EscrowHoldId);
        Assert.Equal(1, payload.SubmissionVersion);
    }

    [Fact]
    public async Task SubmitAsync_InvalidFundingChainCreatesNoSubmissionOrEvent()
    {
        await using var context = CreateContext();
        var (milestone, _, paymentTransaction) =
            await AddFundedChainAsync(context);
        paymentTransaction.Amount = milestone.Amount + 1m;
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.SubmitAsync(
                milestone.Id,
                new SubmitMilestoneRequest(
                    "تم إيداع مستندات المرحلة.",
                    [Guid.NewGuid()]),
                CancellationToken.None));

        Assert.Contains("تمويل", exception.Message);
        Assert.Equal(
            MilestoneStatus.FundedInProgress,
            milestone.Status);
        Assert.Null(milestone.SubmittedAt);
        Assert.Null(milestone.AutoAcceptEligibleAt);
        Assert.Empty(await context.MilestoneSubmissions.ToListAsync());
        Assert.Empty(
            await context.MilestoneSubmissionAttachments.ToListAsync());
        Assert.DoesNotContain(
            await context.OutboxMessages.ToListAsync(),
            message =>
                message.EventType
                    == ContractPaymentEventTypes.MilestoneSubmitted);
    }

    [Fact]
    public async Task SubmitAsync_RejectsClientAndUnauthorizedFilesWithoutMutation()
    {
        await using var context = CreateContext();
        var (milestone, _, _) =
            await AddFundedChainAsync(context);
        var request = new SubmitMilestoneRequest(
            "تم إيداع مستندات المرحلة.",
            [Guid.NewGuid()]);
        var clientService = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            clientService.SubmitAsync(
                milestone.Id,
                request,
                CancellationToken.None));

        var deniedFileService = new TestFileAccessService
        {
            DenyUse = true
        };
        var lawyerService = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active),
            deniedFileService);
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            lawyerService.SubmitAsync(
                milestone.Id,
                request,
                CancellationToken.None));

        Assert.Equal(
            MilestoneStatus.FundedInProgress,
            milestone.Status);
        Assert.Empty(await context.MilestoneSubmissions.ToListAsync());
    }

    [Fact]
    public async Task SubmitAsync_ResubmissionUsesNextImmutableVersionAndNewDeadline()
    {
        await using var context = CreateContext();
        var (milestone, hold, _) =
            await AddFundedChainAsync(context);
        milestone.SubmissionVersion = 1;
        context.MilestoneSubmissions.Add(
            new MilestoneSubmission(
                Guid.NewGuid(),
                milestone.Id,
                hold.Id,
                _lawyerUserId,
                1,
                "التسليم الأول.",
                _utcNow.AddDays(-2)));
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        await service.SubmitAsync(
            milestone.Id,
            new SubmitMilestoneRequest(
                "التسليم الثاني بعد تنفيذ التعديلات.",
                [Guid.NewGuid()]),
            CancellationToken.None);

        var submissions = await context.MilestoneSubmissions
            .OrderBy(submission => submission.Version)
            .ToListAsync();
        Assert.Equal([1, 2], submissions.Select(
            submission => submission.Version));
        Assert.Equal(2, milestone.SubmissionVersion);
        Assert.Equal(
            _utcNow.AddDays(7),
            milestone.AutoAcceptEligibleAt);
    }

    [Fact]
    public async Task AcceptAsync_ValidCurrentSubmissionStartsFourteenDayHold()
    {
        await using var context = CreateContext();
        var (milestone, hold, _) =
            await AddSubmittedChainAsync(context);
        var service = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));

        var result = await service.AcceptAsync(
            milestone.Id,
            CancellationToken.None);

        Assert.Equal(MilestoneStatus.AcceptedHold, result.Status);
        Assert.Equal(MilestoneStatus.AcceptedHold, milestone.Status);
        Assert.Equal(MilestoneAcceptanceSource.Manual, milestone.AcceptanceSource);
        Assert.Equal(_utcNow, milestone.AcceptedAt);
        Assert.Equal(_utcNow, milestone.HoldStartsAt);
        Assert.Equal(_utcNow.AddDays(14), milestone.HoldExpiresAt);
        Assert.Null(milestone.AutoAcceptEligibleAt);
        Assert.Null(milestone.AutoAcceptJobId);
        Assert.Equal(EscrowHoldStatus.Funded, hold.Status);
        Assert.Equal(_utcNow, hold.HoldStartsAt);
        Assert.Equal(_utcNow.AddDays(14), hold.HoldExpiresAt);
        var history = await context.MilestoneStateHistories.SingleAsync();
        Assert.Equal(MilestoneStatus.Submitted, history.PreviousStatus);
        Assert.Equal(MilestoneStatus.AcceptedHold, history.NewStatus);

        var message = await context.OutboxMessages.SingleAsync(
            item =>
                item.EventType
                    == ContractPaymentEventTypes.MilestoneAccepted);
        var payload =
            JsonSerializer.Deserialize<MilestoneAcceptanceEventPayload>(
                message.Payload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        Assert.NotNull(payload);
        Assert.Equal(milestone.Id, payload.MilestoneId);
        Assert.Equal(hold.Id, payload.EscrowHoldId);
    }

    [Fact]
    public async Task AcceptAsync_InvalidFundingOrSubmissionVersionDoesNotMutate()
    {
        await using var context = CreateContext();
        var (milestone, hold, paymentTransaction) =
            await AddSubmittedChainAsync(context);
        paymentTransaction.Amount = milestone.Amount + 1m;
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.AcceptAsync(
                milestone.Id,
                CancellationToken.None));

        Assert.Equal(MilestoneStatus.Submitted, milestone.Status);
        Assert.Null(milestone.AcceptedAt);
        Assert.Null(milestone.HoldStartsAt);
        Assert.Null(milestone.HoldExpiresAt);
        Assert.Null(hold.HoldStartsAt);
        Assert.Null(hold.HoldExpiresAt);
        Assert.DoesNotContain(
            await context.OutboxMessages.ToListAsync(),
            item =>
                item.EventType
                    == ContractPaymentEventTypes.MilestoneAccepted);
    }

    [Fact]
    public async Task RequestChangesAsync_PreservesFundingAndImmutableSubmission()
    {
        await using var context = CreateContext();
        var (milestone, hold, paymentTransaction) =
            await AddSubmittedChainAsync(context);
        milestone.AutoAcceptJobId = "old-auto-accept-job";
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));
        const string reason =
            "يرجى استكمال المستندات وتصحيح بيانات الطلب.";

        var result = await service.RequestChangesAsync(
            milestone.Id,
            new RequestMilestoneChangesRequest(reason),
            CancellationToken.None);

        Assert.Equal(MilestoneStatus.FundedInProgress, result.Status);
        Assert.Equal(
            MilestoneStatus.FundedInProgress,
            milestone.Status);
        Assert.Null(milestone.SubmittedAt);
        Assert.Null(milestone.AutoAcceptEligibleAt);
        Assert.Null(milestone.AutoAcceptJobId);
        Assert.Equal(reason, milestone.RejectionReason);
        Assert.Equal(1, milestone.SubmissionVersion);
        Assert.Single(await context.MilestoneSubmissions.ToListAsync());
        Assert.Single(await context.EscrowHolds.ToListAsync());
        Assert.Single(await context.PaymentTransactions.ToListAsync());
        Assert.Equal(EscrowHoldStatus.Funded, hold.Status);
        Assert.Equal(
            PaymentTransactionStatus.Completed,
            paymentTransaction.Status);
        var history = await context.MilestoneStateHistories.SingleAsync();
        Assert.Equal(MilestoneStatus.Submitted, history.PreviousStatus);
        Assert.Equal(
            MilestoneStatus.FundedInProgress,
            history.NewStatus);
        Assert.Contains(reason, history.Reason);
        Assert.Contains(
            ContractPaymentEventTypes.MilestoneChangesRequested,
            (await context.OutboxMessages.ToListAsync())
            .Select(item => item.EventType));
    }

    [Fact]
    public async Task ReviewCommands_RequireClientAndCurrentSubmissionVersion()
    {
        await using var context = CreateContext();
        var (milestone, _, _) =
            await AddSubmittedChainAsync(context);
        var lawyerService = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            lawyerService.AcceptAsync(
                milestone.Id,
                CancellationToken.None));

        milestone.SubmissionVersion = 2;
        await context.SaveChangesAsync();
        var clientService = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));
        await Assert.ThrowsAsync<BusinessException>(() =>
            clientService.RequestChangesAsync(
                milestone.Id,
                new RequestMilestoneChangesRequest(
                    "يرجى تعديل التسليم الحالي."),
                CancellationToken.None));

        Assert.Equal(MilestoneStatus.Submitted, milestone.Status);
        Assert.NotNull(milestone.SubmittedAt);
        Assert.NotNull(milestone.AutoAcceptEligibleAt);
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());
    }

    private Milestone CreateFundedMilestone()
    {
        var milestone = CreateMilestone(MilestoneStatus.FundedInProgress, 1);
        milestone.FundedAt = _utcNow.AddHours(-1);
        return milestone;
    }

    private async Task<(
        Milestone Milestone,
        EscrowHold Hold,
        PaymentTransaction PaymentTransaction)> AddFundedChainAsync(
            ApplicationDbContext context)
    {
        var milestone = CreateFundedMilestone();
        var account = new EscrowAccount(
            Guid.NewGuid(),
            _contractId,
            _utcNow.AddHours(-1));
        var transactionId = Guid.NewGuid();
        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            _contractId,
            milestone.Id,
            milestone.Amount,
            50m,
            950m,
            transactionId,
            milestone.FundedAt!.Value,
            milestone.FundedAt.Value);
        var paymentTransaction = new PaymentTransaction(
            transactionId,
            _contractId,
            milestone.Id,
            PaymentOperationType.Deposit,
            "MockPaymentProvider",
            $"funding-{Guid.NewGuid():N}",
            milestone.Amount,
            milestone.FundedAt.Value)
        {
            EscrowHoldId = hold.Id,
            ProviderTransactionId =
                $"provider-{Guid.NewGuid():N}",
            Status = PaymentTransactionStatus.Completed,
            ProcessedAt = milestone.FundedAt.Value,
            UpdatedAt = milestone.FundedAt.Value
        };
        context.Milestones.Add(milestone);
        context.EscrowAccounts.Add(account);
        context.EscrowHolds.Add(hold);
        context.PaymentTransactions.Add(paymentTransaction);
        await context.SaveChangesAsync();
        return (milestone, hold, paymentTransaction);
    }

    private async Task<(
        Milestone Milestone,
        EscrowHold Hold,
        PaymentTransaction PaymentTransaction)> AddSubmittedChainAsync(
            ApplicationDbContext context)
    {
        var (milestone, hold, paymentTransaction) =
            await AddFundedChainAsync(context);
        milestone.Status = MilestoneStatus.Submitted;
        milestone.SubmittedAt = _utcNow.AddHours(-1);
        milestone.AutoAcceptEligibleAt = _utcNow.AddDays(7);
        milestone.SubmissionVersion = 1;
        context.MilestoneSubmissions.Add(
            new MilestoneSubmission(
                Guid.NewGuid(),
                milestone.Id,
                hold.Id,
                _lawyerUserId,
                1,
                "تم تسليم أعمال المرحلة للمراجعة.",
                milestone.SubmittedAt.Value));
        await context.SaveChangesAsync();
        return (milestone, hold, paymentTransaction);
    }

    private MilestoneService CreateService(
        ApplicationDbContext context,
        MutableCurrentUser currentUser,
        ContractServiceStub contracts,
        IContractFileAccessService? fileAccessService = null)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new MilestoneService(
            context,
            currentUser,
            contracts,
            new MilestoneFundingVerifier(context),
            fileAccessService ?? new TestFileAccessService(),
            new OutboxWriter(context, timeProvider),
            timeProvider);
    }

    private MilestoneDraftService CreateDraftService(
        ApplicationDbContext context,
        MutableCurrentUser currentUser,
        ContractServiceStub contracts)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new MilestoneDraftService(
            context,
            currentUser,
            contracts,
            timeProvider);
    }

    private MilestoneChangeRequestService CreateChangeRequestService(
        ApplicationDbContext context,
        MutableCurrentUser currentUser,
        ContractServiceStub contracts)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new MilestoneChangeRequestService(
            context,
            currentUser,
            contracts,
            new OutboxWriter(context, timeProvider),
            timeProvider);
    }

    private ContractServiceStub CreateContractStub(
        ContractStatus status)
    {
        return new ContractServiceStub(
            new ContractDetailDto(
                _contractId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                _clientUserId,
                _lawyerUserId,
                "Legal representation",
                "Contract terms",
                "EGP",
                status,
                null,
                null,
                status == ContractStatus.Active ? _utcNow : null,
                null,
                null,
                0m,
                string.Empty,
                [],
                [],
                []),
            _utcNow);
    }

    private AddMilestoneRequest ValidAddRequest(int orderNumber)
    {
        return new AddMilestoneRequest(
            $"Milestone {orderNumber}",
            null,
            orderNumber,
            1_000m,
            14,
            _utcNow.AddDays(14));
    }

    private Milestone CreateMilestone(
        MilestoneStatus status,
        int orderNumber)
    {
        return new Milestone(
            Guid.NewGuid(),
            _contractId,
            $"Milestone {orderNumber}",
            null,
            orderNumber,
            1_000m,
            14,
            _utcNow.AddDays(14),
            _utcNow.AddHours(-1))
        {
            Status = status,
            RowVersion = [1, 2, 3, (byte)orderNumber]
        };
    }

    private static async Task AddMilestonesAsync(
        ApplicationDbContext context,
        params Milestone[] milestones)
    {
        context.Milestones.AddRange(milestones);
        await context.SaveChangesAsync();
        foreach (var milestone in milestones)
        {
            if (milestone.RowVersion.Length == 0)
            {
                milestone.RowVersion = [1, 2, 3, 4];
            }
        }
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"milestone-service-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private static string ToETag(byte[] rowVersion)
        => $"\"{Convert.ToBase64String(rowVersion)}\"";

    private sealed class MutableCurrentUser(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; set; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class TestFileAccessService
        : IContractFileAccessService
    {
        public bool DenyUse { get; set; }

        public Task<IReadOnlyList<AuthorizedContractFile>>
            AuthorizeForUseAsync(
                Guid actorUserId,
                IReadOnlyCollection<Guid> storedFileIds,
                ContractFilePurpose purpose,
                Guid relatedEntityId,
                CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<AuthorizedContractFile> result = DenyUse
                ? []
                : storedFileIds
                    .Select(fileId => new AuthorizedContractFile(
                        fileId,
                        actorUserId))
                    .ToArray();
            return Task.FromResult(result);
        }

        public Task<ContractFileReadAccess?>
            GetAuthorizedReadAccessAsync(
                Guid actorUserId,
                Guid storedFileId,
                ContractFilePurpose purpose,
                Guid relatedEntityId,
                CancellationToken cancellationToken)
        {
            return Task.FromResult<ContractFileReadAccess?>(null);
        }
    }

    private sealed class ContractServiceStub(
        ContractDetailDto contract,
        DateTime utcNow) : IContractService
    {
        public int EvaluateActivationCallCount { get; private set; }

        public Task<ContractDetailDto> GetAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(contract);
        }

        public Task<ContractActionResultDto> EvaluateActivationAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EvaluateActivationCallCount++;
            return Task.FromResult(
                new ContractActionResultDto(
                    contractId,
                    contract.Status.ToString(),
                    utcNow));
        }

        public Task<ContractDetailDto> CreateAsync(
            CreateContractRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PagedResult<ContractSummaryDto>> ListAsync(
            ContractListQuery query,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractDetailDto> UpdateDraftAsync(
            Guid contractId,
            UpdateContractRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractActionResultDto> AcceptAsync(
            Guid contractId,
            string ifMatch,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PagedResult<ContractStateHistoryDto>>
            GetStateHistoryAsync(
                Guid contractId,
                ContractStateHistoryQuery query,
                CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractActionResultDto> EvaluateCompletionAsync(
            Guid contractId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractDetailDto> TerminateAsync(
            Guid contractId,
            TerminateContractRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(utcNow);
    }
}
