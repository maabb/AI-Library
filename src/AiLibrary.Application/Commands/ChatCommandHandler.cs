using AiLibrary.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.AI;
using ChatResponse = AiLibrary.Application.Dtos.Chat.ChatResponse;

namespace AiLibrary.Application.Commands;

public record ChatCommand(string Message) : IRequest<ChatResponse>;

public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatResponse>
{
    private readonly IChatService _aiChatService;
    private readonly IPromptBuilder _promptBuilder;

    public ChatCommandHandler(
        IChatService aiChatService,
        IPromptBuilder promptBuilder)
    {
        _aiChatService = aiChatService;
        _promptBuilder = promptBuilder;
    }

    public async Task<ChatResponse> Handle(
        ChatCommand request,
        CancellationToken cancellationToken)
    {

        var context = new ChatMessage(
    ChatRole.User,
    request.Message);

        var prompt = _promptBuilder.BuildPrompt(context);

        return await _aiChatService.SendMessageAsync(
            prompt,
            cancellationToken);
    }
}