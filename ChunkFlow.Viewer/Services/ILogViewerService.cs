namespace ChunkFlow.Viewer.Services;

public interface ILogViewerService
{
    Task<(string RequestId, Stream Stream)> StartStreamAsync(CancellationToken cancellationToken = default);
    string GetClientBaseUrl();
    string GetClientStreamUrl(string requestId);
    IAsyncEnumerable<ChunkFlow.Shared.Models.ApiLog> ReadStreamAsync(Stream stream, CancellationToken cancellationToken = default);
}
