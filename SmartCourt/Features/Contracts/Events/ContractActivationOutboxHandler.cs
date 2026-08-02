using System.Text.Json;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Contracts.Events;

public sealed class ContractActivationOutboxHandler(
    IContractActivationEvaluator activationEvaluator)
    : IOutboxEventHandler
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ContractActivationRequested
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<
                ContractActivationRequestedEventPayload>(
                message.Payload,
                SerializerOptions)
            ?? throw new InvalidOperationException(
                "Contract activation request payload is invalid.");
        if (payload.ContractId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Contract activation request aggregate does not match its payload.");
        }

        await activationEvaluator.EvaluateActivationAsync(
            payload.ContractId,
            payload.RequestedByUserId,
            cancellationToken);
    }
}
