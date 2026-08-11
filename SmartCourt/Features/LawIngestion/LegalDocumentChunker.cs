using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SmartCourt.Features.LawIngestion.DTOs;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.LawIngestion;

public class LegalDocumentChunker
{
    private readonly ChunkingOptions _options;

    // Arabic article pattern (e.g. "مادة 1" or "المادة رقم 1")
    private static readonly Regex ArabicArticlePattern = new(
        @"(?:المادة|مادة)\s*[(\(]?\s*رقم\s*(\d+)|(?:المادة|مادة)\s*[(\(]?\s*(\d+)\s*[)\)]?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // English article pattern
    private static readonly Regex EnglishArticlePattern = new(
        @"Article\s+(\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Part/Chapter headers (heuristic)
    private static readonly Regex PartPattern = new(
        @"(?:الباب\s+.+|Part\s+[IVXLCDM]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ChapterPattern = new(
        @"(?:الفصل\s+.+|Chapter\s+\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public LegalDocumentChunker(IOptions<ChunkingOptions> options)
    {
        _options = options.Value;
    }

    public List<LawChunkDto> ChunkDocument(IReadOnlyList<PdfPageContent> pages, string language)
    {
        var fullText = string.Join("\n\n", pages.Select(p => p.Text));
        return ChunkText(fullText, language);
    }

    public List<LawChunkDto> ChunkText(string fullText, string language)
    {
        var chunks = new List<LawChunkDto>();

        string currentPart = "";
        string currentChapter = "";
        string currentSection = "";

        // Determine if there is any structure. If no articles are found, fallback to generic chunks.
        var articlePattern = language == "en" ? EnglishArticlePattern : ArabicArticlePattern;
        var articleMatches = articlePattern.Matches(fullText);

        if (articleMatches.Count == 0)
        {
            // Fallback: chunk by simple paragraphs/tokens if no structured law articles are found
            return FallbackChunking(fullText, currentPart, currentChapter, currentSection);
        }

        // Preserve the title/preamble: it is often the only place where a law's
        // scope and jurisdiction are stated.
        var preamble = fullText[..articleMatches[0].Index].Trim();
        if (!string.IsNullOrWhiteSpace(preamble))
        {
            UpdateStructuralMetadata(preamble, ref currentPart, ref currentChapter, ref currentSection);
            ProcessArticleBody(preamble, currentPart, currentChapter, currentSection, 0, chunks);
        }

        int lastIndex = articleMatches[0].Index;
        int currentArticleNumber = 0;

        foreach (Match match in articleMatches)
        {
            // Text before this article belongs to the previous article
            var textBefore = fullText.Substring(lastIndex, match.Index - lastIndex).Trim();
            
            if (!string.IsNullOrWhiteSpace(textBefore) && currentArticleNumber > 0)
            {
                ProcessArticleBody(textBefore, currentPart, currentChapter, currentSection, currentArticleNumber, chunks);
            }

            // Extract heading info from textBefore if it contains Part/Chapter updates
            UpdateStructuralMetadata(textBefore, ref currentPart, ref currentChapter, ref currentSection);

            // Determine article number
            var numberGroup = match.Groups[1].Success ? match.Groups[1] : match.Groups[2];
            if (int.TryParse(numberGroup.Value, out int articleNum))
            {
                currentArticleNumber = articleNum;
            }

            lastIndex = match.Index + match.Length;
        }

        // Process the last article body
        var remainingText = fullText.Substring(lastIndex).Trim();
        if (!string.IsNullOrWhiteSpace(remainingText) && currentArticleNumber > 0)
        {
            ProcessArticleBody(remainingText, currentPart, currentChapter, currentSection, currentArticleNumber, chunks);
        }

        return MergeShortChunks(chunks, _options.MinChunkTokens);
    }

    /// <summary>Chunks ordinary user documents without interpreting contract clauses as legislation.</summary>
    public List<LawChunkDto> ChunkPlainText(string text)
        => FallbackChunking(text, string.Empty, string.Empty, string.Empty);

    private void UpdateStructuralMetadata(string text, ref string part, ref string chapter, ref string section)
    {
        var partMatch = PartPattern.Matches(text).Cast<Match>().LastOrDefault();
        if (partMatch != null) part = partMatch.Value.Trim();

        var chapterMatch = ChapterPattern.Matches(text).Cast<Match>().LastOrDefault();
        if (chapterMatch != null) chapter = chapterMatch.Value.Trim();
    }

    private void ProcessArticleBody(string body, string part, string chapter, string section, int article, List<LawChunkDto> chunks)
    {
        // Simple token estimation (1 word ~= 1.3 tokens for Arabic, ~1.5 for English usually, let's use a rough split by whitespace)
        var words = body.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        int tokenEstimate = words.Length;

        if (tokenEstimate <= _options.MaxChunkTokens)
        {
            chunks.Add(new LawChunkDto
            {
                Text = body,
                Part = part,
                Chapter = chapter,
                Section = section,
                Article = article,
                ChunkIndex = 0
            });
        }
        else
        {
            // Split with overlap
            var subChunks = SplitWithOverlap(words, _options.MaxChunkTokens, _options.OverlapTokens);
            for (int i = 0; i < subChunks.Count; i++)
            {
                chunks.Add(new LawChunkDto
                {
                    Text = subChunks[i],
                    Part = part,
                    Chapter = chapter,
                    Section = section,
                    Article = article,
                    ChunkIndex = i
                });
            }
        }
    }

    private List<string> SplitWithOverlap(string[] words, int maxTokens, int overlap)
    {
        var result = new List<string>();
        int i = 0;
        while (i < words.Length)
        {
            int take = Math.Min(maxTokens, words.Length - i);
            var chunkWords = words.Skip(i).Take(take);
            result.Add(string.Join(" ", chunkWords));
            
            i += (maxTokens - overlap);
            if (i >= words.Length || maxTokens <= overlap) break; // Avoid infinite loop
        }
        return result;
    }

    private List<LawChunkDto> FallbackChunking(string fullText, string part, string chapter, string section)
    {
        var chunks = new List<LawChunkDto>();
        var words = fullText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var subChunks = SplitWithOverlap(words, _options.MaxChunkTokens, _options.OverlapTokens);
        
        for (int i = 0; i < subChunks.Count; i++)
        {
            chunks.Add(new LawChunkDto
            {
                Text = subChunks[i],
                Part = part,
                Chapter = chapter,
                Section = section,
                Article = 0, // No article
                ChunkIndex = i
            });
        }
        return chunks;
    }

    private List<LawChunkDto> MergeShortChunks(List<LawChunkDto> chunks, int minTokens)
    {
        if (chunks.Count <= 1) return chunks;

        var merged = new List<LawChunkDto>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var current = chunks[i];
            int tokenEstimate = current.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (tokenEstimate < minTokens && merged.Count > 0
                && merged.Last().Article == current.Article
                && merged.Last().Part == current.Part
                && merged.Last().Chapter == current.Chapter)
            {
                // Merge with previous
                var prev = merged.Last();
                prev.Text += " " + current.Text;
            }
            else
            {
                merged.Add(current);
            }
        }
        return merged;
    }
}
