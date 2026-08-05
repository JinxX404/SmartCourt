using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IDocumentParsingProvider
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
