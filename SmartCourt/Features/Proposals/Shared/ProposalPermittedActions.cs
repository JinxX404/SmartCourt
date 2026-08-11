using SmartCourt.Features.Proposals.Enums;

namespace SmartCourt.Features.Proposals.Shared;

public static class ProposalActionNames
{
    public const string Accept = "Accept";
    public const string Reject = "Reject";
    public const string Cancel = "Cancel";
    public const string TerminateProposal = "TerminateProposal";
    public const string CreateContract = "CreateContract";
    public const string ViewContract = "ViewContract";
    public const string OpenChat = "OpenChat";
    public const string ViewChatHistory = "ViewChatHistory";
}

internal static class ProposalPermittedActions
{
    public static IReadOnlyList<string> Resolve(
        Guid actorUserId,
        Guid clientUserId,
        Guid lawyerUserId,
        ProposalStatus proposalStatus,
        Guid? contractId,
        Guid? conversationId,
        bool conversationIsClosed)
    {
        var isClient = actorUserId == clientUserId;
        var isLawyer = actorUserId == lawyerUserId;
        if (!isClient && !isLawyer)
        {
            return [];
        }

        var actions = new List<string>();
        if (proposalStatus == ProposalStatus.Pending)
        {
            if (isClient)
            {
                actions.Add(ProposalActionNames.Cancel);
            }
            else
            {
                actions.Add(ProposalActionNames.Accept);
                actions.Add(ProposalActionNames.Reject);
            }

            return actions;
        }

        if (conversationId.HasValue)
        {
            if (!ProposalChatVisibility.IsHiddenFromActor(
                    actorUserId,
                    lawyerUserId,
                    proposalStatus))
            {
                actions.Add(
                    proposalStatus == ProposalStatus.Accepted
                    && !conversationIsClosed
                        ? ProposalActionNames.OpenChat
                        : ProposalActionNames.ViewChatHistory);
            }
        }

        if (contractId.HasValue)
        {
            actions.Add(ProposalActionNames.ViewContract);
        }
        else if (proposalStatus == ProposalStatus.Accepted)
        {
            actions.Add(ProposalActionNames.TerminateProposal);
            if (isLawyer)
            {
                actions.Add(ProposalActionNames.CreateContract);
            }
        }

        return actions;
    }
}

internal static class ProposalChatVisibility
{
    public static bool IsHiddenFromActor(
        Guid actorUserId,
        Guid lawyerUserId,
        ProposalStatus proposalStatus)
    {
        return proposalStatus == ProposalStatus.Superseded
            && actorUserId == lawyerUserId;
    }
}
