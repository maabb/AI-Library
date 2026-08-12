using System.Collections.Concurrent;
using AiLibrary.Application.Abstractions;
using AiLibrary.Application.Dtos.Chat;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Services;

/// <summary>
/// In-memory multi-turn store for unit tests. Production uses <see cref="EfChatHistoryStore"/>.
/// </summary>
public class ChatHistoryStore : IChatHistoryStore
{
    // Shared caps with EfChatHistoryStore (model context + local disk bounds).
    public const int MaxMessagesPerSession = 40;
    public const int MaxSessionsRetained = 100;
    public const int MaxListTake = 100;

    private readonly IPromptBuilder _promptBuilder;
    private readonly ConcurrentDictionary<string, SessionState> _sessions = new();

    public ChatHistoryStore(IPromptBuilder promptBuilder)
    {
        _promptBuilder = promptBuilder;
    }

    public Task<IReadOnlyList<ChatMessage>> AddUserMessageAsync(
        string sessionId,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        var session = GetOrCreateSession(sessionId);
        lock (session.Gate)
        {
            session.UpdatedAt = DateTimeOffset.UtcNow;
            session.Messages.Add(new ChatMessage(ChatRole.User, userMessage));
            TrimIfNeeded(session.Messages);
            PruneOldSessions_NoLock();
            return Task.FromResult<IReadOnlyList<ChatMessage>>(session.Messages.ToList());
        }
    }

    public Task AddAssistantMessageAsync(
        string sessionId,
        string assistantMessage,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        var session = GetOrCreateSession(sessionId);
        lock (session.Gate)
        {
            session.UpdatedAt = DateTimeOffset.UtcNow;
            session.Messages.Add(new ChatMessage(ChatRole.Assistant, assistantMessage ?? string.Empty));
            TrimIfNeeded(session.Messages);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult<IReadOnlyList<ChatMessage>>(Array.Empty<ChatMessage>());
        }

        lock (session.Gate)
        {
            return Task.FromResult<IReadOnlyList<ChatMessage>>(session.Messages.ToList());
        }
    }

    public Task<IReadOnlyList<ChatSessionInfo>> ListSessionsAsync(
        int take = 30,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, MaxListTake);

        var list = _sessions
            .Select(kv =>
            {
                lock (kv.Value.Gate)
                {
                    var preview = kv.Value.Messages
                        .LastOrDefault(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
                        ?.Text;
                    if (preview is { Length: > 120 })
                    {
                        preview = preview[..117] + "...";
                    }

                    return new ChatSessionInfo
                    {
                        Id = kv.Key,
                        UpdatedAt = kv.Value.UpdatedAt,
                        Preview = preview
                    };
                }
            })
            .OrderByDescending(s => s.UpdatedAt)
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<ChatSessionInfo>>(list);
    }

    private SessionState GetOrCreateSession(string sessionId)
    {
        return _sessions.GetOrAdd(sessionId, _ => new SessionState
        {
            UpdatedAt = DateTimeOffset.UtcNow,
            Messages = [_promptBuilder.GetSystemMessage()]
        });
    }

    private void PruneOldSessions_NoLock()
    {
        if (_sessions.Count <= MaxSessionsRetained)
        {
            return;
        }

        var oldest = _sessions
            .OrderBy(kv => kv.Value.UpdatedAt)
            .Take(_sessions.Count - MaxSessionsRetained)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var id in oldest)
        {
            _sessions.TryRemove(id, out _);
        }
    }

    private static void TrimIfNeeded(List<ChatMessage> messages)
    {
        if (messages.Count <= MaxMessagesPerSession)
        {
            return;
        }

        var system = messages[0];
        var keep = messages
            .Skip(1)
            .TakeLast(MaxMessagesPerSession - 1)
            .ToList();

        messages.Clear();
        messages.Add(system);
        messages.AddRange(keep);
    }

    private sealed class SessionState
    {
        public object Gate { get; } = new();
        public DateTimeOffset UpdatedAt { get; set; }
        public List<ChatMessage> Messages { get; init; } = [];
    }
}
