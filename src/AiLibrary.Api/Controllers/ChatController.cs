using AiLibrary.Api.Streaming;
using AiLibrary.Application.Commands;
using AiLibrary.Application.Dtos.Chat;
using AiLibrary.Application.Queries.Chat;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace AiLibrary.Api.Controllers;

// HTTP edge only — no business logic beyond MediatR + SSE.
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

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

    // SSE: session → token* → done{toolsUsed}.
    [HttpPost("stream")]
    public async Task<IResult> Stream(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StreamChatCommand(request.SessionId, request.Message),
            cancellationToken);

        // Proxies otherwise buffer the full response before the client sees tokens.
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        Response.Headers["X-Accel-Buffering"] = "no";

        return TypedResults.ServerSentEvents(
            ChatSseStream.ToSseItems(result, cancellationToken));
    }

    // Durable history sidebar.
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IReadOnlyList<ChatSessionInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChatSessionInfo>>> ListSessions(
        [FromQuery] int take = 30,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _mediator.Send(new ListChatSessionsQuery(take), cancellationToken);
        return Ok(sessions);
    }

    // Open a prior chat. MEAI ChatMessage → { role, text } for Angular.
    [HttpGet("sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(string sessionId, CancellationToken cancellationToken)
    {
        var messages = await _mediator.Send(new GetChatSessionQuery(sessionId), cancellationToken);
        if (messages is null)
        {
            return NotFound();
        }

        return Ok(messages.Select(m => new
        {
            role = m.Role.Value,
            text = m.Text ?? string.Empty
        }));
    }
}
