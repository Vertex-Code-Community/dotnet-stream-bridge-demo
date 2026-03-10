namespace ChunkFlow.Api.Models;

public sealed class LogStreamState
{
    public TaskCompletionSource<Stream> SourceStreamTcs { get; } = new();
    public TaskCompletionSource ReleaseTcs { get; } = new();
}
