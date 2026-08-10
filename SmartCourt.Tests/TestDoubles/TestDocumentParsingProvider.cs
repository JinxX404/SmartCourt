using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Tests.TestDoubles;

public class TestDocumentParsingProvider : IDocumentParsingProvider
{
    public string ExtractedTextToReturn { get; set; } = "محتوى المستند للاختبار";

    public Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ExtractedTextToReturn);
    }
}
