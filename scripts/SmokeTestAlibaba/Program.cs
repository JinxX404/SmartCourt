using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Embeddings;

namespace SmokeTestAlibaba;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = Environment.GetEnvironmentVariable("DASHSCOPE_API_KEY");
        string baseUrl = Environment.GetEnvironmentVariable("ALIBABA_BASE_URL") ?? "https://dashscope-intl.aliyuncs.com/compatible-mode/v1";

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("FAIL: DASHSCOPE_API_KEY environment variable not set.");
            Environment.Exit(1);
        }

        Console.WriteLine($"Using Base URL: {baseUrl}");
        
        var openAIClientOptions = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
        var client = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), openAIClientOptions);
        var embeddingClient = client.GetEmbeddingClient("text-embedding-v4");

        string inputFile = "../cleaned_legal_chunks.jsonl";
        if (!File.Exists(inputFile))
        {
            inputFile = "../../cleaned_legal_chunks.jsonl";
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"FAIL: Input file {inputFile} not found.");
                Environment.Exit(1);
            }
        }

        var chunks = new List<string>();
        int count = 0;
        foreach (var line in File.ReadLines(inputFile, System.Text.Encoding.UTF8))
        {
            if (count >= 3) break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var data = JsonNode.Parse(line);
                string text = data?["text"]?.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    chunks.Add(text);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAIL: Could not parse line {count} as JSON. Error: {ex.Message}");
                Environment.Exit(1);
            }
            count++;
        }

        if (chunks.Count == 0)
        {
            Console.WriteLine("FAIL: No valid text chunks found in the input file.");
            Environment.Exit(1);
        }

        Console.WriteLine($"Sending {chunks.Count} chunks to Alibaba text-embedding-v4...");

        try
        {
            var options = new EmbeddingGenerationOptions { Dimensions = 1536 };
            var response = await embeddingClient.GenerateEmbeddingsAsync(chunks, options);

            var embeddings = response.Value;

            if (embeddings == null || embeddings.Count == 0)
            {
                Console.WriteLine("FAIL: Response contains no embedding data.");
                Environment.Exit(1);
            }

            if (embeddings.Count != chunks.Count)
            {
                Console.WriteLine($"FAIL: Expected {chunks.Count} embeddings, got {embeddings.Count}.");
                Environment.Exit(1);
            }

            for (int i = 0; i < embeddings.Count; i++)
            {
                var emb = embeddings[i].ToFloats().ToArray();
                if (emb.Length != 1536)
                {
                    Console.WriteLine($"FAIL: Embedding {i} has {emb.Length} dimensions, expected 1536.");
                    Environment.Exit(1);
                }

                bool allZero = true;
                foreach (var v in emb)
                {
                    if (v != 0)
                    {
                        allZero = false;
                        break;
                    }
                }

                if (allZero)
                {
                    Console.WriteLine($"FAIL: Embedding {i} contains all zeros.");
                    Environment.Exit(1);
                }
            }

            Console.WriteLine("\nSUCCESS: All embeddings validated successfully.");
            Console.WriteLine("- HTTP Response Status: 200 OK (Implicit from successful execution)");
            Console.WriteLine("- Dimensions: 1536");
            Console.WriteLine("- Values: Non-zero");
            
            Console.WriteLine("- Token Usage: Not reported in the response object.");

            Console.WriteLine("\n*** VERDICT: PASS ***");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nFAIL: Exception occurred during API call:");
            Console.WriteLine(ex.Message);
            Environment.Exit(1);
        }
    }
}
