using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Chat;
using MediatR;

namespace AiLibrary.Application.Commands;

// Non-streaming chat use case: history → model → history → JSON (with toolsUsed).
public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatResponse>
{
    private readonly IChatService _chatService;
    private readonly IChatHistoryStore _historyStore;
    private readonly IToolCallSink _toolCallSink;

    public ChatCommandHandler(
        IChatService chatService,
        IChatHistoryStore historyStore,
        IToolCallSink toolCallSink)
    {
        _chatService = chatService;
        _historyStore = historyStore;
        _toolCallSink = toolCallSink;
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

        // Sink was filled during CompleteAsync if the model called catalog tools.
        return new ChatResponse
        {
            SessionId = sessionId,
            Message = reply,
            ToolsUsed = ToolCallMapping.FromSink(_toolCallSink)
        };
    }
}
