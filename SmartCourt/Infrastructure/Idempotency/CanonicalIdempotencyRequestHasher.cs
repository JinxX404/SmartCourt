using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmartCourt.Infrastructure.Idempotency;

public sealed class CanonicalIdempotencyRequestHasher
    : IIdempotencyRequestHasher
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public string ComputeHash<TRequest>(
        IdempotencyScope scope,
        TRequest request)
    {
        var envelope = JsonSerializer.SerializeToElement(
            new
            {
                actorId = scope.UserId,
                operation = scope.Operation,
                resourceType = scope.ResourceType,
                resourceId = scope.ResourceId,
                payload = request
            },
            SerializerOptions);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteCanonical(writer, envelope);
        }

        return Convert.ToHexString(
            SHA256.HashData(stream.ToArray()));
    }

    private static void WriteCanonical(
        Utf8JsonWriter writer,
        JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element
                    .EnumerateObject()
                    .OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonical(writer, property.Value);
                }

                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteCanonical(writer, item);
                }

                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteRawValue(
                    element.GetRawText(),
                    skipInputValidation: true);
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;

            default:
                throw new JsonException(
                    $"Unsupported canonical JSON value kind: {element.ValueKind}.");
        }
    }
}
