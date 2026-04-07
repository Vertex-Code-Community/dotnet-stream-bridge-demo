using Microsoft.AspNetCore.Components;
using ChunkFlow.Shared.Models;

namespace ChunkFlow.Client.Components.Pages.LogExplorer.LogsDialog;

public partial class LogDetailDialog : ComponentBase
{
    [Parameter, EditorRequired] public required string Username { get; set; }
    [Parameter, EditorRequired] public required string ConnectionId { get; set; }
    [Parameter, EditorRequired] public required List<HttpRequestLog> LogsModel { get; set; }
    [Parameter, EditorRequired] public required EventCallback OnCloseModal { get; set; }

    private string? FilterHeader { get; set; }
    private string? FilterUrl { get; set; }
    private string? FilterRequest { get; set; }
    private int? MinExecutionTime { get; set; }
    private int? MaxExecutionTime { get; set; }
    private int? StatusCodeFilter { get; set; }

    private int _currentPage = 1;
    private int _pageSize = 5;
    private bool _hasEverLoaded;
    private List<HttpRequestLog> _filteredLogs = new();
    private List<HttpRequestLog> _pagedLogs = new();

    private int _totalPages => (int)Math.Ceiling((double)_filteredLogs.Count / _pageSize);

    protected override void OnInitialized()
    {
        _hasEverLoaded = LogsModel.Any();
        ApplyFilters();
    }

    protected override void OnParametersSet()
    {
        ApplyFilters();
    }

    private bool ShouldApplyCompact() => !_hasEverLoaded && !_pagedLogs.Any();

    private void ApplyFilters()
    {
        _filteredLogs = (LogsModel ?? new List<HttpRequestLog>())
            .Where(log =>
                (string.IsNullOrWhiteSpace(FilterHeader) || (log.Header?.Contains(FilterHeader, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                (string.IsNullOrWhiteSpace(FilterUrl) || (log.Url?.Contains(FilterUrl, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                (string.IsNullOrWhiteSpace(FilterRequest) || (log.Request?.Contains(FilterRequest, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                (!MinExecutionTime.HasValue || log.ExecutionTimeMs >= MinExecutionTime.Value) &&
                (!MaxExecutionTime.HasValue || log.ExecutionTimeMs <= MaxExecutionTime.Value) &&
                (!StatusCodeFilter.HasValue || log.StatusCode == StatusCodeFilter.Value)
            )
            .ToList();

        _currentPage = 1;
        UpdatePagedLogs();
    }

    private void ResetFilters()
    {
        FilterHeader = FilterUrl = FilterRequest = null;
        MinExecutionTime = MaxExecutionTime = StatusCodeFilter = null;
        ApplyFilters();
    }

    private void UpdatePagedLogs()
    {
        _pagedLogs = _filteredLogs
            .Skip((_currentPage - 1) * _pageSize)
            .Take(_pageSize)
            .ToList();

        StateHasChanged();
    }

    private void HandlePageChange(int page)
    {
        _currentPage = page;
        UpdatePagedLogs();
    }
}
