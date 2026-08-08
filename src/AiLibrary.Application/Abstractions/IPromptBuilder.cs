using Microsoft.Extensions.AI;

namespace AiLibrary.Application.Abstractions;

public interface IPromptBuilder
{
    IEnumerable<ChatMessage> BuildPrompt(
            IEnumerable<ChatMessage> history,
            ChatMessage userMessage);
}
