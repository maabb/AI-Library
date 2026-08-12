using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Chat;
using MediatR;

namespace AiLibrary.Application.Queries.Chat;

// Sidebar: newest sessions (take clamped in the store).
public record ListChatSessionsQuery(int Take = 30) : IRequest<IReadOnlyList<ChatSessionInfo>>;

public sealed class ListChatSessionsQueryHandler
    : IRequestHandler<ListChatSessionsQuery, IReadOnlyList<ChatSessionInfo>>
{
    private readonly IChatHistoryStore _historyStore;

    public ListChatSessionsQueryHandler(IChatHistoryStore historyStore)
    {
        _historyStore = historyStore;
    }

    public Task<IReadOnlyList<ChatSessionInfo>> Handle(
        ListChatSessionsQuery request,
        CancellationToken cancellationToken) =>
        _historyStore.ListSessionsAsync(request.Take, cancellationToken);
}
