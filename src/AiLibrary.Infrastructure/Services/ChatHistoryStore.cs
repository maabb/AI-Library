using System.Collections.Concurrent;
using AiLibrary.Application.Abstractions;
using Microsoft.Extensions.AI;

namespace AiLibrary.Infrastructure.Services;

/// <summary>
/// Thread-safe in-memory multi-turn conversation store.
/// Each session starts with the librarian system prompt.
/// </summary>
public class ChatHistoryStore : IChatHistoryStore
{
    private const int MaxMessagesPerSession = 40;

    private readonly IPromptBuilder _promptBuilder;
    private readonly ConcurrentDictionary<string, SessionState> _sessions = new();

    public ChatHistoryStore(IPromptBuilder promptBuilder)
    {
        _promptBuilder = promptBuilder;
    }

    public IReadOnlyList<ChatMessage> AddUserMessage(string sessionId, string userMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        var session = GetOrCreateSession(sessionId);
        lock (session.Gate)
        {
            session.Messages.Add(new ChatMessage(ChatRole.User, userMessage));
            TrimIfNeeded(session.Messages);
            return session.Messages.ToList();
        }
    }

    public void AddAssistantMessage(string sessionId, string assistantMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        var session = GetOrCreateSession(sessionId);
        lock (session.Gate)
        {
            session.Messages.Add(new ChatMessage(ChatRole.Assistant, assistantMessage ?? string.Empty));
            TrimIfNeeded(session.Messages);
        }
    }

    public IReadOnlyList<ChatMessage> GetHistory(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return Array.Empty<ChatMessage>();
        }

        lock (session.Gate)
        {
            return session.Messages.ToList();
        }
    }

    public bool Exists(string sessionId) =>
        !string.IsNullOrWhiteSpace(sessionId) && _sessions.ContainsKey(sessionId);

    private SessionState GetOrCreateSession(string sessionId)
    {
        return _sessions.GetOrAdd(sessionId, _ => new SessionState
        {
            Messages = [_promptBuilder.GetSystemMessage()]
        });
    }

    /// <summary>
    /// Keeps the system message and the newest turns so context stays within model limits.
    /// </summary>
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
        public List<ChatMessage> Messages { get; init; } = [];
    }
}
