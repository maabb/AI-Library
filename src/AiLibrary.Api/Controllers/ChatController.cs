using AiLibrary.Api.Streaming;
using AiLibrary.Application.Commands;
using AiLibrary.Application.Dtos.Chat;
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

    // SSE: session → token* → done{toolsUsed}. Framing via ASP.NET TypedResults.ServerSentEvents.
    [HttpPost("stream")]
    public async Task<IResult> Stream(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StreamChatCommand(request.SessionId, request.Message),
            cancellationToken);

        // Proxies (nginx etc.) otherwise buffer the whole response before the client sees tokens.
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();
        Response.Headers["X-Accel-Buffering"] = "no";

        return TypedResults.ServerSentEvents(
            ChatSseStream.ToSseItems(result, cancellationToken));
    }
}
