namespace ChunkFlow.Api.Services;

public interface ILogStreamRelayService
{
    string RegisterStreamRequest();
    Task SubmitStreamAsync(string requestId, Stream sourceStream);
    Task RelayToAsync(string requestId, Stream destinationStream);
}
