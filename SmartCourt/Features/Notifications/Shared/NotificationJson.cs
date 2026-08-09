using System.Text.Json;

namespace SmartCourt.Features.Notifications.Shared;

internal static class NotificationJson
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public static string Serialize(
        IReadOnlyDictionary<string, string> values)
    {
        return JsonSerializer.Serialize(values, SerializerOptions);
    }

    public static IReadOnlyDictionary<string, string>? Deserialize(
        string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(
            json,
            SerializerOptions);
    }
}
