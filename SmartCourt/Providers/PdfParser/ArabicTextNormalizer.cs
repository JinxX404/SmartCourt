using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartCourt.Providers.PdfParser;

public static class ArabicTextNormalizer
{
    /// <summary>
    /// Attempts to reorder visual RTL text into logical RTL text,
    /// and normalizes common Arabic characters.
    /// Note: Full complex text shaping (BiDi) requires a heavy library (e.g. ICU),
    /// but for basic indexing and search, simple reversing and normalization often suffices
    /// for text extracted from PDFs that store RTL text backwards.
    /// </summary>
    public static string NormalizeAndReorder(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Strip diacritics (Tashkeel)
        text = Regex.Replace(text, @"[\u0617-\u061A\u064B-\u0652]", "");

        // Normalize Alef, Ya, Ta Marbuta
        text = text.Replace('أ', 'ا')
                   .Replace('إ', 'ا')
                   .Replace('آ', 'ا')
                   .Replace('ى', 'ي')
                   .Replace('ة', 'ه');

        // Simple word reversal (heuristic for visual RTL stored in PDFs)
        // If the PDF stores whole lines backwards, this reverses the word order.
        // It's a best-effort approach without a full BiDi engine.
        var words = text.Split(new[] { ' ', '\r', '\n', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Check if words contain Arabic characters
        bool hasArabic = words.Any(w => Regex.IsMatch(w, @"\p{IsArabic}"));
        if (hasArabic)
        {
            // Reverse the words (visual to logical)
            System.Array.Reverse(words);
        }

        return string.Join(" ", words);
    }
}
