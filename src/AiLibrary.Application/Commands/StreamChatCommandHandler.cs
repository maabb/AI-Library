using System.Runtime.CompilerServices;
using System.Text;
using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Chat;
using MediatR;
using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Commands;

// Streaming chat use case: yield tokens; set ToolsUsed only after the stream ends.
public class StreamChatCommandHandler : IRequestHandler<StreamChatCommand, StreamChatResult>
{
    private readonly IChatService _chatService;
    private readonly IChatHistoryStore _historyStore;
    private readonly IToolCallSink _toolCallSink;

    public StreamChatCommandHandler(
        IChatService chatService,
        IChatHistoryStore historyStore,
        IToolCallSink toolCallSink)
    {
        _chatService = chatService;
        _historyStore = historyStore;
        _toolCallSink = toolCallSink;
    }

    public async Task<StreamChatResult> Handle(StreamChatCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message is required.", nameof(request));
        }

        var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
            ? Guid.NewGuid().ToString("N")
            : request.SessionId.Trim();

        // Same as JSON path: durable user turn + prompt snapshot for streaming.
        var prompt = await _historyStore.AddUserMessageAsync(
            sessionId,
            request.Message.Trim(),
            cancellationToken);

        // Tokens close over `result` so ToolsUsed can be set when the stream ends.
        StreamChatResult? result = null;
        result = new StreamChatResult
        {
            SessionId = sessionId,
            Tokens = StreamAndPersistAsync(() => result!, sessionId, prompt, cancellationToken)
        };

        return result;
    }

    private async IAsyncEnumerable<string> StreamAndPersistAsync(
        Func<StreamChatResult> resultAccessor,
        string sessionId,
        IReadOnlyList<ChatMessage> prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var full = new StringBuilder();

        await foreach (var token in _chatService.StreamAsync(prompt, cancellationToken))
        {
            full.Append(token);
            yield return token;
        }

        // After all tokens: save full reply + attach tool chips for SSE done event.
        await _historyStore.AddAssistantMessageAsync(sessionId, full.ToString(), cancellationToken);
        resultAccessor().ToolsUsed = ToolCallMapping.FromSink(_toolCallSink);
    }
}
