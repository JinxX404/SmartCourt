using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Interfaces.Providers;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartCourt.Providers.PdfParser;

public class PdfPigParserProvider : IPdfParserProvider
{
    /// <summary>
    /// Y-coordinate tolerance (in PDF points) for grouping words onto the same line.
    /// </summary>
    private const double LineGroupingTolerance = 3.0;

    public Task<PdfParseResult> ParseAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        using var document = PdfDocument.Open(pdfStream);
        var pages = new List<PdfPageContent>();

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();
            bool isVisualRtl = DetectVisualRtl(words);

            string text;
            if (isVisualRtl && words.Count > 0)
            {
                // Visual RTL: PdfPig stored characters left-to-right (visual order).
                // Reconstruct text using word bounding-box positions so that
                // line order is preserved and character order is corrected.
                text = ReconstructFromWordPositions(words);
            }
            else
            {
                // Logical RTL (or LTR): page.Text is already in correct order.
                text = page.Text;
            }

            var normalizedText = ArabicTextNormalizer.Normalize(text);

            // Heading extraction by font-size heuristic
            var headings = ExtractHeadings(words, isVisualRtl);

            pages.Add(new PdfPageContent(page.Number, normalizedText, headings));
        }

        return Task.FromResult(new PdfParseResult(pages, document.NumberOfPages));
    }

    /// <summary>
    /// Detects whether the words on a page are stored in Visual RTL order
    /// (characters within each word are reversed) by comparing how often the
    /// Arabic definite article "ال" appears at word beginnings versus reversed
    /// as "لا" at word endings.
    /// </summary>
    private static bool DetectVisualRtl(List<Word> words)
    {
        var arabicWords = words
            .Where(w => Regex.IsMatch(w.Text, @"\p{IsArabic}"))
            .ToList();

        if (arabicWords.Count == 0) return false;

        int startsWithAl = arabicWords.Count(w => w.Text.StartsWith("ال"));
        int endsWithLa = arabicWords.Count(w => w.Text.EndsWith("لا"));

        return endsWithLa > startsWithAl;
    }

    /// <summary>
    /// Reconstructs readable text from PdfPig <see cref="Word"/> positions.
    /// <list type="number">
    ///   <item>Groups words into lines by Y-coordinate (top of page → bottom).</item>
    ///   <item>Sorts words within each line right → left (Arabic reading order).</item>
    ///   <item>Reverses the characters inside each Arabic word to convert visual → logical order.</item>
    /// </list>
    /// </summary>
    private static string ReconstructFromWordPositions(List<Word> words)
    {
        if (words.Count == 0) return string.Empty;

        // 1. Sort all words top-to-bottom by Y position (PDF origin is bottom-left,
        //    so higher Y = top of page → sort descending).
        var sorted = words.OrderByDescending(w => w.BoundingBox.Bottom).ToList();

        // 2. Group words that share the same baseline into lines.
        var lines = new List<List<Word>>();
        var currentLine = new List<Word> { sorted[0] };
        var currentBaseline = sorted[0].BoundingBox.Bottom;

        for (int i = 1; i < sorted.Count; i++)
        {
            var word = sorted[i];
            if (Math.Abs(word.BoundingBox.Bottom - currentBaseline) <= LineGroupingTolerance)
            {
                currentLine.Add(word);
            }
            else
            {
                lines.Add(currentLine);
                currentLine = new List<Word> { word };
                currentBaseline = word.BoundingBox.Bottom;
            }
        }
        lines.Add(currentLine);

        // 3. Build text: within each line, sort words right-to-left (descending X)
        //    and reverse characters inside Arabic words.
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var sortedLine = line
                .OrderByDescending(w => w.BoundingBox.Left)
                .ToList();

            var lineTokens = new List<string>();
            foreach (var word in sortedLine)
            {
                string wordText = word.Text;

                // Reverse characters of Arabic words (visual → logical),
                // preserving Lam-Alef ligatures as atomic units.
                if (Regex.IsMatch(wordText, @"\p{IsArabic}"))
                {
                    wordText = ReversePreservingLigatures(word);
                }

                lineTokens.Add(wordText);
            }

            sb.AppendLine(string.Join(" ", lineTokens));
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Reverses an Arabic word from visual (L-to-R) to logical (R-to-L) order
    /// while keeping ligature components together. Arabic's mandatory Lam-Alef
    /// ligature (لا) is stored as a single glyph in PDFs but decomposed by PdfPig
    /// into two <see cref="Letter"/> objects at the same X position. By grouping
    /// letters that share the same X-coordinate, we treat ligature components as
    /// a single atomic unit during reversal, preventing لا from being corrupted to ال.
    /// </summary>
    private static string ReversePreservingLigatures(Word word)
    {
        var letters = word.Letters;
        if (letters == null || letters.Count == 0)
            return word.Text;

        if (letters.Count == 1)
            return letters[0].Value;

        // Sort letters by X position (left-to-right, visual order on page)
        var sorted = letters.OrderBy(l => l.Location.X).ToList();

        // Group letters that share the same X position (tolerance ~1.5 PDF points).
        // Ligature components originate from a single glyph and share the same origin;
        // normal adjacent characters are spaced 3+ points apart.
        var groups = new List<string>();
        var currentGroup = new StringBuilder(sorted[0].Value);
        double currentX = sorted[0].Location.X;

        for (int i = 1; i < sorted.Count; i++)
        {
            if (Math.Abs(sorted[i].Location.X - currentX) < 1.5)
            {
                // Same glyph position → ligature component, keep together
                currentGroup.Append(sorted[i].Value);
            }
            else
            {
                groups.Add(currentGroup.ToString());
                currentGroup.Clear();
                currentGroup.Append(sorted[i].Value);
                currentX = sorted[i].Location.X;
            }
        }
        groups.Add(currentGroup.ToString());

        // Reverse the order of groups (visual → logical), but preserve
        // the character order within each group (keeps ligatures intact).
        groups.Reverse();
        return string.Concat(groups);
    }

    /// <summary>
    /// Extracts headings from the page using a font-size heuristic:
    /// words whose average letter point-size exceeds the page average + 2.
    /// </summary>
    private static List<string> ExtractHeadings(List<Word> words, bool isVisualRtl)
    {
        var headings = new List<string>();
        if (words.Count == 0) return headings;

        var avgFontSize = words.Average(w => w.Letters.Average(l => l.PointSize));
        var headingWords = words
            .Where(w => w.Letters.Average(l => l.PointSize) > avgFontSize + 2)
            .ToList();

        if (headingWords.Count > 0)
        {
            string headingText = isVisualRtl
                ? ReconstructFromWordPositions(headingWords)
                : string.Join(" ", headingWords.Select(w => w.Text));

            headings.Add(ArabicTextNormalizer.Normalize(headingText));
        }

        return headings;
    }
}
