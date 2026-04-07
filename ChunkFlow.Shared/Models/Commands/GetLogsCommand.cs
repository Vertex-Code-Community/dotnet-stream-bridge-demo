namespace ChunkFlow.Shared.Models.Commands;

public class GetLogsCommand : RemoteCommand
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
