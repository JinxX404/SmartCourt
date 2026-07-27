using System.Text.Json;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Infrastructure.Providers.Events;

internal static class OutboxPayloadPolicy
{
    private static readonly string[] ForbiddenPropertyFragments =
    [
        "paymentmethod",
        "evidence",
        "cardnumber",
        "cvv",
        "password",
        "secret",
        "token",
        "fileurl"
    ];

    public static void Validate(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        Visit(document.RootElement);
    }

    private static void Visit(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (ForbiddenPropertyFragments.Any(fragment =>
                    property.Name.Contains(
                        fragment,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    throw new BusinessException(
                        $"Outbox payload contains forbidden field '{property.Name}'.");
                }

                Visit(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                Visit(item);
            }
        }
    }
}
