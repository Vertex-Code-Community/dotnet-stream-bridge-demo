using Microsoft.AspNetCore.Components;
using ChunkFlow.Shared.Models;
using ChunkFlow.Client.Services.Logs;
using ChunkFlow.Client.Services.StateServices;

namespace ChunkFlow.Client.Components.Pages.ApiLogs;

public partial class ApiRequestLogsComponent : ComponentBase, IDisposable
{
    [Inject] public required IClientLogsService ClientLogsService { get; set; }
    [Inject] public required AppStateService AppStateService { get; set; }

    [Parameter] public required string ConnectionId { get; set; }
    [Parameter] public required string Username { get; set; }

    private bool _showModal;
    private bool _isLoading;
    private List<ApiRequestLog>? _response;

    protected override async Task OnInitializedAsync()
    {
        if (!AppStateService.ApiRequestLogsTasks.TryGetValue(ConnectionId, out var task)) return;

        _isLoading = true;
        StateHasChanged();

        _response = await task;

        _isLoading = false;
        StateHasChanged();
    }

    private async Task OnReloadAsync()
    {
        _isLoading = true;
        StateHasChanged();

        var startDateTime = DateTime.UtcNow.AddHours(-24);
        var endDateTime = DateTime.UtcNow;

        var newTask = ClientLogsService.GetApiRequestLogsAsync(ConnectionId, startDateTime, endDateTime, true);

        if (newTask is null)
        {
            _isLoading = false;
            StateHasChanged();
            return;
        }

        _response = await newTask;
        _isLoading = false;
        StateHasChanged();
    }

    private async Task OnShowModalAsync()
    {
        _showModal = true;
        _response = null;

        var task = AppStateService.ApiRequestLogsTasks.GetValueOrDefault(ConnectionId);
        if (task is not null) _response = await task;

        StateHasChanged();
    }

    public void Dispose() { }
}
