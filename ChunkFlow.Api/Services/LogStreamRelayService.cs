using System.Collections.Concurrent;
using ChunkFlow.Api.Models;

namespace ChunkFlow.Api.Services;

public sealed class LogStreamRelayService : ILogStreamRelayService
{
    private readonly ConcurrentDictionary<string, LogStreamState> _state = new();
    private const int TimeoutMs = 5 * 60 * 1000;

    public string RegisterStreamRequest()
    {
        var requestId = Guid.NewGuid().ToString("N");
        var state = new LogStreamState();
        _state.TryAdd(requestId, state);
        return requestId;
    }

    public Task SubmitStreamAsync(string requestId, Stream sourceStream)
    {
        if (!_state.TryGetValue(requestId, out var streamState))
            return Task.CompletedTask;

        streamState.SourceStreamTcs.TrySetResult(sourceStream);
        return Task.WhenAny(streamState.ReleaseTcs.Task, Task.Delay(TimeoutMs));
    }

    public async Task RelayToAsync(string requestId, Stream destinationStream)
    {
        if (!_state.TryGetValue(requestId, out var streamState))
            return;

        var sourceStream = await WaitForSourceAsync(streamState);
        if (sourceStream is null)
            return;

        await sourceStream.CopyToAsync(destinationStream);
        streamState.ReleaseTcs.TrySetResult();
        _state.TryRemove(requestId, out _);
    }

    private static async Task<Stream?> WaitForSourceAsync(LogStreamState streamState)
    {
        var completed = await Task.WhenAny(streamState.SourceStreamTcs.Task, Task.Delay(TimeoutMs));
        return completed == streamState.SourceStreamTcs.Task ? streamState.SourceStreamTcs.Task.Result : null;
    }
}
