using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using ChunkFlow.Api.Services;

namespace ChunkFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogStreamRelayService _relayService;

    public LogsController(ILogStreamRelayService relayService)
    {
        _relayService = relayService;
    }

    [HttpPost("stream")]
    public async Task StartStreamAsync()
    {
        var requestId = _relayService.RegisterStreamRequest();
        Response.Headers["X-Request-Id"] = requestId;
        Response.ContentType = "application/x-ndjson";
        Response.Headers["Cache-Control"] = "no-cache";

        var bodyFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();
        bodyFeature?.DisableBuffering();
        await Response.StartAsync();

        await _relayService.RelayToAsync(requestId, Response.Body);
    }

    [HttpPost("stream/{requestId}")]
    public async Task<IActionResult> SubmitStreamAsync([FromRoute] string requestId)
    {
        await _relayService.SubmitStreamAsync(requestId, Request.Body);
        return Ok();
    }
}
