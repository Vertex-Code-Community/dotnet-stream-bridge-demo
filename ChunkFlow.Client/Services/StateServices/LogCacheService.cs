using ChunkFlow.Shared.Models;

namespace ChunkFlow.Client.Services.StateServices;

public class LogCacheService
{
    public Dictionary<string, Task<List<HttpRequestLog>>> LogTasks { get; set; } = new();

    public void Clear()
    {
        LogTasks.Clear();
    }
}
