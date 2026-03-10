using Microsoft.JSInterop;

namespace ChunkFlow.Client.Services;

public sealed class LogClientService : ILogClientService
{
    private readonly IJSRuntime _jsRuntime;

    public LogClientService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<int> GenerateLogsAsync(IJSObjectReference? progressCallback)
    {
        await WaitForModuleAsync();
        return await _jsRuntime.InvokeAsync<int>("ChunkFlowClient.generateLogs", progressCallback);
    }

    public async Task<int> StreamLogsToApiAsync(string apiBaseUrl, string requestId, IJSObjectReference? progressCallback)
    {
        await WaitForModuleAsync();
        return await _jsRuntime.InvokeAsync<int>("ChunkFlowClient.streamLogsToApi", apiBaseUrl, requestId, progressCallback);
    }

    public async Task<int> GetLogCountAsync()
    {
        await WaitForModuleAsync();
        return await _jsRuntime.InvokeAsync<int>("ChunkFlowClient.getLogCount");
    }

    private async Task WaitForModuleAsync()
    {
        await _jsRuntime.InvokeAsync<object>("waitForChunkFlowClient");
    }
}
