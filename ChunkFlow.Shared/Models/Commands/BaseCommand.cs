namespace ChunkFlow.Shared.Models.Commands;

public class BaseCommand
{
    public BaseCommand? NextCommand { get; set; }
    public BaseCommand? CancelCommand { get; set; }
}
