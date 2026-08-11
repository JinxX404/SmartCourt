using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.DocumentParsing;

public class CompositeDocumentParsingProvider : IDocumentParsingProvider
{
    private readonly IPdfParserProvider _pdfParser;
    private readonly ILogger<CompositeDocumentParsingProvider> _logger;

    public CompositeDocumentParsingProvider(
        IPdfParserProvider pdfParser,
        ILogger<CompositeDocumentParsingProvider> logger)
    {
        _pdfParser = pdfParser;
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        try
        {
            return extension switch
            {
                ".pdf" => await ParsePdfAsync(fileStream, cancellationToken),
                ".docx" => ParseDocx(fileStream),
                _ => throw new BusinessException($"Unsupported file type for document analysis: {extension}")
            };
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            _logger.LogError(ex, "Failed to parse document: {FileName}", fileName);
            throw new BusinessException($"An error occurred while parsing the document {fileName}.", ex);
        }
    }

    private async Task<string> ParsePdfAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        var parseResult = await _pdfParser.ParseAsync(fileStream, cancellationToken);
        var sb = new StringBuilder();
        foreach (var page in parseResult.Pages)
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString().Trim();
    }

    private string ParseDocx(Stream fileStream)
    {
        // DocumentFormat.OpenXml works synchronously on streams
        using var wordDocument = WordprocessingDocument.Open(fileStream, false);
        var body = wordDocument.MainDocumentPart?.Document.Body;
        if (body == null)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var paragraph in body.Elements<Paragraph>())
        {
            sb.AppendLine(paragraph.InnerText);
        }

        return SmartCourt.Providers.PdfParser.ArabicTextNormalizer.Normalize(sb.ToString().Trim());
    }
}
