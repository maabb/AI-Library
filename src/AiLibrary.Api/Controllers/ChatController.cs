using System.Text;
using System.Text.Json;
using AiLibrary.Application.Commands;
using AiLibrary.Application.Dtos.Chat;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace AiLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Non-streaming multi-turn chat. Best default for Angular HttpClient.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ChatCommand(request.SessionId, request.Message),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Streaming chat (Server-Sent Events) for Angular EventSource/fetch streams.
    /// Events: session (once), token (many), done (once).
    /// </summary>
    [HttpPost("stream")]
    public async Task Stream(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StreamChatCommand(request.SessionId, request.Message),
            cancellationToken);

        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        await WriteSseAsync("session", new { sessionId = result.SessionId }, cancellationToken);

        await foreach (var token in result.Tokens.WithCancellation(cancellationToken))
        {
            await WriteSseAsync("token", new { text = token }, cancellationToken);
        }

        await WriteSseAsync("done", new { sessionId = result.SessionId }, cancellationToken);
    }

    private async Task WriteSseAsync(string eventName, object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var chunk = $"event: {eventName}\ndata: {json}\n\n";
        await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(chunk), cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
