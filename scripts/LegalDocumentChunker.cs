using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

public class LawChunkDto
{
    public string Text { get; set; }
    public string Part { get; set; }
    public string Chapter { get; set; }
    public string Section { get; set; }
    public int Article { get; set; }
    public int ChunkIndex { get; set; }
}

public class LegalDocumentChunker
{
    private readonly int _maxTokens;
    private readonly int _overlap;
    private readonly int _minTokens;

    private static readonly Regex ArabicArticlePattern = new Regex(
        @"(?:المادة|مادة)\s*[(\(]?\s*رقم\s*(\d+)|(?:المادة|مادة)\s*[(\(]?\s*(\d+)\s*[)\)]?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex EnglishArticlePattern = new Regex(
        @"Article\s+(\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PartPattern = new Regex(
        @"(?:الباب\s+.+|Part\s+[IVXLCDM]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ChapterPattern = new Regex(
        @"(?:الفصل\s+.+|Chapter\s+\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public LegalDocumentChunker(int maxTokens, int overlap, int minTokens)
    {
        _maxTokens = maxTokens;
        _overlap = overlap;
        _minTokens = minTokens;
    }

    public LawChunkDto[] ChunkText(string fullText, string language)
    {
        var chunks = new List<LawChunkDto>();
        string currentPart = "";
        string currentChapter = "";
        string currentSection = "";

        var articlePattern = language == "en" ? EnglishArticlePattern : ArabicArticlePattern;
        var articleMatches = articlePattern.Matches(fullText);

        if (articleMatches.Count == 0)
        {
            return FallbackChunking(fullText, currentPart, currentChapter, currentSection).ToArray();
        }

        int lastIndex = 0;
        int currentArticleNumber = 0;

        foreach (Match match in articleMatches)
        {
            var textBefore = fullText.Substring(lastIndex, match.Index - lastIndex).Trim();
            
            if (!string.IsNullOrWhiteSpace(textBefore) && currentArticleNumber > 0)
            {
                ProcessArticleBody(textBefore, currentPart, currentChapter, currentSection, currentArticleNumber, chunks);
            }

            UpdateStructuralMetadata(textBefore, ref currentPart, ref currentChapter, ref currentSection);

            var numberGroup = match.Groups[1].Success ? match.Groups[1] : match.Groups[2];
            int articleNum;
            if (int.TryParse(numberGroup.Value, out articleNum))
            {
                currentArticleNumber = articleNum;
            }

            lastIndex = match.Index + match.Length;
        }

        var remainingText = fullText.Substring(lastIndex).Trim();
        if (!string.IsNullOrWhiteSpace(remainingText) && currentArticleNumber > 0)
        {
            ProcessArticleBody(remainingText, currentPart, currentChapter, currentSection, currentArticleNumber, chunks);
        }

        return MergeShortChunks(chunks, _minTokens).ToArray();
    }

    private void UpdateStructuralMetadata(string text, ref string part, ref string chapter, ref string section)
    {
        var partMatch = PartPattern.Matches(text).Cast<Match>().LastOrDefault();
        if (partMatch != null) part = partMatch.Value.Trim();

        var chapterMatch = ChapterPattern.Matches(text).Cast<Match>().LastOrDefault();
        if (chapterMatch != null) chapter = chapterMatch.Value.Trim();
    }

    private void ProcessArticleBody(string body, string part, string chapter, string section, int article, List<LawChunkDto> chunks)
    {
        var words = body.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        int tokenEstimate = words.Length;

        if (tokenEstimate <= _maxTokens)
        {
            chunks.Add(new LawChunkDto { Text = body, Part = part, Chapter = chapter, Section = section, Article = article, ChunkIndex = 0 });
        }
        else
        {
            var subChunks = SplitWithOverlap(words, _maxTokens, _overlap);
            for (int i = 0; i < subChunks.Count; i++)
            {
                chunks.Add(new LawChunkDto { Text = subChunks[i], Part = part, Chapter = chapter, Section = section, Article = article, ChunkIndex = i });
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
            if (i >= words.Length || maxTokens <= overlap) break;
        }
        return result;
    }

    private List<LawChunkDto> FallbackChunking(string fullText, string part, string chapter, string section)
    {
        var chunks = new List<LawChunkDto>();
        var words = fullText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var subChunks = SplitWithOverlap(words, _maxTokens, _overlap);
        
        for (int i = 0; i < subChunks.Count; i++)
        {
            chunks.Add(new LawChunkDto { Text = subChunks[i], Part = part, Chapter = chapter, Section = section, Article = 0, ChunkIndex = i });
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

            if (tokenEstimate < minTokens && merged.Count > 0)
            {
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
    
    public static string GenerateDeterministicGuid(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new Guid(hash).ToString();
        }
    }
}
