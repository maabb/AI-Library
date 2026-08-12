using AiLibrary.Application.Dtos.Chat;
using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

// Port for multi-turn memory. Prod = EF/SQLite; tests = in-memory.
// Does not create the database file — that is MigrateAsync on startup.
public interface IChatHistoryStore
{
    // Creates session + system seed on first use, then appends user turn; returns full prompt for the model.
    Task<IReadOnlyList<ChatMessage>> AddUserMessageAsync(
        string sessionId,
        string userMessage,
        CancellationToken cancellationToken = default);

    Task AddAssistantMessageAsync(
        string sessionId,
        string assistantMessage,
        CancellationToken cancellationToken = default);

    // Empty list if session never existed (read-only — does not create a session).
    Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    // Sidebar list; take is clamped by the implementation.
    Task<IReadOnlyList<ChatSessionInfo>> ListSessionsAsync(
        int take = 30,
        CancellationToken cancellationToken = default);
}
