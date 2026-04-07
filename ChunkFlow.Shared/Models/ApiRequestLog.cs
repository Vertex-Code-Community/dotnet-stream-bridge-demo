using System.Text.Json.Serialization;

namespace ChunkFlow.Shared.Models;

public class ApiRequestLog
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("requester")]
    public string Requester { get; set; } = string.Empty;

    [JsonPropertyName("header")]
    public string Header { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("request")]
    public string Request { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("executionTimeMs")]
    public int ExecutionTimeMs { get; set; }

    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }

    [JsonIgnore]
    public string? FormattedRequest { get; set; }

    [JsonIgnore]
    public string? FormattedResponse { get; set; }
}
