namespace SmartCourt.Infrastructure.Providers.Payments;

public enum ProviderOperationOutcome
{
    Succeeded = 0,
    Failed = 1,
    Unknown = 2,
    Processing = 3,
    RequiresCustomerAction = 4
}
