using ChunkFlow.Shared.Models.Commands;

namespace ChunkFlow.Client.Services.CommandStream;

public interface ICommandStreamService
{
    Task<List<TItem>?> ExecuteAsync<TItem>(string connectionId, RemoteCommand command) where TItem : class, new();
}
