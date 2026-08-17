namespace SmartCourt.Features.Contracts.Enums;

public enum ContractStatus : int
{
    Draft = 0,
    Active = 1,
    SuspendedByDispute = 2,
    Completed = 3,
    Terminated = 4,
    CompletedOnHold = 5
}
