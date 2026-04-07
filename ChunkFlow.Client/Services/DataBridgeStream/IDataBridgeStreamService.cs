using ChunkFlow.Shared.Models.Commands;

namespace ChunkFlow.Client.Services.DataBridgeStream;

public interface IDataBridgeStreamService
{
    Task<List<TItem>?> ExecuteAsync<TItem>(string connectionId, BaseCommand command) where TItem : class, new();
}
