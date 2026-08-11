using System.IO.Compression;

namespace SmartCourt.Features.Chat.Attachments;

internal static class ChatAttachmentPolicy
{
    public const int MaximumFileCount = 5;
    public const long MaximumFileSizeBytes = 10 * 1024 * 1024;
    public const long MaximumRequestSizeBytes = 25 * 1024 * 1024;
    public const int MaximumFileNameLength = 255;

    private static readonly IReadOnlyDictionary<string, string> ContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".txt"] = "text/plain",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg"
        };

    public static async Task<ChatAttachmentInspection> InspectAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeFileName)
            || safeFileName.Any(char.IsControl))
        {
            return ChatAttachmentInspection.Invalid(
                "The attachment filename is invalid.");
        }

        var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
        if (!ContentTypes.TryGetValue(extension, out var contentType))
        {
            return ChatAttachmentInspection.Invalid(
                "Only PDF, DOCX, TXT, PNG, and JPEG files are allowed.");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var contentMatches = extension switch
            {
                ".pdf" => await StartsWithAsync(
                    stream,
                    "%PDF-"u8.ToArray(),
                    cancellationToken),
                ".png" => await StartsWithAsync(
                    stream,
                    [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
                    cancellationToken),
                ".jpg" or ".jpeg" => await StartsWithAsync(
                    stream,
                    [0xFF, 0xD8, 0xFF],
                    cancellationToken),
                ".docx" => IsWordDocument(stream),
                ".txt" => await IsPlainTextAsync(stream, cancellationToken),
                _ => false
            };

            return contentMatches
                ? ChatAttachmentInspection.Valid(
                    safeFileName,
                    extension,
                    contentType)
                : ChatAttachmentInspection.Invalid(
                    "The file content does not match its extension.");
        }
        catch (InvalidDataException)
        {
            return ChatAttachmentInspection.Invalid(
                "The file is corrupt or has an invalid format.");
        }
    }

    private static async Task<bool> StartsWithAsync(
        Stream stream,
        byte[] signature,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[signature.Length];
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        return bytesRead == signature.Length
            && buffer.AsSpan().SequenceEqual(signature);
    }

    private static bool IsWordDocument(Stream stream)
    {
        using var archive = new ZipArchive(
            stream,
            ZipArchiveMode.Read,
            leaveOpen: false);
        return archive.GetEntry("[Content_Types].xml") is not null
            && archive.GetEntry("word/document.xml") is not null;
    }

    private static async Task<bool> IsPlainTextAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[Math.Min(4096, (int)stream.Length)];
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        return !buffer.AsSpan(0, bytesRead).Contains((byte)0);
    }
}

internal sealed record ChatAttachmentInspection(
    bool IsValid,
    string? SafeFileName,
    string? Extension,
    string? ContentType,
    string? Error)
{
    public static ChatAttachmentInspection Valid(
        string safeFileName,
        string extension,
        string contentType) => new(
            true,
            safeFileName,
            extension,
            contentType,
            null);

    public static ChatAttachmentInspection Invalid(string error) => new(
        false,
        null,
        null,
        null,
        error);
}
