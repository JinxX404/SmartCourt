using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GenerateNewChunks;

class Program
{
    static void Main(string[] args)
    {
        string baseDir = @"d:\ITI 9 Month\Graduation Project\SmartCourt\docs\Egyptian law\القوانين";
        string outputDir = @"d:\ITI 9 Month\Graduation Project\SmartCourt\scripts\chunks";

        if (!Directory.Exists(baseDir))
        {
            Console.WriteLine($"Directory {baseDir} not found");
            return;
        }

        Directory.CreateDirectory(outputDir);

        int totalChunks = 0;
        var fileHandles = new Dictionary<string, StreamWriter>();

        try
        {
            var files = Directory.GetFiles(baseDir, "*.*", SearchOption.AllDirectories);
            
            foreach (var filePath in files)
            {
                if (!filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase) && 
                    !filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string relPath = Path.GetRelativePath(baseDir, filePath);
                var parts = relPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                
                string lawCategory = parts.Length > 1 ? parts[0] : "القوانين";
                string lawName = Path.GetFileNameWithoutExtension(filePath);
                string documentId = GenerateId(relPath.Replace('\\', '/'));

                if (!fileHandles.TryGetValue(lawCategory, out StreamWriter outfile))
                {
                    string outPath = Path.Combine(outputDir, $"{lawCategory}.jsonl");
                    outfile = new StreamWriter(outPath, false, Encoding.UTF8);
                    fileHandles[lawCategory] = outfile;
                }

                string content;
                try
                {
                    content = File.ReadAllText(filePath, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading {filePath}: {e.Message}");
                    continue;
                }

                var paragraphs = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < paragraphs.Length; i++)
                {
                    string para = paragraphs[i].Trim();
                    if (string.IsNullOrEmpty(para)) continue;

                    string chunkId = GenerateId($"{documentId}_{i}");
                    string hierarchyPath = $"{lawCategory} → {lawName}";

                    var chunkObj = new
                    {
                        chunk_id = chunkId,
                        document_id = documentId,
                        text = para,
                        law_name = lawName,
                        law_name_normalized = lawName,
                        law_category = lawCategory,
                        article_number = (i + 1).ToString(),
                        semantic_unit = "general",
                        hierarchy_path = hierarchyPath,
                        language = "ar",
                        source_dataset = "custom_docs",
                        jurisdiction = "EG",
                        text_len = para.Length
                    };

                    outfile.WriteLine(JsonSerializer.Serialize(chunkObj, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
                    totalChunks++;
                }
            }
        }
        finally
        {
            foreach (var f in fileHandles.Values)
            {
                f.Close();
            }
        }

        Console.WriteLine($"Finished writing {totalChunks} chunks to separate files in {outputDir}");
    }

    static string GenerateId(string inputStr)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(inputStr));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
