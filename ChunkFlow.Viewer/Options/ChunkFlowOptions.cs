namespace ChunkFlow.Viewer.Options;

public sealed class ChunkFlowOptions
{
    public const string SectionName = "ChunkFlow";
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string ClientBaseUrl { get; set; } = string.Empty;
}
