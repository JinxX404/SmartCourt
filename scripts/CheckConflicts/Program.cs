using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CheckConflicts;

class Program
{
    static async Task Main(string[] args)
    {
        string url = "https://183802b4-6558-4533-a6ff-a087359ca5f8.us-west-1-0.aws.cloud.qdrant.io:6333/collections/egyptian_law/points/scroll";
        string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3MiOiJtIiwiZXhwIjoxNzg4ODYzNzU3LCJzdWJqZWN0IjoiYXBpLWtleTozMTJjZGYzMS01NWU4LTQ2OTUtOTlkYi00MDA2MzQ5NDIwMDUifQ.iTSWONq5Jd-QqoN0LHZx7TNma0pE8RysC5n4i5xceYk";

        var qdrantChunkIds = new HashSet<string>();
        var qdrantDocIds = new HashSet<string>();

        object offset = null;

        Console.WriteLine("Fetching existing chunks from Qdrant...");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", apiKey);

        while (true)
        {
            var data = new Dictionary<string, object>
            {
                { "limit", 1000 },
                { "with_payload", new[] { "chunk_id", "document_id" } },
                { "with_vector", false }
            };

            if (offset != null)
            {
                data["offset"] = offset;
            }

            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, jsonContent);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error fetching from Qdrant: {response.StatusCode}");
                break;
            }

            var respJson = await response.Content.ReadAsStringAsync();
            var respNode = JsonNode.Parse(respJson);
            
            var points = respNode?["result"]?["points"]?.AsArray();
            if (points != null)
            {
                foreach (var p in points)
                {
                    var payload = p["payload"];
                    if (payload != null)
                    {
                        var chunkId = payload["chunk_id"]?.ToString();
                        if (chunkId != null) qdrantChunkIds.Add(chunkId);

                        var docId = payload["document_id"]?.ToString();
                        if (docId != null) qdrantDocIds.Add(docId);
                    }
                }
            }

            var nextOffset = respNode?["result"]?["next_page_offset"];
            if (nextOffset == null)
            {
                break;
            }
            
            if (nextOffset is JsonValue jval && jval.TryGetValue<string>(out var strOffset))
            {
                offset = strOffset;
            }
            else
            {
                offset = nextOffset.GetValue<object>();
            }
        }

        Console.WriteLine($"Found {qdrantChunkIds.Count} unique chunk_ids and {qdrantDocIds.Count} unique document_ids in Qdrant.");

        Console.WriteLine("Reading new_legal_chunks.jsonl...");
        var newChunkIds = new HashSet<string>();
        var newDocIds = new HashSet<string>();

        string filePath = "../new_legal_chunks.jsonl";
        if (!File.Exists(filePath))
        {
            filePath = "../../new_legal_chunks.jsonl";
        }
        
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadLines(filePath, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var obj = JsonNode.Parse(line);
                    var chunkId = obj?["chunk_id"]?.ToString();
                    if (chunkId != null) newChunkIds.Add(chunkId);

                    var docId = obj?["document_id"]?.ToString();
                    if (docId != null) newDocIds.Add(docId);
                }
                catch { }
            }
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }

        Console.WriteLine($"Found {newChunkIds.Count} unique chunk_ids and {newDocIds.Count} unique document_ids in new JSONL.");

        var chunkConflicts = new HashSet<string>(qdrantChunkIds);
        chunkConflicts.IntersectWith(newChunkIds);

        var docConflicts = new HashSet<string>(qdrantDocIds);
        docConflicts.IntersectWith(newDocIds);

        Console.WriteLine($"Conflicts:");
        Console.WriteLine($"Duplicate chunk_ids: {chunkConflicts.Count}");
        Console.WriteLine($"Duplicate document_ids: {docConflicts.Count}");
    }
}
