using Microsoft.JSInterop;

namespace ChunkFlow.Client.Services;

public interface ILogClientService
{
    Task<int> GenerateLogsAsync(IJSObjectReference? progressCallback);
    Task<int> StreamLogsToApiAsync(string apiBaseUrl, string requestId, IJSObjectReference? progressCallback);
    Task<int> GetLogCountAsync();
}
