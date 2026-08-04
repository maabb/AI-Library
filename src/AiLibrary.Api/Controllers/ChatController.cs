using AiLibrary.Application.Commands;
using AiLibrary.Application.Dtos.Chat;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChatResponse = AiLibrary.Application.Dtos.Chat.ChatResponse;

namespace AiLibrary.Api.Controllers;

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
    public async Task<ActionResult<ChatResponse>> Chat(ChatRequest request)
    {
        var result = await _mediator.Send(
            new ChatCommand(request.Message));

        return Ok(result);
    }
}
