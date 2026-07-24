using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Interfaces.Providers;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartCourt.Providers.PdfParser;

public class PdfPigParserProvider : IPdfParserProvider
{
    public Task<PdfParseResult> ParseAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        using var document = PdfDocument.Open(pdfStream);
        var pages = new List<PdfPageContent>();

        foreach (var page in document.GetPages())
        {
            var text = page.Text; // Raw text from PdfPig
            var normalizedText = ArabicTextNormalizer.NormalizeAndReorder(text);

            // Basic heuristic for headings: font size > average + 2
            var words = page.GetWords().ToList();
            var headings = new List<string>();
            if (words.Count > 0)
            {
                var avgFontSize = words.Average(w => w.Letters.Average(l => l.PointSize));
                var headingWords = words.Where(w => w.Letters.Average(l => l.PointSize) > avgFontSize + 2).ToList();
                // Simple grouping could be done here, but for now just take the text
                if (headingWords.Any())
                {
                    headings.Add(ArabicTextNormalizer.NormalizeAndReorder(string.Join(" ", headingWords.Select(w => w.Text))));
                }
            }

            pages.Add(new PdfPageContent(page.Number, normalizedText, headings));
        }

        return Task.FromResult(new PdfParseResult(pages, document.NumberOfPages));
    }
}
