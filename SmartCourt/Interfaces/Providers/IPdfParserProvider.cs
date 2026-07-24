using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IPdfParserProvider
{
    /// <summary>
    /// Extract text content from a PDF, returning pages with raw text
    /// and any detected structural elements.
    /// </summary>
    Task<PdfParseResult> ParseAsync(Stream pdfStream,
        CancellationToken cancellationToken = default);
}

public record PdfParseResult(
    IReadOnlyList<PdfPageContent> Pages,
    int TotalPages);

public record PdfPageContent(
    int PageNumber,
    string Text,
    IReadOnlyList<string> DetectedHeadings);
