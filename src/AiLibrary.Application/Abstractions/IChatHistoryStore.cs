using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

public interface IChatHistoryStore
{
    /// <summary>
    /// Adds the user message, returns a snapshot of the full conversation
    /// (system + prior turns + this user message) for the model call.
    /// </summary>
    IReadOnlyList<ChatMessage> AddUserMessage(string sessionId, string userMessage);

    /// <summary>Appends the assistant reply after a successful model call.</summary>
    void AddAssistantMessage(string sessionId, string assistantMessage);

    /// <summary>Returns a copy of the current history for inspection/tests.</summary>
    IReadOnlyList<ChatMessage> GetHistory(string sessionId);

    bool Exists(string sessionId);
}
