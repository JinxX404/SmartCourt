using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Text;

namespace CleanLegalChunks;

class Program
{
    static void Main(string[] args)
    {
        string inputDir = @"d:\ITI 9 Month\Graduation Project\SmartCourt\docs\Egyptian law\exports";
        string outputFile = @"d:\ITI 9 Month\Graduation Project\SmartCourt\scripts\cleaned_legal_chunks.jsonl";

        if (!Directory.Exists(inputDir))
        {
            Console.WriteLine($"Directory {inputDir} not found");
            return;
        }

        var files = Directory.GetFiles(inputDir, "legal-chunks-*.jsonl").OrderBy(f => f).ToList();

        if (files.Count == 0)
        {
            Console.WriteLine($"No files found matching legal-chunks-*.jsonl in {inputDir}");
            return;
        }

        var seenChunkIds = new System.Collections.Generic.HashSet<string>();
        int totalInput = 0;
        int totalCleaned = 0;
        int totalFiltered = 0;
        int totalDuplicates = 0;

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

        using var outStream = new StreamWriter(outputFile, false, Encoding.UTF8);

        foreach (var f in files)
        {
            Console.WriteLine($"Processing {Path.GetFileName(f)}...");
            foreach (var line in File.ReadLines(f, Encoding.UTF8))
            {
                totalInput++;
                try
                {
                    var data = JsonNode.Parse(line.Trim());
                    if (data == null) continue;

                    var chunkId = data["chunk_id"]?.ToString();
                    if (string.IsNullOrEmpty(chunkId)) continue;

                    if (seenChunkIds.Contains(chunkId))
                    {
                        totalDuplicates++;
                        continue;
                    }

                    var embText = data["embedding_text"]?.ToString() ?? "";
                    var rawText = data["text"]?.ToString() ?? "";
                    string text = (!string.IsNullOrWhiteSpace(embText)) ? embText : rawText;

                    string cleanedText = CleanArabicText(text);

                    if (cleanedText.Length < 20)
                    {
                        totalFiltered++;
                        continue;
                    }

                    var cleanedRecord = new
                    {
                        chunk_id = chunkId,
                        document_id = data["document_id"]?.ToString() ?? "",
                        text = cleanedText,
                        law_name = data["law_name"]?.ToString() ?? "",
                        law_name_normalized = data["law_name_normalized"]?.ToString() ?? "",
                        law_category = data["law_category"]?.ToString() ?? "",
                        article_number = data["article_number"]?.ToString() ?? "",
                        semantic_unit = data["semantic_unit"]?.ToString() ?? "",
                        hierarchy_path = data["hierarchy_path"]?.ToString() ?? "",
                        language = data["language"]?.ToString() ?? "ar",
                        source_dataset = data["source_dataset"]?.ToString() ?? "",
                        jurisdiction = data["jurisdiction"]?.ToString() ?? "EG",
                        text_len = cleanedText.Length
                    };

                    outStream.WriteLine(JsonSerializer.Serialize(cleanedRecord, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
                    seenChunkIds.Add(chunkId);
                    totalCleaned++;
                }
                catch
                {
                    // Ignore JSON parse errors
                }
            }
        }

        Console.WriteLine("\n--- Summary ---");
        Console.WriteLine($"Total Input Chunks: {totalInput}");
        Console.WriteLine($"Total Output Cleaned: {totalCleaned}");
        Console.WriteLine($"Total Filtered (<20 chars): {totalFiltered}");
        Console.WriteLine($"Total Duplicates: {totalDuplicates}");
        Console.WriteLine($"Output written to: {outputFile}");
    }

    static string CleanArabicText(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        text = Regex.Replace(text, @"[\u200B\u200C\u200D\uFEFF]", "");
        text = text.Normalize(NormalizationForm.FormC);
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }
}
