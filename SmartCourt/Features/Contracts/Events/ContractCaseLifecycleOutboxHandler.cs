using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts.Events;

public sealed class ContractCaseLifecycleOutboxHandler
    : IOutboxEventHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IReadOnlyCollection<IContractCaseLifecycleService>
        _caseLifecycleServices;

    public ContractCaseLifecycleOutboxHandler(
        ApplicationDbContext dbContext,
        IEnumerable<IContractCaseLifecycleService> caseLifecycleServices)
    {
        _dbContext = dbContext;
        _caseLifecycleServices = caseLifecycleServices.ToArray();
    }

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ContractCompleted,
        ContractPaymentEventTypes.ContractTerminated
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var context = await new ContractIntegrationEventResolver(_dbContext)
            .ResolveAsync(message, cancellationToken);
        await GetCaseLifecycleService().ApplyAsync(
            new ContractCaseLifecycleUpdate(
                message.Id,
                context.LegalCaseId,
                context.ContractId,
                message.EventType == ContractPaymentEventTypes.ContractCompleted
                    ? ContractCaseLifecycleTransition.ContractCompleted
                    : ContractCaseLifecycleTransition.ContractTerminated,
                message.CreatedAt),
            cancellationToken);
    }

    private IContractCaseLifecycleService GetCaseLifecycleService()
    {
        if (_caseLifecycleServices.Count == 1)
        {
            return _caseLifecycleServices.Single();
        }

        throw new InvalidOperationException(
            "خدمة تحديث دورة حياة القضية غير متاحة، وسيعاد إرسال الحدث تلقائيًا.");
    }
}
