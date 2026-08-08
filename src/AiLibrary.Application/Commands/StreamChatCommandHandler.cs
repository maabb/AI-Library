using System.Runtime.CompilerServices;
using System.Text;
using AiLibrary.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Commands;

public record StreamChatCommand(string? SessionId, string Message)
    : IRequest<StreamChatResult>;

public sealed class StreamChatResult
{
    public required string SessionId { get; init; }
    public required IAsyncEnumerable<string> Tokens { get; init; }
}

public class StreamChatCommandHandler : IRequestHandler<StreamChatCommand, StreamChatResult>
{
    private readonly IChatService _chatService;
    private readonly IChatHistoryStore _historyStore;

    public StreamChatCommandHandler(IChatService chatService, IChatHistoryStore historyStore)
    {
        _chatService = chatService;
        _historyStore = historyStore;
    }

    public Task<StreamChatResult> Handle(StreamChatCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException("Message is required.", nameof(request));
        }

        var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
            ? Guid.NewGuid().ToString("N")
            : request.SessionId.Trim();

        var prompt = _historyStore.AddUserMessage(sessionId, request.Message.Trim());

        return Task.FromResult(new StreamChatResult
        {
            SessionId = sessionId,
            Tokens = StreamAndPersistAsync(sessionId, prompt, cancellationToken)
        });
    }

    private async IAsyncEnumerable<string> StreamAndPersistAsync(
        string sessionId,
        IReadOnlyList<ChatMessage> prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var full = new StringBuilder();

        await foreach (var update in _chatService.StreamAsync(prompt, cancellationToken))
        {
            if (string.IsNullOrEmpty(update.Text))
            {
                continue;
            }

            full.Append(update.Text);
            yield return update.Text;
        }

        _historyStore.AddAssistantMessage(sessionId, full.ToString());
    }
}
