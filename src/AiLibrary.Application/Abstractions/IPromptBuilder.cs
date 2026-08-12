using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

public interface IPromptBuilder
{
    /// <summary>Generic system persona message seeded once per session.</summary>
    ChatMessage GetSystemMessage();
}
