using System.Text.RegularExpressions;

namespace SmartCourt.Providers.PdfParser;

/// <summary>
/// Normalizes Arabic text by stripping diacritics and standardizing character forms.
/// Text reordering (visual RTL → logical RTL) is handled by <see cref="PdfPigParserProvider"/>
/// using word-position-based reconstruction.
/// </summary>
public static class ArabicTextNormalizer
{
    /// <summary>
    /// Normalizes Arabic text characters without any reordering.
    /// </summary>
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Strip diacritics (Tashkeel)
        text = Regex.Replace(text, @"[\u0617-\u061A\u064B-\u0652]", "");

        // Normalize Alef variants to bare Alef
        // Normalize Ya variants (Alef Maqsurah → Ya)
        // Normalize Ta Marbuta → Ha
        text = text.Replace('أ', 'ا')
                   .Replace('إ', 'ا')
                   .Replace('آ', 'ا')
                   .Replace('ى', 'ي')
                   .Replace('ة', 'ه');

        return text;
    }

    /// <summary>
    /// Backward-compatible alias for <see cref="Normalize"/>.
    /// RTL reordering is now handled at the PDF parser level using word positions.
    /// </summary>
    public static string NormalizeAndReorder(string text) => Normalize(text);
}
