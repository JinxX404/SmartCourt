using SmartCourt.Features.Cases.Integration;
using SmartCourt.Features.Chat.Integration;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Notifications.Integration;
using SmartCourt.Features.Proposals.Integration;
using SmartCourt.Features.Users.Integration;

namespace SmartCourt.Tests.TestDoubles.ContractAndPayment;

public sealed class FakeProposalContractAccessService : IProposalContractAccessService
{
    public AcceptedProposalContractFacts? AcceptedProposal { get; set; }
    public int CallCount { get; private set; }

    public async Task<AcceptedProposalContractFacts?> FindAcceptedForContractAsync(
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        return await Task.FromResult(AcceptedProposal);
    }
}

public sealed class FakeCaseContractAccessService : ICaseContractAccessService
{
    public CaseContractEligibilityFacts? EligibleCase { get; set; }
    public int CallCount { get; private set; }

    public async Task<CaseContractEligibilityFacts?> FindEligibleForContractAsync(
        Guid legalCaseId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        return await Task.FromResult(EligibleCase);
    }
}

public sealed class FakeContractUserEligibilityService : IContractUserEligibilityService
{
    public Dictionary<Guid, ContractUserEligibilityFacts> Results { get; } = [];
    public int CallCount { get; private set; }

    public async Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        Results.TryGetValue(userId, out var result);
        return await Task.FromResult(result);
    }
}

public sealed class FakeContractConversationService : IContractConversationService
{
    public List<ContractConversationSystemMessage> Messages { get; } = [];

    public async Task AppendSystemMessageAsync(
        ContractConversationSystemMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Messages.Add(message);
        await Task.CompletedTask;
    }
}

public sealed class FakeContractFileAccessService : IContractFileAccessService
{
    public IReadOnlyList<AuthorizedContractFile> AuthorizedFiles { get; set; } =
        Array.Empty<AuthorizedContractFile>();

    public ContractFileReadAccess? ReadAccess { get; set; }

    public async Task<IReadOnlyList<AuthorizedContractFile>> AuthorizeForUseAsync(
        Guid actorUserId,
        IReadOnlyCollection<Guid> storedFileIds,
        ContractFilePurpose purpose,
        Guid relatedEntityId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult(AuthorizedFiles);
    }

    public async Task<ContractFileReadAccess?> GetAuthorizedReadAccessAsync(
        Guid actorUserId,
        Guid storedFileId,
        ContractFilePurpose purpose,
        Guid relatedEntityId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult(ReadAccess);
    }
}

public sealed class FakeContractNotificationService : IContractNotificationService
{
    public List<ContractNotification> Notifications { get; } = [];

    public async Task PublishAsync(
        ContractNotification notification,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Notifications.Add(notification);
        await Task.CompletedTask;
    }
}
