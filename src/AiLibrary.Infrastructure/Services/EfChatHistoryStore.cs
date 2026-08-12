using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Chat;
using AiLibrary.Infrastructure.Data;
using AiLibrary.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Services;

/// <summary>
/// Durable chat history. Uses the request-scoped <see cref="SqlContext"/> (not a new context per call).
/// </summary>
public sealed class EfChatHistoryStore : IChatHistoryStore
{
    private readonly SqlContext _db;
    private readonly IPromptBuilder _promptBuilder;

    public EfChatHistoryStore(SqlContext db, IPromptBuilder promptBuilder)
    {
        _db = db;
        _promptBuilder = promptBuilder;
    }

    public async Task<IReadOnlyList<ChatMessage>> AddUserMessageAsync(
        string sessionId,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        // First message of a conversation: create session + system seed, then user turn.
        await CreateSessionIfMissingAsync(sessionId, cancellationToken);
        await AppendMessageAsync(sessionId, ChatRole.User.Value, userMessage, cancellationToken);

        await TouchSessionAsync(sessionId, cancellationToken);
        await TrimSessionAsync(sessionId, cancellationToken);
        await PruneOldSessionsAsync(cancellationToken);

        return await LoadMessagesAsync(sessionId, cancellationToken);
    }

    public async Task AddAssistantMessageAsync(
        string sessionId,
        string assistantMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        // One query: max sequence also proves the session exists (we always seed system at 0).
        var maxSeq = await _db.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .Select(m => (int?)m.Sequence)
            .MaxAsync(cancellationToken);

        if (maxSeq is null)
        {
            throw new InvalidOperationException(
                $"Cannot add assistant message: session '{sessionId}' does not exist.");
        }

        _db.ChatMessages.Add(new ChatMessageRow
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = ChatRole.Assistant.Value,
            Content = assistantMessage ?? string.Empty,
            Sequence = maxSeq.Value + 1,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        await TouchSessionAsync(sessionId, cancellationToken);
        await TrimSessionAsync(sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        // Read-only: empty if unknown — never creates a session.
        return await LoadMessagesAsync(sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<ChatSessionInfo>> ListSessionsAsync(
        int take = 30,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, ChatHistoryStore.MaxListTake);

        // SQLite cannot ORDER BY DateTimeOffset in SQL — sort in memory (count is capped).
        var sessions = (await _db.ChatSessions
                .AsNoTracking()
                .Select(s => new { s.Id, s.UpdatedAt })
                .ToListAsync(cancellationToken))
            .OrderByDescending(s => s.UpdatedAt)
            .Take(take)
            .ToList();

        var result = new List<ChatSessionInfo>(sessions.Count);
        foreach (var s in sessions)
        {
            var preview = await _db.ChatMessages
                .AsNoTracking()
                .Where(m => m.SessionId == s.Id &&
                            (m.Role == ChatRole.User.Value || m.Role == ChatRole.Assistant.Value))
                .OrderByDescending(m => m.Sequence)
                .Select(m => m.Content)
                .FirstOrDefaultAsync(cancellationToken);

            if (preview is { Length: > 120 })
            {
                preview = preview[..117] + "...";
            }

            result.Add(new ChatSessionInfo
            {
                Id = s.Id,
                UpdatedAt = s.UpdatedAt,
                Preview = preview
            });
        }

        return result;
    }

    private async Task CreateSessionIfMissingAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        if (await _db.ChatSessions.AnyAsync(s => s.Id == sessionId, cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var system = _promptBuilder.GetSystemMessage();

        _db.ChatSessions.Add(new ChatSession
        {
            Id = sessionId,
            CreatedAt = now,
            UpdatedAt = now
        });

        _db.ChatMessages.Add(new ChatMessageRow
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = system.Role.Value,
            Content = system.Text ?? string.Empty,
            Sequence = 0,
            CreatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task AppendMessageAsync(
        string sessionId,
        string role,
        string content,
        CancellationToken cancellationToken)
    {
        var maxSeq = await _db.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .Select(m => (int?)m.Sequence)
            .MaxAsync(cancellationToken);

        _db.ChatMessages.Add(new ChatMessageRow
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = role,
            Content = content,
            Sequence = (maxSeq ?? -1) + 1,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }

    // Normal load + property assign (clearer than ExecuteUpdate SetProperty).
    private async Task TouchSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        var session = await _db.ChatSessions.FirstAsync(s => s.Id == sessionId, cancellationToken);
        session.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ChatMessage>> LoadMessagesAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var rows = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.Sequence)
            .ToListAsync(cancellationToken);

        return rows.Select(ToChatMessage).ToList();
    }

    private static ChatMessage ToChatMessage(ChatMessageRow row)
    {
        var role = row.Role switch
        {
            "system" => ChatRole.System,
            "assistant" => ChatRole.Assistant,
            "tool" => ChatRole.Tool,
            _ => ChatRole.User
        };

        return new ChatMessage(role, row.Content);
    }

    private async Task TrimSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        var count = await _db.ChatMessages
            .CountAsync(m => m.SessionId == sessionId, cancellationToken);

        if (count <= ChatHistoryStore.MaxMessagesPerSession)
        {
            return;
        }

        var overflow = count - ChatHistoryStore.MaxMessagesPerSession;
        var oldest = await _db.ChatMessages
            .Where(m => m.SessionId == sessionId && m.Role != ChatRole.System.Value)
            .OrderBy(m => m.Sequence)
            .Take(overflow)
            .ToListAsync(cancellationToken);

        if (oldest.Count == 0)
        {
            return;
        }

        _db.ChatMessages.RemoveRange(oldest);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task PruneOldSessionsAsync(CancellationToken cancellationToken)
    {
        var total = await _db.ChatSessions.CountAsync(cancellationToken);
        if (total <= ChatHistoryStore.MaxSessionsRetained)
        {
            return;
        }

        var drop = total - ChatHistoryStore.MaxSessionsRetained;
        var oldest = (await _db.ChatSessions.ToListAsync(cancellationToken))
            .OrderBy(s => s.UpdatedAt)
            .Take(drop)
            .ToList();

        if (oldest.Count == 0)
        {
            return;
        }

        // Cascade deletes messages via FK.
        _db.ChatSessions.RemoveRange(oldest);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
