using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace ManshuratScraper
{
    class Program
    {
        private static readonly string BaseUrl = "https://manshurat.org";
        private static readonly string DocsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "docs", "egyptian law", "manshurat.org"));
        private static readonly string ProgressFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "manshurat_progress.json");
        
        private static readonly int StartNode = 1;
        private static readonly int MaxNode = 85000;
        
        private static readonly HttpClient HttpClient;

        static Program()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };
            HttpClient = new HttpClient(handler);
            HttpClient.Timeout = TimeSpan.FromSeconds(15);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        static async Task Main(string[] args)
        {
            Directory.CreateDirectory(DocsDir);
            int currentNode = LoadProgress();
            
            Console.WriteLine($"Starting MULTI-THREADED scrape from node {currentNode}...");
            Console.WriteLine($"Saving to: {DocsDir}");

            for (int chunkStart = currentNode; chunkStart <= MaxNode; chunkStart += 500)
            {
                int chunkEnd = Math.Min(chunkStart + 499, MaxNode);
                var chunkNodes = Enumerable.Range(chunkStart, chunkEnd - chunkStart + 1);
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 30 };

                await Parallel.ForEachAsync(chunkNodes, parallelOptions, async (nodeId, token) =>
                {
                    var localContext = BrowsingContext.New(Configuration.Default);
                    await ScrapeNodeAsync(nodeId, localContext);
                });

                SaveProgress(chunkEnd + 1);
                Console.WriteLine($"*** Chunk {chunkStart}-{chunkEnd} COMPLETED and Saved ***");
            }
            
            Console.WriteLine("Scraping completed.");
        }

        static int LoadProgress()
        {
            if (File.Exists(ProgressFile))
            {
                try
                {
                    var json = File.ReadAllText(ProgressFile);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("current_node", out var el))
                    {
                        return el.GetInt32();
                    }
                }
                catch { }
            }
            return StartNode;
        }

        static void SaveProgress(int nodeId)
        {
            var json = $"{{\"current_node\": {nodeId}}}";
            File.WriteAllText(ProgressFile, json);
        }

        static string CleanFilename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Unknown";
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            var clean = Regex.Replace(name, invalidRegStr, "_");
            return clean.Trim().Replace("\n", "").Replace("\r", "");
        }

        static async Task<bool> ScrapeNodeAsync(int nodeId, IBrowsingContext context)
        {
            string url = $"{BaseUrl}/node/{nodeId}";
            Console.WriteLine($"Fetching Node {nodeId}: {url}");
            
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [ERROR] Connection failed: {ex.Message}");
                return false;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"  [SKIP] 404 Not Found");
                return true; // Skip gracefully
            }
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"  [ERROR] Status code: {response.StatusCode}");
                return false;
            }

            var html = await response.Content.ReadAsStringAsync();
            var document = await context.OpenAsync(req => req.Content(html));

            // Extract Title
            string title = null;
            var titleDiv = document.QuerySelector("div[property='dc:title']");
            if (titleDiv != null)
            {
                title = titleDiv.TextContent.Trim();
            }
            else
            {
                var h2 = document.QuerySelector("h2");
                if (h2 != null)
                {
                    title = h2.TextContent.Trim();
                }
            }
            
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("  [SKIP] No title found. Possibly invalid page.");
                return true;
            }
            
            title = CleanFilename(title);

            // Extract Category
            string category = "General";
            var inlineInfos = document.QuerySelectorAll("div.inline-info");
            foreach (var info in inlineInfos)
            {
                var label = info.QuerySelector("div.label-inline");
                if (label != null && (label.TextContent.Contains("القطاع") || label.TextContent.Contains("نوع الوثيقة")))
                {
                    var lineageItems = info.QuerySelectorAll("span[class*='lineage-item']");
                    if (lineageItems.Length > 0)
                    {
                        category = CleanFilename(lineageItems.Last().TextContent.Trim());
                        break;
                    }
                }
            }

            string catDir = Path.Combine(DocsDir, category);
            Directory.CreateDirectory(catDir);

            bool savedSomething = false;

            // Extract HTML Text
            var contentDiv = document.QuerySelector("div[property='content:encoded']");
            if (contentDiv != null)
            {
                string textContent = string.Join(Environment.NewLine, contentDiv.QuerySelectorAll("p, li, h1, h2, h3, h4, h5, h6, div").Select(e => e.TextContent.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)));
                if (!string.IsNullOrWhiteSpace(textContent))
                {
                    string txtPath = Path.Combine(catDir, $"{title}.txt");
                    await File.WriteAllTextAsync(txtPath, textContent);
                    Console.WriteLine($"  [SAVED TXT] {txtPath}");
                    savedSomething = true;
                }
            }

            // Extract PDF
            var pdfLink = document.QuerySelector("a[type='application/pdf']") as IHtmlAnchorElement;
            if (pdfLink == null)
            {
                pdfLink = document.QuerySelector("a[href*='/file/'][href*='/download']") as IHtmlAnchorElement;
            }

            if (pdfLink != null && !string.IsNullOrWhiteSpace(pdfLink.GetAttribute("href")))
            {
                string pdfHref = pdfLink.GetAttribute("href");
                string pdfUrl = pdfHref.StartsWith("http") ? pdfHref : $"{BaseUrl}{pdfHref}";
                string pdfPath = Path.Combine(catDir, $"{title}.pdf");
                
                if (!File.Exists(pdfPath))
                {
                    try
                    {
                        var pdfBytes = await HttpClient.GetByteArrayAsync(pdfUrl);
                        await File.WriteAllBytesAsync(pdfPath, pdfBytes);
                        Console.WriteLine($"  [SAVED PDF] {pdfPath}");
                        savedSomething = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  [ERROR] Failed to download PDF: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"  [SKIP PDF] Already exists.");
                    savedSomething = true;
                }
            }

            if (!savedSomething)
            {
                Console.WriteLine("  [SKIP] No text or PDF found on this page.");
            }

            return true;
        }
    }
}
