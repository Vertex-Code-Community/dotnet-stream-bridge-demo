namespace ChunkFlow.Shared.Models.Commands;

public class FetchLogsCommand : BaseCommand
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
