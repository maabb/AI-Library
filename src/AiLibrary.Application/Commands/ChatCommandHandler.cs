using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Chat;
using MediatR;

namespace AiLibrary.Application.Commands;

public record ChatCommand(string? SessionId, string Message) : IRequest<ChatResponse>;

public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatResponse>
{
    private readonly IChatService _chatService;
    private readonly IChatHistoryStore _historyStore;

    public ChatCommandHandler(IChatService chatService, IChatHistoryStore historyStore)
    {
        _chatService = chatService;
        _historyStore = historyStore;
    }

    public async Task<ChatResponse> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message is required.", nameof(request));
        }

        var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
            ? Guid.NewGuid().ToString("N")
            : request.SessionId.Trim();

        var prompt = _historyStore.AddUserMessage(sessionId, request.Message.Trim());
        var reply = await _chatService.CompleteAsync(prompt, cancellationToken);
        _historyStore.AddAssistantMessage(sessionId, reply);

        return new ChatResponse
        {
            SessionId = sessionId,
            Message = reply
        };
    }
}
