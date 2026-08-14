using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace BulkEmbedToQdrant;

class Program
{
    static async Task Main(string[] args)
    {
        string dashscopeApiKey = Environment.GetEnvironmentVariable("DASHSCOPE_API_KEY");
        string alibabaBaseUrl = Environment.GetEnvironmentVariable("ALIBABA_BASE_URL") ?? "https://ws-1l3xj6rpnm5xmxss.ap-southeast-1.maas.aliyuncs.com/compatible-mode/v1";
        string qdrantUrl = Environment.GetEnvironmentVariable("QDRANT_URL");
        string qdrantApiKey = Environment.GetEnvironmentVariable("QDRANT_API_KEY");
        string collectionName = Environment.GetEnvironmentVariable("QDRANT_COLLECTION") ?? "egyptian_law";

        long freeQuotaTokens = long.TryParse(Environment.GetEnvironmentVariable("FREE_QUOTA_TOKENS"), out var q) ? q : 768820;
        double costPer1MTokens = double.TryParse(Environment.GetEnvironmentVariable("COST_PER_1M_TOKENS_USD"), out var c) ? c : 0.10; // Alibaba approx pricing
        long totalTokensUsed = 0;
        long globalTotalChunks = 58424; // Hardcoded based on directory count for global tracking
        long globalProcessedChunks = 0;

        if (string.IsNullOrEmpty(dashscopeApiKey) || string.IsNullOrEmpty(qdrantUrl) || string.IsNullOrEmpty(qdrantApiKey))
        {
            Console.WriteLine("FAIL: Missing required environment variables.");
            return;
        }

        Console.WriteLine("Initializing clients...");
        
        var openAIClientOptions = new OpenAIClientOptions { Endpoint = new Uri(alibabaBaseUrl) };
        // The standard OpenAIClient in v2
        var openAI = new OpenAIClient(new System.ClientModel.ApiKeyCredential(dashscopeApiKey), openAIClientOptions);
        var embeddingClient = openAI.GetEmbeddingClient("text-embedding-v4");

        // Parse Qdrant URL. The QdrantClient uses Grpc by default on port 6334, or REST. 
        // We will assume https://host for Grpc, usually Qdrant Cloud handles Grpc on 6334 if TLS is true.
        var qdrant = new QdrantClient(new Uri(qdrantUrl), apiKey: qdrantApiKey);

        Console.WriteLine("Ensuring Qdrant collection exists...");
        try
        {
            var collections = await qdrant.ListCollectionsAsync();
            if (!collections.Contains(collectionName))
            {
                Console.WriteLine($"Collection '{collectionName}' does not exist. Creating...");
                await qdrant.CreateCollectionAsync(
                    collectionName,
                    new VectorParams { Size = 1536, Distance = Distance.Cosine }
                );
                Console.WriteLine($"Collection '{collectionName}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Collection '{collectionName}' already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: Failed to check/create collection. Error: {ex.Message}");
            return;
        }

        List<string> jsonlFiles = new List<string>();

        if (args.Length > 0 && File.Exists(args[0]))
        {
            jsonlFiles.Add(args[0]);
            Console.WriteLine($"Using specific input file: {args[0]}");
        }
        else
        {
            string inputDir = "../chunks";

            if (!Directory.Exists(inputDir))
            {
                // Try fallback path if run directly from bin
                inputDir = "../../../../chunks";
                if (!Directory.Exists(inputDir))
                {
                    Console.WriteLine($"FAIL: Data directory not found at {inputDir}");
                    return;
                }
            }

            jsonlFiles = Directory.GetFiles(inputDir, "*.jsonl").OrderBy(f => f).ToList();

            if (args.Length > 0)
            {
                var targetFiles = args[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).ToList();
                jsonlFiles = jsonlFiles.Where(f => 
                    targetFiles.Contains(Path.GetFileNameWithoutExtension(f)) || 
                    targetFiles.Contains(Path.GetFileName(f))).ToList();
                Console.WriteLine($"Filtering to specific files: {string.Join(", ", targetFiles)}");
            }
        }

        Console.WriteLine($"Found {jsonlFiles.Count} .jsonl files to process.");

        foreach (var inputFile in jsonlFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);
            string progressFile = Path.Combine(Path.GetDirectoryName(inputFile), $"progress_{fileName}.json");
            Console.WriteLine($"\n--- Processing file: {Path.GetFileName(inputFile)} ---");

            int startIndex = 0;
            if (File.Exists(progressFile))
            {
                try
                {
                    string pContent = File.ReadAllText(progressFile);
                    var progressDoc = JsonNode.Parse(pContent);
                    if (progressDoc != null && progressDoc["last_processed_index"] != null)
                    {
                        startIndex = progressDoc["last_processed_index"].GetValue<int>();
                        Console.WriteLine($"Found progress file. Resuming from chunk index: {startIndex}");
                    }
                }
                catch
                {
                    Console.WriteLine("Could not parse progress file. Starting from beginning.");
                }
            }

            Console.WriteLine("Loading data into memory...");
            var allChunks = new List<JsonNode>();
            using (var reader = new StreamReader(inputFile, Encoding.UTF8))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        allChunks.Add(JsonNode.Parse(line));
                    }
                }
            }

            int totalAvailable = allChunks.Count;
            Console.WriteLine($"Total chunks in dataset: {totalAvailable}");

            var chunksToProcess = allChunks.Skip(startIndex).ToList();
            Console.WriteLine($"Chunks to process in this run: {chunksToProcess.Count}");

            if (chunksToProcess.Count == 0)
            {
                Console.WriteLine("Nothing to process for this file.");
                continue;
            }

            int batchSize = 10;
            int processedCount = 0;

            Console.WriteLine("Starting bulk embed and upsert...");

            for (int i = 0; i < chunksToProcess.Count; i += batchSize)
            {
            var batch = chunksToProcess.Skip(i).Take(batchSize).ToList();
            var texts = batch.Select(item => item["text"]?.ToString() ?? "").ToList();

            try
            {
                var embedOptions = new EmbeddingGenerationOptions { Dimensions = 1536 };
                var response = await embeddingClient.GenerateEmbeddingsAsync(texts, embedOptions);

                var embeddings = response.Value;

                int batchTokens = 0;
                try
                {
                    // Attempt to extract usage from raw response stream
                    var rawContent = response.GetRawResponse().Content;
                    using var jsonDoc = JsonDocument.Parse(rawContent);
                    if (jsonDoc.RootElement.TryGetProperty("usage", out var usageProp) && 
                        usageProp.TryGetProperty("total_tokens", out var totalTokensProp))
                    {
                        batchTokens = totalTokensProp.GetInt32();
                    }
                }
                catch { }

                if (batchTokens == 0)
                {
                    // Fallback to estimation (~4 chars per token for Arabic usually 2-3 chars, let's use 3)
                    batchTokens = texts.Sum(t => t.Length) / 3;
                }

                totalTokensUsed += batchTokens;
                
                var points = new List<PointStruct>();

                for (int j = 0; j < batch.Count; j++)
                {
                    var item = batch[j];
                    string chunkId = item["chunk_id"]?.ToString() ?? "";
                    Guid pointId = GenerateDeterministicGuid(chunkId);

                    var vector = embeddings[j].ToFloats().ToArray();

                    var payload = new Dictionary<string, Value>();
                    
                    if (item["chunk_id"] != null) payload["chunk_id"] = item["chunk_id"].ToString();
                    if (item["document_id"] != null) payload["document_id"] = item["document_id"].ToString();
                    if (item["law_name"] != null) payload["law_name"] = item["law_name"].ToString();
                    if (item["law_category"] != null) payload["law_category"] = item["law_category"].ToString();
                    if (item["article_number"] != null) payload["article_number"] = item["article_number"].ToString();
                    if (item["semantic_unit"] != null) payload["semantic_unit"] = item["semantic_unit"].ToString();
                    if (item["hierarchy_path"] != null) payload["hierarchy_path"] = item["hierarchy_path"].ToString();
                    if (item["language"] != null) payload["language"] = item["language"].ToString();
                    if (item["text"] != null) payload["chunk_text"] = item["text"].ToString();
                    if (item["source_dataset"] != null) payload["source_dataset"] = item["source_dataset"].ToString();

                    var point = new PointStruct
                    {
                        Id = pointId,
                        Vectors = vector
                    };
                    
                    foreach (var kvp in payload)
                    {
                        point.Payload.Add(kvp.Key, kvp.Value);
                    }

                    points.Add(point);
                }

                await qdrant.UpsertAsync(collectionName, points);

                processedCount += batch.Count;
                globalProcessedChunks += batch.Count;
                int currentAbsoluteIndex = startIndex + processedCount;

                var pObj = new JsonObject { ["last_processed_index"] = currentAbsoluteIndex };
                File.WriteAllText(progressFile, pObj.ToJsonString());

                double cost = 0;
                if (totalTokensUsed > freeQuotaTokens)
                {
                    long billableTokens = totalTokensUsed - freeQuotaTokens;
                    cost = (billableTokens / 1_000_000.0) * costPer1MTokens;
                }

                long chunksRemaining = globalTotalChunks - globalProcessedChunks;
                Console.WriteLine($"[{currentAbsoluteIndex}/{totalAvailable}] File Chunks. Global Remaining: {chunksRemaining} | Tokens: {totalTokensUsed} | Est. Cost: ${cost:F4}");

                if (totalTokensUsed >= freeQuotaTokens)
                {
                    Console.WriteLine("\n========================================================");
                    Console.WriteLine("WARNING: FREE QUOTA EXHAUSTED!");
                    Console.WriteLine($"Total Tokens Used: {totalTokensUsed} (Quota: {freeQuotaTokens})");
                    Console.WriteLine($"Current Est. Cost: ${cost:F4}");
                    Console.WriteLine("Press any key to continue and incur charges, or Ctrl+C to abort and save progress...");
                    Console.WriteLine("========================================================\n");
                    
                    // Prevent repeated prompts for every single batch after quota is reached
                    // by artificially inflating the quota, so user is only warned once.
                    freeQuotaTokens = long.MaxValue; 
                }

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFAIL: Error occurred during batch starting at relative index {i}");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                return;
            }
        }
        Console.WriteLine($"\nFinished processing file: {Path.GetFileName(inputFile)}");
    }

    Console.WriteLine("\nBulk embedding for all files completed successfully!");
}

    static Guid GenerateDeterministicGuid(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
