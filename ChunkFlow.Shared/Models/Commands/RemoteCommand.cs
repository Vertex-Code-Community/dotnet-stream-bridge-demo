namespace ChunkFlow.Shared.Models.Commands;

public class RemoteCommand
{
    public RemoteCommand? NextCommand { get; set; }
    public RemoteCommand? CancelCommand { get; set; }
}
