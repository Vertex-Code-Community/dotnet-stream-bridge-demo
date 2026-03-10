namespace ChunkFlow.Shared.Models;

public sealed class ApiLog
{
    public string Id { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long DurationMs { get; set; }
}
