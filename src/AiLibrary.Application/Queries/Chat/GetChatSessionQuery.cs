using AiLibrary.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Queries.Chat;

// Resume a chat for the UI. Uses MEAI ChatMessage (no custom message DTO).
// null ⇒ session unknown (404). Empty list after filter ⇒ session exists but no user turns yet.
public record GetChatSessionQuery(string SessionId) : IRequest<IReadOnlyList<ChatMessage>?>;

public sealed class GetChatSessionQueryHandler
    : IRequestHandler<GetChatSessionQuery, IReadOnlyList<ChatMessage>?>
{
    private readonly IChatHistoryStore _historyStore;

    public GetChatSessionQueryHandler(IChatHistoryStore historyStore)
    {
        _historyStore = historyStore;
    }

    public async Task<IReadOnlyList<ChatMessage>?> Handle(
        GetChatSessionQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return null;
        }

        var id = request.SessionId.Trim();
        var history = await _historyStore.GetHistoryAsync(id, cancellationToken);

        // No rows at all ⇒ never created (GetHistory does not create a session).
        if (history.Count == 0)
        {
            return null;
        }

        // Hide system prompt from the UI; model still gets it via AddUserMessageAsync path.
        return history
            .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
            .ToList();
    }
}
