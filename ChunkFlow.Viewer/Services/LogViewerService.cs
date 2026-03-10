using System.Runtime.CompilerServices;
using System.Text.Json;
using ChunkFlow.Viewer.Options;
using Microsoft.Extensions.Options;

namespace ChunkFlow.Viewer.Services;

public sealed class LogViewerService : ILogViewerService
{
    private readonly HttpClient _httpClient;
    private readonly ChunkFlowOptions _options;

    public LogViewerService(HttpClient httpClient, IOptions<ChunkFlowOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<(string RequestId, Stream Stream)> StartStreamAsync(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.ApiBaseUrl.TrimEnd('/')}/api/logs/stream");
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (!response.Headers.TryGetValues("X-Request-Id", out var values))
            throw new InvalidOperationException("Missing X-Request-Id header");
        var requestId = values.First();
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return (requestId, stream);
    }

    public string GetClientBaseUrl() => _options.ClientBaseUrl.TrimEnd('/');

    public string GetClientStreamUrl(string requestId)
    {
        var apiUrl = Uri.EscapeDataString(_options.ApiBaseUrl.TrimEnd('/'));
        return $"{_options.ClientBaseUrl.TrimEnd('/')}/logs?requestId={requestId}&apiUrl={apiUrl}";
    }

    public async IAsyncEnumerable<ChunkFlow.Shared.Models.ApiLog> ReadStreamAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(stream);
        var buffer = new char[4096];
        var lineBuffer = new List<char>();
        while (true)
        {
            var read = await reader.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;
            for (var i = 0; i < read; i++)
            {
                var c = buffer[i];
                if (c == '\n' || c == '\r')
                {
                    if (lineBuffer.Count == 0) continue;
                    var line = new string(lineBuffer.ToArray());
                    lineBuffer.Clear();
                    var log = JsonSerializer.Deserialize<ChunkFlow.Shared.Models.ApiLog>(line);
                    if (log is not null)
                        yield return log;
                    continue;
                }
                lineBuffer.Add(c);
            }
        }
        if (lineBuffer.Count > 0)
        {
            var line = new string(lineBuffer.ToArray());
            var log = JsonSerializer.Deserialize<ChunkFlow.Shared.Models.ApiLog>(line);
            if (log is not null)
                yield return log;
        }
    }
}
