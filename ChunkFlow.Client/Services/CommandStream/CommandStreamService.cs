using System.Text.Json;
using Newtonsoft.Json;
using ChunkFlow.Client.Helpers;
using ChunkFlow.Shared.Models.Commands;

namespace ChunkFlow.Client.Services.CommandStream;

public class CommandStreamService : ICommandStreamService
{
    private readonly HttpClient _httpClient;

    public CommandStreamService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TItem>?> ExecuteAsync<TItem>(string connectionId, RemoteCommand command) where TItem : class, new()
    {
        var commandJson = JsonConvert.SerializeObject(command, new RemoteCommandConverter());

        var request = new { ConnectionId = connectionId, CommandJson = commandJson };

        var response = await _httpClient.PostAsJsonAsync("/api/Logs/stream", request);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var results = new List<TItem>();

        using var reader = new StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var item = System.Text.Json.JsonSerializer.Deserialize<TItem>(line);
            if (item is not null) results.Add(item);
        }

        return results;
    }
}
