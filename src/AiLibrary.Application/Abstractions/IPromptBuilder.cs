using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

public interface IPromptBuilder
{
    /// <summary>Librarian system message (persona + catalog block) seeded once per session.</summary>
    ChatMessage GetSystemMessage();
}
