using ChunkFlow.Shared.Models;

namespace ChunkFlow.Client.Services.StateServices;

public class AppStateService
{
    public Dictionary<string, Task<List<ApiRequestLog>>> ApiRequestLogsTasks { get; set; } = new();

    public void Clear()
    {
        ApiRequestLogsTasks.Clear();
    }
}
